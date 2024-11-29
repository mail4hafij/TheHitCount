using Microsoft.AspNetCore.Mvc;

namespace Admin.Views.Shared.Components
{
    public class LoadjsViewComponent : ViewComponent
    {
        public LoadjsViewComponent()
        {

        }

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
