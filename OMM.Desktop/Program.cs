using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OMM.Desktop.Data.OsuDataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("####################################################################");
            Console.WriteLine("# Welcome!                                                         #");
            Console.WriteLine("# You can access the website with this url: http://localhost:16302 #");
            Console.WriteLine("####################################################################");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseUrls(new[] { "http://localhost:16302" });
                });
    }
}
