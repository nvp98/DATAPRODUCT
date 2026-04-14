using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
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
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Data_Product.Models.ModelView;
using Org.BouncyCastle.Ocsp;
using System.Text.RegularExpressions;
using MySqlConnector;
using Data_Product.Services;
using System;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;


namespace Data_Product.Controllers
{

    public class BM16_GangThoiController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ILogger<BM16_GangThoiController> _logger;
        private readonly IChiaGangService _chiaGangService;
        private readonly GetBkmisService _getBKmisService;

        public BM16_GangThoiController(DataContext _context, ICompositeViewEngine viewEngine, ILogger<BM16_GangThoiController> logger, IChiaGangService chiaGangService, GetBkmisService getBkmisService)
        {
            this._context = _context;
            this._chiaGangService = chiaGangService;
            this._getBKmisService = getBkmisService;
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
                    string query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName,Temp,Si " +
                        "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc1 " +
                        "where bkmis_kcshpsdq.view_dq1_lg_daura_lc1.ProductionDate = '" +
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc1.ShiftName ='" + cakip + "'";

                    if (ID_LoCao == 2)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName,Temp,Si " +
                        "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc2 " +
                        "where bkmis_kcshpsdq.view_dq1_lg_daura_lc2.ProductionDate = '" +
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc2.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 3)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName,Temp,Si " +
                        "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc3 " +
                        "where bkmis_kcshpsdq.view_dq1_lg_daura_lc3.ProductionDate = '" +
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc3.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 4)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName,Temp,Si " +
                       "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc4 " +
                       "where bkmis_kcshpsdq.view_dq1_lg_daura_lc4.ProductionDate = '" +
                        ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc4.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 5)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName,Temp,Si " +
                       "FROM bkmis_kcshpsdq.view_dq2_kqganglocao " +
                       "where bkmis_kcshpsdq.view_dq2_kqganglocao.ProductionDate = '" +
                        ngay + "'" + " and bkmis_kcshpsdq.view_dq2_kqganglocao.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 6)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName,Temp,Si " +
                       "FROM bkmis_kcshpsdq.view_dq2_kqganglocao_6 " +
                       "where bkmis_kcshpsdq.view_dq2_kqganglocao_6.ProductionDate = '" +
                        ngay + "'" + " and bkmis_kcshpsdq.view_dq2_kqganglocao_6.ShiftName ='" + cakip + "'";
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
                                    Temp = reader["Temp"].ToString(),
                                    Si = reader["Si"] != DBNull.Value ? reader.GetDecimal(reader.GetOrdinal("Si")) : 0m,
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
            dvt = dvt
        .Where(x => !string.IsNullOrEmpty(x.TestPatternCode) && x.TestPatternCode.Length > 9)
                .OrderBy(x => int.Parse(x.TestPatternCode.Substring(9)))
                .ToList();

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

        public async Task<IActionResult> GetCurrentCaKip()
        {
            var now = DateTime.Now;

            string tenCa;
            DateTime ngayLamViec;

            TimeSpan timeNow = now.TimeOfDay;

            TimeSpan startCa1 = new TimeSpan(8, 0, 0);   // bắt đầu ca 1 08:00
            TimeSpan endCa1 = new TimeSpan(20, 0, 0);    // kết thúc ca 1 20:00

            if (timeNow >= startCa1 && timeNow < endCa1)
            {
                // Trong khoảng 08:00 - 20:00 => Ca 1, ngày hiện tại
                tenCa = "1";
                ngayLamViec = now.Date;
            }
            else
            {
                // Trong khoảng 20:00 - 08:00 => Ca 2
                tenCa = "2";

                // Nếu giờ < 08:00 sáng => đang là ca 2 của hôm trước
                ngayLamViec = (timeNow < startCa1) ? now.Date.AddDays(-1) : now.Date;
            }

            var CaKip = await _context.Tbl_Kip
                .Where(x => x.NgayLamViec.HasValue && x.NgayLamViec.Value.Date == ngayLamViec && x.TenCa == tenCa)
                .Select(x => new
                {
                    x.ID_Kip,
                    x.TenKip,
                    x.TenCa,
                    x.NgayLamViec
                })
                .ToListAsync();

            return Json(CaKip);
        }
        public async Task<SelectList> GetLoCaoList()
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = await _context.Tbl_TaiKhoan
                         .FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);

            if (TaiKhoan == null)
            {
                return new SelectList(Enumerable.Empty<object>());
            }

            var quyenLo = await (from map in _context.Tbl_BM_16_LoSanXuat_TaiKhoan
                                 join lo in _context.Tbl_BM_16_LoSanXuat on map.ID_LoSanXuat equals lo.ID
                                 where map.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan && lo.IsActived == true
                                 select new
                                 {
                                     ID_BoPhan = lo.ID_BoPhan,
                                     MaLo = lo.MaLo
                                 }).ToListAsync();

            // Group theo ID_BoPhan
            var quyenGroup = quyenLo
                .GroupBy(x => x.ID_BoPhan)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.MaLo).Distinct().ToList()
                );


            // Danh sách lò cao cuối cùng
            List<Tbl_LoCao> loCaos = new();

            foreach (var kvp in quyenGroup)
            {
                int idBoPhan = kvp.Key;
                var dsMaLo = kvp.Value;

                var loCaoTrongBoPhan = await _context.Tbl_LoCao
                    .Where(x => x.ID_PhongBan == idBoPhan && dsMaLo.Contains(x.ID))
                    .ToListAsync();

                loCaos.AddRange(loCaoTrongBoPhan);
            }
            return new SelectList(loCaos, "ID", "TenLoCao");
        }
        public async Task<IActionResult> Danhsachphieu(string maPhieu, DateTime? ngay, DateTime? ngaypg, string ca, string locao, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;
            var loCaos = await _context.Tbl_LoCao.OrderBy(l => l.TenLoCao).ToListAsync();
            var loCaoList = await GetLoCaoList();
            ViewBag.LoCaoList = loCaoList;
            var data = new List<PhieuViewModel>();
            var pager = new Pager();
            if (loCaoList.Any())
            {
                //var query = _context.Tbl_BM_16_Phieu.OrderByDescending(p => p.NgayPhieuGang)
                var query = _context.Tbl_BM_16_Phieu
                    .OrderByDescending(p => p.NgayPhieuGang.Date) // Ngày mới trước
                        .ThenByDescending(p => p.ThoiGianTao)
            .Select(p => new PhieuViewModel
            {
                MaPhieu = p.MaPhieu,
                NgayTaoPhieu = p.NgayTaoPhieu,
                NgayPhieuGang = p.NgayPhieuGang,
                ThoiGianTao = p.ThoiGianTao.ToString("HH:mm:ss"),
                ID_LoCao = p.ID_Locao,
                TenNguoiTao = _context.Tbl_TaiKhoan
                                .Where(tk => tk.ID_TaiKhoan == p.ID_NguoiTao)
                                .Select(tk => tk.TenTaiKhoan + " " + "-" + " " + tk.HoVaTen).FirstOrDefault(),
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
                // Lọc theo ngày phiếu gang
                if (ngaypg.HasValue)
                {
                    query = query.Where(s => s.NgayPhieuGang.Date == ngaypg.Value.Date);
                }
                // Lọc theo ngày
                if (ngay.HasValue)
                {
                    query = query.Where(s => s.NgayTaoPhieu.Date == ngay.Value.Date);
                }

                // Lọc theo Ca (chuỗi)
                if (!string.IsNullOrEmpty(ca))
                {
                    string caLower = ca.ToLower();
                    query = query.Where(s => s.TenCa.ToLower() == caLower);
                }

                if (!string.IsNullOrEmpty(locao))
                {
                    if (int.TryParse(locao, out int locaoId))
                    {
                        query = query.Where(s => _context.Tbl_LoCao
                                                  .Where(lc => lc.ID == locaoId)
                                                  .Select(lc => lc.TenLoCao)
                                                  .FirstOrDefault() == s.TenLoCao);
                    }


                }
                else
                {
                    if (loCaoList.Any())
                    {
                        var idList = loCaoList.Items.Cast<Tbl_LoCao>().Select(x => x.ID).ToList();
                        query = query.Where(s => idList.Contains(s.ID_LoCao));
                    }
                }

                int resCount = await query.CountAsync();

                data = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                pager = new Pager(resCount, page, pageSize);
            }

            ViewBag.Pager = pager;

            // Truyền lại giá trị tìm kiếm cho view
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Ngay = ngay?.ToString("dd-MM-yyyy") ?? "";
            ViewBag.NgayPG = ngaypg?.ToString("dd-MM-yyyy") ?? "";
            ViewBag.Ca = ca;
            ViewBag.TenLoCao = locao;

            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> XoaPhieu(string maPhieu)
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
        public async Task<IActionResult> TaoPhieu()
        {

            ViewBag.LoCaoList = await GetLoCaoList();
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
                bool PhieuDaTonTai = await _context.Tbl_BM_16_Phieu.AnyAsync(p => p.ID_Locao == model.ID_Locao
                && p.ID_Kip == model.ID_Kip
                && p.NgayPhieuGang.Date == model.NgayPhieuGang.Date);

                if (PhieuDaTonTai)
                {
                    return BadRequest(new { success = false, message = "Đã tồn tại phiếu cho lò cao này trong ngày được chọn." });
                }
                var cakip = await _context.Tbl_Kip.Where(x => x.ID_Kip == model.ID_Kip)
                    .Select(x => x.TenCa + x.TenKip).FirstOrDefaultAsync();


                var maxIndex = await _context.Tbl_BM_16_Phieu
                .OrderByDescending(x => x.ID)
                .Select(x => x.ID)
                .FirstOrDefaultAsync();

                var maPhieu = "GL" + "-" + "LG" + "-" + "L" + model.ID_Locao + cakip + model.NgayPhieuGang.ToString("yyMMdd");

                var phieu = new Tbl_BM_16_Phieu
                {
                    MaPhieu = maPhieu,
                    NgayTaoPhieu = DateTime.Today,
                    ThoiGianTao = DateTime.Now,
                    ID_Locao = model.ID_Locao,
                    ID_Kip = model.ID_Kip,
                    ID_NguoiTao = idNhanVienTao,
                    NgayPhieuGang = model.NgayPhieuGang

                };
                var tenCaStr = await _context.Tbl_Kip
                   .Where(k => k.ID_Kip == model.ID_Kip)
                   .Select(k => k.TenCa)   // kiểu string
                   .FirstOrDefaultAsync();

                int? soCa = int.TryParse(tenCaStr, out int result) ? result : null;

                await _context.Tbl_BM_16_Phieu.AddAsync(phieu);
                await _context.SaveChangesAsync();
                //ChuaXuLy = 1, ChoXuLy = 2, DaXuly = 3, DaNhan = 4, DaChot = 5
                foreach (var thung in model.DanhSachThung)
                {
                    var chuyenDen = thung.ChuyenDen ?? "";

                    bool duDuLieu = thung.KL_XeGoong != null &&
                                    thung.G_KLThungChua != null &&
                                    thung.G_KLThungVaGang != null &&
                                    thung.G_KLGangLong != null &&
                                    !string.IsNullOrEmpty(thung.ChuyenDen) &&
                                    thung.Gio_NM != null;
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
                        G_ID_TrangThai = duDuLieu ? 3 : 1,
                        NgayTao = model.NgayPhieuGang,
                        G_ID_NguoiLuu = idNhanVienTao,
                        ID_Locao = model.ID_Locao,
                        G_ID_Kip = model.ID_Kip,
                        G_Ca = soCa,
                        T_ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 4 : 2,
                        ID_TrangThai = 2,
                        T_copy = false,
                    };

                    await _context.Tbl_BM_16_GangLong.AddAsync(thungGang);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                string url = Url.Action("DetailPhieu", "BM16_GangThoi", new { id = maPhieu });

                return Json(new { success = true, redirectUrl = url });

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
                .Where(t => t.MaPhieu == id && t.T_copy == false)
                .ToListAsync();
            int soluongme = danhSachThung.Where(x => !string.IsNullOrWhiteSpace(x.BKMIS_SoMe))
                .Select(x => x.BKMIS_SoMe)
                .Distinct()
                .Count();
            // Lấy danh sách user nhận thùng và join
            var thungUserList = await _context.Tbl_BM_16_TaiKhoan_Thung
                .Where(tk => tk.MaPhieu == id) // Lọc trước theo phiếu đang xử lý
                .Join(_context.Tbl_TaiKhoan,
                      tk => tk.ID_taiKhoan,
                      user => user.ID_TaiKhoan,
                      (tk, user) => new
                      {
                          tk.MaThungGang,
                          user.HoVaTen,
                          user.ID_PhongBan
                      })
                .Join(_context.Tbl_PhongBan,
                      temp => temp.ID_PhongBan,
                      pb => pb.ID_PhongBan,
                      (temp, pb) => new
                      {
                          temp.MaThungGang,
                          HoVaTen = $"{temp.HoVaTen} - {pb.TenNgan}"
                      })
                .ToListAsync();

            // Lấy danh sách ID người lưu từ danh sách thùng
            var idNguoiLuuList = danhSachThung
                .Where(x => x.G_ID_NguoiLuu.HasValue)
                .Select(x => x.G_ID_NguoiLuu.Value)
                .Distinct()
                .ToList();

            // Tạo dictionary ID -> Họ và tên người lưu
            var nguoiLuuDict = await _context.Tbl_TaiKhoan
                .Where(x => idNguoiLuuList.Contains(x.ID_TaiKhoan))
                .ToDictionaryAsync(x => x.ID_TaiKhoan, x => x.TenTaiKhoan + "-" + x.HoVaTen);
            // Gom nhóm theo MaThungGang, nhóm tiếp theo theo tên user đếm số lần nhận
            var userStats = thungUserList
                .GroupBy(x => x.MaThungGang)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(u => u.HoVaTen)
                          .Select(x => $"{x.Key} ({x.Count()})")
                          .ToList()
                );
            var tongTheoMe = await _context.Tbl_BM_16_GangLong
                    .Where(t => t.MaPhieu == id && !string.IsNullOrEmpty(t.BKMIS_SoMe))
                    .GroupBy(t => t.BKMIS_SoMe)
                    .Select(g => new
                    {
                        SoMe = g.Key,
                        Tong = g.Sum(x => x.KLGangChia ?? x.T_KLGangLong ?? 0)
                    })
                    .OrderBy(x => x.SoMe)
                    .ToListAsync();
            // 1) Lấy % đúc
            var phanTramDuc = await _context.Tbl_BM_16_PhanTramDuc
                .Where(x => x.ID == 1)
                .FirstOrDefaultAsync();

            decimal phanTramDucValue = phanTramDuc?.PhanTram ?? 0m;

            // 2) Gom theo "Số Mẻ" (BKMIS_SoMe) và chuẩn bị các tổng cần thiết
            //    - SumG: chỉ cộng ở bản ghi gốc (T_Copy == false)
            //    - SumT: cộng toàn bộ T_KLGangLong
            //    - SumChiaRaw: tổng KLGangChia (kể cả khi tất cả là null/0)
            //    - HasChia: có ít nhất 1 bản ghi có KLGangChia hợp lệ (> 0)
            var tongKLTheoMeRaw = await _context.Tbl_BM_16_GangLong
                .Where(t => t.MaPhieu == id
                         && t.ChuyenDen != "HRC1"
                         && t.ChuyenDen != "HRC2"
                         && !string.IsNullOrEmpty(t.BKMIS_SoMe))
                .GroupBy(t => t.BKMIS_SoMe)
                .Select(g => new
                {
                    SoMe = g.Key,
                    SumG = g.Where(x => x.T_copy == false).Sum(x => (decimal?)(x.G_KLGangLong ?? 0)) ?? 0m,
                    SumT = g.Sum(x => (decimal?)(x.T_KLGangLong ?? 0)) ?? 0m,
                    SumChiaRaw = g.Sum(x => (decimal?)(x.KLGangChia ?? 0)) ?? 0m,
                    HasChia = g.Any(x => x.KLGangChia != null && x.KLGangChia > 0),
                    ChuyenDen = g.Select(x => x.ChuyenDen).FirstOrDefault()
                })
                .ToListAsync();

            // 3) Áp logic JS: nếu không có KLGangChia hợp lệ → SumChia = 0; ngược lại = SumChiaRaw
            var tongKLTheoMe = tongKLTheoMeRaw
                .Select(x => new
                {
                    x.SoMe,
                    x.SumG,
                    x.SumT,
                    SumChia = x.HasChia ? x.SumChiaRaw : 0m,
                    x.ChuyenDen
                })
                .ToList();

            // 4) Tính KL_Đúc cho từng "Số Mẻ"
            //    Base = (SumChia > 0 ? SumChia : SumT)
            //    KL_Đúc = (SumG - Base) * (%Đúc / 100)
            var klDucByMe = tongKLTheoMe.ToDictionary(
                x => x.SoMe,
                x =>
                {
                    var @base = x.SumChia > 0m ? x.SumChia : x.SumT;
                    var value = (x.SumG - @base) * (phanTramDucValue / 100m);
                    return Math.Round(value, 2);
                }
            );
            // 5 lấy giờ nhận gang của luyện thép
            var gioChonMeByMe = await _context.Tbl_BM_16_GangLong
                 .Where(a => a.MaPhieu == id
                          && a.T_copy == false
                          && !string.IsNullOrEmpty(a.BKMIS_SoMe)
                          && a.ID_TTG != null)
                 .Join(_context.Tbl_BM_16_ThungTrungGian,
                       a => a.ID_TTG,
                       b => b.ID,
                       (a, b) => new { a.BKMIS_SoMe, b.GioChonMe })
                 .ToDictionaryAsync(x => x.BKMIS_SoMe, x => x.GioChonMe);
            // Chuyển sang view model nhẹ cho hiển thị
            var viewData = danhSachThung.Select(t => new
            {
                MaThung = t.MaThungGang,
                SoMe = t.BKMIS_SoMe,
                ThuTuThung = t.BKMIS_ThungSo,
                GioNhaMay = t.BKMIS_Gio,
                PhanLoai = t.BKMIS_PhanLoai,
                KL_XeThungVaGang = t.G_KLXeThungVaGang,
                KL_XeVaThung = t.G_KLXeVaThung,
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
                TrangThaiGang = t.G_ID_TrangThai == 1,
                TrangThai = t.ID_TrangThai,
                NguoiLuu = t.G_ID_NguoiLuu.HasValue && nguoiLuuDict.ContainsKey(t.G_ID_NguoiLuu.Value)
                ? nguoiLuuDict[t.G_ID_NguoiLuu.Value]
                : null,
                NguoiNhanList = userStats.ContainsKey(t.MaThungGang) ? userStats[t.MaThungGang] : new List<string>(),
                XacNhan = t.XacNhan,
                G_SanRaGang = t.G_SanRaGang,
                TongKL_TheoMe = tongTheoMe.FirstOrDefault(x => x.SoMe == t.BKMIS_SoMe)?.Tong ?? 0m,
                KLDuc = (t.ChuyenDen == "DUC1" || t.ChuyenDen == "DUC2")
                    && !string.IsNullOrEmpty(t.BKMIS_SoMe)
                    && klDucByMe.TryGetValue(t.BKMIS_SoMe, out var klducVal)
                        ? (decimal?)klducVal
                        : null,
                // Dùng để sort:
                //MaThungPrefix = t.MaThungGang.Split('.')[0],
                //MaThungSuffix = int.Parse(t.MaThungGang.Split('.')[1])
                GioSortKey = GetSortKeyFromTime(t.BKMIS_Gio),
                GioChonMe = gioChonMeByMe.ContainsKey(t.BKMIS_SoMe)
                ? gioChonMeByMe[t.BKMIS_SoMe]
                : null,
                Si = t.Si
            })//.OrderBy(x => x.MaThungPrefix)
              //  .ThenBy(x => x.MaThungSuffix)
                 .OrderBy(x => x.GioSortKey)
                .Select(x => new
                {
                    x.MaThung,
                    x.SoMe,
                    x.ThuTuThung,
                    x.GioNhaMay,
                    x.PhanLoai,
                    x.KL_XeThungVaGang,
                    x.KL_XeVaThung,
                    x.KL_XeGoong,
                    x.KL_Thung,
                    x.KL_Thung_GangLong,
                    x.KL_GangLong_CanRay,
                    x.DenHRC1,
                    x.DenHRC2,
                    x.DenDuc1,
                    x.DenDuc2,
                    x.GioNM,
                    x.GhiChu,
                    x.TrangThaiGang,
                    x.TrangThai,
                    x.NguoiLuu,
                    x.NguoiNhanList,
                    x.XacNhan,
                    x.TongKL_TheoMe,
                    x.KLDuc,
                    x.G_SanRaGang,
                    x.GioChonMe,
                    x.Si
                })
                .ToList();
            ViewBag.DanhSachThung = viewData;
            ViewBag.SoLuongMe = soluongme;
            ViewBag.MaPhieu = phieu.MaPhieu;
            ViewBag.Ngay = phieu.NgayTaoPhieu.ToString("yyyy-MM-dd");
            ViewBag.NgayPhieuGang = phieu.NgayPhieuGang.ToString("yyyy-MM-dd");
            ViewBag.ID_Kip = phieu.ID_Kip;
            ViewBag.ID_Locao = phieu.ID_Locao;
            ViewBag.ThoiGianTao = phieu.ThoiGianTao;

            ViewBag.TenKip = kip?.TenKip;
            ViewBag.TenCa = kip?.TenCa;
            return View();
        }
        private static int GetSortKeyFromTime(string gioStr)
        {
            if (string.IsNullOrWhiteSpace(gioStr))
                return int.MaxValue;

            var parts = gioStr.Split('H');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int hour) || !int.TryParse(parts[1], out int minute))
                return int.MaxValue;

            int ca = (hour >= 20 || hour < 8) ? 0 : 1;

            int timeOrder = (ca == 0)
                ? (hour < 8 ? (hour + 24) * 60 + minute : hour * 60 + minute)
                : hour * 60 + minute;

            return ca * 10000 + timeOrder;
        }

        [HttpPost]
        public async Task<IActionResult> SaveThung([FromBody] SaveThungDto req)
        {
            if (req == null || req.DanhSachThung == null || !req.DanhSachThung.Any())
                return BadRequest("Dữ liệu không hợp lệ.");

            try
            {
                var phieu = await _context.Tbl_BM_16_Phieu.FirstOrDefaultAsync(p => p.MaPhieu == req.MaPhieu);
                if (phieu == null) return NotFound("Không tìm thấy phiếu.");

                var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(tenTaiKhoan)) return Unauthorized("Phiên đăng nhập không hợp lệ.");

                var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
                if (taiKhoan == null) return Unauthorized("Tài khoản không tồn tại.");

                int idNhanVienLuu = taiKhoan.ID_TaiKhoan;

                var tenCaStr = await _context.Tbl_Kip
                    .Where(k => k.ID_Kip == req.ID_Kip)
                    .Select(k => k.TenCa)
                    .FirstOrDefaultAsync();
                int? soCa = int.TryParse(tenCaStr, out int ca) ? ca : null;
                foreach (var item in req.DanhSachThung)
                {
                    var daTonTai = await _context.Tbl_BM_16_GangLong
                    .AnyAsync(x => x.MaPhieu == req.MaPhieu && x.BKMIS_SoMe == item.BKMIS_SoMe);

                    if (daTonTai)
                    {
                        continue;
                    }

                    bool duDuLieu = item.KL_XeGoong != null &&
                        item.G_KLThungChua != null &&
                        item.G_KLThungVaGang != null &&
                        item.G_KLGangLong != null &&
                        item.ChuyenDen != null &&
                        item.Gio_NM != null;

                    var gangThoi = new Tbl_BM_16_GangLong
                    {
                        MaPhieu = req.MaPhieu,
                        MaThungGang = item.MaThungGang,
                        BKMIS_SoMe = item.BKMIS_SoMe,
                        BKMIS_ThungSo = item.BKMIS_ThungSo,
                        BKMIS_Gio = item.BKMIS_Gio,
                        BKMIS_PhanLoai = item.BKMIS_PhanLoai,
                        KL_XeGoong = item.KL_XeGoong,
                        G_KLXeThungVaGang = item.G_KLXeThungVaGang,
                        G_KLXeVaThung = item.G_KLXeVaThung,
                        G_KLThungChua = item.G_KLThungChua,
                        G_KLThungVaGang = item.G_KLThungVaGang,
                        G_KLGangLong = item.G_KLGangLong,
                        ChuyenDen = item.ChuyenDen ?? "",
                        Gio_NM = item.Gio_NM,
                        G_GhiChu = item.G_GhiChu,
                        G_ID_TrangThai = duDuLieu ? 3 : 1,
                        NgayTao = req.NgayPhieuGang,
                        G_ID_NguoiLuu = idNhanVienLuu,
                        ID_Locao = req.ID_Locao,
                        G_ID_Kip = req.ID_Kip,
                        G_Ca = soCa,
                        T_ID_TrangThai = (item.ChuyenDen == "DUC1" || item.ChuyenDen == "DUC2") ? 4 : 2,
                        ID_TrangThai = 2,
                        T_copy = false,
                        NgayLuyenGang = DateTime.Now,
                        G_SanRaGang = item.G_SanRaGang
                    };

                    _context.Tbl_BM_16_GangLong.Add(gangThoi);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu thùng gang.");
                return StatusCode(500, "Có lỗi xảy ra trong quá trình lưu dữ liệu.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditThung([FromBody] UpdateThungRequest req)
        {
            if (req == null || req.DanhSachThung == null || !req.DanhSachThung.Any())
                return BadRequest("Không có dữ liệu.");
            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(tenTaiKhoan))
                return Unauthorized("Phiên đăng nhập không hợp lệ.");

            var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
            if (taiKhoan == null)
                return Unauthorized("Tài khoản không tồn tại.");

            int idNhanVienLuu = taiKhoan.ID_TaiKhoan;

            foreach (var item in req.DanhSachThung)
            {
                if (string.IsNullOrEmpty(item.MaThungGang)) continue;
                var thung = await _context.Tbl_BM_16_GangLong
                    .FirstOrDefaultAsync(x => x.MaPhieu == item.MaPhieu && x.MaThungGang == item.MaThungGang && x.T_copy == false);
                if (thung == null || thung.ID_TrangThai == 5) continue;
                var chuyenDen = item.ChuyenDen ?? "";

                //  Cập nhật dữ liệu
                thung.KL_XeGoong = item.KL_XeGoong;
                thung.G_KLXeThungVaGang = item.G_KLXeThungVaGang;
                thung.G_KLXeVaThung = item.G_KLXeVaThung;
                thung.G_KLThungChua = item.G_KLThungChua;
                thung.G_KLThungVaGang = item.G_KLThungVaGang;
                thung.G_KLGangLong = item.G_KLGangLong;
                thung.G_GhiChu = item.G_GhiChu;
                thung.Gio_NM = item.Gio_NM;
                thung.ChuyenDen = item.ChuyenDen;
                thung.G_ID_NguoiLuu = idNhanVienLuu;
                thung.G_SanRaGang = item.G_SanRaGang;


                bool daNhan = await _context.Tbl_BM_16_TaiKhoan_Thung.AnyAsync(x => x.MaThungGang == thung.MaThungGang);
                //  Xử lý trạng thái chuyển đến
                thung.T_ID_TrangThai = daNhan ? 4 : (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 4 : 2;

                //  Kiểm tra dữ liệu đầy đủ
                bool duDuLieu = item.KL_XeGoong != null &&
                                item.G_KLThungChua != null &&
                                item.G_KLThungVaGang != null &&
                                item.G_KLGangLong != null &&
                                !string.IsNullOrEmpty(item.ChuyenDen) &&
                                item.Gio_NM != null;

                thung.G_ID_TrangThai = duDuLieu ? 3 : 1;


                // ==== Cập nhật các thùng T_Copy ====
                var thungCopyList = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.MaThungGang == item.MaThungGang && x.T_copy == true && x.ID != thung.ID)
                    .ToListAsync();

                foreach (var copy in thungCopyList)
                {
                    copy.KL_XeGoong = item.KL_XeGoong;
                    copy.G_KLXeThungVaGang = item.G_KLXeThungVaGang;
                    copy.G_KLXeVaThung = item.G_KLXeVaThung;
                    copy.G_KLThungChua = item.G_KLThungChua;
                    copy.G_KLThungVaGang = item.G_KLThungVaGang;
                    copy.G_KLGangLong = item.G_KLGangLong;
                    copy.G_GhiChu = item.G_GhiChu;
                    copy.Gio_NM = item.Gio_NM;
                    copy.ChuyenDen = item.ChuyenDen;
                    copy.G_ID_NguoiLuu = idNhanVienLuu;
                    copy.G_SanRaGang = item.G_SanRaGang;

                    copy.T_ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 4 : copy.T_ID_TrangThai;
                    copy.G_ID_TrangThai = duDuLieu ? 3 : 1;
                }
            }

            await _context.SaveChangesAsync();
            foreach (var item in req.DanhSachThung)
            {
                var thung = await _context.Tbl_BM_16_GangLong
                    .FirstOrDefaultAsync(x => x.MaPhieu == item.MaPhieu && x.MaThungGang == item.MaThungGang);
                if (thung == null || thung.ID_TrangThai == 5) continue;
                await _chiaGangService.KiemTraVaTinhLaiTheoMaThungGangAsync(thung.MaThungGang);
            }
            return Ok("Đã cập nhật thành công.");
        }

        [HttpPost]
        public async Task<IActionResult> Xoathungcopy([FromBody] string maThungcp)
        {
            try
            {
                var thung = await _context.Tbl_BM_16_GangLong.FirstOrDefaultAsync(x => x.MaThungGang == maThungcp);

                if (thung == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy thùng cần xóa." });
                }
                if (maThungcp.EndsWith(".00"))
                {
                    return BadRequest(new { success = false, message = "Không được xóa thùng gốc (.00)." });
                }
                _context.Tbl_BM_16_GangLong.Remove(thung);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Đã xóa thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống.", detail = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> XacNhanThung([FromBody] XacNhanThungReq req)
        {
            try
            {
                if (req == null || string.IsNullOrEmpty(req.MaPhieu) || req.DsMaThung == null || !req.DsMaThung.Any())
                {
                    return Json(new { success = false, message = "Thiếu thông tin mã phiếu hoặc danh sách thùng." });
                }
                var maThungList = req.DsMaThung.Select(x => x.MaThungGang).ToList();

                var thungs = await _context.Tbl_BM_16_GangLong
                    .Where(t => t.MaPhieu == req.MaPhieu && maThungList.Contains(t.MaThungGang))
                    .ToListAsync();
                if (!thungs.Any())
                {
                    return Json(new { success = false, message = "Không tìm thấy thùng nào" });
                }
                var thungKhongHopLe = thungs.Where(t => t.XacNhan == true).ToList();

                if (thungKhongHopLe.Any())
                {
                    var maThungs = string.Join(", ", thungKhongHopLe.Select(t => t.MaThungGang));
                    return Json(new { success = false, message = $"Thùng đã được xác nhận': {maThungs}" });
                }

                if (thungs.Any(x =>x.T_copy != true && x.Si == null))
                {
                    throw new Exception("Thùng không có dữ liệu Si");
                }

                var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(tenTaiKhoan))
                    return Unauthorized("Phiên đăng nhập không hợp lệ.");

                var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
                if (taiKhoan == null)
                    return Unauthorized("Tài khoản không tồn tại.");

                int idNhanVienXacNhan = taiKhoan.ID_TaiKhoan;

                foreach (var item in thungs)
                {
                    item.XacNhan = true;
                    item.ID_NguoiXacNhan = idNhanVienXacNhan;
                }
                await _context.SaveChangesAsync();
                return Ok(new { message = "Xác nhận thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi xác nhận thùng", error = ex.Message });
            }

        }
        [HttpPost]
        public async Task<IActionResult> HuyXacNhanThung([FromBody] XacNhanThungReq req)
        {
            try
            {
                if (req == null || string.IsNullOrEmpty(req.MaPhieu) || req.DsMaThung == null || !req.DsMaThung.Any())
                    return BadRequest(new { success = false, message = "Thiếu thông tin mã phiếu hoặc danh sách thùng." });

                var maThungList = req.DsMaThung.Select(x => x.MaThungGang).ToList();

                var thungs = await _context.Tbl_BM_16_GangLong
                    .Where(t => t.MaPhieu == req.MaPhieu && maThungList.Contains(t.MaThungGang))
                    .ToListAsync();

                if (!thungs.Any())
                    return BadRequest(new { success = false, message = "Không tìm thấy thùng nào" });

                // Thùng chưa xác nhận
                var thungChuaXacNhan = thungs.Where(t => t.XacNhan == false).ToList();
                if (thungChuaXacNhan.Any())
                {
                    var maThungs = string.Join(", ", thungChuaXacNhan.Select(t => t.MaThungGang));
                    return BadRequest(new { success = false, message = $"Thùng chưa được xác nhận, không thể hủy: {maThungs}" });
                }

                // Thùng có trạng thái ID_TrangThai = 5 không được hủy
                var thungKhongDuocHuy = thungs.Where(t => t.ID_TrangThai == 5).ToList();
                if (thungKhongDuocHuy.Any())
                {
                    var maThungs = string.Join(", ", thungKhongDuocHuy.Select(t => t.MaThungGang));
                    return BadRequest(new { success = false, message = $"Thùng có trạng thái không được hủy: {maThungs}" });
                }

                var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
                int idNguoiHuy = taiKhoan.ID_TaiKhoan;
                foreach (var item in thungs)
                {
                    item.XacNhan = false;
                    item.ID_NguoiXacNhan = idNguoiHuy;
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Hủy xác nhận thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Lỗi khi hủy xác nhận thùng", error = ex.Message });
            }
        }

        public async Task<IActionResult> ExportToExcel(string MaPhieu)
        {
            try
            {
                var danhSachRaw = await _context.Tbl_BM_16_GangLong
                .Where(g => g.MaPhieu == MaPhieu && g.T_copy == false)
                .ToListAsync();

                var dsachthung = danhSachRaw
                    .Select(t => new
                    {
                        Thung = t,
                        MaThungPrefix = t.MaThungGang.Split('.')[0],
                        MaThungSuffix = int.Parse(t.MaThungGang.Split('.')[1])
                    })
                    .OrderBy(x => x.MaThungPrefix)
                    .ThenBy(x => x.MaThungSuffix)
                    .Select(x => x.Thung)
                    .ToList();
                // 3) Sum by SoMe (for the "Tổng KL theo Mẻ" column)
                var tongTheoMeDict = await _context.Tbl_BM_16_GangLong
                    .Where(t => t.MaPhieu == MaPhieu && !string.IsNullOrEmpty(t.BKMIS_SoMe))
                    .GroupBy(t => t.BKMIS_SoMe)
                    .Select(g => new
                    {
                        SoMe = g.Key,
                        Tong = g.Sum(x => (decimal?)(x.KLGangChia ?? x.T_KLGangLong ?? 0m)) ?? 0m
                    })
                    .ToDictionaryAsync(k => k.SoMe, v => v.Tong);
                // 1) Lấy % đúc
                var phanTramDuc = await _context.Tbl_BM_16_PhanTramDuc
                    .Where(x => x.ID == 1)
                    .FirstOrDefaultAsync();

                decimal phanTramDucValue = phanTramDuc?.PhanTram ?? 0m;

                // 2) Gom theo "Số Mẻ" (BKMIS_SoMe) và chuẩn bị các tổng cần thiết
                //    - SumG: chỉ cộng ở bản ghi gốc (T_Copy == false)
                //    - SumT: cộng toàn bộ T_KLGangLong
                //    - SumChiaRaw: tổng KLGangChia (kể cả khi tất cả là null/0)
                //    - HasChia: có ít nhất 1 bản ghi có KLGangChia hợp lệ (> 0)
                var tongKLTheoMeRaw = await _context.Tbl_BM_16_GangLong
                    .Where(t => t.MaPhieu == MaPhieu
                             && t.ChuyenDen != "HRC1"
                             && t.ChuyenDen != "HRC2"
                             && !string.IsNullOrEmpty(t.BKMIS_SoMe))
                    .GroupBy(t => t.BKMIS_SoMe)
                    .Select(g => new
                    {
                        SoMe = g.Key,
                        SumG = g.Where(x => x.T_copy == false).Sum(x => (decimal?)(x.G_KLGangLong ?? 0)) ?? 0m,
                        SumT = g.Sum(x => (decimal?)(x.T_KLGangLong ?? 0)) ?? 0m,
                        SumChiaRaw = g.Sum(x => (decimal?)(x.KLGangChia ?? 0)) ?? 0m,
                        HasChia = g.Any(x => x.KLGangChia != null && x.KLGangChia > 0),
                        ChuyenDen = g.Select(x => x.ChuyenDen).FirstOrDefault()
                    })
                    .ToListAsync();

                // 3) Áp logic JS: nếu không có KLGangChia hợp lệ → SumChia = 0; ngược lại = SumChiaRaw
                var tongKLDucTheoMe = tongKLTheoMeRaw
                    .Select(x => new
                    {
                        x.SoMe,
                        x.SumG,
                        x.SumT,
                        SumChia = x.HasChia ? x.SumChiaRaw : 0m,
                        x.ChuyenDen
                    })
                    .ToList();

                // 4) Tính KL_Đúc cho từng "Số Mẻ"
                //    Base = (SumChia > 0 ? SumChia : SumT)
                //    KL_Đúc = (SumG - Base) * (%Đúc / 100)
                var klDucByMe = tongKLDucTheoMe.ToDictionary(
                    x => x.SoMe,
                    x =>
                    {
                        var @base = x.SumChia > 0m ? x.SumChia : x.SumT;
                        var value = (x.SumG - @base) * (phanTramDucValue / 100m);
                        return Math.Round(value, 2);
                    }
                );

                // Lấy thông tin dòng đầu tiên (vì tất cả chung MaPhieu)
                var firstItem = dsachthung.First();
                var ngayLuyen = firstItem.NgayLuyenGang?.ToString("dd/MM/yyyy") ?? "";
                var ca = firstItem.G_Ca?.ToString() ?? "";

                // Lấy tên Kíp và Lò cao
                var tenKip = firstItem.G_ID_Kip.HasValue
                    ? await _context.Tbl_Kip.Where(k => k.ID_Kip == firstItem.G_ID_Kip.Value).Select(k => k.TenKip).FirstOrDefaultAsync()
                    : "";

                var tenLoCao = firstItem.ID_Locao.HasValue
                    ? await _context.Tbl_LoCao.Where(l => l.ID == firstItem.ID_Locao.Value).Select(l => l.TenLoCao).FirstOrDefaultAsync()
                    : "";
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "template_QTGNGL.xlsx");
                using (var ms = new MemoryStream())
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet("Sheet1");

                        var infoRange = worksheet.Range("A5:R5");
                        infoRange.Merge();
                        infoRange.Value = $"Ngày luyện: {ngayLuyen} - Ca: {ca} - Kíp: {tenKip} - Lò cao: {tenLoCao}";
                        infoRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        infoRange.Style.Font.SetBold();

                        // Xóa dữ liệu cũ (nếu có) (mở rộng vùng tới U để chắc chắn bao phủ cột mới)
                        var lastRow = Math.Max(worksheet.LastRowUsed()?.RowNumber() ?? 8, 8);
                        if (lastRow >= 9)
                        {
                            var rangeClear = worksheet.Range($"A9:U{lastRow}");
                            rangeClear.Clear(XLClearOptions.Contents | XLClearOptions.NormalFormats);
                        }

                        int row = 9, stt = 1;

                        foreach (var item in dsachthung)
                        {
                            int icol = 1;

                            worksheet.Cell(row, icol++).Value = stt++;                // A: STT (1)
                            worksheet.Cell(row, icol++).Value = item.MaThungGang;     // B (2)
                            worksheet.Cell(row, icol++).Value = item.BKMIS_SoMe;      // C (3)
                            worksheet.Cell(row, icol++).Value = item.BKMIS_ThungSo;   // D (4)
                            worksheet.Cell(row, icol++).Value = item.BKMIS_Gio;       // E (5)
                            worksheet.Cell(row, icol++).Value = item.BKMIS_PhanLoai;  // F (6)

                            // NEW COLUMN: G_SanRaGang -> G (7)
                            worksheet.Cell(row, icol++).Value = item.G_SanRaGang;     // G (7) - đảm bảo property tồn tại

                            // KL_XeGoong giờ ở cột H (8)
                            var cellKLXe = worksheet.Cell(row, icol++);
                            if (item.KL_XeGoong.HasValue)
                            {
                                cellKLXe.Value = item.KL_XeGoong.Value;
                                cellKLXe.Style.NumberFormat.Format = "0.00";
                                cellKLXe.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cellKLXe.Style.Font.Bold = true;
                            }

                            // G_KLThungChua -> I (9)
                            var cellThungChua = worksheet.Cell(row, icol++);
                            if (item.G_KLThungChua.HasValue)
                            {
                                cellThungChua.Value = item.G_KLThungChua.Value;
                                cellThungChua.Style.NumberFormat.Format = "0.00";
                                cellThungChua.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cellThungChua.Style.Font.Bold = true;
                            }

                            // G_KLThungVaGang -> J (10)
                            var cellThungVaGang = worksheet.Cell(row, icol++);
                            if (item.G_KLThungVaGang.HasValue)
                            {
                                cellThungVaGang.Value = item.G_KLThungVaGang.Value;
                                cellThungVaGang.Style.NumberFormat.Format = "0.00";
                                cellThungVaGang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cellThungVaGang.Style.Font.Bold = true;
                            }

                            // G_KLGangLong -> K (11)
                            var cellGangLong = worksheet.Cell(row, icol++);
                            if (item.G_KLGangLong.HasValue)
                            {
                                cellGangLong.Value = item.G_KLGangLong.Value;
                                cellGangLong.Style.NumberFormat.Format = "0.00";
                                cellGangLong.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cellGangLong.Style.Font.Bold = true;
                            }

                            // Chuyển đích: HRC1..DUC2
                            var chuyenDenStr = item.ChuyenDen ?? "";
                            var denList = chuyenDenStr.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                      .Select(x => x.Trim())
                                                      .ToList();

                            worksheet.Cell(row, 12).Value = denList.Contains("HRC1") ? "X" : ""; // L (12)
                            worksheet.Cell(row, 13).Value = denList.Contains("HRC2") ? "X" : ""; // M (13)
                            worksheet.Cell(row, 14).Value = denList.Contains("DUC1") ? "X" : ""; // N (14)
                            worksheet.Cell(row, 15).Value = denList.Contains("DUC2") ? "X" : ""; // O (15)

                            // Tiếp tục: Gio_NM -> P (16)
                            worksheet.Cell(row, 16).Value = item.Gio_NM;
                            // Ghi chú -> Q (17)
                            worksheet.Cell(row, 17).Value = item.G_GhiChu;

                            // Trạng thái -> R (18), S (19)
                            RenderTrangThaiCell(worksheet.Cell(row, 18), item.G_ID_TrangThai); // R
                            RenderTrangThaiCell(worksheet.Cell(row, 19), item.ID_TrangThai);   // S

                            // Tổng KL theo Mẻ (SoMe) -> T (20)
                            decimal tongKLTheoMe = 0m;
                            if (!string.IsNullOrWhiteSpace(item.BKMIS_SoMe))
                                tongKLTheoMe = tongTheoMeDict.TryGetValue(item.BKMIS_SoMe, out var val) ? val : 0m;

                            var cellTongMe = worksheet.Cell(row, 20);
                            cellTongMe.Value = tongKLTheoMe;
                            cellTongMe.Style.NumberFormat.Format = "0.00";
                            cellTongMe.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            cellTongMe.Style.Font.Bold = true;

                            // KL_Đúc -> U (21)
                            var cellKLDuc = worksheet.Cell(row, 21);
                            bool isDuc = denList.Contains("DUC1") || denList.Contains("DUC2");
                            if (isDuc && !string.IsNullOrEmpty(item.BKMIS_SoMe)
                                     && klDucByMe.TryGetValue(item.BKMIS_SoMe, out var klducVal))
                            {
                                cellKLDuc.Value = klducVal;
                                cellKLDuc.Style.NumberFormat.Format = "#,##0.00";
                                cellKLDuc.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cellKLDuc.Style.Font.Bold = true;
                            }

                            worksheet.Row(row).Height = 25;

                            // Chuẩn hóa NumberFormat (nếu cần) cho tất cả cột đến U (21)
                            for (int col = 1; col <= 21; col++)
                            {
                                // Chỉ reset về General cho các ô chưa đặt riêng
                                if (worksheet.Cell(row, col).Style.NumberFormat.Format == "")
                                    worksheet.Cell(row, col).Style.NumberFormat.SetNumberFormatId(0);
                            }

                            row++;
                        }

                        // Dòng tổng
                        int sumRow = row;
                        // Mở rộng label đến cột trước cột số liệu đầu tiên tính tổng K (11) => tức J (10) => index 10 = J
                        var totalLabel = worksheet.Range($"A{sumRow}:J{sumRow}");
                        totalLabel.Merge();
                        totalLabel.Value = "Tổng:";
                        totalLabel.Style.Font.SetBold();
                        totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng G_KLGangLong: cột K (11)
                        worksheet.Cell(sumRow, 11).FormulaA1 = $"=SUM(K9:K{row - 1})";
                        worksheet.Cell(sumRow, 11).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 11).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 11).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Tổng T (20) = Tổng KL theo Mẻ
                        worksheet.Cell(sumRow, 20).FormulaA1 = $"=SUM(T9:T{row - 1})";
                        worksheet.Cell(sumRow, 20).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 20).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 20).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Tổng U (21) = KL_Đúc
                        worksheet.Cell(sumRow, 21).FormulaA1 = $"=SUM(U9:U{row - 1})";
                        worksheet.Cell(sumRow, 21).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 21).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 21).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Format vùng dữ liệu (A9:U{sumRow})
                        var usedRange = worksheet.Range($"A9:U{sumRow}");
                        usedRange.Style.Font.SetFontName("Times New Roman").Font.SetFontSize(11);
                        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        usedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        usedRange.Style.Alignment.WrapText = true;

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
        [HttpPost]
        public async Task<IActionResult> ExportToPDF([FromBody] string MaPhieu)
        {
            try
            {
                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);
                if (TaiKhoan == null) return BadRequest("Tài khoản không tồn tại.");

                var PhongBan = await _context.Tbl_PhongBan
                    .Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan)
                    .Select(x => x.TenNgan)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(PhongBan)) return BadRequest("Không có phòng ban.");

                var data = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.MaPhieu == MaPhieu && x.ID_TrangThai == 5 && x.T_copy == false && x.XacNhan == true)
                    .ToListAsync();

                if (data == null || !data.Any())
                    return BadRequest("Danh sách trống.");

                // Join dữ liệu người chuyển
                var chiTietGangLong = (from thung in data
                                       join user in _context.Tbl_TaiKhoan on thung.G_ID_NguoiLuu equals user.ID_TaiKhoan into g_user
                                       from user in g_user.DefaultIfEmpty()
                                       join phongBan in _context.Tbl_PhongBan on user.ID_PhongBan equals phongBan.ID_PhongBan into g_phongBan
                                       from phongBan in g_phongBan.DefaultIfEmpty()
                                       join vitri in _context.Tbl_ViTri on user.ID_ChucVu equals vitri.ID_ViTri into g_vitri
                                       from vitri in g_vitri.DefaultIfEmpty()
                                       select new Tbl_BM_16_GangLong
                                       {
                                           ID = thung.ID,
                                           BKMIS_SoMe = thung.BKMIS_SoMe,
                                           BKMIS_Gio = thung.BKMIS_Gio,
                                           BKMIS_PhanLoai = thung.BKMIS_PhanLoai,
                                           MaThungThep = thung.MaThungThep,
                                           BKMIS_ThungSo = thung.BKMIS_ThungSo,
                                           NgayLuyenThep = thung.NgayLuyenThep,
                                           ChuyenDen = thung.ChuyenDen,
                                           KL_XeGoong = thung.KL_XeGoong,
                                           G_KLThungChua = thung.G_KLThungChua,
                                           G_KLThungVaGang = thung.G_KLThungVaGang,
                                           G_KLGangLong = thung.G_KLGangLong,
                                           G_GhiChu = thung.G_GhiChu,
                                           G_Ca = thung.G_Ca,
                                           T_ID_TrangThai = thung.T_ID_TrangThai,
                                           T_KLThungVaGang = thung.T_KLThungVaGang,
                                           T_KLThungChua = thung.T_KLThungChua,
                                           T_KLGangLong = thung.T_KLGangLong,
                                           ThungTrungGian = thung.ThungTrungGian,
                                           T_KLThungVaGang_Thoi = thung.T_KLThungVaGang_Thoi,
                                           T_KLThungChua_Thoi = thung.T_KLThungChua_Thoi,
                                           T_KLGangLongThoi = thung.T_KLGangLongThoi,
                                           T_GhiChu = thung.T_GhiChu,
                                           ID_Locao = thung.ID_Locao,
                                           ID_TrangThai = thung.ID_TrangThai,
                                           TrangThai = thung.TrangThai,
                                           MaMeThoi = thung.MaMeThoi,
                                           T_KL_phe = thung.T_KL_phe,
                                           Gio_NM = thung.Gio_NM,
                                           T_Ca = thung.T_Ca,
                                           T_TenKip = thung.T_TenKip,

                                           // Thông tin người chuyển
                                           HoVaTen = user?.HoVaTen ?? "",
                                           TenTaiKhoan = user?.TenTaiKhoan ?? "",
                                           TenPhongBan = phongBan?.TenNgan ?? "",
                                           ChuKy = user?.ChuKy ?? "",
                                           TenViTri = vitri?.TenViTri ?? ""
                                       }).ToList();

                // Người chuyển (dạng group)
                var nguoiChuyenList = (from gl in _context.Tbl_BM_16_GangLong
                                       join user in _context.Tbl_TaiKhoan on gl.G_ID_NguoiLuu equals user.ID_TaiKhoan
                                       join pb in _context.Tbl_PhongBan on user.ID_PhongBan equals pb.ID_PhongBan into g_pb
                                       from pb in g_pb.DefaultIfEmpty()
                                       join vt in _context.Tbl_ViTri on user.ID_ChucVu equals vt.ID_ViTri into g_vt
                                       from vt in g_vt.DefaultIfEmpty()
                                       where gl.MaPhieu == MaPhieu && gl.ID_TrangThai == 5  //&& gl.MaThungGang
                                       select new NguoiInfo
                                       {
                                           HoVaTen = user.HoVaTen,
                                           TenPhongBan = pb != null ? pb.TenNgan : "",
                                           TenViTri = vt != null ? vt.TenViTri : "",
                                           ChuKy = user.ChuKy
                                       }).Distinct().ToList();
                // Người chuyển (dạng group)
                var nguoiXacNhanList = (from gl in _context.Tbl_BM_16_GangLong
                                        join user in _context.Tbl_TaiKhoan on gl.ID_NguoiXacNhan equals user.ID_TaiKhoan
                                        join pb in _context.Tbl_PhongBan on user.ID_PhongBan equals pb.ID_PhongBan into g_pb
                                        from pb in g_pb.DefaultIfEmpty()
                                        join vt in _context.Tbl_ViTri on user.ID_ChucVu equals vt.ID_ViTri into g_vt
                                        from vt in g_vt.DefaultIfEmpty()
                                        where gl.MaPhieu == MaPhieu && gl.ID_TrangThai == 5 //&& gl.MaThungGang
                                        select new NguoiInfo
                                        {
                                            HoVaTen = user.HoVaTen,
                                            TenPhongBan = pb != null ? pb.TenNgan : "",
                                            TenViTri = vt != null ? vt.TenViTri : "",
                                            ChuKy = user.ChuKy
                                        }).Distinct().ToList();

                // Người nhận (dạng group)
                var nguoiNhanList = (from tkThung in _context.Tbl_BM_16_TaiKhoan_Thung
                                     join user in _context.Tbl_TaiKhoan on tkThung.ID_taiKhoan equals user.ID_TaiKhoan
                                     join pb in _context.Tbl_PhongBan on user.ID_PhongBan equals pb.ID_PhongBan into g_pb
                                     from pb in g_pb.DefaultIfEmpty()
                                     join vt in _context.Tbl_ViTri on user.ID_ChucVu equals vt.ID_ViTri into g_vt
                                     from vt in g_vt.DefaultIfEmpty()
                                     where tkThung.MaPhieu == MaPhieu  //&& tkThung.MaThungGang
                                     select new NguoiInfo
                                     {
                                         HoVaTen = user.HoVaTen,
                                         TenPhongBan = pb != null ? pb.TenNgan : "",
                                         TenViTri = vt != null ? vt.TenViTri : "",
                                         ChuKy = user.ChuKy
                                     }).Distinct().ToList();


                // Chuẩn bị ViewModel
                var viewModel = new BBGN_GangLong_ViewModel
                {
                    DanhSachGangLong = chiTietGangLong,
                    NguoiChuyen = nguoiChuyenList,
                    NguoiNhan = nguoiNhanList,
                    NguoiXacNhan = nguoiXacNhanList
                    //.Select(x => new NguoiInfo
                    //{
                    //    HoVaTen = x.HoVaTen,
                    //    TenPhongBan = x.TenPhongBan,
                    //    TenViTri = x.TenViTri,
                    //    ChuKy = x.ChuKy
                    //}).Distinct().ToList(),




                };

                // Render View -> HTML
                string html = await RenderViewToStringAsync("BBGN_Gang_PDF", viewModel);


                // 2. Chuyển đổi HTML sang PDF
                byte[] pdfBytes = ConvertHtmlToPdf(html);

                string filename = $"BBGN Gang long - Luyen Gang {DateTime.Now.ToString("yyyyMMddHHmm")}.pdf";

                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("DetailPhieu", "BM16_GangThoi");
            }
        }

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (!viewResult.Success)
                {
                    throw new FileNotFoundException($"View '{viewName}' không tìm thấy.");
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
                return await Task.FromResult(writer.ToString());
            }
        }

        private byte[] ConvertHtmlToPdf(string htmlContent)
        {
            using (var memoryStream = new MemoryStream())
            {
                // 1. Cấu hình FontProvider để hỗ trợ Times New Roman
                var fontProvider = new FontProvider();
                fontProvider.AddFont("C:/Windows/Fonts/times.ttf");     // Regular
                fontProvider.AddFont("C:/Windows/Fonts/timesbd.ttf");   // Bold
                fontProvider.AddFont("C:/Windows/Fonts/timesi.ttf");    // Italic
                fontProvider.AddFont("C:/Windows/Fonts/timesbi.ttf");   // Bold Italic

                // 2. Tạo writer và document
                var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream);
                var pdfDocument = new iText.Kernel.Pdf.PdfDocument(writer);
                pdfDocument.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4.Rotate()); // Trang ngang

                // 3. Cấu hình Converter
                var converterProperties = new ConverterProperties();
                converterProperties.SetFontProvider(fontProvider);

                // 4. Cấu hình baseUri nếu HTML chứa ảnh
                string baseUri = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                converterProperties.SetBaseUri(baseUri);

                // 5. Chuyển đổi HTML sang PDF
                HtmlConverter.ConvertToPdf(htmlContent, pdfDocument, converterProperties);

                // 6. Trả về PDF dưới dạng byte[]
                return memoryStream.ToArray();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DongBoBkMis(string maPhieu, string ngayStr, int idLoCao, string idKip, int idCa)
        {
            if (string.IsNullOrWhiteSpace(maPhieu) || !DateTime.TryParse(ngayStr, out var ngay))
            {
                TempData["msgError"] = "Thiếu dữ liệu đồng bộ.";
                return RedirectToAction(nameof(DetailPhieu), new { id = maPhieu });
            }

            // 1. Lấy danh sách thùng trong phiếu hiện tại
            var thungTrongPhieu = await _context.Tbl_BM_16_GangLong
                .Where(x => x.MaPhieu == maPhieu)
                .ToListAsync();

            // Map số mẻ -> thùng trong phiếu này
            //var mapSoMeToThung = thungTrongPhieu
            //  .Where(x => !string.IsNullOrEmpty(x.BKMIS_SoMe))
            //    .ToDictionary(x => x.BKMIS_SoMe, x => x);

            var mapSoMeToThung = thungTrongPhieu
            .Where(x => !string.IsNullOrEmpty(x.BKMIS_SoMe))
            .GroupBy(x => x.BKMIS_SoMe.Trim())
            .ToDictionary(g => g.Key, g => g.ToList());

            // 2. Lấy dữ liệu mới từ BK-MIS
            var bkData = await _getBKmisService.GetSoMeBKMisAsync(ngayStr, idLoCao, idKip);
            var klXe = await _getBKmisService.GetKhoiLuongXeLoCaoAsync(idLoCao);

            var soMeBK = bkData
                .Select(x => x.TestPatternCode.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToHashSet();

            int cntUpdate = 0, cntInsert = 0, cntDelete = 0;

            // 3. Lấy số thứ tự (stt) lớn nhất hiện có trong danh sách mã thùng
            int lastStt = thungTrongPhieu
                .Select(x => x.MaThungGang)
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(ma =>
                {
                    // Regex lấy 3 chữ số trước ".00" ở cuối chuỗi
                    var m = System.Text.RegularExpressions.Regex.Match(ma, @"(\d{3})\.00$");
                    return m.Success ? int.Parse(m.Groups[1].Value) : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            // 4. Cập nhật hoặc thêm mới thùng
            foreach (var rec in bkData)
            {
                var soMe = rec.TestPatternCode.Trim();
                if (string.IsNullOrEmpty(soMe)) continue;

                if (mapSoMeToThung.TryGetValue(soMe, out var danhSachThung))
                {
                    foreach (var thung in danhSachThung)
                    {
                        thung.Temp = int.TryParse(rec.Temp, out int tempValue) ? tempValue : (int?)null;
                        // Kiểm tra trạng thái thùng cho phép cập nhật
                        bool choPhepCapNhat =
                            (thung.G_ID_TrangThai == 1 || thung.G_ID_TrangThai == 3) &&
                            thung.XacNhan == false &&
                            (thung.T_ID_TrangThai == 2 || thung.T_ID_TrangThai == 4) &&
                            thung.ID_TrangThai == 2;

                        if (!choPhepCapNhat) continue;
                       
                        thung.Si = rec.Si;
                        // Cập nhật thông tin mới từ BK-MIS
                        thung.BKMIS_PhanLoai = rec.ClassifyName;
                        thung.BKMIS_Gio = rec.Patterntime ?? string.Empty;
                        thung.BKMIS_ThungSo = soMe.Length >= 2 ? soMe[^2..] : null;                       
                        cntUpdate++;
                    }
                }
                else
                {
                    // Thêm mới thùng → tăng lastStt lên 1 rồi tạo mã mới
                    lastStt++;

                    var maThung = GenerateMaThung(ngay, idLoCao, idCa, lastStt);
                    var thungSo = soMe.Length >= 2 ? soMe[^2..] : null;

                    _context.Tbl_BM_16_GangLong.Add(new Tbl_BM_16_GangLong
                    {
                        MaPhieu = maPhieu,
                        MaThungGang = maThung,
                        BKMIS_SoMe = soMe,
                        BKMIS_ThungSo = thungSo,
                        BKMIS_Gio = rec.Patterntime ?? string.Empty,
                        BKMIS_PhanLoai = rec.ClassifyName,
                        KL_XeGoong = klXe,
                        G_ID_TrangThai = 1,
                        T_ID_TrangThai = 2,
                        ID_TrangThai = 2,
                        T_copy = false,
                        ID_Locao = idLoCao,
                        G_ID_Kip = int.TryParse(idKip, out var kipInt2) ? kipInt2 : 0,
                        G_Ca = idCa,
                        NgayLuyenGang = DateTime.Now,
                        NgayTao = ngay,
                        Temp = int.TryParse(rec.Temp, out int tempValue) ? tempValue : (int?)null,
                        Si = rec.Si,
                    });

                    cntInsert++;
                }
            }

            // 5. Xóa thùng có số mẻ không còn trong BK-MIS
            var thungCanXoa = thungTrongPhieu
                .Where(x => !soMeBK.Contains(x.BKMIS_SoMe.Trim()))
                .Where(x =>
                    (x.XacNhan == false || x.XacNhan == null) &&
                    (x.G_ID_TrangThai == 1 || x.G_ID_TrangThai == 3) &&
                    x.T_ID_TrangThai == 2 &&
                    x.ID_TrangThai == 2)
                .ToList();

            cntDelete = thungCanXoa.Count;

            if (cntDelete > 0)
                _context.Tbl_BM_16_GangLong.RemoveRange(thungCanXoa);

            await _context.SaveChangesAsync();

            TempData["msgSuccess"] = $"Đồng bộ BK-MIS: cập nhật {cntUpdate}, thêm mới {cntInsert}, xóa {cntDelete}.";

            return RedirectToAction(nameof(DetailPhieu), new { id = maPhieu });
        }

        private static string GenerateMaThung(DateTime date, int loCao, int ca, int stt)
        {
            string dd = date.Day.ToString("00");
            string mm = date.Month.ToString("00");
            string yy = (date.Year % 100).ToString("00");
            string caChar = (ca == 1) ? "N" : "D"; // 1: N, else: D
            string sttFormatted = stt.ToString("000");
            return $"{dd}{mm}{yy}L{loCao}{caChar}{sttFormatted}.00";
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSoMe(string maPhieu, string ngayStr, int idLoCao, int idCa, string term = null, int take = 200, bool select2 = true)
        {
            if (string.IsNullOrWhiteSpace(maPhieu) || !DateTime.TryParse(ngayStr, out var ngay))
            {
                return BadRequest("Thiếu hoặc sai dữ liệu (mã phiếu / ngày).");
            }

            // Base query: điều chỉnh tên cột cho đúng với schema thực tế nếu khác
            var query = _context.Tbl_BM_16_GangLong
                .AsNoTracking()
                .Where(x => x.MaPhieu == maPhieu
                            && x.NgayTao == ngay
                            && x.ID_TrangThai == (int)TinhTrang.ChoXuLy
                            && !string.IsNullOrEmpty(x.BKMIS_SoMe));

            if (!string.IsNullOrWhiteSpace(term))
            {
                // Lọc “contains” khi người dùng gõ
                query = query.Where(x => x.BKMIS_SoMe.Contains(term));
            }

            var list = await query
                .Select(x => x.BKMIS_SoMe.Trim())
                .Distinct()
                .OrderBy(x => x)
                .Take(take)
                .ToListAsync();

            if (select2)
            {

                var result = list.Select(v => new { id = v, text = v });
                return Ok(result);
            }

            return Ok(list);
        }




        [HttpGet]
        public async Task<IActionResult> GetAutoSourceData(string fromDateStr, string toDateStr, int idLoCao, string maPhieu = null)
        {
            if (!DateTime.TryParse(fromDateStr, out var fromTime))
                return BadRequest("Từ ngày/giờ không hợp lệ.");

            if (!DateTime.TryParse(toDateStr, out var toTime))
                return BadRequest("Đến ngày/giờ không hợp lệ.");

            if (idLoCao <= 0)
                return BadRequest("ID lò cao không hợp lệ.");

            // Dispatch: lò 1..4 => RailScale; lò 5/6 => lấy từ DbContext (LogDataBf5/6)
            if (idLoCao >= 1 && idLoCao <= 4)
            {
                return await GetAutoSourceDataRail_Internal(fromTime, toTime, idLoCao, maPhieu);
            }
            else if (idLoCao == 5 || idLoCao == 6)
            {
                return await GetAutoSourceDataBF_Internal(fromTime, toTime, idLoCao, maPhieu);
            }
            else
            {
                return BadRequest("ID lò cao không được hỗ trợ.");
            }
        }

        private async Task<IActionResult> GetAutoSourceDataRail_Internal(DateTime fromTime, DateTime toTime, int idLoCao, string maPhieu = null)
        {
            try
            {
                //var maPhieu = HttpContext.Request.Query["maPhieu"].ToString();

                var query = _context.Tbl_CanRayLG1.AsQueryable();
                query = query.Where(d => d.ID_LoCao == idLoCao && d.Gio >= fromTime && d.Gio <= toTime);

                var rawData = await query
                    .OrderByDescending(d => d.Gio)
                    .Take(1000)
                    .Select(d => new
                    {
                        ID = d.ID,
                        ID_LoCao = d.ID_LoCao,
                        Gio = d.Gio,
                        ThungSo = d.ThungSo,
                        Ray = d.Ray,
                        SanRaGang = d.SanRaGang,
                        KL_Bi = d.KL_Bi,
                        KL_Tong = d.KL_Tong,
                        KL_Gang = d.KL_Gang,
                        BKMIS_SoMe = d.BKMIS_SoMe,
                        GhiChu = d.Ghi_Chu,
                        SoMe_Cleared = d.SoMe_Cleared
                    })
                    .ToListAsync();
                // Lấy danh sách số mẻ đã chốt (G_ID_TrangThai = 3)
                var lockedSoMeList = new HashSet<string>();
                if (!string.IsNullOrWhiteSpace(maPhieu))
                {
                    lockedSoMeList = (await _context.Tbl_BM_16_GangLong
                        .Where(x => x.MaPhieu == maPhieu && x.ID_TrangThai == 5 && !string.IsNullOrEmpty(x.BKMIS_SoMe))
                        .Select(x => x.BKMIS_SoMe.Trim())
                        .Distinct()
                        .ToListAsync())
                        .ToHashSet();
                }
                int rowId = 0;
                var list = rawData.Select(d =>
                {
                    rowId++;
                    var soMe = d.BKMIS_SoMe?.Trim() ?? "";
                    var isLocked = !string.IsNullOrEmpty(soMe) && soMe != "0" && lockedSoMeList.Contains(soMe);

                    return new MappingCanRayDto
                    {
                        RowId = rowId,
                        ID_LoCao = d.ID_LoCao ?? idLoCao,
                        CanRayId = d.ID,
                        GioChotGang = d.Gio,
                        GioStr = d.Gio.HasValue ? d.Gio.Value.ToString("HH:mm") : string.Empty,
                        ThungSo = d.ThungSo.HasValue ? (decimal?)d.ThungSo.Value : null,
                        KL_Bi = d.KL_Bi,
                        KL_Tong = d.KL_Tong,
                        KL_Gang = d.KL_Gang,
                        SanRaGang = d.SanRaGang,
                        BKMIS_SoMe = d.BKMIS_SoMe,
                        GhiChu = d.GhiChu,
                        SoMe_Cleared = d.SoMe_Cleared,
                        IsLocked = isLocked
                    };
                }).ToList();

                return Ok(list);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi không xác định khi lấy dữ liệu RailScale.");
            }
        }
        private async Task<IActionResult> GetAutoSourceDataBF_Internal(DateTime fromTime, DateTime toTime, int idLoCao, string maPhieu = null)
        {


            try
            {
                // 1. KIỂM TRA ĐẦU VÀO
                if (idLoCao != 5 && idLoCao != 6)
                    return BadRequest("idLoCao phải là 5 hoặc 6.");
              ///  var maPhieu = HttpContext.Request.Query["maPhieu"].ToString();
              ///  
                var query = _context.Tbl_CanRayLG2.AsQueryable();

                // 2.2. Lọc theo ID Lò Cao và Khoảng thời gian
                query = query.Where(d => d.BF_no == idLoCao &&
                                         d.BF_Timestap >= fromTime &&
                                         d.BF_Timestap <= toTime);

                // 2.3. Sắp xếp và Giới hạn TOP (1000)
                var rawData = await query
                    .OrderByDescending(d => d.BF_Timestap)
                    .Take(1000)
                    .Select(d => new
                    {
                        // Chọn các cột cần thiết trực tiếp từ Database
                        ID = d.ID,
                        BF_no = d.BF_no,
                        Laddle_no = d.Laddle_no,
                        Shift = d.Shift,
                        BF_Timestap = d.BF_Timestap,
                        Casthouse = d.Casthouse,
                        Weight_no = d.Weight_no,
                        Weight_TARE = d.Weight_TARE,
                        Weight_GROSS = d.Weight_GROSS,
                        Weight_NET = d.Weight_NET,
                        BKMIS_SoMe = d.BKMIS_SoMe,
                        GhiChu = d.Ghi_Chu,
                        SoMe_Cleared = d.SoMe_Cleared

                    })
                    .ToListAsync(); // Thực thi truy vấn và tải dữ liệu

                // Lấy danh sách số mẻ đã chốt
                var lockedSoMeList = new HashSet<string>();
                if (!string.IsNullOrWhiteSpace(maPhieu))
                {
                    lockedSoMeList = (await _context.Tbl_BM_16_GangLong
                        .Where(x => x.MaPhieu == maPhieu && x.ID_TrangThai == 5 && !string.IsNullOrEmpty(x.BKMIS_SoMe))
                        .Select(x => x.BKMIS_SoMe.Trim())
                        .Distinct()
                        .ToListAsync())
                        .ToHashSet();
                }

                int rowId = 0;
                var list = rawData.Select(d =>
                {
                    rowId++;
                    var soMe = d.BKMIS_SoMe?.Trim() ?? "";
                    var isLocked = !string.IsNullOrEmpty(soMe) && soMe != "0" && lockedSoMeList.Contains(soMe);
                    return new MappingCanRayDto
                    {
                        RowId = rowId,
                        ID_LoCao = idLoCao,
                        CanRayId = d.ID,
                        GioChotGang = d.BF_Timestap,
                        GioStr = d.BF_Timestap.HasValue ? d.BF_Timestap.Value.ToString("HH:mm") : string.Empty,
                        ThungSo = d.Laddle_no,
                        // Chuyển từ double? (Model) sang decimal? (DTO)
                        KL_Bi = (decimal?)d.Weight_TARE,
                        KL_Tong = (decimal?)d.Weight_GROSS,
                        KL_Gang = (decimal?)d.Weight_NET,
                        BF_no = d.BF_no,
                        Laddle_no = d.Laddle_no,
                        Shift = d.Shift,
                        Casthouse = d.Casthouse,
                        SanRaGang = d.Casthouse,
                        BKMIS_SoMe = d.BKMIS_SoMe,
                        GhiChu = d.GhiChu,
                        SoMe_Cleared = d.SoMe_Cleared,
                        IsLocked = isLocked
                    };
                }).ToList();

                return Ok(list);
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi SQL, sử dụng ex.Message để log chi tiết hơn
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi SQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                return StatusCode(StatusCodes.Status500InternalServerError, $"Lỗi không xác định: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> SaveAutoMapping([FromBody] List<AutoMappingItemDto> items)
        {
            if (items == null || items.Count == 0)
                return BadRequest("Không có dữ liệu để lưu.");

            // Kiểm tra MaPhieu đồng nhất
            var maPhieu = items.FirstOrDefault()?.MaPhieu?.Trim();
            if (string.IsNullOrWhiteSpace(maPhieu))
                return BadRequest("Thiếu mã phiếu.");

            using (var txn = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // PHẦN 1: XỬ LÝ CÁC DÒNG CÓ SỐ MẺ = "0" (KHÁC)
                    var itemsWithZero = items.Where(x => x.SoMe == "0").ToList();
                    foreach (var item in itemsWithZero)
                    {
                        if (!item.CanRayId.HasValue || item.ID_LoCao <= 0)
                            continue;

                        var canRayId = item.CanRayId.Value;
                        var idLoCao = item.ID_LoCao;

                        // Cập nhật vào bảng Cân Ray
                        if (idLoCao >= 1 && idLoCao <= 4)
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                                @"UPDATE Tbl_CanRayLG1 
                                  SET BKMIS_SoMe = '0', Ghi_Chu = {0}, 
                                  SoMe_Cleared = 1,
                                  OriginalSoMe = ISNULL(OriginalSoMe, BKMIS_SoMe)
                                  WHERE ID = {1} AND ID_LoCao = {2}",
                                  item.G_GhiChu, canRayId, idLoCao);
                        }
                        else if (idLoCao == 5 || idLoCao == 6)
                        {
                            await _context.Database.ExecuteSqlRawAsync(
                           @"UPDATE Tbl_CanRayLG2 
                            SET BKMIS_SoMe = '0', Ghi_Chu = {0}, 
                            SoMe_Cleared = 1, 
                            OriginalSoMe = ISNULL(OriginalSoMe, BKMIS_SoMe)
                            WHERE ID = {1} AND BF_no = {2}"
                            ,item.G_GhiChu, canRayId, idLoCao);
                        }
                    }

                    // PHẦN 2: XỬ LÝ CÁC DÒNG CÓ SỐ MẺ THẬT (KHÔNG PHẢI "0")
                    var soMes = items
                        .Where(x => !string.IsNullOrWhiteSpace(x.SoMe) && x.SoMe != "0")
                        .Select(x => x.SoMe.Trim())
                        .Distinct()
                        .ToList();

                    if (!soMes.Any())
                    {
                        // Nếu chỉ có items với SoMe = "0", commit và return
                        await _context.SaveChangesAsync();
                        await txn.CommitAsync();
                        return Ok(new { success = true, message = "Đã cập nhật số mẻ = 0 (Khác)." });
                    }

                    var listThung = await _context.Tbl_BM_16_GangLong
                        .Where(x => x.MaPhieu == maPhieu
                                    && x.BKMIS_SoMe != null
                                    && x.ID_TrangThai != 5)
                        .ToListAsync();

                    if (!listThung.Any())
                    {
                        await _context.SaveChangesAsync();
                        await txn.CommitAsync();
                        return Ok(new { success = true, message = "Đã cập nhật dữ liệu." });
                    }

                    // Group items theo SoMe và tính KL gang lỏng
                    var groups = items
                        .Where(x => !string.IsNullOrWhiteSpace(x.SoMe) && x.SoMe != "0")
                        .GroupBy(x => x.SoMe.Trim())
                        .Select(g => new
                        {
                            SoMe = g.Key,
                            HasBi = g.Any(i => i.G_KLXeVaThung.HasValue),
                            HasTong = g.Any(i => i.G_KLXeThungVaGang.HasValue),
                            Min_Bi = g.Where(i => i.G_KLXeVaThung.HasValue)
                                       .Select(i => i.G_KLXeVaThung!.Value)
                                       .DefaultIfEmpty()
                                       .Min(),
                            Max_Tong = g.Where(i => i.G_KLXeThungVaGang.HasValue)
                                         .Select(i => i.G_KLXeThungVaGang!.Value)
                                         .DefaultIfEmpty()
                                         .Max()
                        })
                        .ToList();

                    foreach (var grp in groups)
                    {
                        var thungsCungSoMe = listThung
                            .Where(t => !string.IsNullOrWhiteSpace(t.BKMIS_SoMe) && t.BKMIS_SoMe.Trim() == grp.SoMe)
                            .ToList();

                        if (!thungsCungSoMe.Any())
                            continue;

                        if (grp.HasBi && grp.HasTong)
                        {
                            var klTinh = grp.Max_Tong - grp.Min_Bi;
                            if (klTinh < 0) klTinh = 0;

                            foreach (var thung in thungsCungSoMe)
                            {
                                thung.G_KLXeVaThung = grp.Min_Bi;
                                thung.G_KLXeThungVaGang = grp.Max_Tong;

                                var klXeGoong = thung.KL_XeGoong ?? 0m;

                                var klThung = grp.Min_Bi - klXeGoong;
                                if (klThung < 0) klThung = 0;
                                thung.G_KLThungChua = klThung;

                                var klThungGang = grp.Max_Tong - klXeGoong;
                                if (klThungGang < 0) klThungGang = 0;
                                thung.G_KLThungVaGang = klThungGang;

                                var klGangLongCalc = klThungGang - klThung;
                                if (klGangLongCalc < 0) klGangLongCalc = 0;
                                thung.G_KLGangLong = klGangLongCalc;

                                var gioChot = items
                                    .Where(i => i.SoMe != null && i.SoMe.Trim() == grp.SoMe && i.GioChotGang.HasValue)
                                    .Select(i => i.GioChotGang.Value)
                                    .OrderByDescending(i => i)
                                    .FirstOrDefault();
                                if (gioChot != default(DateTime))
                                    thung.Gio_NM = gioChot.ToString("HH:mm");

                                // Kiểm tra đủ dữ liệu để cập nhật trạng thái "Đã xử lý"
                                bool duDuLieu = thung.KL_XeGoong != null &&
                                                thung.G_KLThungChua != null &&
                                                thung.G_KLThungVaGang != null &&
                                                thung.G_KLGangLong != null &&
                                                !string.IsNullOrEmpty(thung.ChuyenDen) &&
                                                thung.Gio_NM != null;

                                // Cập nhật trạng thái: 3 = Đã xử lý, 1 = Chưa xử lý
                                thung.G_ID_TrangThai = duDuLieu ? 3 : 1;
                            }
                        }

                        // Cập nhật số mẻ vào bảng cân ray
                        var relatedById = items
                            .Where(i => !string.IsNullOrWhiteSpace(i.SoMe)
                                        && i.SoMe.Trim() == grp.SoMe
                                        && i.CanRayId.HasValue
                                        && i.ID_LoCao > 0)
                            .Select(i => new { Id = i.CanRayId!.Value, BfNo = i.ID_LoCao })
                            .Distinct()
                            .ToList();

                        var ghiChu = items.FirstOrDefault(i => i.SoMe != null && i.SoMe.Trim() == grp.SoMe)?.G_GhiChu;

                        foreach (var it in relatedById)
                        {
                            if (it.BfNo >= 1 && it.BfNo <= 4)
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    @"UPDATE Tbl_CanRayLG1 
                      SET BKMIS_SoMe = {0}, 
                          Ghi_Chu = {1}, 
                          SoMe_Cleared = 0, 
                          OriginalSoMe = ISNULL(OriginalSoMe, BKMIS_SoMe) 
                      WHERE ID = {2} AND ID_LoCao = {3}",
                                    grp.SoMe, ghiChu, it.Id, it.BfNo);
                            }
                            else
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    @"UPDATE Tbl_CanRayLG2 
                      SET BKMIS_SoMe = {0}, 
                          Ghi_Chu = {1}, 
                          SoMe_Cleared = 0, 
                          OriginalSoMe = ISNULL(OriginalSoMe, BKMIS_SoMe) 
                      WHERE ID = {2} AND BF_no = {3}",
                                    grp.SoMe, ghiChu, it.Id, it.BfNo);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await txn.CommitAsync();

                    return Ok(new { success = true, message = "Đã cập nhật khối lượng theo Số mẻ." });
                }
                catch (Exception ex)
                {
                    await txn.RollbackAsync();
                    return StatusCode(500, "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> ClearSoMe([FromBody] ClearSoMeRequest request)
        {
            if (request.CanRayId <= 0 || request.ID_LoCao <= 0)
            {
                return BadRequest("Thiếu thông tin CanRayId hoặc ID_LoCao.");
            }

            try
            {
                // Lò 1-4: Update Tbl_CanRayLG1
                if (request.ID_LoCao >= 1 && request.ID_LoCao <= 4)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE Tbl_CanRayLG1 
                  SET BKMIS_SoMe = NULL, 
                      OriginalSoMe = ISNULL(OriginalSoMe, BKMIS_SoMe),
                      Ghi_Chu = NULL, 
                      SoMe_Cleared = 1 
                  WHERE ID = {0} AND ID_LoCao = {1}",
                        request.CanRayId, request.ID_LoCao);
                }
                // Lò 5-6: Update Tbl_CanRayLG2
                else if (request.ID_LoCao == 5 || request.ID_LoCao == 6)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        @"UPDATE Tbl_CanRayLG2 
                  SET BKMIS_SoMe = NULL, 
                      OriginalSoMe = ISNULL(OriginalSoMe, BKMIS_SoMe),
                      Ghi_Chu = NULL, 
                      SoMe_Cleared = 1 
                  WHERE ID = {0} AND BF_no = {1}",
                        request.CanRayId, request.ID_LoCao);
                }
                else
                {
                    return BadRequest("ID lò cao không hợp lệ.");
                }

                return Ok(new { success = true, message = "Đã xóa số mẻ thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa số mẻ: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddCanRayLG1([FromBody] CanRayLG1Dto dto)
        {
            if (dto == null || dto.ID_LoCao <= 0)
                return BadRequest("Thiếu/lỗi thông tin đầu vào");

            try
            {
                var entity = new Tbl_CanRayLG1
                {
                    ID_LoCao = dto.ID_LoCao,
                    Gio = DateTime.TryParse(dto.Gio, out var gioVal) ? gioVal : (DateTime?)null,
                    ThungSo = dto.ThungSo,
                    Ray = dto.Ray,
                    SanRaGang = dto.SanRaGang,
                    KL_Bi = dto.KL_Bi,
                    KL_Tong = dto.KL_Tong,
                    KL_Gang = dto.KL_Gang,
                    BKMIS_SoMe = dto.BKMIS_SoMe,
                    Ghi_Chu = dto.GhiChu,
                    SoMe_Cleared = null
                };
                _context.Tbl_CanRayLG1.Add(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã thêm mới dòng cân ray (LG1)!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi thêm mới: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCanRayLG2([FromBody] CanRayLG2Dto dto)
        {
            if (dto == null || dto.BF_no <= 0)
                return BadRequest("Thiếu/lỗi thông tin đầu vào");

            try
            {

                var entity = new Tbl_CanRayLG2
                {

                    BF_no = dto.BF_no,
                    Laddle_no = dto.ThungSo,
                    Shift = dto.Shift ?? 0,
                    BF_Timestap = DateTime.TryParse(dto.Gio, out var gioVal) ? gioVal : DateTime.Now,
                    Casthouse = dto.SanRaGang,
                    Weight_TARE = dto.KL_Bi ?? 0,
                    Weight_GROSS = dto.KL_Tong ?? 0,
                    Weight_NET = dto.KL_Gang ?? 0,
                    BKMIS_SoMe = dto.BKMIS_SoMe,
                    Ghi_Chu = dto.GhiChu,
                    Is_Nhap = true,
                    SoMe_Cleared = false,
                    OriginalSoMe = null
                };
                _context.Entry(entity).State = EntityState.Added;
                _context.Tbl_CanRayLG2.Add(entity);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Đã thêm mới dòng cân ray (LG2)!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi khi thêm mới: " + ex.Message);
            }
        }
    }

}

