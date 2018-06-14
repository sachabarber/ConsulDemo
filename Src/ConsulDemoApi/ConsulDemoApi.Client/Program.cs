using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;

namespace ConsulDemoApi.Client
{
    public class Program
    {
        private static IConfigurationRoot _configuration;
        private static ApiClient _apiClient;
        private static int _howMany = 10;

        private static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            LoadConfig();
            Console.WriteLine("Press a key to call api/values using Consul");
            Console.ReadLine();
            var logger = new LoggerFactory().AddConsole().CreateLogger<ApiClient>();
            _apiClient = new ApiClient(_configuration, logger);

            try
            {
                await Task.CompletedTask;

                await _apiClient.Initialize();
                await DeleteValues();
                await PutValues();
                await ListValues();
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

        private static async Task DeleteValues()
        {
            for (int i = 1; i < _howMany; i++)
            {
                await _apiClient.DeleteValueAsync(i);
            }
        }

        private static async Task PutValues()
        {
            for (int i = 1; i < _howMany; i++)
            {
                await _apiClient.PutValueAsync(i, Guid.NewGuid().ToString());
            }
        }

    }
}
