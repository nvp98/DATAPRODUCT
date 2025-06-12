using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class MenuController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        //public PdfController(IConverter converter)
        //{
        //    _converter = converter;
        //}

        public MenuController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public IActionResult Index()
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            ViewBag.TaiKhoan = TaiKhoan;
            return View();
        }

        public IActionResult Index_bs()
        {
            return View();
        }
    }
}
