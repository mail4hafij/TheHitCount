using Common.Constant;
using Common.Contract.Messaging;
using Core.Integrations;
using Core.LIB;
using Newtonsoft.Json;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Core.Handler
{
    public class GetHitCountHandler : RequestHandler<GetHitCountReq, GetHitCountResp>
    {
        private readonly ISerpService _serpService;
        private readonly IResponseFactory _responseFactory;
        public GetHitCountHandler(ISerpService serpService, IResponseFactory responseFactory) : base(responseFactory)
        {
            _serpService = serpService;
            _responseFactory = responseFactory;
        }

        public override GetHitCountResp Process(GetHitCountReq req)
        {
            return new GetHitCountResp()
            {
                Hits = ProcessAll(req).Result
            };
        }

        private async Task<Dictionary<string, List<long>>> ProcessAll(GetHitCountReq req)
        {
            Dictionary<string, List<long>> hits = new Dictionary<string, List<long>>();
            
            // Let's make a list of tasks that we want to run concurrently.
            var tasks = new List<Task<List<long>>>();
            // google
            tasks.Add(GetHitCount("q", req.Search, Values.AllEngine.Google.ToString()));
            // bing
            tasks.Add(GetHitCount("q", req.Search, Values.AllEngine.Bing.ToString()));
            // youtube
            tasks.Add(GetHitCount("search_query", req.Search, Values.AllEngine.Youtube.ToString()));

            // Run the tasks concurrently
            List<long>[] results = await Task.WhenAll(tasks);

            // google
            hits.Add(Values.AllEngine.Google.ToString(), results[0].ToList());
            // bing
            hits.Add(Values.AllEngine.Bing.ToString(), results[1].ToList());
            // youtube
            hits.Add(Values.AllEngine.Youtube.ToString(), results[2].ToList());

            return hits;
        }

        private async Task<List<long>> GetHitCount(string queryKey, string search, string engine)
        {
            // Here too, let's make a list of tasks that we want to run concurrently.
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
    }
}
