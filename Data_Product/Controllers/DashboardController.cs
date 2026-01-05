using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace Data_Product.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataContext _context;
        public DashboardController(ILogger<HomeController> logger, DataContext _context)
        {
            _logger = logger;
            this._context = _context;
        }
        public async Task<IActionResult> Index()
        {

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = await _context.Tbl_TaiKhoan.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);

            if (TaiKhoan == null)
                return RedirectToAction("Index", "DangNhap");
            var user = new
            {
                id = TaiKhoan.ID_TaiKhoan,
                name = TaiKhoan.HoVaTen,
                role = "admin",
                username = TaiKhoan.TenTaiKhoan
            };
            var userinfo = TaiKhoan;

            var token = "token";

            ViewBag.Token = token;
            ViewBag.User = JsonSerializer.Serialize(user);
            ViewBag.Userinfo = JsonSerializer.Serialize(userinfo);
            ViewBag.UserName = JsonSerializer.Serialize(user.name);
            return View();
        }
    }
}
