using Admin.Models;
using Common;
using Common.Contract.Forms;
using Common.Contract.Messaging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using Web.Models;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHitCountService _hitCountService;

        public HomeController(ILogger<HomeController> logger, IHitCountService hitCountService)
        {
            _logger = logger;
            _hitCountService = hitCountService;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Search(GetHitCountForm getHitCountForm)
        {
            var json = new JsonDictionary();
            if (string.IsNullOrEmpty(getHitCountForm.Search))
            {
                json.Add("error", "Keywords can not be empty");
                return Json(json.GetResponse());
            }

            var resp = _hitCountService.GetHitCount(new GetHitCountReq()
            {
                Search = getHitCountForm.Search,
            });

            if (!resp.Success)
            { 
                json.Add("error", resp.Error.Text);
                return Json(json.GetResponse());
            }

            TempData["resp"] = JsonConvert.SerializeObject(resp);

            // This will reload the page.
            json.Add("url", "current");
            return Json(json.GetResponse());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
