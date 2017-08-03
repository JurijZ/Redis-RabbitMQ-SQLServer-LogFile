using System;
using System.Configuration;
using StackExchange.Redis;

namespace PerformanceComparison
{
    public class RedisStore
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisStore()
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { ConfigurationManager.AppSettings["redis.connection"] }
                //EndPoints = { {"192.168.1.162", 6379} }
            };

            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        public static ConnectionMultiplexer Connection { get { return LazyConnection.Value; }}

        public static IDatabase RedisCache { get { return Connection.GetDatabase(); }}
    }
}