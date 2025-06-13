using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Data_Product.DTO.BM_16_DTO;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Org.BouncyCastle.Asn1.Ocsp;
using iTextSharp.text;
using DocumentFormat.OpenXml.Office2016.Excel;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClosedXML.Excel;
using System.Reflection;
using Data_Product.Common.Enums;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Layout.Font;
using static Data_Product.Controllers.BM_11Controller;


namespace Data_Product.Controllers
{

    public class BM16_GangThoiController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ILogger<BM16_GangThoiController> _logger;

        public BM16_GangThoiController(DataContext _context, ICompositeViewEngine viewEngine, ILogger<BM16_GangThoiController> logger)
        {
            this._context = _context;
            _viewEngine = viewEngine;
            _logger = logger;
        }

        public async Task<IActionResult> GetSoMeGangBKMis(string ngay, int? ID_LoCao, string IDKip)
        {
            string cakip = "";
            if (IDKip != null)
            {
                var dt = DateTime.Parse(ngay);
                var ca = _context.Tbl_Kip.FirstOrDefault(x => x.ID_Kip == Int32.Parse(IDKip) && x.NgayLamViec == dt);
                cakip = ca?.TenCa + ca?.TenKip; //1A,2A,1B...
            }
            var dvt = new List<Bkmis_view>();
            // Chuỗi kết nối MySQL
            // string connectionString = "Server=10.192.215.11,3307;Database=bkmis_kcshpsdq;User Id=viewkcs;Password=viewkcs@2024;";
            string connectionString = "Server=10.192.215.11;Database=bkmis_kcshpsdq;User Id=viewkcs;Password=viewkcs@2024;Port=3307;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Kết nối thành công!");

                    // Câu lệnh SQL cần thực thi
                    string query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                        "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc1 " +
                        "where bkmis_kcshpsdq.view_dq1_lg_daura_lc1.ProductionDate = '" +
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc1.ShiftName ='" + cakip + "'";

