using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using steamrev_backend.Server.Helpers;

namespace steamrev_backend.Server
{
    public class Metrics
    {
        public async Task<Dictionary<string, object>> GetAverageEarningsForYear(int year)
        {
            List<Dictionary<string, object>> kvp;

            await using var conn = new NpgsqlConnection(DatabaseSettings.ConnectionCredentials);
            await conn.OpenAsync();
            await using (var cmd = new NpgsqlCommand($@"
                SELECT AVG((reviewdetails->>'total_reviews')::int) AS average_sales,
                AVG((details->'price_overview'->>'initial')::bigint * (reviewdetails->>'total_reviews')::int) AS average_earnings
                FROM appdetails
                LEFT JOIN appreviews USING (appid)
                WHERE DATE_PART('year', cast_to_date(details->'release_date'->>'date', NULL)) = {year}
                AND (reviewdetails->>'total_reviews')::int >= 10
                AND (details->'price_overview'->>'initial') IS NOT NULL
                ", conn))

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                kvp = await CommonHelpers.ConvertReaderToJSON(reader);

            }
            
            return kvp[0];
        }
        public async Task<List<Dictionary<string, object>>> GetAllGames()
        {
            List<Dictionary<string, object>> gameList;

            await using var conn = new NpgsqlConnection(DatabaseSettings.ConnectionCredentials);
            await conn.OpenAsync();

            await using (var cmd = new NpgsqlCommand(@"
                SELECT steamapps.appid, steamapps.name, appdetails.details->>'header_image' AS header_image FROM steamapps
                LEFT JOIN appdetails USING (appid) WHERE appdetails.details->'header_image' IS NOT NULL LIMIT 10", conn))

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                gameList = await CommonHelpers.ConvertReaderToJSON(reader);
            }

            return gameList;
        }
        public async Task<Dictionary<string, dynamic>> GetGameDetailsUsingID(int appid)
        {
            List<Dictionary<string, dynamic>> gameDetails;

            await using var conn = new NpgsqlConnection(DatabaseSettings.ConnectionCredentials);
            await conn.OpenAsync();

            await using (var cmd = new NpgsqlCommand($"SELECT * FROM appdetails LEFT JOIN appreviews USING (appid) WHERE appid = {appid}", conn))

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                gameDetails = await CommonHelpers.ConvertReaderToJSON(reader);
            }
            gameDetails[0]["details"] = JObject.Parse(gameDetails[0]["details"]);
            return gameDetails[0];
        }
    }
}
