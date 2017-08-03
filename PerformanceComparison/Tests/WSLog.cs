using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Redis;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using SyslogNet.Client;
using System.Diagnostics;

namespace PerformanceComparison
{
    class WSLog
    {


        public static void EventLogInsert(int numberOfInserts)
        {
            try
            {
                // Create the source, if it does not already exist.
                //if (!EventLog.SourceExists("MyTestSource"))
                //{

                //    //cmd: eventcreate /ID 1 /L APPLICATION /T INFORMATION  /SO MyTestSource /D "MyEventLog"
                //    // PS: New-EventLog -LogName Application -Source MyTestSource

                //    // PS: Write-EventLog -LogName Application -Source PerfLib -EntryType Information  -Message "----Test----" -EventId 1


                //    //An event log source should not be created and immediately used.
                //    //There is a latency time to enable the source, it should be created
                //    //prior to executing the application that uses the source.
                //    //Execute this sample a second time to use the new source.
                //    EventLog.CreateEventSource("MySource", "MyNewLog");
                //    Console.WriteLine("CreatedEventSource");
                //    Console.WriteLine("Exiting, execute the application a second time to use the source.");
                //    // The source is created.  Exit the application to allow it to be registered.
                //    return;
                //}

                // Create an EventLog instance and assign its source.
                //EventLog myLog = new EventLog();
                //myLog.Source = "PerfLib";


                // Write an informational entry to the event log. 
                Random random = new Random();

                var watch11 = System.Diagnostics.Stopwatch.StartNew();
                for (int a = 1; a < numberOfInserts; a = a + 1)
                {
                    int randomNumber = random.Next(0, 100);
                    EventLog.WriteEntry("PerfLib", "--- ws_call_" + randomNumber + " ---", EventLogEntryType.Information, 1);                 
                }
                watch11.Stop();

                Console.WriteLine("   EventLog Inserts: " + watch11.ElapsedMilliseconds + " ms");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static void RedisHashInsert(int numberOfInserts)
        {
            try
            {

                Random random = new Random();
                //-----------------------------------------------------
                // Insert into Redis
                var redis = RedisStore.RedisCache; // Setting up connectinon to Redis

                var clientName = "client1";
                var date = DateTime.Now.AddDays(1).ToString("yyyyMMdd");

                var RedisHashKey = clientName + ":" + date;

                // Creation by definition
                //redis.HashSet(RedisHashKey, "ws_call_1", 1);

                // Generate the load
                //System.GC.Collect();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                for (int a = 1; a < numberOfInserts; a = a + 1)
                {
                    int randomNumber = random.Next(0, 100);
                    redis.HashIncrement(RedisHashKey, "ws_call_" + randomNumber, 1);
                }
                //System.GC.Collect();
                watch.Stop();

                Console.WriteLine("________________________________");
                Console.WriteLine("Inserting " + numberOfInserts + " rows:");
                Console.WriteLine("   Redis Incremental Inserts: " + watch.ElapsedMilliseconds + " ms");
                
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


        public static void RedisHashSelect()
        {
            try
            {
                // Read from Redis
                // Redis - Read the value of a single key
                var redis = RedisStore.RedisCache; // Setting up connectino to Redis

                var clientName = "client1";
                var date = DateTime.Now.AddDays(1).ToString("yyyyMMdd");

                var RedisHashKey = clientName + ":" + date;

                var watch3 = System.Diagnostics.Stopwatch.StartNew();
                if (redis.HashExists(RedisHashKey, "ws_call_1"))
                {
                    var ws_call = redis.HashGet(RedisHashKey, "ws_call_1");
                    Console.WriteLine("Read the count of a single key (ws_call_1) - " + ws_call);
                }
                watch3.Stop();
                Console.WriteLine("   Redis Reads: " + watch3.ElapsedMilliseconds + " ms");


                // Redis - Accumulate all the values from the hash 
                var watch4 = System.Diagnostics.Stopwatch.StartNew();
                var values = redis.HashValues(RedisHashKey);
                int accumulator = 0;

                foreach (var val in values)
                {
                    accumulator = accumulator + (int)val;
                }
                Console.WriteLine("Accumulated value of all the keys in the hash - " + accumulator); // accumulated result for a client
                watch4.Stop();
                Console.WriteLine("   Redis Reads: " + watch4.ElapsedMilliseconds + " ms");


                // Redis - get the length of the hash
                var watch5 = System.Diagnostics.Stopwatch.StartNew();
                var len = redis.HashLength(RedisHashKey);
                Console.WriteLine("Get the number of different ws calls - " + len);

                watch5.Stop();
                Console.WriteLine("   Redis Reads: " + watch5.ElapsedMilliseconds + " ms");
            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }



        public static void SQLInsert(int numberOfInserts)
        {
            try
            {
                Random random = new Random();

                //-----------------------------------------------------
                // Upsert into SQL Server
                var watch10 = System.Diagnostics.Stopwatch.StartNew();
                var date10 = DateTime.Now.ToString("yyyyMMdd");

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["sql.connection"]))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = @";with cte([ws_name], [date], [usage_count])
                                                AS
                                                (SELECT @ws_name,@call_date,1)

                                                MERGE 
                                                   [ws_call_log3] AS target
                                                USING 
                                                   cte AS source
                                                ON 
                                                   target.ws_name = source.ws_name 
                                                WHEN MATCHED THEN 
                                                   UPDATE SET usage_count = ISNULL(target.usage_count,0) + 1
                                                WHEN NOT MATCHED THEN 
                                                   INSERT ([ws_name], [date], [usage_count]) VALUES (source.[ws_name], source.[date], source.[usage_count]);";
                        command.Parameters.Add("@ws_name", SqlDbType.VarChar);
                        command.Parameters.Add("@call_date", SqlDbType.VarChar);

                        for (int a = 1; a < numberOfInserts; a = a + 1)
                        {
                            int randomNumber = random.Next(0, 100);
                            var ws_name10 = "ws_call_" + randomNumber;

                            command.Parameters["@ws_name"].Value = ws_name10;
                            command.Parameters["@call_date"].Value = date10;

                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
                //System.GC.Collect();
                watch10.Stop();

                //-----------------------------------------------------
                // Insert into SQL Server
                var watch2 = System.Diagnostics.Stopwatch.StartNew();
                var date2 = DateTime.Now.ToString("yyyyMMdd");

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["sql.connection"]))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO ws_call_log (ws_name, date) VALUES (@ws_name, @call_date);";

                        command.Prepare();

                        command.Parameters.Add("@ws_name", SqlDbType.VarChar);
                        command.Parameters.Add("@call_date", SqlDbType.VarChar);

                        for (int a = 1; a < numberOfInserts; a = a + 1)
                        {
                            int randomNumber = random.Next(0, 100);
                            var ws_name2 = "ws_call_" + randomNumber;

                            command.Parameters["@ws_name"].Value = ws_name2;
                            command.Parameters["@call_date"].Value = date2;

                            command.ExecuteNonQuery();
                            //command.Parameters.Clear();
                        }
                    }
                    connection.Close();
                }
                //System.GC.Collect();
                watch2.Stop();

                //-----------------------------------------------------
                // Insert into SQL Server 2
                var watch9 = System.Diagnostics.Stopwatch.StartNew();
                var date9 = DateTime.Now.ToString("yyyyMMdd");

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["sql.connection"]))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO ws_call_log2 (ws_name, date) VALUES (@ws_name, @call_date);";
                        command.Parameters.Add("@ws_name", SqlDbType.VarChar);
                        command.Parameters.Add("@call_date", SqlDbType.VarChar);

