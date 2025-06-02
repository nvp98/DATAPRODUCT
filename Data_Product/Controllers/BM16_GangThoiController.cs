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



        public async Task<IActionResult> Danhsachphieu()
        {
           var danhSachPhieu = await _context.Tbl_BM_16_Phieu.OrderByDescending(p => p.Ngay).ToListAsync();
            return View(danhSachPhieu);
        }
        [HttpGet]
        public IActionResult TaoPhieu()
        {
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
                var maPhieu = "PGL" + DateTime.Now.ToString("yyyyMMddHHmmss");

                var phieu = new Tbl_BM_16_Phieu
                {
                    MaPhieu = maPhieu,
                    Ngay = DateTime.Today,
                    ThoiGianTao = DateTime.Now,
                    ID_Locao = model.ID_Locao,
                    ID_Kip = model.ID_Kip,
                    ID_NguoiTao = idNhanVienTao
                };
               
                await _context.Tbl_BM_16_Phieu.AddAsync(phieu);
                await _context.SaveChangesAsync();

                foreach (var thung in model.DanhSachThung)
                {
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
                        Gio_NM = DateTime.Now.ToString("HH:mm"),
                        G_GhiChu = thung.G_GhiChu,
                        G_ID_TrangThai = 1,
                        NgayTao = DateTime.Now,
                        G_ID_NguoiLuu = idNhanVienTao,
                        ID_Locao = model.ID_Locao,
                        G_ID_Kip = model.ID_Kip,
                        T_ID_TrangThai = 2,
                        ID_TrangThai = 2
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

        public async Task<IActionResult> Edit_Demo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Mã phiếu không hợp lệ.");
            }

            var phieu = await _context.Tbl_BM_16_Phieu.FirstOrDefaultAsync(p => p.MaPhieu == id);
            if (phieu == null)
            {
                return NotFound("Không tìm thấy phiếu.");
            }

            var danhSachThung = await _context.Tbl_BM_16_GangLong
                .Where(t => t.MaPhieu == id)
                .OrderBy(t => t.BKMIS_ThungSo)
                .ToListAsync();

            ViewBag.MaPhieu = phieu.MaPhieu;
            ViewBag.Ngay = phieu.Ngay;
            ViewBag.ID_Kip = phieu.ID_Kip;
            ViewBag.ID_Locao = phieu.ID_Locao;
            ViewBag.ThoiGianTao = phieu.ThoiGianTao;

            return View(danhSachThung);
        }
    }
}
