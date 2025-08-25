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


namespace Data_Product.Controllers
{

    public class BM16_GangThoiController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ILogger<BM16_GangThoiController> _logger;
        private readonly IChiaGangService _chiaGangService;
        public BM16_GangThoiController(DataContext _context, ICompositeViewEngine viewEngine, ILogger<BM16_GangThoiController> logger, IChiaGangService chiaGangService)
        {
            this._context = _context;
            this._chiaGangService = chiaGangService;
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
                    else if (ID_LoCao == 5)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                       "FROM bkmis_kcshpsdq.view_dq2_kqganglocao " +
                       "where bkmis_kcshpsdq.view_dq2_kqganglocao.ProductionDate = '" +
                        ngay + "'" + " and bkmis_kcshpsdq.view_dq2_kqganglocao.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 6)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
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
                .Where(x => x.NgayLamViec.HasValue &&x.NgayLamViec.Value.Date == ngayLamViec && x.TenCa == tenCa)
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
        public async Task<IActionResult> Danhsachphieu(string maPhieu, DateTime? ngay, DateTime? ngaypg, string ca,string locao, int page = 1)
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
                    NgayPhieuGang =p.NgayPhieuGang,
                    ThoiGianTao = p.ThoiGianTao.ToString("HH:mm:ss"),
                    ID_LoCao = p.ID_Locao,
                    TenNguoiTao = _context.Tbl_TaiKhoan
                                    .Where(tk => tk.ID_TaiKhoan == p.ID_NguoiTao)
                                    .Select(tk => tk.TenTaiKhoan + " "+ "-"+" "+ tk.HoVaTen ).FirstOrDefault(),
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
                .Where(t => t.MaPhieu == id && t.T_copy == false )
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

                // Dùng để sort:
                //MaThungPrefix = t.MaThungGang.Split('.')[0],
                //MaThungSuffix = int.Parse(t.MaThungGang.Split('.')[1])
                GioSortKey = GetSortKeyFromTime(t.BKMIS_Gio)
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
                    x.XacNhan
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
                        NgayLuyenGang = DateTime.Now
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

                //  Xử lý trạng thái chuyển đến
                thung.T_ID_TrangThai = (chuyenDen == "DUC1" || chuyenDen == "DUC2") ? 4 : thung.T_ID_TrangThai;

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
        public async Task <IActionResult> CapNhatThung([FromBody] CapNhatRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.MaPhieu) || req.DsMaThung == null || !req.DsMaThung.Any())
            {
                return BadRequest("Danh sách cập nhật rỗng.");
            }
            //ChuaXuLy = 1, ChoXuLy = 2, DaXuLy = 3, DaNhan = 4, DaChot = 5
            var danhSachTrung = new List<string>();
            foreach (var item in req.DsMaThung)
            {
                // check duplicate 
                var isDuplicateSoMe = _context.Tbl_BM_16_GangLong.Any(x =>
                                    x.MaPhieu == req.MaPhieu &&
                                    x.BKMIS_SoMe == item.BKMIS_SoMe &&
                                    x.MaThungGang != item.MaThungGang);

                if (isDuplicateSoMe)
                {
                    danhSachTrung.Add(item.BKMIS_SoMe);
                    continue;
                }
                var thung = _context.Tbl_BM_16_GangLong
                    .FirstOrDefault(x => x.MaPhieu == req.MaPhieu 
                        && x.MaThungGang == item.MaThungGang 
                        && x.BKMIS_SoMe == item.BKMIS_SoMe
                        && (x.G_ID_TrangThai == 1 || x.G_ID_TrangThai == 3)
                        && x.XacNhan == false
                        && ( x.T_ID_TrangThai == 2 || x.T_ID_TrangThai == 4)
                        &&  x.ID_TrangThai == 2 ) ;

                if (thung != null)
                {
                    thung.BKMIS_PhanLoai = item.BKMIS_PhanLoai;
                    thung.BKMIS_Gio = item.BKMIS_Gio;
                    //thung.BKMIS_SoMe = item.BKMIS_SoMe;
                    thung.BKMIS_ThungSo = !string.IsNullOrEmpty(item.BKMIS_SoMe) && item.BKMIS_SoMe.Length >= 2
                    ? item.BKMIS_SoMe.Substring(item.BKMIS_SoMe.Length - 2): null;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> XoaNhungSoMeKhongConTrongBK([FromBody] XoaSoMeRequest req)
        {
            if (req == null || string.IsNullOrEmpty(req.MaPhieu) ||req.SoMes == null || !req.SoMes.Any())
            {
                return BadRequest("Danh sách số mẻ cần xóa bị trống.");
            }
            //ChuaXuLy = 1, ChoXuLy = 2, DaXuLy = 3, DaNhan = 4, DaChot = 5
            try
            {
                var allThung = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.MaPhieu == req.MaPhieu && req.SoMes.Contains(x.BKMIS_SoMe))
                    .ToListAsync();

                // Những thùng được phép xóa
                var thungCanXoa = allThung
                  .Where(x => (x.XacNhan == false || x.XacNhan == null)
                  && (x.G_ID_TrangThai == 1 || x.G_ID_TrangThai == 3)
                  && x.T_ID_TrangThai == 2
                  && x.ID_TrangThai == 2
                  )
                  .ToList();

                // Những số mẻ không được xóa (đã xác nhận hoặc trạng thái khác)
                var soMesKhongXoaDuoc = allThung
                    .Where(x => !thungCanXoa.Contains(x))
                    .Select(x => x.BKMIS_SoMe)
                    .Distinct()
                    .ToList();

                if (thungCanXoa.Any())
                {
                    _context.Tbl_BM_16_GangLong.RemoveRange(thungCanXoa);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = thungCanXoa.Any() ? "Đã xóa thành công." : "Không có thùng nào cần xóa.",
                    thungdaxoa = thungCanXoa
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi xóa.", error = ex.Message });
            }
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
                    .Where(t => t.MaPhieu == req.MaPhieu && maThungList.Contains(t.MaThungGang) && t.T_copy == false)
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
            catch(Exception ex)
            {
                return BadRequest(new {message ="Lỗi khi xác nhận thùng", error = ex.Message});
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
                    .Where(t => t.MaPhieu == req.MaPhieu && maThungList.Contains(t.MaThungGang) && t.T_copy == false)
                    .ToListAsync();

                if (!thungs.Any())
                    return BadRequest(new { success = false, message = "Không tìm thấy thùng nào" });

                // Thùng chưa xác nhận
                var thungChuaXacNhan = thungs.Where(t => t.XacNhan ==false).ToList();
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
        [HttpGet]
        public IActionResult GetLastThungIndex(string maPhieu)
        {
            var lastStt = _context.Tbl_BM_16_GangLong
                .Where(x => x.MaPhieu == maPhieu)
                .Select(x => x.MaThungGang)
                .ToList()
                .Select(ma =>
                {
                    var match = Regex.Match(ma ?? "", @"(\d{3})\.00$");
                    return match.Success ? int.Parse(match.Groups[1].Value) : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return Ok(new { lastStt });
        }

        [HttpGet]
        public async Task<IActionResult> GetSoMeDaCoTrongPhieu(string maPhieu)
        {
            var list = await _context.Tbl_BM_16_GangLong
                .Where(x => x.MaPhieu == maPhieu)
                .Select(x => new { soMe = x.BKMIS_SoMe, maThung = x.MaThungGang })
                .ToListAsync();

            return Json(new
            {
                success = true,
                soMes = list.Select(x => x.soMe).ToList(),
                data = list
            });
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
                        var usedRange = worksheet.Range($"A9:R{sumRow}");
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
        [HttpPost]
        public async Task<IActionResult> ExportToPDF([FromBody]string MaPhieu)
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
                                     where gl.MaPhieu == MaPhieu //&& gl.MaThungGang
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
                                       where gl.MaPhieu == MaPhieu //&& gl.MaThungGang
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
                                     where tkThung.MaPhieu == MaPhieu //&& tkThung.MaThungGang
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


    }
}
