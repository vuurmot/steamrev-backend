using Newtonsoft.Json;
using Npgsql;

namespace steamrev_backend.Server.Helpers
{
    public class CommonHelpers
    {
        public static async Task<List<Dictionary<string, dynamic>>> ConvertReaderToJSON(NpgsqlDataReader reader)
        {
            List<Dictionary<string, dynamic>> objectList = new List<Dictionary<string, dynamic>>();

            do while (await reader.ReadAsync())
                {
                    var rowData = new Dictionary<string, dynamic>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        rowData.Add(reader.GetName(i), reader.GetValue(i));
                    }
                    objectList.Add(rowData);
                } while (await reader.NextResultAsync());

            return objectList;
        }
    }
}
