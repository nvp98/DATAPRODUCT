using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Data_Product.Models;
using Data_Product.Repositorys;
using Data_Product.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Data_Product.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly DataContext _context;

        public DangNhapController(DataContext _context)
        {
            this._context = _context;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> DangNhapTaiKhoan(Tbl_TaiKhoan u)
        {
            if (u.TenTaiKhoan != "" && u.MatKhau != "" && u.TenTaiKhoan != null && u.MatKhau != null)
            {
                string mk = Common.Encryptor.MD5Hash(u.MatKhau);
                Tbl_TaiKhoan? user = _context.Tbl_TaiKhoan?.Where(x => x.TenTaiKhoan == u.TenTaiKhoan && x.MatKhau == mk && x.ID_TrangThai ==1)?.FirstOrDefault();
                if (user != null)
                {
                    var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Name, user?.TenTaiKhoan)
                    }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var principal = new ClaimsPrincipal(identity);

                    var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    TempData["msglg"] = "<script>alert('Tài khoản hoặc mật khẩu không đúng, liên hệ B.CNTT nếu bạn quên mật khẩu')</script>";
                    return RedirectToAction("", "DangNhap");
                }
            }
            else
            {
                TempData["msglg"] = "<script>alert('Vui lòng nhập tài khoản và mật khẩu')</script>";
                return RedirectToAction("", "DangNhap");
            }
        }
        public async Task<IActionResult> DangXuatTaiKhoan()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "DangNhap");

        }

        public async Task<IActionResult> ThongTinTaiKhoan(int? id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan", TaiKhoan.ID_PhongBan);

            List<Tbl_ViTri> cv = _context.Tbl_ViTri.ToList();
            ViewBag.ID_ViTri = new SelectList(cv, "ID_ViTri", "TenViTri", TaiKhoan.ID_ChucVu);

            List<Tbl_Xuong> x = _context.Tbl_Xuong.ToList();
            ViewBag.ID_Xuong = new SelectList(x, "ID_Xuong", "TenXuong", TaiKhoan.ID_PhanXuong);
            return View(TaiKhoan);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThongTinTaiKhoan(int id, Tbl_TaiKhoan _DO, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return RedirectToAction("ThongTinTaiKhoan", "DangNhap");
                }
                if (file.Length > 5 * 1024 * 1024) // 5MB
                {
                    return BadRequest("File quá lớn. Chỉ chấp nhận file dưới 5MB.");
                }
                // Danh sách phần mở rộng ảnh hợp lệ
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff" };

                // Kiểm tra phần mở rộng
                var extension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Định dạng file không được hỗ trợ.");
                }

                // Tạo đường dẫn lưu file
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Signature");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filename = $"{_DO.TenTaiKhoan+"_"+ DateTime.Now.ToString("yyyyMMddHHmm")}"+ extension;
                var filePath = Path.Combine(folderPath, filename);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                // Trả về đường dẫn tương đối của ảnh
                var relativePath = $"/Signature/{filename}";
                if (relativePath != null)
                {
                    var taikhoan = _context.Tbl_TaiKhoan.Find(_DO.ID_TaiKhoan);
                    if(taikhoan != null)
                    {
                        taikhoan.ChuKy = relativePath;
                        _context.SaveChanges();
                    }
                }
                //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_PhongBan_update {0},{1},{2}", id, _DO.TenPhongBan, _DO.TenNgan);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }

            return RedirectToAction("ThongTinTaiKhoan", "DangNhap");
        }

        public ActionResult DoiMatKhau()
        {
            return View();
        }
        [HttpPost]

        public ActionResult DoiMatKhau( Tbl_TaiKhoan _DO)
        {
            try
            {
                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

                string mk = Common.Encryptor.MD5Hash(_DO.MatKhauCu);
                var check_mk = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan).FirstOrDefault();
                if (mk != check_mk.MatKhau)
                {
                    TempData["msgSuccess"] = "<script>alert('Mật khẩu cũ không đúng. VUi lòng kiểm tra lại');</script>";
                    return RedirectToAction("DoiMatKhau", "DangNhap");
                }
                if (_DO.MatKhau != _DO.MatKhauNhapLai)
                {
                    TempData["msgSuccess"] = "<script>alert('Nhập lại mật khẩu mới không đúng. Vui lòng kiểm tra lại');</script>";
                    return RedirectToAction("DoiMatKhau", "DangNhap");
                }
                string mk_new = Common.Encryptor.MD5Hash(_DO.MatKhau);

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_pass {0},{1}", TaiKhoan.ID_TaiKhoan, mk_new);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }
            return RedirectToAction("Index", "DangNhap");
        }
    }
}
