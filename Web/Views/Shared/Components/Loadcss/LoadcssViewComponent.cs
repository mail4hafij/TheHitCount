using Microsoft.AspNetCore.Mvc;

namespace Admin.Views.Shared.Components
{
    public class LoadcssViewComponent : ViewComponent
    {
        public LoadcssViewComponent()
        {
            
        }

        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
