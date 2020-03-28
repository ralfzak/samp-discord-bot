using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace discordBot.Services
{
    public static class DataService
    {
        private static string CONNECTION_STRING = $"server={Program.DB_SERVER};port=3306;database={Program.DB_DB};user={Program.DB_USER};password={Program.DB_PASS};";

        private static MySqlConnection GetConnection()
        {
            return new MySqlConnection(CONNECTION_STRING);
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
