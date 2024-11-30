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
using Core.Integrations;

namespace Functions
{
    public class DurableFunction
    {
        private readonly IConfiguration _configuration;
        private readonly ISerpService _serpService;

        public DurableFunction(IConfiguration configuration, ISerpService serpService)
        {
            _configuration = configuration;
            _serpService = serpService;
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
                tasks.Add(_serpService.GetHitCount(queryKey, keyword, engine));
            }

            // Run the tasks concurrently
            long[] results = await Task.WhenAll(tasks);
            return results.ToList();
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
                if(status.Result.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    return status.Result.Output.ToObject<GetHitCountResp>();
                }
                else if(status.Result.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
                {
                    throw new Exception($"Orchestration failed: {status.Exception.Message}");
                }

                // Optional: Add delay to avoid hitting the API too frequently
                await Task.Delay(1000);
            }
        }
    }
}