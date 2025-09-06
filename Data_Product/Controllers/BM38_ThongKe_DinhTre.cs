using Microsoft.AspNetCore.Mvc;

namespace Data_Product.Controllers
{
    public class BM38_ThongKe_DinhTreController : Controller
    {
        public async  Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> View_Details()
        {
            return View();
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }
        public async Task<IActionResult> Edit()
        {
            return View();
        }

        public async Task<IActionResult> ExportPdfView(int id)
        {
            return View("ExportPdfView", new { id = id });
        }


    }
}
