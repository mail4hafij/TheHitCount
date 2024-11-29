using Common;
using Common.Contract;
using Common.Contract.Forms;
using Common.Contract.Messaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace Rest.Controllers
{
    // Route for URI versioning
    // [Route("api/v{version:apiVersion}/Search/[action]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IHitCountService _hitCountService;
        private readonly IConfiguration _configuration;

        public SearchController(IConfiguration configuration, ILogger<SearchController> logger, IHitCountService hitCountService) 
        {
            _configuration = configuration;
            _logger = logger;
            _hitCountService = hitCountService;
        }

        [Route("api/Search")]
        [HttpPost]
        public GetHitCountResp CreateDoor([FromBody] GetHitCountForm getHitCountForm)
        {
            /*
            var resp = _hitCountService.GetHitCount(new GetHitCountReq()
            {
                Search = getHitCountForm.Search,
            });
            return resp;
            */

            // Let's trigger the function app to orchestrate our search into different engines.
            Task<GetHitCountResp> r = TriggerOrchestration(new GetHitCountReq()
            {
                Search = getHitCountForm.Search,
            });

            return r.Result;
        }

        private async Task<GetHitCountResp> TriggerOrchestration(GetHitCountReq req)
        {
            var client = new HttpClient();
            var url = _configuration["DurableFunctionUrl"]; ;
            var content = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");

            var resp = await client.PostAsync(url, content);
            var result = await resp.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GetHitCountResp>(result);
        }
    }
}
