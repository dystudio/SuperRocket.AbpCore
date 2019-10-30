using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SuperRocket.Message;
using Volo.Abp.Application.Services;
using RestSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace SuperRocket.AspNetCore.Services
{
    public class RabbitMQService : ApplicationService, IRabbitMQService
    {
        IConfiguration Configuration { get; set; }

        public RabbitMQService(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        

        public async Task<ExcelFileMessage> PublishAsync(ExcelFileMessage message)
        {
            var result = await Task.Run(() =>
            {

                string contentType = "text/plain"; //Content-Type
                try
                {
                    var url = "http://localhost:6000/RabbitMQ/Send";
                    var client = new RestClient(url);
                    var request = new RestRequest(Method.POST);
                    request.Timeout = 10000;
                    request.AddParameter(contentType, "message=111", ParameterType.RequestBody);


                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return new ExcelFileMessage
                        {
                            FileName = "test",
                            HeaderList = new List<Dictionary<string, string>>(),
                            ProcessedSql = "",
                            ReturnProcessedSql = "",
                            SheetNameList = new List<string>()
                        };
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connecting to the server：" + ex.Message);
                }
                return new ExcelFileMessage
                {
                    FileName = "test",
                    HeaderList = new List<Dictionary<string, string>>(),
                    ProcessedSql = "",
                    ReturnProcessedSql = "",
                    SheetNameList = new List<string>()
                };
            });


            return new ExcelFileMessage
            {
                FileName = "test",
                HeaderList = new List<Dictionary<string, string>>(),
                ProcessedSql = "",
                ReturnProcessedSql = "",
                SheetNameList = new List<string>()
            };
        }

        public async Task<bool> PublishMessageAsync(ExcelFileMessage message)
        {
            var result = await Task.Run(() =>
            {
                string contentType = "application/json"; //Content-Type
                try
                {
                    var content = Newtonsoft.Json.JsonConvert.SerializeObject(new ExcelFileMessageDto
                    {
                        fileName = message.FileName,
                        processedSql = message.ProcessedSql,
                        returnProcessedSql = message.ReturnProcessedSql,
                        headerList = message.HeaderList,
                        sheetNameList = message.SheetNameList
                    });
                    var url = Configuration["RabbitMQApiServer:Url"].ToString();
                    var client = new RestClient(url);
                    var request = new RestRequest("RabbitMQ/PublishMessageToRabbitMQ", Method.POST);
                    request.Timeout = 10000;
                    request.AddParameter(contentType, content, ParameterType.RequestBody);
                    
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connecting to the server-PublishMessageToRabbitMQ：" + ex.Message);
                }
                return false;
            });

            return result;
        }
    }


    public class ExcelFileMessageDto
    {
        public string fileName { get; set; }

        public string processedSql { get; set; }

        public string returnProcessedSql { get; set; }

        public List<Dictionary<string, string>> headerList { get; set; }

        public List<string> sheetNameList { get; set; }
    }
}

