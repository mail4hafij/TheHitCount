using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Common.Contract.Messaging;
using Common;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Common.Constant;
using System;

namespace Functions
{
    public class DurableFunction
    {
        private readonly IConfiguration _configuration;

        public DurableFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("DurableFunction")]
        public async Task<GetHitCountResp> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            Dictionary<string, List<long>> hits = new Dictionary<string, List<long>>();

            // Read the data
            GetHitCountReq req = context.GetInput<GetHitCountReq>();
            
            // Let's make a list of tasks that we want to run concurrently.
            var tasks = new List<Task<List<long>>>();

            tasks.Add(context.CallActivityAsync<List<long>>(nameof(GetGoogleHitCount), req.Search));
            tasks.Add(context.CallActivityAsync<List<long>>(nameof(GetBingHitCount), req.Search));
            tasks.Add(context.CallActivityAsync<List<long>>(nameof(GetYoutubeHitCount), req.Search));

            // Run the tasks concurrently
            List<long>[] results = await Task.WhenAll(tasks);

            // google
            hits.Add(Values.AllEngine.Google.ToString(), results[0].ToList());
            // bing
            hits.Add(Values.AllEngine.Bing.ToString(), results[1].ToList());
            // youtube
            hits.Add(Values.AllEngine.Youtube.ToString(), results[2].ToList());

            return new GetHitCountResp()
            {
                Hits = hits
            };
        }

        [FunctionName(nameof(GetGoogleHitCount))]
        public async Task<List<long>> GetGoogleHitCount([ActivityTrigger] string search, ILogger log)
        {
            log.LogInformation("Searching in google...");
            return await GetHitCount("q", search, Values.AllEngine.Google.ToString());
        }

        [FunctionName(nameof(GetBingHitCount))]
        public async Task<List<long>> GetBingHitCount([ActivityTrigger] string search, ILogger log)
        {
            log.LogInformation("Searching in bing...");
            return await GetHitCount("q", search, Values.AllEngine.Bing.ToString());
        }

        [FunctionName(nameof(GetYoutubeHitCount))]
        public async Task<List<long>> GetYoutubeHitCount([ActivityTrigger] string search, ILogger log)
        {
            log.LogInformation("Searching in youtube...");
            return await GetHitCount("search_query", search, Values.AllEngine.Youtube.ToString());
        }

        private async Task<List<long>> GetHitCount(string queryKey, string search, string engine)
        {
            // Let's make a list of tasks that we want to run concurrently.
            var tasks = new List<Task<long>>();

            var keywords = search.Split(' ');
            foreach (var keyword in keywords)
            {
                tasks.Add(GetSerpHitCount(queryKey, keyword, engine));
            }

            // Run the tasks concurrently
            long[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<long> GetSerpHitCount(string queryKey, string keyword, string engine)
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



        [FunctionName("DurableFunction_HttpStart")]
        public async Task<GetHitCountResp> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // STARTS HERE
            // Receiving data from the http request.
            var data = await req.Content.ReadAsAsync<GetHitCountReq>();

            // Let's Orchestration.
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableFunction", data);
            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            while (true)
            {
                var status = starter.GetStatusAsync(instanceId);
                if(status.IsCompleted)
                {
                    return status.Result.Output.ToObject<GetHitCountResp>();
                }
            }
        }
    }
}