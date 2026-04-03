using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

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

        public async Task<IActionResult> Index()
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            // var TaiKhoan = await _context.Tbl_TaiKhoan.AsNoTracking()
            //     .FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);

            var TaiKhoan = await (from a in _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan)
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

            if (TaiKhoan == null)
                return RedirectToAction("Index", "DangNhap");

            int ID_NhanVien = TaiKhoan.ID_TaiKhoan;
            ViewBag.ID_Quyen = TaiKhoan.ID_Quyen;

            var today = DateTime.Today;

            var listViecDenToi = await _context.Tbl_BienBanGiaoNhan
                .AsNoTracking()
                .Where(x => x.ID_NhanVien_BN == ID_NhanVien && x.NgayTao.Date == today && x.ID_TrangThai_BG == 1)
                .ToListAsync();

            var listViecToiBatDau = await _context.Tbl_BienBanGiaoNhan
                .AsNoTracking()
                .Where(x => x.ID_NhanVien_BG == ID_NhanVien && x.NgayTao.Date == today)
                .ToListAsync();

            ViewBag.ViecDenToi = listViecDenToi.Count;
            ViewBag.DaXuLy = listViecDenToi.Count(x => x.ID_TrangThai_BN == 1);
            ViewBag.ChuaXuLy = listViecDenToi.Count(x => x.ID_TrangThai_BN == 0);
            ViewBag.ViecToiBatDau = listViecToiBatDau.Count;

            var tongPhieu = await _context.Tbl_NhatKy_SanXuat.CountAsync();
            var PhieuDaXuLy = await _context.Tbl_NhatKy_SanXuat.CountAsync(x => x.TinhTrang == 1);
            var PhieuChuaXuLy = tongPhieu - PhieuDaXuLy;

            var tongPhieuBBGN = await _context.Tbl_BienBanGiaoNhan.CountAsync();
            var PhieuDaXuLyBBGN = await _context.Tbl_BienBanGiaoNhan.CountAsync(x => x.ID_TrangThai_BBGN == 1);
            var PhieuChuaXuLyBBGN = tongPhieuBBGN - PhieuDaXuLyBBGN;

            //var TGDung = await _context.Tbl_NhatKy_SanXuat_ChiTiet
            //                       .Where(ct => ct.Tbl_NhatKy_SanXuat.TinhTrang == 1)
            //                       .SumAsync(p => (int?)p.ThoiGianDung) ?? 0;
            var user = new
            {
                id = TaiKhoan.ID_TaiKhoan,
                name = TaiKhoan.HoVaTen,
                role = "admin",
                username = TaiKhoan.TenTaiKhoan
            };


            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var token = "token";

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

            ViewBag.Token = token;
            ViewBag.User = JsonSerializer.Serialize(user);
            ViewBag.Userinfo = JsonSerializer.Serialize(userinfo, options);
            ViewBag.UserName = JsonSerializer.Serialize(user.name);

            ViewBag.TongPhieuNhatKy = new Dictionary<string, int>
            {
                ["Tong_NK"] = tongPhieu,
                ["DaXuLy_NK"] = PhieuDaXuLy,
                ["ChuaXuLy_NK"] = PhieuChuaXuLy,
                ["Tong_BBGN"] = tongPhieuBBGN,
                ["DaXuLy_BBGN"] = PhieuDaXuLyBBGN,
                ["ChuaXuLy_BBGN"] = PhieuChuaXuLyBBGN,
            };

            return View();
        }

        [HttpGet]
        public IActionResult GetDataTong(DateTime? thang)
        {
            if (thang == null) thang = DateTime.Now;
            var thangBatDau = new DateTime(thang.Value.Year, thang.Value.Month, 1);
            var thangKetThuc = thangBatDau.AddMonths(1);
            // Tổng dữ liệu
            var listPhieuNhatKySX = _context.Tbl_NhatKy_SanXuat
                                            .Where(x => x.NgayDungSX >= thangBatDau && x.NgayDungSX < thangKetThuc)
                                            .AsNoTracking()
                                            .ToList();
            var Tbl_NhatKy_SanXuat_ChiTiet = _context.Tbl_NhatKy_SanXuat_ChiTiet.AsNoTracking().ToList();
            var resultTG = listPhieuNhatKySX.Where(x => x.TinhTrang == 1)
                                            .Select(pb => new
                                            {
                                                ID_NhatKy = pb.ID,
                                                TongGioDungMay = Tbl_NhatKy_SanXuat_ChiTiet
                                                                  .Where(p => p.ID_NhatKy == pb.ID)
                                                                  .Sum(p => (int?)p.ThoiGianDung) ?? 0
                                            })
                                            .ToList();
            float TGDung = resultTG?.Sum(x => x.TongGioDungMay) ?? 0;
            int tongPhieu = listPhieuNhatKySX.Count();
            int PhieuDaXuLy = listPhieuNhatKySX.Where(x => x.TinhTrang == 1).Count();
            int PhieuChuaXuLy = listPhieuNhatKySX.Where(x => x.TinhTrang != 1).Count();
            var listPhieuBBGN = _context.Tbl_BienBanGiaoNhan
                                        .Where(x => x.NgayTao >= thangBatDau && x.NgayTao < thangKetThuc)
                                        .AsNoTracking()
                                        .ToList();
            var Tbl_ChiTiet_BienBanGiaoNhan = _context.Tbl_ChiTiet_BienBanGiaoNhan.AsNoTracking().ToList();
            var resultBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN == 1)
                                           .Select(pb => new
                                           {
                                               ID_BBGN = pb.ID_BBGN,
                                               KhoiLuong = Tbl_ChiTiet_BienBanGiaoNhan
                                                                 .Where(p => p.ID_BBGN == pb.ID_BBGN)
                                                                 .Sum(p => (int?)p.KhoiLuong_BN) ?? 0
                                           })
                                           .ToList();
            float KLBenNhan = resultBBGN?.Sum(x => x.KhoiLuong) ?? 0;
            int tongPhieuBBGN = listPhieuBBGN.Count();
            int PhieuDaXuLyBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN == 1).Count();
            int PhieuChuaXuLyBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN != 1).Count();
            var DataNhatKy = new Dictionary<string, float>
            {
                { "Tong_NK", tongPhieu },
                { "TongSanLuong_NK", TGDung },
                { "DaXuLy_NK", PhieuDaXuLy },
                { "ChuaXuLy_NK", PhieuChuaXuLy },
                { "Tong_BBGN", tongPhieuBBGN },
                { "TongSanLuong_BBGN", KLBenNhan },
                { "DaXuLy_BBGN", PhieuDaXuLyBBGN },
                { "ChuaXuLy_BBGN", PhieuChuaXuLyBBGN },
            };
            return Json(DataNhatKy);
        }

        [HttpGet]
        public IActionResult GetPhieuTuan()
        {
            var phieuNK = _context.Tbl_NhatKy_SanXuat.AsNoTracking().ToList();
            var phieuBBGN = _context.Tbl_BienBanGiaoNhan.AsNoTracking().ToList();
            DateTime now = DateTime.Now;
            var label = new[] { now.AddDays(-6).ToString("dd/MM"), now.AddDays(-5).ToString("dd/MM"), now.AddDays(-4).ToString("dd/MM"),
                now.AddDays(-3).ToString("dd/MM"), now.AddDays(-2).ToString("dd/MM"), now.AddDays(-1).ToString("dd/MM"),now.ToString("dd/MM") };

            var data = new
            {
                labels = label,
                datasets = new[]
                {
                new {
                    label = "Biên bản giao nhận",
                    data = new[] {
                        phieuBBGN.Where(x => x.NgayTao >= now.AddDays(-6) && x.NgayTao < now.AddDays(-5)).Count(),
                        phieuBBGN.Where(x => x.NgayTao >= now.AddDays(-5) && x.NgayTao < now.AddDays(-4)).Count(),
                        phieuBBGN.Where(x => x.NgayTao >= now.AddDays(-4) && x.NgayTao < now.AddDays(-3)).Count(),
                        phieuBBGN.Where(x => x.NgayTao >= now.AddDays(-3) && x.NgayTao < now.AddDays(-2)).Count(),
                        phieuBBGN.Where(x => x.NgayTao >= now.AddDays(-2) && x.NgayTao < now.AddDays(-1)).Count(),
                        phieuBBGN.Where(x => x.NgayTao >= now.AddDays(-1) && x.NgayTao < now).Count(),
                        phieuBBGN.Where(x => x.NgayTao >= now && x.NgayTao < now.AddDays(1)).Count()},
                    borderColor = "#198754",
                    tension = 0.3
                },
                new {
                    label = "Nhật ký dừng sản xuất",
                    data = new[] {
                        phieuNK.Where(x => x.NgayTao >= now.AddDays(-6) && x.NgayTao < now.AddDays(-5)).Count(),
                        phieuNK.Where(x => x.NgayTao >= now.AddDays(-5) && x.NgayTao < now.AddDays(-4)).Count(),
                        phieuNK.Where(x => x.NgayTao >= now.AddDays(-4) && x.NgayTao < now.AddDays(-3)).Count(),
                        phieuNK.Where(x => x.NgayTao >= now.AddDays(-3) && x.NgayTao < now.AddDays(-2)).Count(),
                        phieuNK.Where(x => x.NgayTao >= now.AddDays(-2) && x.NgayTao < now.AddDays(-1)).Count(),
                        phieuNK.Where(x => x.NgayTao >= now.AddDays(-1) && x.NgayTao < now).Count(),
                        phieuNK.Where(x => x.NgayTao >= now && x.NgayTao < now.AddDays(1)).Count()},
                    borderColor = "#06428b",
                    tension = 0.3
                }
            }
            };
            return Json(data);
        }

        [HttpGet]
        public IActionResult GetTopPhongBanNK()
        {
            var data = _context.Tbl_NhatKy_SanXuat
                            .GroupBy(x => x.ID_PhongBan_SX)
                            .Select(g => new
                            {
                                PhongBanId = g.Key,
                                SoLuongPhieu = g.Count()
                            })
                            .OrderByDescending(g => g.SoLuongPhieu)
                            .Take(5)
                            .Join(_context.Tbl_PhongBan,
                              p => p.PhongBanId,
                              b => b.ID_PhongBan,
                              (p, b) => new
                              {
                                  TenPhongBan = b.TenPhongBan,
                                  SoLuongPhieu = p.SoLuongPhieu
                              })
                        .ToList();
            return Json(data);
        }
        [HttpGet]
        public IActionResult GetTopPhongBanBBGN()
        {
            var data = _context.Tbl_BienBanGiaoNhan
                            .GroupBy(x => x.ID_PhongBan_BG)
                            .Select(g => new
                            {
                                PhongBanId = g.Key,
                                SoLuongPhieu = g.Count()
                            })
                            .OrderByDescending(g => g.SoLuongPhieu)
                            .Take(5)
                            .Join(_context.Tbl_PhongBan,
                              p => p.PhongBanId,
                              b => b.ID_PhongBan,
                              (p, b) => new
                              {
                                  TenPhongBan = b.TenPhongBan,
                                  SoLuongPhieu = p.SoLuongPhieu
                              })
                        .ToList();
            return Json(data);
        }
        [HttpGet]
        public IActionResult GetBienBan(DateTime? thang)
        {
            if (thang == null) thang = DateTime.Now;
            var phongban = _context.Tbl_PhongBan.AsNoTracking().Where(x => x.ID_TrangThai == 1).OrderByDescending(g => g.ID_PhongBan);
            var dataBBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.NgayTao.Month == thang.Value.Month && x.NgayTao.Year == thang.Value.Year).AsNoTracking()
                            .GroupBy(x => x.ID_PhongBan_BG)
                            .Select(g => new
                            {
                                PhongBanId = g.Key,
                                SoLuongPhieu = g.Count()
                            })
                            .Join(phongban,
                              p => p.PhongBanId,
                              b => b.ID_PhongBan,
                              (p, b) => new
                              {
                                  TenPhongBan = b.TenPhongBan,
                                  SoLuongPhieu = p.SoLuongPhieu,
                                  ID_PhongBan = p.PhongBanId
                              }).OrderByDescending(g => g.ID_PhongBan);
            var dataNK = _context.Tbl_NhatKy_SanXuat.Where(x => x.NgayDungSX.Month == thang.Value.Month && x.NgayDungSX.Year == thang.Value.Year).AsNoTracking()
                           .GroupBy(x => x.ID_PhongBan_SX)
                           .Select(g => new
                           {
                               PhongBanId = g.Key,
                               SoLuongPhieu = g.Count()
                           })
                           .OrderByDescending(g => g.SoLuongPhieu)
                           .Join(phongban,
                             p => p.PhongBanId,
                             b => b.ID_PhongBan,
                             (p, b) => new
                             {
                                 TenPhongBan = b.TenPhongBan,
                                 SoLuongPhieu = p.SoLuongPhieu,
                                 ID_PhongBan = p.PhongBanId
                             }).OrderByDescending(g => g.ID_PhongBan);
            var phongBanList = phongban.ToList();

            var dataBBGNMap = dataBBGN.ToDictionary(x => x.ID_PhongBan, x => x.SoLuongPhieu);
            var dataNKMap = dataNK.ToDictionary(x => x.ID_PhongBan, x => x.SoLuongPhieu);

            var labels = phongBanList.Select(x => x.TenPhongBan).ToArray();
            var valuesBBGN = phongBanList.Select(x => dataBBGNMap.ContainsKey(x.ID_PhongBan) ? dataBBGNMap[x.ID_PhongBan] : 0).ToArray();
            var valuesNK = phongBanList.Select(x => dataNKMap.ContainsKey(x.ID_PhongBan) ? dataNKMap[x.ID_PhongBan] : 0).ToArray();

            var data = new
            {
                labels = labels,
                datasets = new[]
                {
                    new {
                        label = "Biên bản giao nhận",
                        data = valuesBBGN,
                        backgroundColor = "#198754",
                        borderColor = "#198754",
                        tension = 0.3
                    },
                    new {
                        label = "Nhật ký dừng sản xuất",
                        data = valuesNK,
                        backgroundColor = "#06428b",
                        borderColor = "#06428b",
                        tension = 0.3
                    }
                }
            };

            return Json(data);
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