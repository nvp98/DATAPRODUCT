using Microsoft.AspNetCore.Mvc;

namespace Data_Product.Controllers
{
    public class MenuController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Index_bs()
        {
            return View();
        }
    }
}
