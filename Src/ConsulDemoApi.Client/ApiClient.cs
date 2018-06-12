using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Consul;
using ConsulDemoApi.Client.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;

namespace ConsulDemoApi.Client
{
    public class ApiClient
    {
        private readonly List<Uri> _serverUrls;
        private readonly IConfigurationRoot _configuration;
        private readonly HttpClient _apiClient;
        private RetryPolicy _serverRetryPolicy;
        private int _currentConfigIndex;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(IConfigurationRoot configuration, ILogger<ApiClient> logger)
        {
            _logger = logger;
            _configuration = configuration;

            _apiClient = new HttpClient();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _serverUrls = new List<Uri>();
        }

        public async Task Initialize()
        {
            var consulClient = new ConsulClient(c =>
            {
                var uri = new Uri(_configuration["ConsulConfig:Address"]);
                c.Address = uri;
            });

            _logger.LogInformation("Discovering Services from Consul.");

            var services = await consulClient.Agent.Services();
            foreach (var service in services.Response)
            {
                var isDemoApi = service.Value.Tags.Any(t => t == "Consul") && 
                    service.Value.Tags.Any(t => t == "SachaBarber-Demo");
                if (isDemoApi)
                {
                    var serviceUri = new Uri($"{service.Value.Address}:{service.Value.Port}");
                    _serverUrls.Add(serviceUri);
                }
            }

            _logger.LogInformation($"{_serverUrls.Count} endpoints found.");
            var retries = _serverUrls.Count * 2 - 1;
            _logger.LogInformation($"Retry count set to {retries}");

            _serverRetryPolicy = Policy.Handle<HttpRequestException>()
               .RetryAsync(retries, (exception, retryCount) =>
               {
                   ChooseNextServer(retryCount);
               });
        }

        private void ChooseNextServer(int retryCount)
        {
            if (retryCount % 2 == 0)
            {
                _logger.LogWarning("Trying next server... \n");
                _currentConfigIndex++;

                if (_currentConfigIndex > _serverUrls.Count - 1)
                    _currentConfigIndex = 0;
            }
        }

        public Task<IEnumerable<string>> GetValuesAsync()
        {
            return _serverRetryPolicy.ExecuteAsync(async () =>
                {
                    var serverUrl = _serverUrls[_currentConfigIndex];
                    var requestPath = $"{serverUrl}api/values";
                    _logger.LogInformation($"Making request to GET {requestPath}");
                    var response = await _apiClient.GetAsync(requestPath).ConfigureAwait(false);
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<IEnumerable<string>>(content);
                });
        }

        public Task<bool> DeleteValueAsync(int id)
        {
            return _serverRetryPolicy.ExecuteAsync(async () =>
            {
                var serverUrl = _serverUrls[_currentConfigIndex];
                var requestPath = $"{serverUrl}api/values/{id}";
                _logger.LogInformation($"Making request to DELETE {requestPath}");
                var response = await _apiClient.DeleteAsync(requestPath).ConfigureAwait(false);
                return response.IsSuccessStatusCode;
            });
        }

        public Task<bool> PutValueAsync(int id, string theValue)
        {
            return _serverRetryPolicy.ExecuteAsync(async () =>
            {
                var valueWrapper = new ValueWrapper() { Value = theValue };
                var json = JsonConvert.SerializeObject(valueWrapper);

                var serverUrl = _serverUrls[_currentConfigIndex];
                var requestPath = $"{serverUrl}api/values/{id}";
                _logger.LogInformation($"Making request to PUT {requestPath}");
                var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _apiClient.PutAsync(requestPath, stringContent);
                return response.IsSuccessStatusCode;
            });
        }
    }
}
