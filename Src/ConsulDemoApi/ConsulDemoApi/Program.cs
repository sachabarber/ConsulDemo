using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ConsulDemoApi
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var host = WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://localhost:68008")
                .UseStartup<Startup>()
                .Build();

            var loggingFactory = host.Services.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var logger = loggingFactory.CreateLogger(nameof(Program));
            logger.LogInformation($"{Process.GetCurrentProcess().Id}");
            host.Run();
        }

       
    }
}
