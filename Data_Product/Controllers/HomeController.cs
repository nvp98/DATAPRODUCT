using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Datatypes;
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
            ViewBag.ID_Quyen = TaiKhoan.ID_Quyen;
            var listViecDenToi = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BN == ID_NhanVien && x.NgayTao.Date == DateTime.Now.Date && x.ID_TrangThai_BG == 1).ToList();
            var listViecToiBatDau = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BG == ID_NhanVien && x.NgayTao.Date == DateTime.Now.Date).ToList();
            ViewBag.ViecDenToi = listViecDenToi.Count();
            ViewBag.DaXuLy = listViecDenToi.Where(x=>x.ID_TrangThai_BN == 1).Count();
            ViewBag.ChuaXuLy = listViecDenToi.Where(x => x.ID_TrangThai_BN == 0).Count();
            ViewBag.ViecToiBatDau = listViecToiBatDau.Count();
            // Tổng dữ liệu
            var listPhieuNhatKySX = _context.Tbl_NhatKy_SanXuat.AsNoTracking().ToList();
            var resultTG = listPhieuNhatKySX.Where(x=>x.TinhTrang ==1)
            .Select(pb => new
            {
                ID_NhatKy = pb.ID,
                TongGioDungMay = _context.Tbl_NhatKy_SanXuat_ChiTiet
                                  .Where(p => p.ID_NhatKy == pb.ID)
                                  .Sum(p => (int?)p.ThoiGianDung) ?? 0
            })
            .ToList();
            float TGDung = resultTG?.Sum(x => x.TongGioDungMay) ?? 0;
            int tongPhieu = listPhieuNhatKySX.Count();
            int PhieuDaXuLy = listPhieuNhatKySX.Where(x => x.TinhTrang == 1).Count();
            int PhieuChuaXuLy = listPhieuNhatKySX.Where(x => x.TinhTrang != 1).Count();
            var listPhieuBBGN = _context.Tbl_BienBanGiaoNhan.AsNoTracking().ToList();
            int tongPhieuBBGN = listPhieuBBGN.Count();
            int PhieuDaXuLyBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN == 1).Count();
            int PhieuChuaXuLyBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN != 1).Count();
            var DataNhatKy = new Dictionary<string, int>
            {
                { "Tong_NK", tongPhieu },
                { "DaXuLy_NK", PhieuDaXuLy },
                { "ChuaXuLy_NK", PhieuChuaXuLy },
                { "Tong_BBGN", tongPhieuBBGN },
                { "DaXuLy_BBGN", PhieuDaXuLyBBGN },
                { "ChuaXuLy_BBGN", PhieuChuaXuLyBBGN },
            };
            ViewBag.TongPhieuNhatKy = DataNhatKy;

            return View();
        }

        [HttpGet]
        public IActionResult GetDataTong(DateTime? thang)
        {
            if (thang == null) thang = DateTime.Now;
            // Tổng dữ liệu
            var listPhieuNhatKySX = _context.Tbl_NhatKy_SanXuat.Where(x=>x.NgayDungSX.Month == thang.Value.Month && x.NgayDungSX.Year == thang.Value.Year).AsNoTracking().ToList();
            var resultTG = listPhieuNhatKySX.Where(x => x.TinhTrang == 1)
            .Select(pb => new
            {
                ID_NhatKy = pb.ID,
                TongGioDungMay = _context.Tbl_NhatKy_SanXuat_ChiTiet
                                  .Where(p => p.ID_NhatKy == pb.ID)
                                  .Sum(p => (int?)p.ThoiGianDung) ?? 0
            })
            .ToList();
            float TGDung = resultTG?.Sum(x => x.TongGioDungMay) ?? 0;
            int tongPhieu = listPhieuNhatKySX.Count();
            int PhieuDaXuLy = listPhieuNhatKySX.Where(x => x.TinhTrang == 1).Count();
            int PhieuChuaXuLy = listPhieuNhatKySX.Where(x => x.TinhTrang != 1).Count();
            var listPhieuBBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.NgayTao.Month == thang.Value.Month && x.NgayTao.Year == thang.Value.Year).AsNoTracking().ToList();
            var resultBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN == 1)
           .Select(pb => new
           {
               ID_BBGN = pb.ID_BBGN,
               KhoiLuong = _context.Tbl_ChiTiet_BienBanGiaoNhan
                                 .Where(p => p.ID_BBGN == pb.ID_BBGN)
                                 .Sum(p => (int?)p.KhoiLuong_BN) ?? 0
           })
           .ToList();
            float KLBenNhan = resultBBGN?.Sum(x => x.KhoiLuong) ?? 0;
            int tongPhieuBBGN = listPhieuBBGN.Count();
            int PhieuDaXuLyBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN == 1).Count();
            int PhieuChuaXuLyBBGN = listPhieuBBGN.Where(x => x.ID_TrangThai_BBGN != 1).Count();
            var DataNhatKy = new Dictionary<string,float>
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
                              (p, b) => new {
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
                              (p, b) => new {
                                  TenPhongBan = b.TenPhongBan,
                                  SoLuongPhieu = p.SoLuongPhieu
                              })
                        .ToList();
            return Json(data);
        }
        [HttpGet]
        public IActionResult GetBienBan(DateTime? thang)
        {
            if(thang == null) thang = DateTime.Now; 
            var phongban = _context.Tbl_PhongBan.AsNoTracking().Where(x => x.ID_TrangThai == 1).OrderByDescending(g => g.ID_PhongBan);
            var dataBBGN = _context.Tbl_BienBanGiaoNhan.Where(x=>x.NgayTao.Month == thang.Value.Month && x.NgayTao.Year == thang.Value.Year).AsNoTracking()
                            .GroupBy(x => x.ID_PhongBan_BG)
                            .Select(g => new
                            {
                                PhongBanId = g.Key,
                                SoLuongPhieu = g.Count()
                            })
                            .Join(phongban,
                              p => p.PhongBanId,
                              b => b.ID_PhongBan,
                              (p, b) => new {
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
                             (p, b) => new {
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