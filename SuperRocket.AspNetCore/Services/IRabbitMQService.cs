using SuperRocket.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SuperRocket.AspNetCore.Services
{
    public interface IRabbitMQService : IApplicationService
    {
        Task<ExcelFileMessage> PublishAsync(ExcelFileMessage message);
        Task<bool> PublishMessageAsync(ExcelFileMessage message);
    }
}
