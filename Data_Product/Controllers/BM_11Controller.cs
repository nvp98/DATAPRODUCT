using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using ClosedXML.Excel;
//using iTextSharp.text.pdf;
//using Document = iTextSharp.text.Document;
//using PageSize = iTextSharp.text.PageSize;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using iText.Html2pdf;
using System.IO;
using iText.Html2pdf.Resolver.Font;
//using iText.Kernel.Font;
using iText.IO.Font;
using iText.Layout.Font;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Pdf.Canvas;

namespace Data_Product.Controllers
{
    public class BM_11Controller : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        //public PdfController(IConverter converter)
        //{
        //    _converter = converter;
        //}

        public BM_11Controller(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int? ID_TrangThai,int page = 1)
        {

            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BG = TaiKhoan.ID_TaiKhoan;

            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai.ToList(), "ID_TrangThai", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BG == ID_NhanVien_BG)
                             select new Tbl_BienBanGiaoNhan
                             {
                                 ID_BBGN = a.ID_BBGN,
                                 ID_NhanVien_BG = a.ID_NhanVien_BG,
                                 ID_PhongBan_BG = a.ID_PhongBan_BG,
                                 ID_Xuong_BG = a.ID_Xuong_BG,
                                 ID_ViTri_BG = a.ID_ViTri_BG,
                                 ThoiGianXuLyBG = (DateTime)a.ThoiGianXuLyBG,
                                 ID_TrangThai_BG = a.ID_TrangThai_BG,
                                 ID_NhanVien_BN = a.ID_NhanVien_BN,
                                 ID_PhongBan_BN = a.ID_PhongBan_BN,
                                 ID_Xuong_BN = a.ID_Xuong_BN,
                                 ID_ViTri_BN = a.ID_ViTri_BN,
                                 ThoiGianXuLyBN = (DateTime?)a.ThoiGianXuLyBN ?? default,
                                 ID_TrangThai_BN = a.ID_TrangThai_BN,
                                 SoPhieu = a.SoPhieu,
                                 ID_TrangThai_BBGN = a.ID_TrangThai_BBGN,
                                 ID_QuyTrinh = a.ID_QuyTrinh,
                                 ID_BBGN_Cu = (int?)a.ID_BBGN_Cu??default
                             }).ToListAsync();
            if (ID_TrangThai != null) res = res.Where(x => x.ID_TrangThai_BBGN == ID_TrangThai).ToList();
            if (begind != null && endd != null) res = res.Where(x => x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay).ToList();
            //if (begind == null && endd == null && ID_TrangThai == null)
            //{
            //    res = res.Where(x => x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay).ToList();
            //}
            //else if(begind != null && endd != null && ID_TrangThai == null)
            //{
            //    res = res.Where(x => x.ThoiGianXuLyBG >= begind && x.ThoiGianXuLyBG <= endd).ToList();
            //}
            //else if(begind != null && endd != null && ID_TrangThai != null)
            //{
            //    res = res.Where(x => x.ThoiGianXuLyBG >= begind && x.ThoiGianXuLyBG <= endd && x.ID_TrangThai_BBGN == ID_TrangThai).ToList();
            //}    
            const int pageSize = 20;
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
        public async Task<IActionResult> Index_All(DateTime? begind, DateTime? endd, int? ID_PhongBan,int? ID_DaiDien, int page = 1)
        {

            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BG = TaiKhoan.ID_TaiKhoan;

            ViewBag.PBList = new SelectList(_context.Tbl_PhongBan.ToList(), "ID_PhongBan", "TenPhongBan", ID_PhongBan);
            var res = await (from a in _context.Tbl_BienBanGiaoNhan.Where(x=>x.ID_TrangThai_BBGN == 1)
                             select new Tbl_BienBanGiaoNhan
                             {
                                 ID_BBGN = a.ID_BBGN,
                                 ID_NhanVien_BG = a.ID_NhanVien_BG,
                                 ID_PhongBan_BG = a.ID_PhongBan_BG,
                                 ID_Xuong_BG = a.ID_Xuong_BG,
                                 ID_ViTri_BG = a.ID_ViTri_BG,
                                 ThoiGianXuLyBG = (DateTime)a.ThoiGianXuLyBG,
                                 ID_TrangThai_BG = a.ID_TrangThai_BG,
                                 ID_NhanVien_BN = a.ID_NhanVien_BN,
                                 ID_PhongBan_BN = a.ID_PhongBan_BN,
                                 ID_Xuong_BN = a.ID_Xuong_BN,
                                 ID_ViTri_BN = a.ID_ViTri_BN,
                                 ThoiGianXuLyBN = (DateTime?)a.ThoiGianXuLyBN ?? default,
                                 ID_TrangThai_BN = a.ID_TrangThai_BN,
                                 SoPhieu = a.SoPhieu,
                                 ID_TrangThai_BBGN = a.ID_TrangThai_BBGN,
                                 ID_QuyTrinh = a.ID_QuyTrinh,
                                 ID_BBGN_Cu = (int?)a.ID_BBGN_Cu ?? default
                             }).ToListAsync();

            if (ID_DaiDien == 1)
            {
                if (begind == null && endd == null && ID_PhongBan == null)
                {
                    res = res.Where(x => x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay).ToList();
                }
                else if (begind != null && endd != null && ID_PhongBan == null)
                {
                    res = res.Where(x => x.ThoiGianXuLyBG >= begind && x.ThoiGianXuLyBG <= endd).ToList();
                }
                else if (begind != null && endd != null && ID_PhongBan != null)
                {
                    res = res.Where(x => x.ThoiGianXuLyBG >= begind && x.ThoiGianXuLyBG <= endd && x.ID_PhongBan_BG == ID_PhongBan).ToList();
                }
            } 
            else
            {
                if (begind == null && endd == null && ID_PhongBan == null)
                {
                    res = res.Where(x => x.ThoiGianXuLyBN >= startDay && x.ThoiGianXuLyBN <= endDay).ToList();
                }
                else if (begind != null && endd != null && ID_PhongBan == null)
                {
                    res = res.Where(x => x.ThoiGianXuLyBN >= begind && x.ThoiGianXuLyBN <= endd).ToList();
                }
                else if (begind != null && endd != null && ID_PhongBan != null)
                {
                    res = res.Where(x => x.ThoiGianXuLyBN >= begind && x.ThoiGianXuLyBN <= endd && x.ID_PhongBan_BN == ID_PhongBan).ToList();
                }
            }    


            var items = new List<SelectListItem>
            {
                new SelectListItem { Text = "Bên giao", Value = "1" },
                new SelectListItem { Text = "Bên nhận", Value = "2" }
            };
            ViewBag.Options = items;
            const int pageSize = 20;
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
        public async Task<IActionResult> Index_Detai(int id)
        { 
            var res = await (from a in _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == id)
                             join vt in _context.Tbl_VatTu on a.ID_VatTu equals vt.ID_VatTu
                             select new Tbl_ChiTiet_BienBanGiaoNhan
                             {
                                 ID_CT_BBGN = a.ID_CT_BBGN,
                                 ID_VatTu = a.ID_VatTu,
                                 TenVatTu = vt.TenVatTu,
                                 DonViTinh = vt.DonViTinh,
                                 MaLo = a.MaLo,
                                 DoAm_W = (double)a.DoAm_W,
                                 KhoiLuong_BG = (double)a.KhoiLuong_BG,
                                 KL_QuyKho_BG = (double)a.KL_QuyKho_BG,
                                 KhoiLuong_BN = (double)a.KhoiLuong_BN,
                                 KL_QuyKho_BN = (double)a.KL_QuyKho_BN,
                                 GhiChu = a.GhiChu,
                                 ID_BBGN = a.ID_BBGN,
                             }).ToListAsync();
            ViewBag.Data = id;
            return View(res);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var ID_BBGN = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == id).FirstOrDefault();
            try
            {
               
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("SuaPhieu", "BM_11", new {id = ID_BBGN.ID_BBGN });
        }
        public async Task<IActionResult> TaoPhieu()
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

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                             select new Tbl_TaiKhoan
                             {
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                             }).ToListAsync();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen");

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x=>x.PhongBan.Contains(TenBP))
                                  select new Tbl_VatTu
                                  {
                                      ID_VatTu = a.ID_VatTu,
                                      TenVatTu = a.TenVatTu
                                  }).ToListAsync();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");

            var MaLo = await (from a in _context.Tbl_MaLo
                               select new Tbl_MaLo
                               {
                                   ID_MaLo = a.ID_MaLo,
                                   TenMaLo = a.TenMaLo
                               }).ToListAsync();

            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");

            var CaKip = await (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == NgayLamViec)
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenCa = a.TenCa
                               }).ToListAsync();
            ViewBag.IDKip = new SelectList(CaKip, "ID_Kip", "TenCa");



            var NV = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.IDNhanVien = new SelectList(NV, "ID_TaiKhoan", "HoVaTen");
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhieu(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection)
        {
            int IDTaiKhoan = 0;
            int IDKip = 0;
            string XacNhan = "";
            int BBGN_ID = 0;
            string ID_Day = "";
            string ID_ca = "";
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {

                foreach (var key in formCollection.ToList())
                {
                    if (key.Key != "__RequestVerificationToken")
                    {
                        IDTaiKhoan = Convert.ToInt32(formCollection["IDTaiKhoan"]);
                        IDKip = Convert.ToInt32(formCollection["IDKip"]);
                        XacNhan = formCollection["xacnhan"];
                        ID_Day = formCollection["ID_Day"];
                        ID_ca = formCollection["IDCa"];
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "IDCa" && key.Key != "IDTaiKhoan" && key.Key != "xacnhan" && key.Key == "ghichu_" + key.Key.Split('_')[1] )
                    {
                        Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                        {
                            ID_VatTu = Convert.ToInt32(formCollection["VatTu_" + key.Key.Split('_')[1]]),
                            MaLo = formCollection["lo_" + key.Key.Split('_')[1]],
                            DoAm_W = double.TryParse(formCollection["doam_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0,
                            KhoiLuong_BG =double.TryParse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 0,
                            //KL_QuyKho_BG = double.Parse(formCollection["quykhobg_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }
                }
                //Inser thông tin phiếu giao nhận
                if (IDTaiKhoan != 0 && XacNhan != "")
                {

                    // Thông tin bên giao
                    var MBVN_BG = User.FindFirstValue(ClaimTypes.Name);
                    var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MBVN_BG).FirstOrDefault();
                    var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();

                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();
                    var ThongTin_BP_BN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BN.ID_PhongBan).FirstOrDefault();

                    // Thông tin ca kíp làm việc
                    //var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == IDKip).FirstOrDefault();
                    //DateTime Ngay_KipCa = (DateTime)ID_Kip.NgayLamViec;
                    //string Ngay = Ngay_KipCa.ToString("dd-MM-yyyy");
                    //DateTime NgayXuLy = DateTime.ParseExact(Ngay, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                    // Thông tin ca kíp làm việc
                    DateTime date = DateTime.Parse(ID_Day);
                    string day_bs = date.ToString("dd-MM-yyyy");
                    DateTime NgayXuLy = DateTime.ParseExact(day_bs, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    int Kip = Convert.ToInt32(ID_ca);
                    var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == Kip).FirstOrDefault();

                    // Kiểm tra điều kiện không nhập dữ liệu 
                    if(Tbl_ChiTiet_BienBanGiaoNhan.Count == 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng điền thông tin vật tư giao nhận');</script>";
                        return RedirectToAction("TaoPhieu", "BM_11");
                    }



                    //DateTime Day = DateTime.Now;
                    //string Day_Convert = Day.ToString("dd-MM-yyyy");
                    DateTime ThoiGianXuLyBG = DateTime.ParseExact(day_bs, "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture); // ngày chọn

                    string stt = "";
                    var count = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBG == ThoiGianXuLyBG).Count();
                    int len = count.ToString().Length;
                    if (len == 1)
                    {
                        stt = "00" + (count + 1);
                    }
                    else if (len == 2)
                    {
                        stt = "0" + (count + 1);
                    }
                    else if (len == 3)
                    {
                        stt = (count + 1).ToString();
                    }
                    string SoPhieu = ThongTin_BP_BG.TenNgan + "-" + ThongTin_BP_BN.TenNgan + "-" + ID_Kip.TenCa + ID_Kip.TenKip + "-" +
                                        NgayXuLy.ToString("dd") + NgayXuLy.ToString("MM") + NgayXuLy.ToString("yy") + stt;

                    var Output_ID_BBGN = new SqlParameter
                    {
                        ParameterName = "ID_BBGN",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = System.Data.ParameterDirection.Output,
                    };
                    if (XacNhan == "0" && XacNhan != "") // lưu
                    {

                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},@ID_BBGN OUTPUT",
                                                               ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                                                               ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_Kip.ID_Kip, 0, 1, Output_ID_BBGN);
                        BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                         // insert chi tiết BBGN
                        foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        {
                            double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                            double KL_QuyKho = Math.Round(QuyKho, 3);
                            if (BBGN_ID != 0)
                            { 
                                if(item.MaLo != "" && item.MaLo != null)
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if(IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, IDMaLo.TenMaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                    }    

                                }  
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                }    
                              

                            }

                        }
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                    else if (XacNhan == "1" && XacNhan != "") // gửi phiếu
                    {
                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},@ID_BBGN OUTPUT",
                                                               ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 1,
                                                               ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_Kip.ID_Kip, 0, 1, Output_ID_BBGN);
                        BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        {
                            double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                            double KL_QuyKho = Math.Round(QuyKho, 3);
                            if (BBGN_ID != 0)
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, IDMaLo.TenMaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                    }
                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                }
                            }

                        }
                        TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
                        return RedirectToAction("Index_Detai", "BM_11", new{id = BBGN_ID });
                    }
                   
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
                return RedirectToAction("TaoPhieu", "BM_11");
            }

            return RedirectToAction("SuaPhieu", "BM_11", new { id = BBGN_ID });
        }


        public async Task<IActionResult> SuaPhieu(int? id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x=>x.ID_BBGN == id).FirstOrDefault();


            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan", ID_BBGN.ID_PhongBan_BN);

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", ID_BBGN.ID_NhanVien_BN);

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP))
                               select new Tbl_VatTu
                               {
                                   ID_VatTu = a.ID_VatTu,
                                   TenVatTu = a.TenVatTu
                               }).ToListAsync();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");


            var MaLo = await (from a in _context.Tbl_MaLo.Where(x => x.PhongBan.Contains(TenBP.Trim()))
                              select new Tbl_MaLo
                              {
                                  ID_MaLo = a.ID_MaLo,
                                  TenMaLo = a.TenMaLo
                              }).ToListAsync();

            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");

            ViewBag.Data = id;
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaPhieu(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, int? id)
        {
            int IDTaiKhoan = 0;
            string XacNhan = "";
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {
                foreach (var key in formCollection.ToList())
                {
                    if (key.Key != "__RequestVerificationToken")
                    {
                        IDTaiKhoan = Convert.ToInt32(formCollection["IDTaiKhoan"]); 
                        XacNhan = formCollection["xacnhan"];
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "IDTaiKhoan" && key.Key != "xacnhan" && key.Key == "ghichu_" + key.Key.Split('_')[1])
                    {
                        Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                        {
                            ID_CT_BBGN = Convert.ToInt32(key.Key.Split('_')[1]),
                            ID_VatTu = Convert.ToInt32(formCollection["VatTu_" + key.Key.Split('_')[1]]),
                            MaLo = formCollection["lo_" + key.Key.Split('_')[1]],
                            DoAm_W = double.Parse(formCollection["doam_" + key.Key.Split('_')[1]]),
                            KhoiLuong_BG = double.Parse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]]),
                            //KL_QuyKho_BG = double.Parse(formCollection["quykhobg_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }

                }

                // Kiểm tra điều kiện không nhập dữ liệu 
                if (Tbl_ChiTiet_BienBanGiaoNhan.Count == 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng điền thông tin vật tư giao nhận');</script>";
                    return RedirectToAction("SuaPhieu", "BM_11", new { id = id });
                }

                // Thông tin BBGB
                var BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                if (XacNhan == "0" && XacNhan != "") // luu
                {
                
                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();

                    var result_tt = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_update {0},{1},{2},{3},{4}", id,
                                                            ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu);

                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, null);
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var ID_CT_BBGN = item.ID_CT_BBGN - 100;
                        var ID_CT = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);
                        if (ID_CT == null)
                        {
                            if (item.MaLo != "" && item.MaLo != null)
                            {
                                int Ma_Lo = Convert.ToInt32(item.MaLo);
                                var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                if (IDMaLo != null)
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                             item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BN, item.KL_QuyKho_BN, item.GhiChu, id);
                                }

                            }
                            else
                            {
                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                            item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BN, item.KL_QuyKho_BN, item.GhiChu, id);
                            }
                         
                        }   
                        else
                        {
                          
                            if (item.ID_VatTu == 0)
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                         ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);
                                    }

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                          ID_CT.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);
                                }

                              

                            }
                            else
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                        item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);
                                    }

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                        item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);
                                }
                              
                            }
                        }

                    }

                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 0);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 0);
                    TempData["msgSuccess"] = "<script>alert('Lưu thành công');</script>";
                }   
                else if(XacNhan == "1" && XacNhan != "") // trình ký
                {
                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();

                    var result_tt = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_update {0},{1},{2},{3},{4}", id,
                                                            ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu);

                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, null);
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var ID_CT_BBGN = item.ID_CT_BBGN - 100;
                        var ID_CT = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == ID_CT_BBGN).FirstOrDefault();

                        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);
                        if (ID_CT == null)
                        {
                            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                             item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BN, item.KL_QuyKho_BN, item.GhiChu, id);
                        }    
                        else
                        {
                         
                            if (item.ID_VatTu == 0)
                            {
                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                          ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);

                            }
                            else
                            {
                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                         item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);
                            }
                        }
                        var check = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                        if (check != null)
                        {
                            if (check.ID_TrangThai == 2)
                            {
                                var result_update = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 1);
                            }


                        }

                    }
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 1);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 0);

                    TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
                    return RedirectToAction("Index_Detai", "BM_11", new { id = id });
                }    
 
             
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }
            return RedirectToAction("SuaPhieu", "BM_11", new { id = id });
        }


        public async Task<IActionResult> BoSungPhieu()
        {
         
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            List<Tbl_NhomVatTu> vt = _context.Tbl_NhomVatTu.ToList();
            ViewBag.VTList = new SelectList(vt, "ID_NhomVatTu", "TenNhomVatTu");

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen");


            var NhanVien_TT = await (from a in _context.Tbl_TaiKhoan.Where(x=>x.ID_Quyen == 3)
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");



            var NhanVien_TT_View = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                     select new Tbl_TaiKhoan
                                     {
                                         ID_TaiKhoan = a.ID_TaiKhoan,
                                         HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                     }).ToListAsync();

            ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT_View, "ID_TaiKhoan", "HoVaTen");

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP))
                               select new Tbl_VatTu
                               {
                                   ID_VatTu = a.ID_VatTu,
                                   TenVatTu = a.TenVatTu
                               }).ToListAsync();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");



            var MaLo = await (from a in _context.Tbl_MaLo.Where(x => x.PhongBan.Contains(TenBP.Trim()))
                              select new Tbl_MaLo
                              {
                                  ID_MaLo = a.ID_MaLo,
                                  TenMaLo = a.TenMaLo
                              }).ToListAsync();

            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");

            var Ca = await (from a in _context.Tbl_Kip
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenCa = a.TenCa
                               }).ToListAsync();
            ViewBag.IDCa = new SelectList(Ca, "ID_Kip", "TenCa");

            var Kip = await (from a in _context.Tbl_Kip
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenKip = a.TenKip
                               }).ToListAsync();
            ViewBag.IDKip = new SelectList(Kip, "ID_Kip", "TenCa");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BoSungPhieu(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection)
        {
            int IDTaiKhoan = 0;
            int IDNhanVienTT = 0;
            int IDNhanVien_TT_View = 0;
            string ID_Day = "";
            string ID_ca = "";
            //string ID_KipLV = "";
            string XacNhan = "";
            int BBGN_ID = 0;
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {

                foreach (var key in formCollection.ToList())
                {
                    if (key.Key != "__RequestVerificationToken")
                    {
                        IDTaiKhoan = Convert.ToInt32(formCollection["IDTaiKhoan"]);
                        ID_Day = formCollection["ID_Day"];
                        ID_ca = formCollection["IDCa"];
                        //ID_KipLV = formCollection["ID_KipLV"];
                        XacNhan = formCollection["xacnhan"];
                        IDNhanVienTT = Convert.ToInt32(formCollection["NhanVienTT"]);
                        IDNhanVien_TT_View = Convert.ToInt32(formCollection["NhanVien_TT_View"]);
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "IDTaiKhoan" && key.Key != "NhanVienTT"
                        && key.Key != "ID_Day" && key.Key != "IDCa" && key.Key != "NhanVien_TT_View"
                        && key.Key != "xacnhan" && key.Key == "ghichu_" + key.Key.Split('_')[1])
                        {
                            Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                            {
                                ID_VatTu = Convert.ToInt32(formCollection["VatTu_" + key.Key.Split('_')[1]]),
                                MaLo = formCollection["lo_" + key.Key.Split('_')[1]],
                                //DoAm_W = double.Parse(formCollection["doam_" + key.Key.Split('_')[1]]),
                                //KhoiLuong_BG = double.Parse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]]),
                                DoAm_W = double.TryParse(formCollection["doam_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0,
                                KhoiLuong_BG = double.TryParse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 0,
                                //KL_QuyKho_BG = double.Parse(formCollection["quykhobg_" + key.Key.Split('_')[1]]),
                                GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                            });
                        }
                }

                // Kiểm tra điều kiện không nhập dữ liệu 
                if (Tbl_ChiTiet_BienBanGiaoNhan.Count == 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng điền thông tin vật tư giao nhận');</script>";
                    return RedirectToAction("BoSungPhieu", "BM_11");
                }

                //Inser thông tin phiếu giao nhận
                if (IDTaiKhoan != 0 && XacNhan != "")
                {
               
                    // Thông tin bên giao
                    var MBVN_BG = User.FindFirstValue(ClaimTypes.Name);
                    var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MBVN_BG).FirstOrDefault();
                    var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();

                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();
                    var ThongTin_BP_BN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BN.ID_PhongBan).FirstOrDefault();

                    // Thông tin ca kíp làm việc
                    DateTime date = DateTime.Parse(ID_Day);
                    string day_bs = date.ToString("dd-MM-yyyy");
                    DateTime NgayXuLy = DateTime.ParseExact(day_bs, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    int Kip = Convert.ToInt32(ID_ca);
                    var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == Kip).FirstOrDefault();




                    DateTime ThoiGianXuLyBG = DateTime.ParseExact(day_bs, "dd-MM-yyyy", CultureInfo.InvariantCulture);

 
                    
            
                    string stt = "";
                    var count = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBG == ThoiGianXuLyBG).Count();
                    int len = count.ToString().Length;
                    if (len == 1)
                    {
                        stt = "00" + (count + 1);
                    }
                    else if (len == 2)
                    {
                        stt = "0" + (count + 1);
                    }
                    else if (len == 3)
                    {
                        stt = (count + 1).ToString();
                    }
                    string SoPhieu = ThongTin_BP_BG.TenNgan + "-" + ThongTin_BP_BN.TenNgan + "-" + ID_Kip.TenCa+ID_Kip.TenKip + "-" +
                                        NgayXuLy.ToString("dd") + NgayXuLy.ToString("MM") + NgayXuLy.ToString("yy") + stt;

                    var Output_ID_BBGN = new SqlParameter
                    {
                        ParameterName = "ID_BBGN",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = System.Data.ParameterDirection.Output,
                    };
                    if (XacNhan == "0" && XacNhan != "")
                    {

                        // Thông tin phê duyệt nhân viên thống kê 
                        if (IDNhanVienTT == 0)
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                            return RedirectToAction("BoSungPhieu", "BM_11");
                        }
                        // insert BBGN mới từ BBGN cũ
                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},@ID_BBGN OUTPUT",
                                                               ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                                                               ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, Kip, 0, 2, Output_ID_BBGN);
                        BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        {
                            if (BBGN_ID != 0)
                            {
                                double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                                double KL_QuyKho = Math.Round(QuyKho, 3);

                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                    }

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu,"", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                }
                            }

                        }
                        // Thêm mới thông tin trình ký bổ sung
                        var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", BBGN_ID, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 0);

                        // Thông tin phê duyệt nhân viên thống kê
                        //if (IDNhanVienTT == 0)
                        //{
                        //    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                        //    return RedirectToAction("BoSungPhieu", "BM_11");
                        //}
                        //else
                        //{
                        //    //var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", BBGN_ID, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 0);
                        //}
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                    else if (XacNhan == "1" && XacNhan != "")
                    {
                        // Thông tin phê duyệt nhân viên thống kê 
                        if (IDNhanVienTT == 0)
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                            return RedirectToAction("BoSungPhieu", "BM_11");
                        }
                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},@ID_BBGN OUTPUT",
                                                               ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                                                               ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, Kip, 0, 2, Output_ID_BBGN);
                        BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        {
                            if (BBGN_ID != 0)
                            {
                                double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                                double KL_QuyKho = Math.Round(QuyKho, 3);


                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                             item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                    }

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                             item.ID_VatTu,"", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                                }
                              
                            }

                        }
                        // Thông tin phê duyệt nhân viên thống kê
                        if (IDNhanVienTT == 0)
                        {
                            TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                            return RedirectToAction("BoSungPhieu", "BM_11");
                        }
                        else
                        {
                            var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", BBGN_ID, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 1);
                        }
                        TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
                        return RedirectToAction("Index_Detai", "BM_11", new { id = BBGN_ID });
                    }

                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
                return RedirectToAction("BoSungPhieu", "BM_11");
            }

            return RedirectToAction("SuaPhieu_PhieuBoSung", "XuLyPhieu", new { id = BBGN_ID });
        }


        public async Task<IActionResult> TaoPhieuHieuChinh(int id)
        {
            int BBGN_ID = 0;
            try
            {
                // Phát sinh phiếu mới
                var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == ID_BBGN.ID_Kip).FirstOrDefault();
                // Thông tin bên giao
                var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == ID_BBGN.ID_NhanVien_BG).FirstOrDefault();
                var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();
                // Thông tin bên nhận
                var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == ID_BBGN.ID_NhanVien_BN).FirstOrDefault();
                var ThongTin_BP_BN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BN.ID_PhongBan).FirstOrDefault();

                DateTime Day = (DateTime)ID_BBGN.ThoiGianXuLyBG;
                string Day_Convert = Day.ToString("dd-MM-yyyy");
                DateTime ThoiGianXuLyBG = DateTime.ParseExact(Day_Convert, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                string stt = "";
                var count = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBG == ThoiGianXuLyBG).Count();
                int len = count.ToString().Length;
                if (len == 1)
                {
                    stt = "00" + (count + 1);
                }
                else if (len == 2)
                {
                    stt = "0" + (count + 1);
                }
                else if (len == 3)
                {
                    stt = (count + 1).ToString();
                }
                string SoPhieu = ThongTin_BP_BG.TenNgan + "-" + ThongTin_BP_BN.TenNgan + "-" + ID_Kip.TenCa + ID_Kip.TenKip + "-" +
                                    ThoiGianXuLyBG.ToString("dd") + ThoiGianXuLyBG.ToString("MM") + ThoiGianXuLyBG.ToString("yy") + stt;

                var Output_ID_BBGN = new SqlParameter
                {
                    ParameterName = "ID_BBGN",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                var result_new = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},@ID_BBGN OUTPUT",
                                                          ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                                                          ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_BBGN.ID_Kip, 0, 3, Output_ID_BBGN);
                BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                // Update ID phiếu cũ
                var result_update = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_update_ID {0},{1}", BBGN_ID, id);

                var List = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).ToList();
                foreach (var item in List)
                {
                    if (BBGN_ID != 0)
                    {
                        var result_Vitri = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                   item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, item.KL_QuyKho_BG, item.KhoiLuong_BN, item.KhoiLuong_BN, item.GhiChu, BBGN_ID);

                    }

                }

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 3);
                var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 3);
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Đề nghị hiệu chỉnh thất bại');</script>";
            }


            return RedirectToAction("YCauHieuChinh", "XuLyPhieu", new { id = BBGN_ID});
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
                                      TenPhongBan = cv.TenPhongBan + " - " + px.TenXuong
                                  }).ToListAsync();

            return Json(NhanVien);
        }
        public async Task<IActionResult> DonViTinh(int IDVatTu)
        {
            var dvt = await (from a in _context.Tbl_VatTu.Where(x => x.ID_VatTu == IDVatTu)
                                  select new Tbl_VatTu
                                  {
                                      ID_VatTu = a.ID_VatTu,
                                      DonViTinh = a.DonViTinh
                                  }).ToListAsync();

            return Json(dvt);
        }
        public async Task<IActionResult> MaLo(int IDVatTu)
        {
            var dvt = await (from a in _context.Tbl_MaLo
                             join x in _context.Tbl_VatTuMaLo.Where(x=>x.ID_VatTu == IDVatTu) on a.ID_MaLo equals x.ID_MaLo
                             select new Tbl_MaLo
                             {
                                 ID_MaLo = a.ID_MaLo,
                                 TenMaLo = a.TenMaLo
                             }).ToListAsync();
            return Json(dvt);
        }
        public async Task<IActionResult> NguyenLieu(int IDTaiKhoan)
        {
            if (IDTaiKhoan == null) IDTaiKhoan = 0;
            var ID = _context.Tbl_TaiKhoan.Where(x=>x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();
            string ID_PhongBan = ID.ID_PhongBan.ToString();

            var NguyenLieu = await (from a in _context.Tbl_VatTu                 
                                  select new Tbl_VatTu
                                  {
                                      ID_VatTu = a.ID_VatTu,
                                      PhongBan = a.PhongBan,
                                      TenVatTu = a.TenVatTu
                                  }).ToListAsync();
            if(ID_PhongBan != "")
            {
                NguyenLieu = NguyenLieu.Where(x => x.PhongBan.Contains(ID_PhongBan)).ToList();
            } 
            return Json(NguyenLieu);
        }
        public async Task<IActionResult> Kip (int IDKip)
        {
            var CaKip = await (from a in _context.Tbl_Kip.Where(x => x.ID_Kip == IDKip)
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenKip = a.TenKip
                               }).ToListAsync();
            return Json(CaKip);
        }

        public async Task<IActionResult> Ngay(DateTime? Ngay)
        {

            DateTime day_datetime = (DateTime)Ngay;
            string Day = day_datetime.ToString("dd-MM-yyyy");
            DateTime Day_Convert = DateTime.ParseExact(Day, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
            var Kip = await (from a in _context.Tbl_Kip.Where(x=>x.NgayLamViec == Day_Convert)
                                    select new Tbl_Kip
                                    {
                                        ID_Kip = a.ID_Kip,
                                        TenCa = a.TenCa
                                    }).ToListAsync();
            return Json(Kip);
        }
        public async Task<IActionResult> HuyPhieu(int id)
        {
            try
            {
                var ID_BB = _context.Tbl_BienBanGiaoNhan.Where(x=>x.ID_BBGN == id).FirstOrDefault();
                if(ID_BB.ID_QuyTrinh == 2)
                {
                    //Thông tin nhân viên thông kê phê duyệt
                    var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan_View, 0);

                    //Thông tin biển bản giao nhận
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 0);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 0);

                    TempData["msgSuccess"] = "<script>alert('Hủy phiếu thành công');</script>";
                    return RedirectToAction("SuaPhieu_PhieuBoSung", "XuLyPhieu", new { id = id });
                } 
                else if(ID_BB.ID_QuyTrinh == 3)
                {
                    //Thông tin nhân viên thông kê phê duyệt
                    var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan_View, 0);

                    //Thông tin biển bản giao nhận
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 0);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 0);

                    TempData["msgSuccess"] = "<script>alert('Hủy phiếu thành công');</script>";
                    return RedirectToAction("YCauHieuChinh", "XuLyPhieu", new { id = id });
                }
                else if(ID_BB.ID_QuyTrinh == 1)
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 0);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 0);

                    TempData["msgSuccess"] = "<script>alert('Hủy phiếu thành công');</script>";
                }    
            

              
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Hủy phiếu thất bại');</script>";
            }


            return RedirectToAction("SuaPhieu", "BM_11", new {id= id});
        }

        public async Task<IActionResult> SaoChepPhieu(int id)
        {
            int BBGN_ID = 0;
            try
            {
                // Phát sinh phiếu mới
                var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                // Thông tin bên giao
                var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == ID_BBGN.ID_NhanVien_BG).FirstOrDefault();
                var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();
                // Thông tin bên nhận
                var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == ID_BBGN.ID_NhanVien_BN).FirstOrDefault();
                var ThongTin_BP_BN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BN.ID_PhongBan).FirstOrDefault();

                DateTime Day = (DateTime)ID_BBGN.ThoiGianXuLyBG;
                string Day_Convert = Day.ToString("dd-MM-yyyy");
                DateTime ThoiGianXuLyBG = DateTime.ParseExact(Day_Convert, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                string stt = "";
                var count = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBG == ThoiGianXuLyBG).Count();
                int len = count.ToString().Length;
                if (len == 1)
                {
                    stt = "00" + (count + 1);
                }
                else if (len == 2)
                {
                    stt = "0" + (count + 1);
                }
                else if (len == 3)
                {
                    stt = (count + 1).ToString();
                }
                string SoPhieu = ThongTin_BP_BG.TenNgan + "-" + ThongTin_BP_BN.TenNgan + "-" + "1C" + "-" +
                                    ThoiGianXuLyBG.ToString("dd") + ThoiGianXuLyBG.ToString("MM") + ThoiGianXuLyBG.ToString("yy") + stt;

                var Output_ID_BBGN = new SqlParameter
                {
                    ParameterName = "ID_BBGN",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                var result_new = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},@ID_BBGN OUTPUT",
                                                          ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                                                          ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, 0, 1, Output_ID_BBGN);
                BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                var List = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).ToList();
                foreach (var item in List)
                {
                    if (BBGN_ID != 0)
                    {
                        var result_Vitri = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                                          item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, item.KL_QuyKho_BG, 0, 0, null, BBGN_ID);
                    }

                }

                TempData["msgSuccess"] = "<script>alert('Sao chép phiếu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Sao chép phiếu thất bại');</script>";
                return RedirectToAction("Index_Detai", "BM_11", new { id = id });
            }


            return RedirectToAction("Index", "BM_11");
        }
        public async Task<IActionResult> ExportToExcel(int BBGN_ID)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("BBGN");
                var  ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x=>x.ID_BBGN == BBGN_ID).FirstOrDefault();
                var Data = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == BBGN_ID).ToList();
                int row = 8, stt = 0, icol = 1;
                if (Data.Count > 0)
                {
                    foreach (var item in Data)
                    {

                        row++; stt++; icol = 1;

                        Worksheet.Cell(row, icol).Value = stt;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_BBGN.ThoiGianXuLyBG;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd/MM/yyyy";


                        var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == ID_BBGN.ID_Kip).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_Kip.TenKip;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                     

                        icol++;
                        if(ID_Kip.TenCa == "1")
                        {
                            Worksheet.Cell(row, icol).Value = "Ngày";
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        }  
                        else
                        {
                            Worksheet.Cell(row, icol).Value = "Đêm";
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        }

                        var ID_VT = _context.Tbl_VatTu.Where(x => x.ID_VatTu == item.ID_VatTu).FirstOrDefault();

                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_VT.TenVatTu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_Lo = _context.Tbl_MaLo.Where(x=>x.TenMaLo == item.MaLo).FirstOrDefault();
                        icol++;
                        if(ID_Lo != null)
                        {
                            Worksheet.Cell(row, icol).Value = ID_Lo.TenMaLo;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        }  
                        else
                        {
                            Worksheet.Cell(row, icol).Value = "";
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        }    
                     

                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_VT.DonViTinh;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KhoiLuong_BN;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 1);
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value =item.KL_QuyKho_BN;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_XBN = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BN).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_XBN.TenXuong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_BPBN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BN).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_BPBN.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;





                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KhoiLuong_BG;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 1);
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KL_QuyKho_BG;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_XBG = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BG).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_XBG.TenXuong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_BPBG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BG).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_BPBG.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.GhiChu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_BBGN.SoPhieu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }

                    Worksheet.Range("A7:T" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:T" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A7:T" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:T" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "BBGN - " + ID_BBGN.SoPhieu + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "BBGN - " + ID_BBGN.SoPhieu + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index_Detai", "BM_11", new { id = BBGN_ID });
            }
        }
        public async Task<IActionResult> ExportToExcel_All(DateTime? begind, DateTime? endd, int ID_PhongBan, int ID_DaiDien)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("BBGN");
                int row = 8, stt = 0, icol = 1;
                if (ID_DaiDien == 1)
                {
                    var BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBG >= begind && x.ThoiGianXuLyBG <= endd && x.ID_PhongBan_BG == ID_PhongBan && x.ID_TrangThai_BBGN == 1).ToList();
                    foreach(var ID_BBGN in BBGN)
                    {
                        var Data = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == ID_BBGN.ID_BBGN).ToList();
                        foreach (var item in Data)
                        {

                            row++; stt++; icol = 1;

                            Worksheet.Cell(row, icol).Value = stt;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BBGN.ThoiGianXuLyBG;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd/MM/yyyy";


                            var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == ID_BBGN.ID_Kip).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_Kip.TenKip;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            if (ID_Kip.TenCa == "1")
                            {
                                Worksheet.Cell(row, icol).Value = "Ngày";
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            }
                            else
                            {
                                Worksheet.Cell(row, icol).Value = "Đêm";
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            }

                            var ID_VT = _context.Tbl_VatTu.Where(x => x.ID_VatTu == item.ID_VatTu).FirstOrDefault();

                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_VT.TenVatTu;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_Lo = _context.Tbl_MaLo.Where(x => x.TenMaLo == item.MaLo).FirstOrDefault();
                            icol++;
                            if (ID_Lo != null)
                            {
                                Worksheet.Cell(row, icol).Value = ID_Lo.TenMaLo;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            }
                            else
                            {
                                Worksheet.Cell(row, icol).Value = "";
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            }


                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_VT.DonViTinh;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KhoiLuong_BN;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 1);
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KL_QuyKho_BN;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_XBN = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BN).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_XBN.TenXuong;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_BPBN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BN).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BPBN.TenPhongBan;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;





                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KhoiLuong_BG;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 1);
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KL_QuyKho_BG;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_XBG = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BG).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_XBG.TenXuong;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_BPBG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BG).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BPBG.TenPhongBan;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            Worksheet.Cell(row, icol).Value = item.GhiChu;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BBGN.SoPhieu;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        }

                    }    
                  
                }   
                else
                {
                    var BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBN >= begind && x.ThoiGianXuLyBN <= endd && x.ID_PhongBan_BN == ID_PhongBan && x.ID_TrangThai_BBGN == 1).ToList();
                    foreach (var ID_BBGN in BBGN)
                    {
                        var Data = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == ID_BBGN.ID_BBGN).ToList();
                        foreach (var item in Data)
                        {

                            row++; stt++; icol = 1;

                            Worksheet.Cell(row, icol).Value = stt;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BBGN.ThoiGianXuLyBG;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd/MM/yyyy";


                            var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == ID_BBGN.ID_Kip).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_Kip.TenKip;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            if (ID_Kip.TenCa == "1")
                            {
                                Worksheet.Cell(row, icol).Value = "Ngày";
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            }
                            else
                            {
                                Worksheet.Cell(row, icol).Value = "Đêm";
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            }

                            var ID_VT = _context.Tbl_VatTu.Where(x => x.ID_VatTu == item.ID_VatTu).FirstOrDefault();

                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_VT.TenVatTu;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_Lo = _context.Tbl_MaLo.Where(x => x.TenMaLo == item.MaLo).FirstOrDefault();
                            icol++;
                            if (ID_Lo != null)
                            {
                                Worksheet.Cell(row, icol).Value = ID_Lo.TenMaLo;
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            }
                            else
                            {
                                Worksheet.Cell(row, icol).Value = "";
                                Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                            }


                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_VT.DonViTinh;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KhoiLuong_BN;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 1);
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KL_QuyKho_BN;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_XBN = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BN).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_XBN.TenXuong;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_BPBN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BN).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BPBN.TenPhongBan;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;





                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KhoiLuong_BG;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 1);
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = item.KL_QuyKho_BG;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_XBG = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BG).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_XBG.TenXuong;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            var ID_BPBG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BG).FirstOrDefault();
                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BPBG.TenPhongBan;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                            icol++;
                            Worksheet.Cell(row, icol).Value = item.GhiChu;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                            icol++;
                            Worksheet.Cell(row, icol).Value = ID_BBGN.SoPhieu;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        }

                    }
                }

                Worksheet.Range("A7:T" + (row)).Style.Font.SetFontName("Times New Roman");
                Worksheet.Range("A7:T" + (row)).Style.Font.SetFontSize(13);
                Worksheet.Range("A7:T" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                Worksheet.Range("A7:T" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                Workbook.SaveAs(fileNamemaunew);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                string fileName = "Thống kê BBGN" + ".xlsx";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index_All", "BM_11", new { begind = begind, endd = endd, ID_PhongBan = ID_PhongBan, ID_DaiDien = ID_PhongBan});
            }
        }

        public async Task<IActionResult> GeneratePdf(int id)
        {
            var bbgn = _context.Tbl_BienBanGiaoNhan.Find(id);
            if (bbgn == null)
            {
                return null;
            }
            // 1. Render Razor View thành chuỗi HTML
            string htmlContent = await RenderViewToStringAsync("ExportPdfView", new { id = id });

            // 2. Chuyển đổi HTML sang PDF
            byte[] pdfBytes = ConvertHtmlToPdf(htmlContent);
            string filename = bbgn.SoPhieu + DateTime.Now.ToString("yyyyMMddHHmm");

            // 3. Trả về file PDF
            return File(pdfBytes, "application/pdf", filename+ ".pdf");
        }
        [HttpPost]
        public async Task<IActionResult> SavePdf(int? id)
        {
            var bbgn = _context.Tbl_BienBanGiaoNhan.Find(id);
            if (bbgn == null)
            {
                return null;
            }
            // 1. Render Razor View thành chuỗi HTML
            string htmlContent = await RenderViewToStringAsync("ExportPdfView", new { id = id });

            // 2. Chuyển đổi HTML sang PDF
            byte[] pdfBytes = ConvertHtmlToPdf(htmlContent);
            string filename = $"{bbgn.SoPhieu + DateTime.Now.ToString("yyyyMMddHHmm")}.pdf";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");

            // Lưu file vào server và trả về path
            string pathsave = SavePdfToFile(pdfBytes, folderPath, filename);
            if (pathsave != "" && pathsave != null)
            {
                bbgn.FileBBGN = pathsave;
                _context.SaveChanges();
            }
            return NoContent();
            // 3. Trả về file PDF
            //return File(pdfBytes, "application/pdf", filename);
        }

        public async Task<IActionResult> GenerateAndSavePdf(int? id)
        {
            var bbgn = _context.Tbl_BienBanGiaoNhan.Find(id);
            if (bbgn == null)
            {
                return null;
            }
            // 1. Render Razor View thành chuỗi HTML
            string htmlContent = await RenderViewToStringAsync("ExportPdfView", new { id = id });

            // 2. Chuyển đổi HTML sang PDF
            byte[] pdfBytes = ConvertHtmlToPdf(htmlContent);
            string filename = $"{bbgn.SoPhieu + DateTime.Now.ToString("yyyyMMddHHmm")}.pdf" ;
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");

            // Lưu file vào server và trả về path
            string pathsave = SavePdfToFile(pdfBytes, folderPath, filename);
            if(pathsave != "" && pathsave != null)
            {
                bbgn.FileBBGN = pathsave;
                _context.SaveChanges();
            }

            // 3. Trả về file PDF
            return File(pdfBytes, "application/pdf", filename);
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
                pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterHandler());

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
        public string SavePdfToFile(byte[] pdfBytes, string folderPath, string fileName)
        {
            // Kiểm tra và tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Đường dẫn đầy đủ của file
            var filePath = Path.Combine(folderPath, fileName);

            // Lưu file từ mảng byte bằng FileStream
            //File.WriteAllBytes(filePath, pdfBytes);
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.Write(pdfBytes, 0, pdfBytes.Length);
            }

            // Trả về đường dẫn tương đối hoặc URL để lưu vào database
            return $"/pdfs/{fileName}"; // Nếu wwwroot/pdfs là thư mục lưu trữ công khai
        }
        // Class xử lý Footer
        public class FooterHandler : IEventHandler
        {

            public void HandleEvent(Event @event)
            {
                var httpContext = new HttpContextAccessor().HttpContext;
                var pdfEvent = (PdfDocumentEvent)@event;
                var pdfPage = pdfEvent.GetPage();
                var canvas = new PdfCanvas(pdfPage);
                var pageSize = pdfPage.GetPageSize();

                // Lấy số trang hiện tại
                int pageNumber = pdfEvent.GetDocument().GetPageNumber(pdfPage);
                // lấy IDBBGN và mã hóa
                var bbgnID = "BIENBANGIAONHAN_BM11.QT05_" + httpContext.Request.RouteValues["id"]?.ToString()+DateTime.Now.ToString("dd/MM/yyyy");

                byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(bbgnID.ToString());

                // Chuỗi cần tính chiều rộng
                string text = Convert.ToBase64String(idBytes);

                // Kích thước font
                float fontSize = 8;

                // Font sử dụng (Helvetica)
                var pdfFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

                // Tính chiều rộng chuỗi
                float textWidth = pdfFont.GetWidth(text, fontSize);

                // Vị trí Footer
                float x = pageSize.GetWidth() - (textWidth +10); // Vị trí giữa trang
                float y = pageSize.GetBottom() + 20; // Cách mép dưới 20 đơn vị

                // Vẽ Footer
                canvas.BeginText()
                      .SetFontAndSize(PdfFontFactory.CreateFont("C:/Windows/Fonts/times.ttf", PdfEncodings.IDENTITY_H), 8)
                      .MoveText(x, y)
                      .ShowText($"{Convert.ToBase64String(idBytes)}") // Nội dung Footer
                      .EndText()
                      .Release();



            }
        }

    }
}
