using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using app.Core;
using MySql.Data.MySqlClient;

namespace app.Services
{
    public class DataService
    {
        private static string connectionString;
        private readonly Configuration _configuration;

        public DataService(Configuration configuration)
        {
            _configuration = configuration;
        }

        public async Task InitializeAsync()
        {
            DataService.connectionString =
                $"server={_configuration.GetVariable("DB_SERVER")};" +
                $"database={_configuration.GetVariable("DB_DB")};" +
                $"user={_configuration.GetVariable("DB_USER")};" +
                $"password={_configuration.GetVariable("DB_PASS")};" +
                $"port=3306;";

            await Task.CompletedTask;
        }

        private static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
        
        public static Dictionary<string, List<object>> Get(string query, Dictionary<string, object> parameters)
        {
            var dataSet = new Dictionary<string, List<object>>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                if (parameters != null)
                {
                    foreach (var kv in parameters)
                    {
                        cmd.Parameters.AddWithValue(kv.Key, kv.Value);
                    }
                }

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int column = 0; column != reader.FieldCount; ++column)
                        {
                            var columnName = reader.GetName(column);

                            if (!dataSet.ContainsKey(columnName))
                            {
                                dataSet.Add(columnName, new List<object>());
                                dataSet[columnName].Add(reader.GetValue(column));
                            }
                            else
                            {
                                dataSet[columnName].Add(reader.GetValue(column));
                            }
                        }
                    }
                }
            }

            return dataSet;
        }

        public static void Update(string query, Dictionary<string, object> data)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                foreach (var kv in data)
                {
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value);
                }

                cmd.ExecuteNonQuery();
            }
        }

        public static void Put(string query, Dictionary<string, object> data)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                foreach (var kv in data)
                {
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value);
                }

                cmd.ExecuteNonQuery();
            }
        }

        public static void Drop(string query, Dictionary<string, object> data)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);

                foreach (var kv in data)
                {
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value);
                }

                cmd.ExecuteNonQuery();
            }
        }
    }
}
