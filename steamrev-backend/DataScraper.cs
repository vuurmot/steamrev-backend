using Npgsql;
using Newtonsoft.Json.Linq;
using NpgsqlTypes;
namespace steamrev_backend.Server
{
    public class DataScraper
    {
        //http://api.steampowered.com/ISteamApps/GetAppList/v0002/?format=json
        //http://store.steampowered.com/api/appdetails?appids=10&cc=us
        //https://store.steampowered.com/appreviews/10?json=1&language=all&num_per_page=0


        public async Task UpdateSteamApps()
        {
            try
            {
                HttpClient httpClient = new() { };
                using HttpResponseMessage response = await httpClient.GetAsync("http://api.steampowered.com/ISteamApps/GetAppList/v0002/?format=json");

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                dynamic apps = JObject.Parse(jsonResponse);
                if (apps == null)
                    return;

                await using var conn = new NpgsqlConnection(DatabaseSettings.ConnectionCredentials);
                await conn.OpenAsync();


                foreach (var app in apps["applist"]["apps"])
                {
                    int appid = int.Parse(app["appid"].ToString());
                    string name = app["name"].ToString();
                    await using var cmd = new NpgsqlCommand("INSERT INTO steamapps VALUES ($1, $2) ON CONFLICT DO NOTHING", conn)
                    {
                        Parameters =
                                {
                                    new() { Value = appid },
                                    new() { Value = name }
                                }

                    };
                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"Inserted appid: {appid} name: {name}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task UpdateSteamAppDetails()
        {
            HttpClient httpClient = new() { };

            try
            {

                await using var conn = new NpgsqlConnection(DatabaseSettings.ConnectionCredentials);
                await conn.OpenAsync();
                List<int> appidList = new List<int>();
                Console.WriteLine("Starting...");
                await using (var cmd = new NpgsqlCommand("SELECT steamapps.appid FROM steamapps LEFT JOIN appdetails USING (appid) WHERE appdetails.appid IS NULL", conn))

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        appidList.Add(reader.GetInt32(0));
                    }
                }
                Console.WriteLine("List added....");
                foreach (int appid in appidList)
                {
                    try
                    {
                        await Task.Delay(2000);
                        Console.WriteLine("Starting work on: " + appid.ToString());

                        using HttpResponseMessage response = await httpClient.GetAsync($"http://store.steampowered.com/api/appdetails?appids={appid}&cc=us");
                        response.EnsureSuccessStatusCode();
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        dynamic details = JObject.Parse(jsonResponse);
                        Console.WriteLine(details);
                        JObject data;
                        if (details[appid.ToString()]["success"] == false)
                        {
                            data = new JObject();
                        }
                        else
                        {
                            data = details[appid.ToString()]["data"];
                        }

                        await using var insertDetails = new NpgsqlCommand("INSERT INTO appdetails VALUES ($1, $2) ON CONFLICT DO NOTHING", conn)
                        {
                            Parameters =
                                {
                                    new() { Value = appid },
                                    new() { Value = data.ToString(), NpgsqlDbType = NpgsqlDbType.Jsonb   }
                                }

                        };
                        await insertDetails.ExecuteNonQueryAsync();
                        Console.WriteLine($"Updated appid: {appid}");
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task UpdateSteamAppReviews()
        {
            HttpClient httpClient = new() { };

            try
            {
                await using var conn = new NpgsqlConnection(DatabaseSettings.ConnectionCredentials);
                await conn.OpenAsync();
                List<int> appidList = new List<int>();
                Console.WriteLine("Starting...");

                await using (var cmd = new NpgsqlCommand(@"
                    SELECT * FROM appdetails
                    LEFT JOIN appreviews
                    USING (appid) WHERE details->> 'type' = 'game'
                    ORDER BY (appreviews.reviewdetails IS NULL) DESC", conn))

                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        appidList.Add(reader.GetInt32(0));
                    }
                }

                Console.WriteLine("List added....");
                foreach (int appid in appidList)
                {
                    try
                    {
                        await Task.Delay(2000);
                        Console.WriteLine("Starting work on: " + appid.ToString());

                        using HttpResponseMessage response = await httpClient.GetAsync($"https://store.steampowered.com/appreviews/{appid}?json=1&language=all&num_per_page=0");
                        response.EnsureSuccessStatusCode();
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        dynamic details = JObject.Parse(jsonResponse);
                        Console.WriteLine(details);
                        JObject data;
                        if (details["success"] == false)
                        {
                            data = new JObject();
                        }
                        else
                        {
                            data = details["query_summary"];
                        }

                        await using var insertDetails = new NpgsqlCommand("INSERT INTO appreviews VALUES ($1, $2) ON CONFLICT DO NOTHING", conn)
                        {
                            Parameters =
                                {
                                    new() { Value = appid },
                                    new() { Value = data.ToString(), NpgsqlDbType = NpgsqlDbType.Jsonb   }
                                }

                        };
                        await insertDetails.ExecuteNonQueryAsync();
                        Console.WriteLine($"Updated appid: {appid}");
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
