using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Core.Integrations
{
    public class SerpService : ISerpService
    {
        private readonly IConfiguration _configuration;
        
        public SerpService(IConfiguration configuration) 
        { 
            _configuration = configuration;
        }

        public async Task<long> GetHitCount(string queryKey, string keyword, string engine)
        {
            var httpClient = new HttpClient();
            var baseUrl = _configuration["SerpBaseUrl"];
            var apiKey = _configuration["SerpApiKey"];
            
            var requestUrl = $"{baseUrl}?{queryKey}={Uri.EscapeDataString(keyword)}&api_key={apiKey}&engine={engine.ToLower()}";
            var response = await httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"SerpAPI error: {response.ReasonPhrase}");

            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            try
            {
                return json["search_information"]["total_results"].Value<long>();
            }
            catch 
            {
                return 0;
            }
        }
    }
}
