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
    static class InitialSetup
    {
        public static void Setup()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.AppSettings["sql.masterconnection"]))
            {
                connection.Open();

                // Check if the Test DB exists
                using (var checkcomm = new SqlCommand($"SELECT db_id('Test')", connection))
                {
                    if (checkcomm.ExecuteScalar() != DBNull.Value)
                    {
                        Console.WriteLine("Test database exists. Deleting and creating a new one");

                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = connection;
                            command.CommandType = CommandType.Text;
                            command.CommandText = "DROP DATABASE Test; CREATE DATABASE Test;";
                            command.ExecuteNonQuery();
                        }

                        // Create Test tables
                        CreateTestTables(connection);
                    }
                    else
                    {
                        Console.WriteLine("Creating new Test database");

                        using (SqlCommand command = new SqlCommand())
                        {
                            command.Connection = connection;
                            command.CommandType = CommandType.Text;
                            command.CommandText = "CREATE DATABASE Test";
                            command.ExecuteNonQuery();
                        }

                        // Create Test tables
                        CreateTestTables(connection);
                    }
                }

                connection.Close();
            }
        }

        public static void CreateTestTables(SqlConnection connection)
        {
            Console.WriteLine("Creating new Test tables");

            using (SqlCommand command = new SqlCommand())
            {
                string sqlTableCreation = "USE Test; " +
                    "CREATE TABLE ws_call_log3 (ws_name nvarchar(20), date nvarchar(20), [usage_count] int);" +
                    "CREATE TABLE ws_call_log2 (ws_name nvarchar(20), date nvarchar(20))" +
                    "CREATE TABLE ws_call_log (ws_name nvarchar(20), date nvarchar(20))";

                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = sqlTableCreation;
                command.ExecuteNonQuery();
            }
        }
    }
}
