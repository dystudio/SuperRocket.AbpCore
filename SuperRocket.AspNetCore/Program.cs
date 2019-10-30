using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;

namespace SuperRocket.AspNetCore
{
    public class Program
    {
        public static Dictionary<string, string> arrayDict =
        new Dictionary<string, string>
        {
            {"array:entries:0", "value0"},
            {"array:entries:1", "value1"},
            {"array:entries:2", "value2"},
            {"array:entries:4", "value4"},
            {"array:entries:5", "value5"}
        };

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .ConfigureHostConfiguration(config =>
                 {
                     //Host Configuration
                     var dict = new Dictionary<string, string>
                                {
                                    {"MemoryCollectionKey1", "value1"},
                                    {"MemoryCollectionKey2", "value2"}
                                };
                     config.AddInMemoryCollection(dict);
                 })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var envName = hostingContext.HostingEnvironment.EnvironmentName;
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddInMemoryCollection(arrayDict);
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{envName}.json", optional: true);
                    //config.AddJsonFile(
                    //    "starship.json", optional: false, reloadOnChange: false);
                    //config.AddXmlFile(
                    //    "tvshow.xml", optional: false, reloadOnChange: false);
                    //config.AddEFConfiguration(
                    //    options => options.UseInMemoryDatabase("InMemoryDb"));
                    config.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseAutofac();
    }
}
