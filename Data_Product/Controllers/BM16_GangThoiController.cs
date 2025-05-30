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

public class PhieuGiaoNhanViewModel
{
    public string MaPhieu { get; set; }
    public string Ngay { get; set; }
    public string Ca { get; set; }
    public string LoCao { get; set; }
    public string BoPhanTao { get; set; }
    public string NguoiTao { get; set; }
    public string ThoiGianTao { get; set; }
}
public class ThungGang
{
    [Key]
    [StringLength(20)]
    public string MaThung { get; set; }

    [StringLength(20)]
    public string MaPhieu { get; set; }

    [StringLength(20)]
    public string SoMe { get; set; }

    public int ThungSo { get; set; }

    public DateTime Gio { get; set; }

    [StringLength(20)]
    public string PhanLoai { get; set; }

    public int MaLoCao { get; set; }

    [Column(TypeName = "date")]
    public DateTime Ngay { get; set; }

    [StringLength(10)]
    public string MaCaKip { get; set; }

    [StringLength(10)]
    public string MaXeGoong { get; set; }

    public float KLThungVaGangLong { get; set; }

    public float KLGangLong { get; set; }

    [NotMapped]
    public float KLGang => KLGangLong - KLThungVaGangLong;

    [StringLength(50)]
    public string NoiDen { get; set; }

    public DateTime GioTaoPhieu { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = "ChuaChuyen";

    [StringLength(10)]
    public string MaNhanVienTao { get; set; }

    [StringLength(10)]
    public string BoPhan { get; set; }

    [StringLength(10)]
    public string MaNhanVienNhan { get; set; }

    public DateTime? ThoiGianNhan { get; set; }

    [StringLength(10)]
    public string MaLoThoi { get; set; }

    [StringLength(20)]
    public string MaMeThoi { get; set; }

    [StringLength(10)]
    public string NguoiChot { get; set; }

    [StringLength(10)]
    public string BoPhanChot { get; set; }

    public DateTime? ThoiGianChot { get; set; }
}
public class Phieu
{
    [Key]
    [StringLength(20)]
    public string MaPhieu { get; set; }

    [Column(TypeName = "date")]
    public DateTime Ngay { get; set; }

    [StringLength(10)]
    public string MaCaKip { get; set; }

    [StringLength(10)]
    public string MaTo { get; set; }

    [StringLength(10)]
    public string MaLoCao { get; set; }

    [StringLength(10)]
    public string NguoiLap { get; set; }

    public DateTime GioTao { get; set; }

    [StringLength(20)]
    public string TrangThai { get; set; } = "ChuaHoanTat";

    [StringLength(10)]
    public string NguoiChot { get; set; }

    public DateTime? ThoiGianChot { get; set; }

    [StringLength(10)]
    public string BoPhanTao { get; set; }

    [StringLength(10)]
    public string BoPhanChot { get; set; }

    // Navigation property - Danh sách các thùng gang thuộc phiếu này
    public virtual ICollection<ThungGang> DanhSachThungGang { get; set; }
}
namespace Data_Product.Controllers
{
    
    public class BM16_GangThoiController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public BM16_GangThoiController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }

        public async Task<IActionResult> GetSoMeGangBKMis(string ngay, int? ID_LoCao, string IDKip)
        {
            string cakip = "";
            if (IDKip != null)
            {
                var dt = DateTime.Parse(ngay);
                var ca = _context.Tbl_Kip.FirstOrDefault(x => x.TenCa == IDKip && x.NgayLamViec == dt);
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
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc1.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 3)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                        "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc3 " +
                        "where bkmis_kcshpsdq.view_dq1_lg_daura_lc3.ProductionDate = '" +
                         ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc1.ShiftName ='" + cakip + "'";
                    }
                    else if (ID_LoCao == 4)
                    {
                        query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                       "FROM bkmis_kcshpsdq.view_dq1_lg_daura_lc4 " +
                       "where bkmis_kcshpsdq.view_dq1_lg_daura_lc4.ProductionDate = '" +
                        ngay + "'" + " and bkmis_kcshpsdq.view_dq1_lg_daura_lc1.ShiftName ='" + cakip + "'";
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
            DateTime day_datetime = (DateTime) Ngay;
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


        public IActionResult Edit_Demo()
        {
            return View(); // Sẽ tìm view: Views/BM16_GangThoi/Edit_Demo.cshtml
        }

        public async Task<IActionResult>Danhsachphieu()
        {
            var danhSachPhieu = new List<PhieuGiaoNhanViewModel>
            {
                new PhieuGiaoNhanViewModel { MaPhieu = "GN001", Ngay = "25/05/2025", Ca = "Ca ngày", LoCao = "Lò 3", BoPhanTao = "Phân xưởng Gang", NguoiTao = "Nguyễn Văn A", ThoiGianTao = "08:30:15" },
                new PhieuGiaoNhanViewModel { MaPhieu = "GN002", Ngay = "25/05/2025", Ca = "Ca ngày", LoCao = "Lò 2", BoPhanTao = "Phân xưởng Thép", NguoiTao = "Trần Thị B", ThoiGianTao = "08:45:20" },
                new PhieuGiaoNhanViewModel { MaPhieu = "GN003", Ngay = "24/05/2025", Ca = "Ca ngày", LoCao = "Lò 1", BoPhanTao = "Phân xưởng Gang", NguoiTao = "Phạm Văn C", ThoiGianTao = "21:10:50" },
                new PhieuGiaoNhanViewModel { MaPhieu = "GN004", Ngay = "23/05/2025", Ca = "Ca đêm", LoCao = "Lò 3", BoPhanTao = "Phân xưởng Thép", NguoiTao = "Đặng Thị D", ThoiGianTao = "14:05:12" },
                new PhieuGiaoNhanViewModel { MaPhieu = "GN005", Ngay = "22/05/2025", Ca = "Ca ngày", LoCao = "Lò 2", BoPhanTao = "Phân xưởng Gang", NguoiTao = "Hoàng Văn E", ThoiGianTao = "07:55:33" },
            };
            return View(danhSachPhieu);
        }

        public async Task<IActionResult> TaoPhieu()
        {
            return View();
        }
    }
}
