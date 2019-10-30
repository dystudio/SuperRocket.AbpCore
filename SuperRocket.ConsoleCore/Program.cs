using System;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Threading;
using Serilog.Events;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using System.IO;
using SuperRocket.Message;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace SuperRocket.ConsoleCore
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeSerilog();

            Log.Information("Starting Console Application...");

            var builder = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            //Console.WriteLine(configuration.GetConnectionString("Storage"));

            using (var application = AbpApplicationFactory.Create<AppModule>(options =>
            {
                options.UseAutofac(); //Autofac integration
                options.Services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog(dispose: true);
                });
            }))
            {
                application.Initialize();

                //Resolve a service and use it
                var helloWorldService =
                    application.ServiceProvider.GetService<HelloWorldService>();
                helloWorldService.SayHello();


                var rabbitmqConnectionString = configuration["RabbitMQServer:ConnectionString"].ToString();
                using (var bus = RabbitHutch.CreateBus(rabbitmqConnectionString))
                {
                    #region test message
                    bus.SubscribeAsync<TextMessage>("test", message => Task.Factory.StartNew(() =>
                                {
                        // Perform some actions here
                        // If there is a exception it will result in a task complete but task faulted which
                        // is dealt with below in the continuation
                    }).ContinueWith(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            // Everything worked out ok
                            //using (var reportGenerator = bootstrapper.IocManager.ResolveAsDisposable<ReportGenerator>())
                            //{
                            //    reportGenerator.Object.Run(message.Text);
                            //} 
                            //Disposes tester and all it's dependencies
                            WriteTextMessageToConsole(message);
                        }
                        else
                        {
                            // Don't catch this, it is caught further up the hierarchy and results in being sent to the default error queue
                            // on the broker
                            throw new EasyNetQException("Message processing exception - look in the default error queue (broker)");
                        }
                    })); 
                    #endregion

                    bus.SubscribeAsync<ExcelFileMessage>("ExcelReport", message => Task.Factory.StartNew(() =>
                    {
                        // Perform some actions here
                        // If there is a exception it will result in a task complete but task faulted which
                        // is dealt with below in the continuation
                    }).ContinueWith(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            WriteMessageToConsole("Thread begin sleeping");
                            Thread.Sleep(10000);
                            WriteMessageToConsole("Thread sleeping for 10 seconds");
                            WriteMessageToConsole(message.FileName);
                            // Everything worked out ok
                            var generator = application.ServiceProvider.GetService<ExcelReportGenerator>();
                            generator.Run(
                                    message.FileName,
                                    message.ProcessedSql,
                                    message.ReturnProcessedSql,
                                    message.HeaderList,
                                    message.SheetNameList);
                        }
                        else
                        {
                            // Don't catch this, it is caught further up the hierarchy and results in being sent to the default error queue
                            // on the broker
                            throw new EasyNetQException("Message processing exception - look in the default error queue (broker)");
                        }
                    }));

                    System.Console.WriteLine("Listening for messages. Hit <return> to quit.");
                    System.Console.ReadLine();
                }

                System.Console.WriteLine("Press ENTER to stop application...");
                System.Console.ReadLine();
            }
        }


        private static void InitializeSerilog()
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();
        }

        static void WriteTextMessageToConsole(TextMessage textMessage)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Got message {textMessage.Text}");
        }

        static void WriteMessageToConsole(string message)
        {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.WriteLine($"Got message {message}");
        }
    }
}