                        for (int a = 1; a < numberOfInserts; a = a + 1)
                        {
                            int randomNumber = random.Next(0, 100);
                            var ws_name9 = "ws_call_" + randomNumber;

                            command.Parameters["@ws_name"].Value = ws_name9;
                            command.Parameters["@call_date"].Value = date9;

                            command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
                //System.GC.Collect();
                watch9.Stop();


                

                Console.WriteLine("   SQL Server Inserts (version 1): " + watch9.ElapsedMilliseconds + " ms");
                Console.WriteLine("   SQL Server Inserts (version 2): " + watch2.ElapsedMilliseconds + " ms");
                Console.WriteLine("   SQL Server Upserts: " + watch10.ElapsedMilliseconds + " ms");
                Console.WriteLine("________________________________");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        public static void SQLSelect()
        {
            try
            {
                // Select from SQL Server
                Console.WriteLine("________________________________");

                string sql1 = "SELECT COUNT(*) FROM ws_call_log WHERE ws_name = 'ws_call_1'";
                string sql2 = "SELECT COUNT(*) FROM ws_call_log";
                string sql3 = "SELECT COUNT(DISTINCT ws_name) AS COUNT FROM ws_call_log";

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["sql.connection"]))
                {
                    var watch6 = System.Diagnostics.Stopwatch.StartNew();
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql1, connection))
                    {
                        var dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            dr.Read(); // read first row
                            var ws_call_SQL = dr.GetInt32(0);
                            Console.WriteLine("Read the count of a single key (ws_call_1) - " + ws_call_SQL);
                        }
                    }
                    connection.Close();
                    watch6.Stop();
                    Console.WriteLine("   SQL Server Reads: " + watch6.ElapsedMilliseconds + " ms");

                    var watch7 = System.Diagnostics.Stopwatch.StartNew();
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql2, connection))
                    {
                        var dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            dr.Read(); // read first row
                            var accumulator_SQL = dr.GetInt32(0);
                            Console.WriteLine("Accumulated value of all the keys in the hash - " + accumulator_SQL);
                        }
                    }
                    connection.Close();
                    watch7.Stop();
                    Console.WriteLine("   SQL Server Reads: " + watch7.ElapsedMilliseconds + " ms");

                    var watch8 = System.Diagnostics.Stopwatch.StartNew();
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql3, connection))
                    {
                        var dr = command.ExecuteReader();
                        if (dr.HasRows)
                        {
                            dr.Read(); // read first row
                            var len_SQL = dr.GetInt32(0);
                            Console.WriteLine("Get the number of different ws calls - " + len_SQL);
                        }
                    }
                    connection.Close();
                    watch8.Stop();
                    Console.WriteLine("   SQL Server Reads: " + watch8.ElapsedMilliseconds + " ms");

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
