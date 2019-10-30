using System;
using Volo.Abp.DependencyInjection;

namespace SuperRocket.ConsoleCore
{
    public class HelloWorldService : ITransientDependency
    {
        public void SayHello()
        {
            Console.WriteLine("Hello World!");
        }
    }
}
