using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        public HomeController(ILogger<HomeController> logger, DataContext _context)
        {
            _logger = logger;
            this._context = _context;
        }

        public IActionResult Index()
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien = TaiKhoan.ID_TaiKhoan;
            var listViecDenToi = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BN == ID_NhanVien && x.NgayTao.Date == DateTime.Now.Date && x.ID_TrangThai_BG == 1).ToList();
            var listViecToiBatDau = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BG == ID_NhanVien && x.NgayTao.Date == DateTime.Now.Date).ToList();
            ViewBag.ViecDenToi = listViecDenToi.Count();
            ViewBag.DaXuLy = listViecDenToi.Where(x=>x.ID_TrangThai_BN == 1).Count();
            ViewBag.ChuaXuLy = listViecDenToi.Where(x => x.ID_TrangThai_BN == 0).Count();
            ViewBag.ViecToiBatDau = listViecToiBatDau.Count();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}