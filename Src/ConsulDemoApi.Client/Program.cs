using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsulDemoApi.Client
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private static ApiClient _apiClient;

        public static void Main(string[] args)
        {
            LoadConfig();
            Console.WriteLine("Press a key to call api/values using Consul");
            Console.ReadLine();
            var logger = new LoggerFactory().AddConsole().CreateLogger<ApiClient>();
            _apiClient = new ApiClient(_configuration, logger);

            try
            {
                _apiClient.Initialize().Wait();

                ListValues().Wait();
            }
            catch (Exception)
            {
                logger.LogError("Unable to request resource");
            }

            Console.ResetColor();
            Console.ReadLine();
        }

        private static void LoadConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = builder.Build();
        }

        private static async Task ListValues()
        {
            var values = await _apiClient.GetValuesAsync();
            Console.WriteLine($"Values : {string.Join(",", values)}");
        }

       
    }
}
