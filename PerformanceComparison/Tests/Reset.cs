using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Redis;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace PerformanceComparison
{
    class Reset
    {
        public static void ResetRedis()
        {
            try
            {
                // Reset Redis hash table
                var redis = RedisStore.RedisCache; // Setting up connectinon to Redis

                var clientName = "client1";
                var date = DateTime.Now.AddDays(1).ToString("yyyyMMdd");

                var RedisHashKey = clientName + ":" + date;
                redis.KeyDelete(RedisHashKey); //Delete hash table
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void ResetSQL()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["sql.connection"]))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;

                        command.CommandText = "TRUNCATE TABLE ws_call_log;";
                        command.ExecuteNonQuery();

                        command.CommandText = "TRUNCATE TABLE ws_call_log2;";
                        command.ExecuteNonQuery();

                        command.CommandText = "TRUNCATE TABLE ws_call_log3;";
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
