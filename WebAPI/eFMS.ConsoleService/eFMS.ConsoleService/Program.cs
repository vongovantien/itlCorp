using eFMS.API.Infrastructure.NoSql;
using eFMS.ConsoleService.Contexts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace eFMS.ConsoleService
{
    public static class ConfigurationBuilderHelper
    {
        public static IConfiguration BuildConfiguration()
        {
            // Create a new ConfigurationBuilder
            var builder = new ConfigurationBuilder();

            var appSettings = JObject.Parse(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")));
            var environmentValue = appSettings["Environment"].ToString();
            // Add the appsettings.json file to the ConfigurationBuilder
            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // Build the Configuration object
            var configuration = builder.Build();

            // Get the value of the "Environment" setting from the Configuration object
            var environment = configuration["Environment"];

            // Add the environment-specific appsettings file to the ConfigurationBuilder
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

            // Build the Configuration object again, with the environment-specific settings
            var config = builder.Build();

            return config;
        }

        public static void Configure(IServiceCollection services, IConfiguration config)
        {
            // Add the ConnectionStrings configuration section
            var connectionStringSection = config.GetSection("ConnectionStrings");
            var connectionStrings = new ConnectionStrings
            {
                eFMSConnection = connectionStringSection["eFMSConnection"],
                Redis = connectionStringSection["Redis"],
                MongoConnection = connectionStringSection["MongoConnection"],
                Rabbit = connectionStringSection["Rabbit"],
            };

            services.Configure<ConnectionStrings>(options => {
                options.eFMSConnection = connectionStrings.eFMSConnection;
                options.Redis = connectionStrings.Redis;
                options.MongoConnection = connectionStrings.MongoConnection;
                options.Rabbit = connectionStrings.Rabbit;
            });

            var serviceProvider = services.BuildServiceProvider();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ConfigurationBuilderHelper.BuildConfiguration();
            var services = new ServiceCollection();
            ConfigurationBuilderHelper.Configure(services, configuration);
            try
            {
                var connectionStrings = new ConnectionStrings
                {
                    eFMSConnection = configuration.GetConnectionString("eFMSConnection"),
                    Redis = configuration.GetConnectionString("Redis"),
                    MongoConnection = configuration.GetConnectionString("MongoConnection"),
                    Rabbit = configuration.GetConnectionString("Rabbit"),
                };
                using (var dbContext = new eFMSDataContext(connectionStrings))
                {
                    var isConnectionSuccessful = dbContext.Database.CanConnect();
                    Console.WriteLine(isConnectionSuccessful);

                    Console.WriteLine("Connect database successful");
                }

                var mongo = MongoDbHelper.GetDatabase(connectionStrings.MongoConnection);
                if(string.IsNullOrEmpty(mongo.DatabaseNamespace.DatabaseName))
                {
                    Console.WriteLine("Connect Mongo successful");

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to database: {ex.Message}");
            }
            Console.ReadKey();
        }
    }
}