                    if (ID_LoCao == 2)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                        "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc2 " +
                        "where bkmis_kcshpsdq.view_dq1_lg_daura_lc2.ProductionDate = '" +
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc2.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 3)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                        "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc3 " +
                        "where bkmis_kcshpsdq.view_dq1_lg_daura_lc3.ProductionDate = '" +
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc3.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 4)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                       "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc4 " +
                       "where bkmis_kcshpsdq.view_dq1_lg_daura_lc4.ProductionDate = '" +
                        ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc4.ShiftName ='" + cakip + "'";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) // Đọc từng dòng dữ liệu
                            {
                                //Console.WriteLine($"ID: {reader["Id"]}, Name: {reader["Name"]}, Age: {reader["Age"]}");
                                dvt.Add(new Bkmis_view()
                                {
                                    ClassifyName = reader["ClassifyName"].ToString(),
                                    InputTime = reader["InputTime"].ToString(),
                                    Patterntime = reader["Patterntime"].ToString(),
                                    ShiftName = reader["ShiftName"].ToString(),
                                    ProductionDate = reader["ProductionDate"].ToString(),
                                    TestPatternCode = reader["TestPatternCode"].ToString(),
                                    TestPatternName = reader["TestPatternName"].ToString(),
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Lỗi: " + ex.Message);
                }
            }

            return Json(dvt);
        }

        public async Task<IActionResult> GetKipFromCa(DateTime? Ngay, string Ca)
        {
            DateTime day_datetime = (DateTime)Ngay;
            string Day = day_datetime.ToString("dd-MM-yyyy");
            DateTime Day_Convert = DateTime.ParseExact(Day, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
            var CaKip = await (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == Day_Convert && x.TenCa == Ca)
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenKip = a.TenKip,
                               }).ToListAsync();

            return Json(CaKip);
        }
        public async Task<IActionResult> Danhsachphieu(string maPhieu, DateTime? ngay, string ca, int page = 1)
        {
            const int pageSize = 20;
            if (page < 1) page = 1;

            var query = _context.Tbl_BM_16_Phieu.OrderByDescending(p => p.Ngay)
                .Select(p => new PhieuViewModel
                {
                    MaPhieu = p.MaPhieu,
                    Ngay = p.Ngay,
                    ThoiGianTao = p.ThoiGianTao.ToString("HH:mm:ss"),
                    TenNguoiTao = _context.Tbl_TaiKhoan
                                    .Where(tk => tk.ID_TaiKhoan == p.ID_NguoiTao)
                                    .Select(tk => tk.HoVaTen).FirstOrDefault(),
                    TenCa = _context.Tbl_Kip
                                .Where(k => k.ID_Kip == p.ID_Kip)
                                .Select(k => k.TenKip)
                                .FirstOrDefault(),
                    TenLoCao = _context.Tbl_LoCao
                                .Where(lc => lc.ID == p.ID_Locao)
                                .Select(lc => lc.TenLoCao)
                                .FirstOrDefault(),
                });

            // Lọc theo Mã Phiếu (chuỗi)
            if (!string.IsNullOrEmpty(maPhieu))
            {
                string maPhieuLower = maPhieu.ToLower();
                query = query.Where(s => s.MaPhieu.ToLower().Contains(maPhieuLower));
            }

            // Lọc theo ngày
            if (ngay.HasValue)
            {
                query = query.Where(s => s.Ngay.Date == ngay.Value.Date);
            }

            // Lọc theo Ca (chuỗi)
            if (!string.IsNullOrEmpty(ca))
            {
                string caLower = ca.ToLower();
                query = query.Where(s => s.TenCa.ToLower() == caLower);
            }


            int resCount = await query.CountAsync();

            var data = await query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

            var pager = new Pager(resCount, page, pageSize);
            ViewBag.Pager = pager;

            // Truyền lại giá trị tìm kiếm cho view
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Ngay = ngay?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Ca = ca;

            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> XoaPhieu (string maPhieu)
        {
            try
            {
                if (string.IsNullOrEmpty(maPhieu))
                {
                    return BadRequest("Mã phiếu không tồn tại");
                }
                var phieu = await _context.Tbl_BM_16_Phieu.FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);
                if (phieu == null)
                {
                    return NotFound("Không tìm thấy phiếu cần xóa.");

                }
                var dsthungphieu = await _context.Tbl_BM_16_GangLong.Where(x => x.MaPhieu == maPhieu).ToListAsync();
                if (dsthungphieu.Any())
                {
                    // Kiểm tra điều kiện G_ID_TrangThai
                    bool allTrangThai1 = dsthungphieu.All(x => x.G_ID_TrangThai == 1);
                    if (!allTrangThai1)
                    {
                        return BadRequest("Có thùng trong phiếu đang ở trạng thái không được phép xóa.");
                    }
                    _context.Tbl_BM_16_GangLong.RemoveRange(dsthungphieu);
                }

                _context.Tbl_BM_16_Phieu.Remove(phieu);
                 await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpGet]
        public async Task< IActionResult> TaoPhieu()
        {
            var loCaoList = await _context.Tbl_LoCao.ToListAsync();
            ViewBag.LoCaoList = loCaoList;
            return View();
        }
        [HttpPost]
       // [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhieu([FromBody] PhieuCreateDto model)
        {

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, errors });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(tenTaiKhoan))
                return Unauthorized("Phiên đăng nhập không hợp lệ.");

            var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
            if (taiKhoan == null)
                return Unauthorized("Tài khoản không tồn tại.");

            int idNhanVienTao = taiKhoan.ID_TaiKhoan;
            if (idNhanVienTao == null)
                return Unauthorized("Phiên đăng nhập đã hết hạn hoặc không hợp lệ.");

            try
            {
                var maPhieu = "PGL" + Guid.NewGuid().ToString("N").ToUpper().Substring(0, 8);

                var phieu = new Tbl_BM_16_Phieu
                {
                    MaPhieu = maPhieu,
                    Ngay = DateTime.Today,
                    ThoiGianTao = DateTime.Now,
                    ID_Locao = model.ID_Locao,
                    ID_Kip = model.ID_Kip,
                    ID_NguoiTao = idNhanVienTao
                };
                var tenCaStr = await _context.Tbl_Kip
                   .Where(k => k.ID_Kip == model.ID_Kip)
                   .Select(k => k.TenCa)   // kiểu string
                   .FirstOrDefaultAsync();

                int? soCa = int.TryParse(tenCaStr, out int result) ? result : null;

                await _context.Tbl_BM_16_Phieu.AddAsync(phieu);
                await _context.SaveChangesAsync();
                //ChuaChuyen = 1, ChoXuLy = 2, DaChuyen = 3, DaNhan = 4, DaChot = 5
                foreach (var thung in model.DanhSachThung)
                {
                    var chuyenDen = thung.ChuyenDen ?? "";

                    var thungGang = new Tbl_BM_16_GangLong
                    {
                        MaPhieu = maPhieu,
                        MaThungGang = thung.MaThungGang,
                        BKMIS_SoMe = thung.BKMIS_SoMe,
                        BKMIS_ThungSo = thung.BKMIS_ThungSo,
                        BKMIS_Gio = thung.BKMIS_Gio,
                        BKMIS_PhanLoai = thung.BKMIS_PhanLoai,
                        KL_XeGoong = thung.KL_XeGoong,
                        NgayLuyenGang = DateTime.Now,
                        G_KLThungChua = thung.G_KLThungChua,
                        G_KLThungVaGang = thung.G_KLThungVaGang,
                        G_KLGangLong = thung.G_KLGangLong,
                        ChuyenDen = thung.ChuyenDen ?? "",
                        Gio_NM = thung.Gio_NM,
                        G_GhiChu = thung.G_GhiChu,
                        G_ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 3 : 1,
                        NgayTao = DateTime.Now,
                        G_ID_NguoiLuu = idNhanVienTao,
                        ID_Locao = model.ID_Locao,
                        G_ID_Kip = model.ID_Kip,
                        G_Ca = soCa,
                        T_ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 4 : 2,
                        ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 2 : 1,
                        T_copy = false,
                    };

                    await _context.Tbl_BM_16_GangLong.AddAsync(thungGang);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true, maPhieu });
               
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
        public async Task<IActionResult> DetailPhieu(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Mã phiếu không hợp lệ.");

            var phieu = await _context.Tbl_BM_16_Phieu.FirstOrDefaultAsync(p => p.MaPhieu == id);
            if (phieu == null)
                return NotFound("Không tìm thấy phiếu.");
            var kip = await _context.Tbl_Kip.FirstOrDefaultAsync(k => k.ID_Kip == phieu.ID_Kip);
            var danhSachThung = await _context.Tbl_BM_16_GangLong
                .Where(t => t.MaPhieu == id && t.T_copy == false )
                .ToListAsync();
            // Lấy danh sách user nhận thùng và join
            var thungUserList = await _context.Tbl_BM_16_TaiKhoan_Thung
                .Join(_context.Tbl_TaiKhoan,
                      thung => thung.ID_taiKhoan,
                      user => user.ID_TaiKhoan,
                      (thung, user) => new
                      {
                          thung.MaThungGang,
                          user.HoVaTen,
                          user.ID_PhongBan
                      })
                .Join(_context.Tbl_PhongBan,
                    temp => temp.ID_PhongBan,
                    phongban => phongban.ID_PhongBan,
                    (temp, phongban) => new
                    {
                        temp.MaThungGang,
                        HoVaTen = $"{temp.HoVaTen} - {phongban.TenNgan}"
                    })
                .ToListAsync();

            // Gom nhóm theo MaThungGang, nhóm tiếp theo theo tên user đếm số lần nhận
            var userStats = thungUserList
                .GroupBy(x => x.MaThungGang)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(u => u.HoVaTen)
                          .Select(x => $"{x.Key} ({x.Count()})")
                          .ToList()
                );
            // Chuyển sang view model nhẹ cho hiển thị
            var viewData = danhSachThung.Select(t => new
            {
                MaThung = t.MaThungGang,
                SoMe = t.BKMIS_SoMe,
                ThuTuThung = t.BKMIS_ThungSo,
                GioNhaMay = t.BKMIS_Gio,
                PhanLoai = t.BKMIS_PhanLoai,
                KL_XeGoong = t.KL_XeGoong,
                KL_Thung = t.G_KLThungChua,
                KL_Thung_GangLong = t.G_KLThungVaGang,
                KL_GangLong_CanRay = t.G_KLGangLong,
                DenHRC1 = t.ChuyenDen == "HRC1",
                DenHRC2 = t.ChuyenDen == "HRC2",
                DenDuc1 = t.ChuyenDen == "DUC1",
                DenDuc2 = t.ChuyenDen == "DUC2",
                GioNM = t.Gio_NM,
                GhiChu = t.G_GhiChu,
                DaChuyen = t.G_ID_TrangThai==1, 
                TrangThai = t.ID_TrangThai,
                NguoiNhanList = userStats.ContainsKey(t.MaThungGang) ? userStats[t.MaThungGang] : new List<string>()
            }).ToList();

            ViewBag.DanhSachThung = viewData;

            ViewBag.MaPhieu = phieu.MaPhieu;
            ViewBag.Ngay = phieu.Ngay.ToString("yyyy-MM-dd");
            ViewBag.ID_Kip = phieu.ID_Kip;
            ViewBag.ID_Locao = phieu.ID_Locao;
            ViewBag.ThoiGianTao = phieu.ThoiGianTao;

            ViewBag.TenKip = kip?.TenKip;
            ViewBag.TenCa = kip?.TenCa;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Chuyenthung([FromBody] List<string> dsthung)
        {
            try 
            { 
                if (dsthung == null || !dsthung.Any())
                {
                   return Json(new { success = false, message = "Vui lòng chọn ít nhất một thùng" });
                }
                var thungs = await _context.Tbl_BM_16_GangLong.Where(t=> dsthung.Contains(t.MaThungGang)).ToListAsync();
                if (!thungs.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy thùng nào" });
                }
                var thungKhongHopLe = thungs.Where(t => t.G_ID_TrangThai != 1).ToList();
               
                if (thungKhongHopLe.Any())
                {
                    var maThungs = string.Join(", ", thungKhongHopLe.Select(t => t.MaThungGang));
                    return Json(new { success = false, message = $"Các thùng sau không ở trạng thái 'Chưa chuyển': {maThungs}" });
                }
                var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(tenTaiKhoan))
                    return Unauthorized("Phiên đăng nhập không hợp lệ.");

                var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
                if (taiKhoan == null)
                    return Unauthorized("Tài khoản không tồn tại.");

                int idNhanVienChuyen = taiKhoan.ID_TaiKhoan;

                foreach (var thung in thungs)
                {
                    thung.G_ID_TrangThai = 3; // Đã chuyển
                    thung.G_ID_NguoiChuyen = idNhanVienChuyen;
                    thung.ID_TrangThai = 2;
                    //thung.Gio_NM = DateTime.Now.ToString("HH:mm");
                    //thung.NgayTao = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"Đã chuyển thành công {thungs.Count} thùng.",
                    count = thungs.Count

                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi chuyển thùng: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult>ThuHoiThung([FromBody] List<string> dsthuhoi)
        {
            try
            { //ChuaChuyen = 1, ChoXuLy = 2, DaChuyen = 3, DaNhan = 4, DaChot = 5
                if (dsthuhoi == null || !dsthuhoi.Any())
                {
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất một thùng" });
                }
                var thungs = await _context.Tbl_BM_16_GangLong.Where( t => dsthuhoi.Contains(t.MaThungGang) 
                && t.G_ID_TrangThai == 3 
                && t.T_ID_TrangThai == 2 
                && t.ID_TrangThai == 2).ToListAsync();

                if (!thungs.Any())
                {
                    return Json(new { success = false, message = "Liên hệ NM.Luyện Thép Hủy Nhận thùng này trước khi thu hồi" });
                }
                var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(tenTaiKhoan))
                    return Unauthorized("Phiên đăng nhập không hợp lệ.");

                var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
                if (taiKhoan == null)
                    return Unauthorized("Tài khoản không tồn tại.");

                int idNhanVienThuHoi = taiKhoan.ID_TaiKhoan;
                foreach (var thung in thungs)
                {
                    thung.G_ID_TrangThai = 1; // Chưa chuyển
                    thung.ID_TrangThai = 1;   // Chưa chuyển
                    thung.G_ID_NguoiThuHoi = idNhanVienThuHoi;             
                }
                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"Đã thu hồi thành công {thungs.Count} thùng.",
                    count = thungs.Count

                });
            }

            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi khi thu hồi thùng: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditThung([FromBody] Tbl_BM_16_GangLong thung)
        {
            if (string.IsNullOrEmpty(thung.MaThungGang))
                return Json(new { success = false, message = "Thiếu mã thùng" });
            var thunggang = await _context.Tbl_BM_16_GangLong.FirstOrDefaultAsync(t => t.MaThungGang == thung.MaThungGang);
            if (thunggang == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thùng" });
                }
            //ChuaChuyen = 1, ChoXuLy = 2, DaChuyen = 3, DaNhan = 4, DaChot = 5
            if ( thunggang.T_ID_TrangThai == 2 && thunggang.G_ID_TrangThai == 1 && ( thunggang.ID_TrangThai == 2 || thunggang.ID_TrangThai == 1))
                {
                    thunggang.G_KLGangLong = thung.G_KLGangLong;
                    thunggang.G_KLThungChua = thung.G_KLThungChua;
                    thunggang.G_KLThungVaGang = thung.G_KLThungVaGang;
                    thunggang.G_GhiChu = thung.G_GhiChu;
                    thunggang.KL_XeGoong = thung.KL_XeGoong;
                    thunggang.ChuyenDen = thung.ChuyenDen;
                    thunggang.Gio_NM = thung.Gio_NM;
                }
            else
            {
                return Json(new { success = false, message = "Trạng thái không hợp lệ" });
            }
            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật thùng thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> SaveThung([FromBody] SaveThungDto req)
        {
            if (req == null || req.DanhSachThung == null || !req.DanhSachThung.Any())
            {
                return  BadRequest("Dữ liệu không hợp lệ.");
            }
            try
            {
                var phieu = await _context.Tbl_BM_16_Phieu.FirstOrDefaultAsync(p => p.MaPhieu == req.MaPhieu);

                var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(tenTaiKhoan))
                    return Unauthorized("Phiên đăng nhập không hợp lệ.");

                var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
                if (taiKhoan == null)
                    return Unauthorized("Tài khoản không tồn tại.");

                int idNhanVienTao = taiKhoan.ID_TaiKhoan;

                if (phieu == null)
                {
                    return NotFound("Không tìm thấy phiếu.");
                }
                // Thêm các dòng gang thỏi vào phiếu này
                //ChuaChuyen = 1, ChoXuLy = 2, DaChuyen = 3, DaNhan = 4, DaChot = 5
                foreach (var item in req.DanhSachThung)
                {
                    var chuyenDen = item.ChuyenDen ?? "";

                    var gangThoi = new Tbl_BM_16_GangLong
                    {
                        MaPhieu = req.MaPhieu,
                        MaThungGang = item.MaThungGang,
                        BKMIS_SoMe = item.BKMIS_SoMe,
                        BKMIS_ThungSo = item.BKMIS_ThungSo,
                        BKMIS_Gio = item.BKMIS_Gio,
                        BKMIS_PhanLoai = item.BKMIS_PhanLoai,
                        KL_XeGoong = item.KL_XeGoong,
                        NgayLuyenGang = DateTime.Now,
                        G_KLThungChua = item.G_KLThungChua,
                        G_KLThungVaGang = item.G_KLThungVaGang,
                        G_KLGangLong = item.G_KLGangLong,
                        ChuyenDen = item.ChuyenDen ?? "",
                        Gio_NM = DateTime.Now.ToString("HH:mm"),
                        G_GhiChu = item.G_GhiChu,
                        G_ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 2 : 1,
                        NgayTao = DateTime.Now,
                        G_ID_NguoiLuu = idNhanVienTao,
                        ID_Locao = req.ID_Locao,
                        G_ID_Kip = req.ID_Kip,
                        T_ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 3 : 2,
                        //T_ID_TrangThai = 2,
                        ID_TrangThai = 1,
                        T_copy = false,
                    };
                     _context.Tbl_BM_16_GangLong.Add(gangThoi);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Json(new { success = true, });
        }
        [HttpPost]
        public async Task <IActionResult> CapNhatThungChuaChuyen([FromBody] List<ThungGangDto> danhSach)
        {
            if (danhSach == null || !danhSach.Any())
                return BadRequest("Danh sách cập nhật rỗng.");
            //ChuaChuyen = 1, ChoXuLy = 2, DaChuyen = 3, DaNhan = 4, DaChot = 5
            foreach (var item in danhSach)
            {
                var thung =  _context.Tbl_BM_16_GangLong
                    .FirstOrDefault(x => x.MaThungGang == item.MaThungGang && x.BKMIS_SoMe == item.BKMIS_SoMe
                        && x.G_ID_TrangThai == 1
                        && (x.ID_TrangThai == 1 || x.ID_TrangThai == 2) ) ;

                if (thung != null)
                {
                    thung.BKMIS_PhanLoai = item.BKMIS_PhanLoai;
                    thung.BKMIS_Gio = item.BKMIS_Gio;
                    thung.BKMIS_SoMe = item.BKMIS_SoMe;
                    thung.BKMIS_ThungSo = item.BKMIS_ThungSo;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> XoaNhungSoMeKhongConTrongBK([FromBody] List<string> soMeCanXoa)
        {
            if (soMeCanXoa == null || !soMeCanXoa.Any())
            {
                return BadRequest("Danh sách số mẻ cần xóa bị trống.");
            }
            //ChuaChuyen = 1, ChoXuLy = 2, DaChuyen = 3, DaNhan = 4, DaChot = 5
            try
            {
                var thungCanXoa = await _context.Tbl_BM_16_GangLong
                    .Where(x => soMeCanXoa.Contains(x.BKMIS_SoMe) && x.G_ID_TrangThai == 1 || x.ID_TrangThai == 1)
                    .ToListAsync();

                if (thungCanXoa.Any())
                {
                    _context.Tbl_BM_16_GangLong.RemoveRange(thungCanXoa);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, message = "Đã xóa thành công." });
                }

                return Ok(new { success = true, message = "Không có thùng nào cần xóa." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa.", error = ex.Message });
            }
        }
       
        public async Task<IActionResult> ExportToExcel(string MaPhieu)
        {
            try
            {
                var dsachthung = await _context.Tbl_BM_16_GangLong.Where(g => g.MaPhieu == MaPhieu && g.T_copy == false).ToListAsync();
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "template_QTGNGL.xlsx");
                using (var ms = new MemoryStream())
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet("Sheet1");

                        // Xóa dữ liệu cũ (nếu có)
                        var lastRow = Math.Max(worksheet.LastRowUsed()?.RowNumber() ?? 8, 8);
                        if (lastRow >= 9)
                        {
                            var rangeClear = worksheet.Range($"A9:AE{lastRow}");
                            rangeClear.Clear(XLClearOptions.Contents | XLClearOptions.NormalFormats);
                        }

                        int row = 9, stt = 1;

                        foreach (var item in dsachthung)
                        {
                            int icol = 1;

                            worksheet.Cell(row, icol++).Value = stt++;                               // A: STT
                            worksheet.Cell(row, icol++).Value = item.MaThungGang;                    // B
                            worksheet.Cell(row, icol++).Value = item.BKMIS_SoMe;                     // C
                            worksheet.Cell(row, icol++).Value = item.BKMIS_ThungSo;                  // D
                            worksheet.Cell(row, icol++).Value = item.BKMIS_Gio;                      // E
                            worksheet.Cell(row, icol++).Value = item.BKMIS_PhanLoai;                 // F

                            var cell7 = worksheet.Cell(row, icol++);                                 // G
                            if (item.KL_XeGoong.HasValue)
                            {
                                cell7.Value = item.KL_XeGoong.Value;
                                cell7.Style.NumberFormat.Format = "0.00";
                                cell7.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell7.Style.Font.Bold = true;
                            }

                            var cell8 = worksheet.Cell(row, icol++);                                 // H
                            if (item.G_KLThungChua.HasValue)
                            {
                                cell8.Value = item.G_KLThungChua.Value;
                                cell8.Style.NumberFormat.Format = "0.00";
                                cell8.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell8.Style.Font.Bold = true;
                            }

                            var cell9 = worksheet.Cell(row, icol++);                                 // I
                            if (item.G_KLThungVaGang.HasValue)
                            {
                                cell9.Value = item.G_KLThungVaGang.Value;
                                cell9.Style.NumberFormat.Format = "0.00";
                                cell9.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell9.Style.Font.Bold = true;
                            }

                            var cell10 = worksheet.Cell(row, icol++);                                // J
                            if (item.G_KLGangLong.HasValue)
                            {
                                cell10.Value = item.G_KLGangLong.Value;
                                cell10.Style.NumberFormat.Format = "0.00";
                                cell10.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell10.Style.Font.Bold = true;
                            }

                            var chuyenDenStr = item.ChuyenDen ?? "";
                            var denList = chuyenDenStr.Split(',').Select(x => x.Trim()).ToList();

                            // Các cột K (11), L (12), M (13), N (14)
                            worksheet.Cell(row, 11).Value = denList.Contains("HRC1") ? "X" : ""; // NM.HRC1 ở cột K
                            worksheet.Cell(row, 12).Value = denList.Contains("HRC2") ? "X" : ""; // NM.HRC2 ở cột L
                            worksheet.Cell(row, 13).Value = denList.Contains("DUC1") ? "X" : ""; // NM.DUC1 ở cột M
                            worksheet.Cell(row, 14).Value = denList.Contains("DUC2") ? "X" : ""; // NM.DUC2 ở cột N

                            icol = 15;
                            worksheet.Cell(row, icol++).Value = item.Gio_NM;                         // O
                            worksheet.Cell(row, icol++).Value = item.G_GhiChu;                       // P
                            RenderTrangThaiCell(worksheet.Cell(row, 17), item.G_ID_TrangThai);   // cột Q
                            RenderTrangThaiCell(worksheet.Cell(row, 18), item.ID_TrangThai);

                            worksheet.Row(row).Height = 25;

                            for (int col = 1; col <= 18; col++)
                            {
                                worksheet.Cell(row, col).Style.NumberFormat.SetNumberFormatId(0);
                            }

                            row++;
                        }

                        // Dòng tổng
                        int sumRow = row;
                        var totalLabel = worksheet.Range($"A{sumRow}:I{sumRow}");
                        totalLabel.Merge();
                        totalLabel.Value = "Tổng:";
                        totalLabel.Style.Font.SetBold();
                        totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột J (cột 10 - G_KLGangLong)
                        worksheet.Cell(sumRow, 10).FormulaA1 = $"=SUM(J9:J{row - 1})";
                        worksheet.Cell(sumRow, 10).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 10).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Format toàn bảng A8:R{sumRow}
                        var usedRange = worksheet.Range($"A8:R{sumRow}");
                        usedRange.Style.Font.SetFontName("Arial").Font.SetFontSize(11);
                        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        usedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        usedRange.Style.Alignment.WrapText = true;


                        // Chiều cao dòng
                        for (int i = 9; i <= sumRow; i++)
                        {
                            worksheet.Row(i).Height = 25;
                        }

                        workbook.SaveAs(ms);
                    }

                    ms.Position = 0;

                    string outputName = $"QTGN_Gang_Long_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(ms.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                outputName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Danhsachphieu", "BM16_GangThoi", new { id = MaPhieu });
            }
        }

        private void RenderTrangThaiCell(IXLCell cell, int? idTrangThai)
        {
            if (idTrangThai == null)
            {
                cell.Value = "";
                cell.Style.Fill.BackgroundColor = XLColor.White;
                return;
            }

            // Lấy tên hiển thị từ enum theo id
            string trangThaiText = GetEnumDisplayName<TinhTrang>(idTrangThai.Value);
            cell.Value = trangThaiText;
            cell.Style.Font.SetBold();

            switch (idTrangThai)
            {
                case 1:
                default:
                    // Xám nhạt
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(215, 215, 215);
                    cell.Style.Font.FontColor = XLColor.Black;
                    break;

                case 2:
                    // Cam
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 153, 0);
                    cell.Style.Font.FontColor = XLColor.White;
                    break;

                case 3:
                case 4:
                case 5:
                    // Xanh lá
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(112, 173, 71);
                    cell.Style.Font.FontColor = XLColor.White;
                    break;
            }
        }

        public static string GetEnumDisplayName<TEnum>(int value) where TEnum : Enum
        {
            var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value);
            var member = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
            if (member != null)
            {
                var displayAttr = member.GetCustomAttribute<DisplayAttribute>();
                if (displayAttr != null)
                {
                    return displayAttr.Name;
                }
            }
            return enumValue.ToString();
        }


        public IActionResult ViewPDF()
        {
            return View();
        }
        //public async Task<IActionResult> GeneratePdf(int id)
        //{
        //    var bbgn = _context.Tbl_BienBanGiaoNhan
        //        .Include(x => x.ChiTietGiaoNhan)
        //        .FirstOrDefault(x => x.ID_BBGN == id);

        //    if (bbgn == null)
        //    {
        //        return NotFound();
        //    }

        //    string htmlContent = await RenderViewToStringAsync("ExportPdfView", bbgn); // truyền object model

        //    byte[] pdfBytes = ConvertHtmlToPdf(htmlContent);
        //    string filename = bbgn.SoPhieu + DateTime.Now.ToString("yyyyMMddHHmm");

        //    return File(pdfBytes, "application/pdf", filename + ".pdf");
        //}
        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (!viewResult.Success)
                {
                    throw new FileNotFoundException($"Không tìm thấy view '{viewName}'.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.ToString();
            }
        }
        private byte[] ConvertHtmlToPdf(string htmlContent)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Tạo FontProvider và quét thư mục hệ thống
                var fontProvider = new FontProvider();
                fontProvider.AddFont("C:/Windows/Fonts/times.ttf"); // Times New Roman Regular
                fontProvider.AddFont("C:/Windows/Fonts/timesbd.ttf"); // Times New Roman Bold
                fontProvider.AddFont("C:/Windows/Fonts/timesi.ttf"); // Times New Roman Italic
                fontProvider.AddFont("C:/Windows/Fonts/timesbi.ttf"); // Times New Roman Bold Italic

                // Khởi tạo PdfWriter và PdfDocument
                var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream);
                var pdfDocument = new iText.Kernel.Pdf.PdfDocument(writer);
                // Đặt kích thước trang mặc định là A4 ngang
                pdfDocument.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4.Rotate());
                pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterHandler(_context));

                // Tạo các thuộc tính chuyển đổi
                ConverterProperties converterProperties = new ConverterProperties();
                // Thiết lập FontProvider cho ConverterProperties
                converterProperties.SetFontProvider(fontProvider);
                // Đảm bảo thư mục chứa hình ảnh có thể truy cập được
                converterProperties.SetBaseUri("./img/");



                // Chuyển đổi HTML thành PDF
                HtmlConverter.ConvertToPdf(htmlContent, pdfDocument, converterProperties);

                // Chuyển HTML thành PDF
                //HtmlConverter.ConvertToPdf(htmlContent, memoryStream, converterProperties);
                return memoryStream.ToArray();
            }
        }

    }
}
