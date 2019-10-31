using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SuperRocket.Message;
using Microsoft.Extensions.Configuration;

namespace RabbitMQ
{
    public class ExcelFileMessageService : BackgroundService
    {
        private readonly ILogger _logger;
        private IModel _channel;
        private IConnection _connection;
        private ConnectionFactory _rabbitConnection;
        private IHostingEnvironment _env;
        public IConfiguration Configuration { get; set; }

        public ExcelFileMessageService(ILoggerFactory loggerFactory, 
            [FromServices] ConnectionFactory rabbitConnection,
            IConfiguration configuration,
            IHostingEnvironment env)
        {
            Configuration = configuration;
            this._logger = loggerFactory.CreateLogger<ExcelFileMessageService>();
            _env = env;
            if (_env.IsDevelopment())
            {
                var hostName = Configuration["RabbitMQ:HostName"].ToString();
                var userName = Configuration["RabbitMQ:UserName"].ToString();
                var password = Configuration["RabbitMQ:Password"].ToString();
                _rabbitConnection = new ConnectionFactory { HostName = hostName, UserName = userName, Password = password };
            }
            else
            {
                _rabbitConnection = rabbitConnection;
            }
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            try
            {
                this._connection = _rabbitConnection.CreateConnection();
                this._channel = _connection.CreateModel();

                this._channel.ExchangeDeclare("superrocket.exchange", ExchangeType.Topic);
                this._channel.QueueDeclare("superrocket.queue.excelfilemessage", false, false, false, null);
                this._channel.QueueBind("superrocket.queue.excelfilemessage", "superrocket.exchange", "superrocket.queue.*", null);
                this._channel.BasicQos(0, 1, false);

                this._connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
            }
            catch (Exception ex)
            {
                _logger.LogError("InitRabbitMQ failed with exception:" + ex.Message);
            }
        }



        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body);

                // handle the received message  
                HandleMessage(content);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;
            if (_channel != null)
            {
                _channel.BasicConsume("superrocket.queue.excelfilemessage", false, consumer);
            }
            
            return Task.CompletedTask;
        }

        private void HandleMessage(string content)
        {
            // we just print this message  and then handle the message
            _logger.LogInformation($"consumer received {content}");
            var excelFileMessage = JsonConvert.DeserializeObject<ExcelFileMessage>(content);
            ExcelReportGenerator.Run(excelFileMessage.FileName,
                excelFileMessage.ProcessedSql,
                excelFileMessage.ReturnProcessedSql,
                excelFileMessage.HeaderList,
                excelFileMessage.SheetNameList);
            
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            this._channel.Close();
            this._connection.Close();
            base.Dispose();
        }
    }
}
