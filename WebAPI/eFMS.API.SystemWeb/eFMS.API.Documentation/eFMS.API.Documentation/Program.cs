using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;

namespace eFMS.API.Shipment
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
                .UseKestrel(o => {
                    o.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
                    o.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
                });
        //public static void Main(string[] args)
        //{
        //    var host = new WebHostBuilder()
        //        .UseKestrel()
        //        .UseContentRoot(Directory.GetCurrentDirectory())
        //        .UseIISIntegration()
        //         .UseUrls("http://localhost:5000/")
        //        .UseStartup<Startup>()
        //        .Build();

        //    host.Run();
        //}
    }
}
