using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StackExchange.Redis;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PerformanceComparison
{
    class Queue
    {

        public static void RabbitMQInsert(int numberOfInserts)
        {
            try
            {
                Random random = new Random();

                var watch12 = System.Diagnostics.Stopwatch.StartNew();

                var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "MyTestQueue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                        for (int m = 1; m < numberOfInserts; m = m + 1)
                        {
                            int randomNumber = random.Next(0, 100);
                            string message = "ws_call_" + randomNumber;
                            var body = Encoding.UTF8.GetBytes(message);

                            channel.BasicPublish(exchange: "",
                                                 routingKey: "MyTestQueue",
                                                 basicProperties: null,
                                                 body: body);
                        }
                    }
                }

                watch12.Stop();

                Console.WriteLine("   RabbitMQ Inserts: " + watch12.ElapsedMilliseconds + " ms");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static void RabbitMQSelectFromQueue()
        {
            try
            {

                var watch13 = System.Diagnostics.Stopwatch.StartNew();

                var factory = new RabbitMQ.Client.ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        // Create a queue if it does not exist
                        //channel.QueueDeclare(queue: "MyTestQueue",
                        //         durable: true,
                        //         exclusive: false,
                        //         autoDelete: false,
                        //         arguments: null);

                        // Get the size of the queue
                        var messageCount = channel.QueueDeclarePassive("MyTestQueue").MessageCount;
                        Console.WriteLine("      Queue length - " + messageCount);
                        
                        // Dequeue a single message at a time
                        for (int a = 0; a < 10; a = a + 1)
                        {
                            var data = channel.BasicGet(queue: "MyTestQueue", noAck: true);
                            Console.WriteLine("        Processing message - " + Encoding.UTF8.GetString(data.Body));
                        }

                        // Dequeue all the messages (this downloads all the messages into the consumer and work continues with the consumers queue)
                        var consumer = new QueueingBasicConsumer(channel);
                        //channel.BasicQos(10, 10, false); // Per consumer limit
                        
                        channel.BasicConsume(queue: "MyTestQueue", noAck: true, consumer: consumer);
                        for (int a = 0; a < 10; a = a + 1)
                        {
                            BasicDeliverEventArgs e = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                            Console.WriteLine("        Recieved Message (via bulk consume) : " + Encoding.ASCII.GetString(e.Body));
                        }
                    }
                }

                watch13.Stop();
                Console.WriteLine("   RabbitMQ Select: " + watch13.ElapsedMilliseconds + " ms");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static void RedisListInsert(int numberOfInserts)
        {
            try
            {

                Random random = new Random();
                //-----------------------------------------------------
                // Insert into Redis
                var redis = RedisStore.RedisCache; // Setting up connectinon to Redis

                var clientName = "queue1";
                var date = DateTime.Now.AddDays(1).ToString("yyyyMMdd");

                var RedisListKey = clientName + ":" + date;

                // Generate the load
                var watch = System.Diagnostics.Stopwatch.StartNew();
                for (int a = 0; a < numberOfInserts; a = a + 1)
                {
                    string json = "email: " + a + "@test.com;" + "body: " + "Test mesage " + a;

                    redis.ListLeftPush(RedisListKey, json, flags: CommandFlags.FireAndForget);
                }
                watch.Stop();

                Console.WriteLine("________________________________");
                Console.WriteLine("   Redis Inserts: " + watch.ElapsedMilliseconds + " ms");

            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void RedisSelectFromQueue()
        {
            try
            {
                // Redis - Read from the queue
                var redis = RedisStore.RedisCache; // Setting up connectinon to Redis

                var clientName = "queue1";
                var date = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
                int counter = 0;

                var RedisListKey = clientName + ":" + date;

                var watch3 = System.Diagnostics.Stopwatch.StartNew();
                for (counter = 1; counter <= 10; counter++)
                {
                    // Read and remove a message from the queue
                    var message = redis.ListRightPop(RedisListKey);
                    Console.WriteLine("        Processing message - " + message);
                }
                watch3.Stop();
                Console.WriteLine("   Redis Reads: " + watch3.ElapsedMilliseconds + " ms");

            }
            catch (RedisConnectionException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}
