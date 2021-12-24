using System;
using System.IO;
using eFMS.API.Report;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;

namespace eFMS.API.Accounting
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var appSettings = JObject.Parse(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")));
            var environmentValue = appSettings["Environment"].ToString();

            var webHostBuilder = CreateWebHostBuilder(args);

            if (!String.IsNullOrEmpty(environmentValue))
            {
                webHostBuilder.UseEnvironment(environmentValue);
            }

            var host = webHostBuilder.Build();
            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel(o =>
                {
                    o.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
                    o.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
                });
    }
}
