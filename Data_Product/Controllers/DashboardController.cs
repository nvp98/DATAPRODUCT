using Data_Product.Models;
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
            var TaiKhoan = await (from a in _context.Tbl_TaiKhoan.Where(x=>x.TenTaiKhoan == TenTaiKhoan)
                             join pb in _context.Tbl_PhongBan on a.ID_PhongBan equals pb.ID_PhongBan
                             join x in _context.Tbl_Xuong on a.ID_PhanXuong equals x.ID_Xuong
                             join vt in _context.Tbl_ViTri on a.ID_ChucVu equals vt.ID_ViTri
                             join q in _context.Tbl_Quyen on a.ID_Quyen equals q.ID_Quyen
                             select new Tbl_TaiKhoan
                             {
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 TenTaiKhoan = a.TenTaiKhoan,
                                 MatKhau = a.MatKhau,
                                 HoVaTen = a.HoVaTen,
                                 ID_PhongBan = a.ID_PhongBan,
                                 TenPhongBan = pb.TenNgan,
                                 ID_PhanXuong = a.ID_PhanXuong,
                                 TenXuong = x.TenXuong,
                                 ID_ChucVu = a.ID_ChucVu,
                                 TenChucVu = vt.TenViTri,
                                 Email = a.Email,
                                 SoDienThoai = a.SoDienThoai,
                                 NgayTao = (DateTime)a.NgayTao,
                                 ID_Quyen = (int?)a.ID_Quyen ?? default,
                                 TenQuyen = q.TenQuyen,
                                 ChuKy = a.ChuKy,
                                 ID_TrangThai = (int)a.ID_TrangThai,
                                 PhongBan_Them = a.PhongBan_Them,
                                 Quyen_Them = a.Quyen_Them,
                                 PhongBan_API = a.PhongBan_API,
                                 Xuong_API = a.Xuong_API
                             }).OrderBy(x => x.TenTaiKhoan).FirstOrDefaultAsync();
            //var TaiKhoan = await _context.Tbl_TaiKhoan.AsNoTracking()
            //    .FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);

            if (TaiKhoan == null)
                return RedirectToAction("Index", "DangNhap");
            var user = new
            {
                id = TaiKhoan.ID_TaiKhoan,
                name = TaiKhoan.HoVaTen,
                role = "admin",
                username = TaiKhoan.TenTaiKhoan
            };
            var result = new
            {
                TaiKhoan.ID_TaiKhoan,
                TaiKhoan.TenTaiKhoan,
                TaiKhoan.HoVaTen,
                TaiKhoan.ChuKy,
                TaiKhoan.PhongBan_API,
                TenNgan = TaiKhoan.TenPhongBan,
                TenPhongBan = TaiKhoan.TenPhongBan,
                TaiKhoan.ID_Quyen
            };
            var userinfo = result;

            var token = "token";

            ViewBag.Token = token;
            ViewBag.User = JsonSerializer.Serialize(user);
            ViewBag.Userinfo = JsonSerializer.Serialize(userinfo);
            ViewBag.UserName = JsonSerializer.Serialize(user.name);
            return View();
        }
    }
}
