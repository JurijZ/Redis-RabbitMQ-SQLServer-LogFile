using StackExchange.Redis;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace PerformanceComparison
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfInserts = 1000; // Number of rows to be inserted

            try
            {
                

                if (args.Length == 0)
                {
                    Console.WriteLine("Use one of these parameters: setup, log, queue, reset");
                }

                if (args.Length > 0)
                {

                    if (args[0] == "setup")
                    {
                        InitialSetup.Setup();
                    }
                    if (args[0] == "log")
                    {
                        PerformanceComparison.WSLog.RedisHashInsert(numberOfInserts);
                        PerformanceComparison.WSLog.EventLogInsert(numberOfInserts);
                        PerformanceComparison.WSLog.SQLInsert(numberOfInserts);
                        PerformanceComparison.WSLog.RedisHashSelect();
                        PerformanceComparison.WSLog.SQLSelect();
                    }
                    if (args[0] == "reset")
                    {
                        PerformanceComparison.Reset.ResetRedis();
                        PerformanceComparison.Reset.ResetSQL();
                    }

                    if (args[0] == "queue")
                    {
                        PerformanceComparison.Queue.RabbitMQInsert(numberOfInserts);
                        PerformanceComparison.Queue.RabbitMQSelectFromQueue();
                        PerformanceComparison.Queue.RedisListInsert(numberOfInserts);
                        PerformanceComparison.Queue.RedisSelectFromQueue();
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
