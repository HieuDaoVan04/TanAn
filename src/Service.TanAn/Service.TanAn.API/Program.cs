// "Một sản phẩm của HieuDV"

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Service.TanAn.API
{
    /// <summary>
    /// Entry Point cho Service.TanAn.API
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main Entry Point
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "Service.TanAn.Api";
            Activity.DefaultIdFormat = ActivityIdFormat.W3C;

            try
            {
                // Khởi tạo logger trước khi build host
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json",
                                 optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                var (logger, levelSwitch) = SerilogConfig.ConfigureLogger(configuration);
                Log.Logger = logger;

                Log.Warning("Starting Service.TanAn.API");

                CreateHostBuilder(args).Build().Run();

                Log.Warning("Service stopped cleanly");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Service.TanAn.API terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
