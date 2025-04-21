using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BMController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public BMController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public async Task<IActionResult> Index()
        {
            DateTime today = DateTime.Today;
            DateTime yesterday = today.AddDays(-1);
          
            ViewBag.DefaultDate = today.ToString("yyyy-MM-dd");
            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);


            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            string query = "SELECT ID_Kip, TenCa FROM Tbl_Kip WHERE NgayLamViec = @NgayLamViec";
            var CaKip = _context.Tbl_Kip
                .FromSqlRaw(query, new SqlParameter("@NgayLamViec", NgayLamViec))
                .Select(a => new Tbl_Kip
                {
                    ID_Kip = a.ID_Kip,
                    TenCa = a.TenCa
                })
                .ToList();

            // Gán vào ViewBag
            ViewBag.IDKip = new SelectList(CaKip, "ID_Kip", "TenCa");

            var NhanVien = (from a in _context.Tbl_TaiKhoan
                            select new Tbl_TaiKhoan
                            {
                                ID_TaiKhoan = a.ID_TaiKhoan,
                                HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                            }).ToList();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen");
            var TaiKhoan_QLCL = (from a in _context.Tbl_TaiKhoan
                            select new Tbl_TaiKhoan
                            {
                                ID_TaiKhoan = a.ID_TaiKhoan,
                                HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                            }).ToList();

            ViewBag.IDTaiKhoan = new SelectList(TaiKhoan_QLCL, "ID_TaiKhoan", "HoVaTen");

            var NhanVien_TT = await(from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong)
                                    join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                    select new Tbl_TaiKhoan
                                    {
                                        ID_TaiKhoan = a.ID_TaiKhoan,
                                        HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                    }).ToListAsync();

            ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");
            ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");

            return PartialView(); 
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

        public async Task<IActionResult> ChucVu(int IDTaiKhoan)
        {
            if (IDTaiKhoan == null) IDTaiKhoan = 0;

            var NhanVien = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan)
                                  join cv in _context.Tbl_ViTri on a.ID_ChucVu equals cv.ID_ViTri
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      ID_ChucVu = a.ID_ChucVu,
                                      TenChucVu = cv.TenViTri
                                  }).ToListAsync();

    
            return Json(NhanVien);
        }
        public async Task<IActionResult> PhongBan(int IDTaiKhoan)
        {
            if (IDTaiKhoan == null) IDTaiKhoan = 0;

            var NhanVien = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan)
                                  join cv in _context.Tbl_PhongBan on a.ID_PhongBan equals cv.ID_PhongBan
                                  join px in _context.Tbl_Xuong on a.ID_PhanXuong equals px.ID_Xuong
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      ID_PhongBan = a.ID_PhongBan,
                                      TenPhongBan = cv.TenPhongBan + " - " + px.TenXuong,
                                      ID_PhanXuong = a.ID_PhanXuong
                                  }).ToListAsync();

            return Json(NhanVien);
        }
        public async Task<IActionResult> NguyenLieu(int IDTaiKhoan)
        {
            if (IDTaiKhoan == null) IDTaiKhoan = 0;
            var ID = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();
            string ID_PhongBan = ID.ID_PhongBan.ToString();

            var NguyenLieu = await (from a in _context.Tbl_VatTu.Where(x => x.ID_TrangThai == 1)
                                    select new Tbl_VatTu
                                    {
                                        ID_VatTu = a.ID_VatTu,
                                        PhongBan = a.PhongBan,
                                        TenVatTu = a.TenVatTu
                                    }).ToListAsync();
            if (ID_PhongBan != "")
            {
                NguyenLieu = NguyenLieu.Where(x => x.PhongBan.Contains(ID_PhongBan)).ToList();
            }
            return Json(NguyenLieu);
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
        [HttpPost]
        public IActionResult SubmitData_BBGangLong_GangThoi([FromBody] Tbl_BBGangLong_GangThoi res)
        {
            int idsert = 0;
            string iddate = "";
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new { success = false, errors });
            }
            var MBVN_BG = User.FindFirstValue(ClaimTypes.Name);
            var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MBVN_BG).FirstOrDefault();
            var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();
            int Kip = Convert.ToInt32(res.Ca); // 1 ngày 2 đêm
                                               //var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == Kip).FirstOrDefault();
            var ID_Kip = _context.Tbl_Kip.Where(x => x.TenCa == res.Ca && x.NgayLamViec==res.NgayXuly_BG).FirstOrDefault();
            string SoPhieu = ThongTin_BP_BG.TenNgan + "-" + ID_Kip.TenCa + ID_Kip.TenKip + "-" +
                                         res.NgayXuly_BG?.Date.ToString("yyyy-MM-dd") ?? "";
            var bs = new Tbl_TrinhKyBoSung();
            bs.ID_BBGN = res.ID_BBGL;
            bs.ID_TaiKhoan = res.ID_TaiKhoan;
            bs.ID_TaiKhoan_View = res.ID_TaiKhoan_View;
            bs.NgayTrinhKy = DateTime.Now;       
            var data = new Tbl_BBGangLong_GangThoi();
            data.ID_NhanVien_BG = res.ID_NhanVien_BG;
            data.ID_PhongBan_BG = res.ID_PhongBan_BG;
            data.ID_ViTri_BG = res.ID_ViTri_BG;
            data.ID_Xuong_BG = res.ID_Xuong_BG;

            data.ID_NhanVien_HRC = res.ID_NhanVien_HRC;
            data.ID_PhongBan_HRC = res.ID_PhongBan_HRC;
            data.ID_ViTri_HRC = res.ID_ViTri_HRC;
            data.ID_Xuong_HRC = res.ID_Xuong_HRC;

            data.ID_NhanVien_QLCL = res.ID_NhanVien_QLCL;
            data.ID_PhongBan_QLCL = res.ID_PhongBan_QLCL;
            data.ID_Xuong_QLCL = res.ID_Xuong_QLCL;
            data.ID_ViTri_QLCL = res.ID_ViTri_QLCL;
            data.SoPhieu = SoPhieu;
            data.NgayXuly_BG = res.NgayXuly_BG;
            data.ID_Kip = res.ID_Kip;
            data.Kip = res.Kip;
            data.Ca = res.Ca;
            data.ID_LOCAO = res.ID_LOCAO;
            data.NoiDungTrichYeu = res.NoiDungTrichYeu;
            data.ID_QuyTrinh = 1;
            data.TinhTrang_BBGN = 0;
            data.TinhTrang_HRC = 0;
            data.TinhTrang_QLCL = 0;
            HttpContext.Session.SetString("ID_LoCao", data.ID_LOCAO.ToString());

            var ID_BBGL = idsert;
            var dataInDb = _context.Tbl_BBGangLong_GangThoi.Find(ID_BBGL);

            if (dataInDb == null)
            {
                _context.Tbl_BBGangLong_GangThoi.Add(data);
            }
            else
            {
                dataInDb.ID_NhanVien_BG = res.ID_NhanVien_BG;
                dataInDb.ID_PhongBan_BG = res.ID_PhongBan_BG;
                dataInDb.ID_ViTri_BG = res.ID_ViTri_BG;
                dataInDb.ID_Xuong_BG = res.ID_Xuong_BG;

                dataInDb.ID_NhanVien_HRC = res.ID_NhanVien_HRC;
                dataInDb.ID_PhongBan_HRC = res.ID_PhongBan_HRC;
                dataInDb.ID_ViTri_HRC = res.ID_ViTri_HRC;
                dataInDb.ID_Xuong_HRC = res.ID_Xuong_HRC;

                dataInDb.ID_NhanVien_QLCL = res.ID_NhanVien_QLCL;
                dataInDb.ID_PhongBan_QLCL = res.ID_PhongBan_QLCL;
                dataInDb.ID_Xuong_QLCL = res.ID_Xuong_QLCL;
                dataInDb.ID_ViTri_QLCL = res.ID_ViTri_QLCL;

                dataInDb.NgayXuly_BG = res.NgayXuly_BG;
                dataInDb.ID_Kip = res.ID_Kip;
                dataInDb.Kip = res.Kip;
                dataInDb.Ca = res.Ca;
                data.ID_QuyTrinh = 1;
                data.TinhTrang_BBGN = 0;
                data.TinhTrang_HRC = 0;
                data.TinhTrang_QLCL = 0;
                data.SoPhieu = SoPhieu;
                dataInDb.NoiDungTrichYeu = res.NoiDungTrichYeu;
            }

            _context.SaveChanges();

            // Tạo đối tượng trả về chứa tất cả dữ liệu cần thiết
            var responseData = new
            {
                success = true,
                Id_BBGL = data.ID_BBGL,
                NgayXuly_BG = data.NgayXuly_BG?.Date.ToString("yyyy-MM-dd") ?? "",
                NoiDungTrichYeu = data.NoiDungTrichYeu,
                IDCa = data.Ca.ToString() ?? "",
                Kip = data.Kip.ToString() ?? "",
                IDKip = data.ID_Kip.ToString() ?? "",
                ID_BBGL_Edit = data.ID_BBGL
            };
            idsert = responseData.Id_BBGL;
            iddate = responseData.NgayXuly_BG;
            return Json(new { success = true, Id_BBGL = idsert, NgayXuly_BG = iddate });
        }

        [HttpPost]
        public IActionResult SubmitData([FromBody] List<Tbl_ChiTiet_BBGangLong_GangThoi> listData)
        {
            if (listData == null || listData.Count == 0)
            {
                return Json(new { success = false, message = "Error!" });
            }

            //HttpContext.Session.SetString("ListData", JsonConvert.SerializeObject(listData));

            foreach (var item in listData)
            {
                 
                var existingData = _context.Tbl_ChiTiet_BBGangLong_GangThoi
                    .Where(x => x.Id_BBGL == item.Id_BBGL && x.ID == item.ID).SingleOrDefault();

                if (existingData != null)
                {
                    existingData.SoMe = item.SoMe;
                    existingData.ThungSo = item.ThungSo;
                    existingData.KhoiLuongXeGoong = item.KhoiLuongXeGoong;
                    existingData.KhoiLuongThung = item.KhoiLuongThung;
                    existingData.KLThungGangLong = item.KLThungGangLong;
                    existingData.KLGangLongCanRay = item.KLGangLongCanRay;
                    existingData.VanChuyenHRC1 = item.VanChuyenHRC1;
                    existingData.VanChuyenHRC2 = item.VanChuyenHRC2;
                    existingData.PhanLoai = item.PhanLoai;
                    existingData.GhiChu = item.GhiChu;
                }
                else
                {
                    var newData = new Tbl_ChiTiet_BBGangLong_GangThoi
                    {
                        SoMe = item.SoMe,
                        ThungSo = item.ThungSo,
                        KhoiLuongXeGoong = item.KhoiLuongXeGoong,
                        KhoiLuongThung = item.KhoiLuongThung,
                        KLThungGangLong = item.KLThungGangLong,
                        KLGangLongCanRay = item.KLGangLongCanRay,
                        PhanLoai = item.PhanLoai,
                        VanChuyenHRC1 =item.VanChuyenHRC1,
                        VanChuyenHRC2 = item.VanChuyenHRC2,
                        GhiChu = item.GhiChu,
                        Id_BBGL = item.Id_BBGL
                    };
                    _context.Tbl_ChiTiet_BBGangLong_GangThoi.Add(newData);
                }
            }

            _context.SaveChanges();

            //HttpContext.Session.Remove("ID_BBGL");

            return Json(new { success = true });
        }
        public async Task<IActionResult> Edit(int id)
        {

            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            List<Tbl_NhomVatTu> vt = _context.Tbl_NhomVatTu.ToList();
            ViewBag.VTList = new SelectList(vt, "ID_NhomVatTu", "TenNhomVatTu");


            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            var NhanVien = (from a in _context.Tbl_TaiKhoan
                            select new Tbl_TaiKhoan
                            {
                                ID_TaiKhoan = a.ID_TaiKhoan,
                                HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                            }).ToList();
            var IDTK_HRC = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == id).FirstOrDefault().ID_NhanVien_HRC;
            var IDTK_QLCL = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == id).FirstOrDefault().ID_NhanVien_QLCL;
          
            ViewBag.IDTaiKhoan_HRC = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", IDTK_HRC);
            ViewBag.IDTaiKhoan_QLCL = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", IDTK_QLCL);

            var VatTu = (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP) && x.ID_TrangThai == 1)
                         select new Tbl_VatTu
                         {
                             ID_VatTu = a.ID_VatTu,
                             TenVatTu = a.TenVatTu
                         }).ToList();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");

            var MaLo = (from a in _context.Tbl_MaLo
                        select new Tbl_MaLo
                        {
                            ID_MaLo = a.ID_MaLo,
                            TenMaLo = a.TenMaLo
                        }).ToList();

            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");

            var CaKip = (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == NgayLamViec)
                         select new Tbl_Kip
                         {
                             ID_Kip = a.ID_Kip,
                             TenCa = a.TenCa
                         }).ToList();
            ViewBag.IDKip = new SelectList(CaKip, "ID_Kip", "TenCa");



                var NV = (from a in _context.Tbl_TaiKhoan
                          select new Tbl_TaiKhoan
                          {
                              ID_TaiKhoan = a.ID_TaiKhoan,
                              HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                          }).ToList();

                ViewBag.IDNhanVien = new SelectList(NV, "ID_TaiKhoan", "HoVaTen");

            DateTime today = DateTime.Today;
            DateTime yesterday = today.AddDays(-1);

           
            ViewBag.DefaultDate = today.ToString("yyyy-MM-dd"); // Giá trị mặc định

              var listData = await _context.Tbl_ChiTiet_BBGangLong_GangThoi
             .Where(x => x.Id_BBGL == id)
             .ToListAsync();

            var dataInDb = await _context.Tbl_BBGangLong_GangThoi
                .FirstOrDefaultAsync(x => x.ID_BBGL == id);

            if (dataInDb == null)
            {
                return NotFound(); // Xử lý trường hợp không tìm thấy dữ liệu
            }

            // Lấy các giá trị trực tiếp từ dataInDb thay vì từ Session
            var ngayXuly_BG = dataInDb.NgayXuly_BG?.ToString("yyyy-MM-dd") ?? string.Empty;
            var idLoCao = HttpContext.Session.GetString("ID_LoCao") ?? " ";
            var idCa = dataInDb.Ca ?? string.Empty;
            var kip = dataInDb.Kip ?? string.Empty;
            var IDKip = dataInDb.ID_Kip?.ToString() ?? string.Empty;
            var NoiDungTrichYeu = dataInDb.NoiDungTrichYeu ?? string.Empty;

            // Truyền dữ liệu vào ViewBag
            ViewBag.NgayXuly_BG = ngayXuly_BG;
            ViewBag.ID_LoCao = idLoCao;
            ViewBag.ID_Ca = idCa;
            ViewBag.KipLamViec = kip;
            ViewBag.IDKip = IDKip;
            ViewBag.Data = id;
            ViewBag.NoiDungTrichYeu = NoiDungTrichYeu;

            return View(listData);

        }
        [HttpPost]
        public IActionResult SubmitData_BBGangLong_GangThoi_Edit([FromBody] Tbl_BBGangLong_GangThoi res)
        {
            
            var dataInDb = _context.Tbl_BBGangLong_GangThoi
                .Where(x => x.ID_BBGL == res.ID_BBGL)
                .FirstOrDefault();

            if (dataInDb == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản ghi cần chỉnh sửa" });
            }

            // Cập nhật các trường dữ liệu
            dataInDb.ID_NhanVien_BG = res.ID_NhanVien_BG;   
            dataInDb.ID_PhongBan_BG = res.ID_PhongBan_BG;
            dataInDb.ID_ViTri_BG = res.ID_ViTri_BG;
            dataInDb.ID_Xuong_BG = res.ID_Xuong_BG;

            dataInDb.ID_NhanVien_HRC = res.ID_NhanVien_HRC;
            dataInDb.ID_PhongBan_HRC = res.ID_PhongBan_HRC;
            dataInDb.ID_ViTri_HRC = res.ID_ViTri_HRC;
            dataInDb.ID_Xuong_HRC = res.ID_Xuong_HRC;

            dataInDb.ID_NhanVien_QLCL = res.ID_NhanVien_QLCL;
            dataInDb.ID_PhongBan_QLCL = res.ID_PhongBan_QLCL;
            dataInDb.ID_Xuong_QLCL = res.ID_Xuong_QLCL;
            dataInDb.ID_ViTri_QLCL = res.ID_ViTri_QLCL;

            dataInDb.NgayXuly_BG = res.NgayXuly_BG;
            dataInDb.ID_Kip = res.ID_Kip;
            dataInDb.Kip = res.Kip;
            dataInDb.Ca = res.Ca;

            dataInDb.NoiDungTrichYeu = res.NoiDungTrichYeu;

            _context.SaveChanges();

           
            return Json(new
            {
                success = true,
                Id_BBGL = dataInDb.ID_BBGL,
                NoiDungTrichYeu = res.NoiDungTrichYeu
            });
        }
       
        [HttpPost]
        public IActionResult SubmitData_Edit([FromBody] List<Tbl_ChiTiet_BBGangLong_GangThoi> listData)
        {
            //var test = HttpContext.Session.GetString("ComplexObject");
            if (listData == null || listData.Count == 0)
            {
                return Json(new { success = false, message = "Error!" });
            }

            HttpContext.Session.SetString("ListData_Edit", JsonConvert.SerializeObject(listData));

            var idBBGLs = listData.Select(x => x.Id_BBGL).Distinct().ToList();

            var dbRecords = _context.Tbl_ChiTiet_BBGangLong_GangThoi
                .Where(x => idBBGLs.Contains(x.Id_BBGL))
                .ToList();

            var itemsToDelete = dbRecords
                .Where(dbItem => !listData.Any(item => item.Id_BBGL == dbItem.Id_BBGL && item.SoMe == dbItem.SoMe))
                .ToList();

            _context.Tbl_ChiTiet_BBGangLong_GangThoi.RemoveRange(itemsToDelete);


            foreach (var item in listData)
            {
                if (item.ID != 0)
                {
                    var existingData = _context.Tbl_ChiTiet_BBGangLong_GangThoi
                        .Where(x => x.Id_BBGL == item.Id_BBGL && x.ID == item.ID).SingleOrDefault();

                    if (existingData != null)
                    {
                        existingData.SoMe = item.SoMe;
                        existingData.ThungSo = item.ThungSo;
                        existingData.KhoiLuongXeGoong = item.KhoiLuongXeGoong;
                        existingData.KhoiLuongThung = item.KhoiLuongThung;
                        existingData.KLThungGangLong = item.KLThungGangLong;
                        existingData.KLGangLongCanRay = item.KLGangLongCanRay;
                        existingData.PhanLoai = item.PhanLoai;
                        existingData.GhiChu = item.GhiChu;
                    }
                }
                else
                {
                    var newData = new Tbl_ChiTiet_BBGangLong_GangThoi
                    {
                        SoMe = item.SoMe,
                        ThungSo = item.ThungSo,
                        KhoiLuongXeGoong = item.KhoiLuongXeGoong,
                        KhoiLuongThung = item.KhoiLuongThung,
                        KLThungGangLong = item.KLThungGangLong,
                        KLGangLongCanRay = item.KLGangLongCanRay,
                        PhanLoai = item.PhanLoai,
                        GhiChu = item.GhiChu,
                        Id_BBGL = item.Id_BBGL
                    };
                    _context.Tbl_ChiTiet_BBGangLong_GangThoi.Add(newData);
                }

            }

            _context.SaveChanges();

            HttpContext.Session.Remove("ListData");
            HttpContext.Session.Remove("NoiDungTrichYeu");

            return Json(new { success = true });
        }
        public async Task<IActionResult> Detail(int id)
        {

            var jsonData = _context.Tbl_ChiTiet_BBGangLong_GangThoi.Where(x=>x.Id_BBGL==id).ToList();

            List<Tbl_ChiTiet_BBGangLong_GangThoi> listData = jsonData;
            var jsondata1 = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == id).FirstOrDefault();
            var idLoCao = HttpContext.Session.GetString("ID_LoCao") ?? " ";
            var NoiDungTrichYeu = jsondata1.NoiDungTrichYeu;
            if (string.IsNullOrEmpty(NoiDungTrichYeu))
            {
                NoiDungTrichYeu = "";
            }

            ViewBag.ID_LoCao = idLoCao;
            ViewBag.Data = id;
            ViewBag.NoiDungTrichYeu = NoiDungTrichYeu;
           // HttpContext.Session.Remove("NoiDungTrichYeu_Edit");

            return View(listData);
        }

        public async Task<IActionResult> Index_detail(int id)
        {
            var res = await (from a in _context.Tbl_ChiTiet_BBGangLong_GangThoi.Where(x => x.Id_BBGL == id)
                             
                             select new Tbl_ChiTiet_BBGangLong_GangThoi
                             {
                                 Id_BBGL = a.Id_BBGL,
                                 SoMe=a.SoMe,
                                 ThungSo=a.ThungSo,
                                 KhoiLuongXeGoong=a.KhoiLuongXeGoong,
                                 KhoiLuongThung=a.KhoiLuongThung,
                                 KLThungGangLong=a.KLThungGangLong,
                                 KLGangLongCanRay=a.KLGangLongCanRay,
                                 VanChuyenHRC1=a.VanChuyenHRC1,
                                 VanChuyenHRC2=a.VanChuyenHRC2,
                                 
                                 PhanLoai=a.PhanLoai,
                                 Gio=a.Gio,
                                 GhiChu=a.GhiChu
                             }).ToListAsync();
            ViewBag.Data = id;
            return View(res);
        }
        public async Task<IActionResult> Index_Started(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = Now;
            //DateTime startDay = Now.AddDays(-1);
            //DateTime endDay = Now;
            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BN = TaiKhoan.ID_TaiKhoan;

            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai_PheDuyet.ToList(), "ID_TrangThai_PheDuyet", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_NhanVien_BG == ID_NhanVien_BN)
                             select new Tbl_BBGangLong_GangThoi
                             {
                                 ID_BBGL = a.ID_BBGL,
                                 NoiDungTrichYeu = a.NoiDungTrichYeu,
                                 ID_NhanVien_BG = a.ID_NhanVien_BG,
                                 ID_PhongBan_BG = a.ID_PhongBan_BG,
                                 ID_Xuong_BG = a.ID_Xuong_BG,
                                 ID_ViTri_BG = a.ID_ViTri_BG,

                                 ID_NhanVien_HRC = a.ID_NhanVien_HRC,
                                 ID_PhongBan_HRC = a.ID_PhongBan_HRC,
                                 ID_Xuong_HRC = a.ID_Xuong_HRC,
                                 ID_ViTri_HRC = a.ID_ViTri_HRC,

                                 ID_NhanVien_QLCL = a.ID_NhanVien_QLCL,
                                 ID_PhongBan_QLCL = a.ID_PhongBan_QLCL,
                                 ID_Xuong_QLCL = a.ID_Xuong_QLCL,
                                 ID_ViTri_QLCL = a.ID_ViTri_QLCL,

                                 TinhTrang_BG = a.TinhTrang_BG,
                                 TinhTrang_HRC = a.TinhTrang_HRC,
                                 TinhTrang_QLCL = a.TinhTrang_QLCL,
                                 TinhTrang_BBGN = a.TinhTrang_BBGN,

                                 NgayXuly_BG = a.NgayXuly_BG,
                                 NgayXuly_HRC = a.NgayXuly_HRC,
                                 NgayXuly_QLCL = a.NgayXuly_QLCL,
                                 SoPhieu = a.SoPhieu,

                                 ID_Kip = a.ID_Kip,
                                 Kip = a.Kip,
                                 Ca = a.Ca,
                                 IDBBGL_Cu = a.IDBBGL_Cu,
                                 //NoiDungTrichYeu =a.NoiDungTrichYeu,

                                 ID_QuyTrinh = a.ID_QuyTrinh,
                             }).OrderBy(x => x.ID_BBGL).ToListAsync();


            if (ID_TrangThai != null) res = res.Where(x => x.TinhTrang_BBGN == ID_TrangThai).ToList();
            if (begind != null && endd != null) res = res.Where(x => x.NgayXuly_BG >= startDay && x.NgayXuly_BG <= endDay).ToList();

            const int pageSize = 1000;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }
        public async Task<IActionResult> confirm_details(int id)
        {
            var res = await (from a in _context.Tbl_ChiTiet_BBGangLong_GangThoi.Where(x => x.Id_BBGL == id)

                             select new Tbl_ChiTiet_BBGangLong_GangThoi
                             {
                                 Id_BBGL = a.Id_BBGL,
                                 SoMe = a.SoMe,
                                 ThungSo = a.ThungSo,
                                 KhoiLuongXeGoong = a.KhoiLuongXeGoong,
                                 KhoiLuongThung = a.KhoiLuongThung,
                                 KLThungGangLong = a.KLThungGangLong,
                                 KLGangLongCanRay = a.KLGangLongCanRay,
                                 VanChuyenHRC1 = a.VanChuyenHRC1,
                                 VanChuyenHRC2 = a.VanChuyenHRC2,
                                 PhanLoai = a.PhanLoai,
                                 Gio = a.Gio,
                                 GhiChu = a.GhiChu
                             }).ToListAsync();
            ViewBag.Data = id;
            return View(res);
        }

    }
}
