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
using DocumentFormat.OpenXml.Spreadsheet;
using iText.Kernel.Pdf;
using iText.Barcodes;
using iText.IO.Image;
using iText.Kernel.Pdf.Xobject;
using DocumentFormat.OpenXml.InkML;
using iText.StyledXmlParser.Jsoup.Nodes;

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
            DateTime endDay = Now;
            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BG = TaiKhoan.ID_TaiKhoan;

            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai.ToList(), "ID_TrangThai", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BG == ID_NhanVien_BG && !x.IsDelete)
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
                                 ID_BBGN_Cu = (int?)a.ID_BBGN_Cu??default,
                                 NoiDungTrichYeu = a.NoiDungTrichYeu,
                                 NgayTao = a.NgayTao
                             }).OrderByDescending(x => x.NgayTao).ToListAsync();
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
        public async Task<IActionResult> Index_All(string search,int? ID_QuyTrinh,DateTime? begind, DateTime? endd, int? ID_PhongBan, int? ID_Xuong, int? ID_PhongBanBG,int? ID_XuongBG, int? ID_PhongBanBN, int? ID_XuongBN,string Kip, int page = 1)
        {

            DateTime Now = DateTime.Now;
            DateTime startDay = Now.AddDays(-1);
            DateTime endDay = Now;
            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BG = TaiKhoan.ID_TaiKhoan;

            var pbls = _context.Tbl_PhongBan.ToList();
            List<string> ListPB = new List<string>();
            List<int> ListPBInt = new List<int>();
            if (TaiKhoan.PhongBan_Them != null)
            {
                ListPB = TaiKhoan.PhongBan_Them.Split(',').Select(item => item.Trim()).ToList();
                foreach (var item in ListPB)
                {
                    var pb = _context.Tbl_PhongBan.Where(x => x.TenNgan == item).FirstOrDefault();
                    if (pb != null) ListPBInt.Add(pb.ID_PhongBan);
                }
            }
            //ViewBag.TTList = new SelectList(_context.Tbl_TrangThai.ToList(), "ID_TrangThai", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_BienBanGiaoNhan.Where(x=> x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay && !x.IsDelete)
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
                                 ID_BBGN_Cu = (int?)a.ID_BBGN_Cu ?? default,
                                 NgayTao = a.NgayTao,
                                 NoiDungTrichYeu = a.NoiDungTrichYeu,
                                 IsLock =a.IsLock
                             }).OrderByDescending(x=>x.NgayTao).ToListAsync();
            //if (TaiKhoan.ID_Quyen == 7)
            //{
            //    pbls = pbls.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan  || ListPB.Contains(x.TenNgan)).ToList();
            //    res = res.Where(x => x.ID_PhongBan_BG == TaiKhoan.ID_PhongBan || x.ID_PhongBan_BN == TaiKhoan.ID_PhongBan || ListPBInt.Contains(x.ID_PhongBan_BG)|| ListPBInt.Contains(x.ID_PhongBan_BN)).ToList();
            //}
            //else if(TaiKhoan.ID_Quyen == 4)
            //{
            //    res = res.Where(x => x.ID_PhongBan_BG == TaiKhoan.ID_PhongBan || x.ID_PhongBan_BN == TaiKhoan.ID_PhongBan).ToList();
            //    pbls = pbls.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).ToList();
            //}
            // Filter Quyền Bổ sung
            if(TaiKhoan.ID_Quyen > 3)
            {
                if(TaiKhoan.ID_Quyen != 8)
                {
                    pbls = pbls.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan || ListPB.Contains(x.TenNgan)).ToList();
                    res = res.Where(x => x.ID_PhongBan_BG == TaiKhoan.ID_PhongBan || x.ID_PhongBan_BN == TaiKhoan.ID_PhongBan || ListPBInt.Contains(x.ID_PhongBan_BG) || ListPBInt.Contains(x.ID_PhongBan_BN)).ToList();
                }
                
            }
           
            var myList = new List<SelectListItem>
            {
                new SelectListItem { Text = "1A", Value = "1A" },
                new SelectListItem { Text = "1B", Value = "1B" },
                new SelectListItem { Text = "1C", Value = "1C" },
                new SelectListItem { Text = "2A", Value = "2A" },
                new SelectListItem { Text = "2B", Value = "2B" },
                new SelectListItem { Text = "2C", Value = "2C" }
            };

            ViewBag.Kip = new SelectList(myList, "Value", "Text");
            ViewBag.PBList = new SelectList(pbls, "ID_PhongBan", "TenPhongBan", ID_PhongBan);
            ViewBag.PBListBG = new SelectList(pbls, "ID_PhongBan", "TenPhongBan", ID_PhongBanBG);
            ViewBag.PBListBN = new SelectList(pbls, "ID_PhongBan", "TenPhongBan", ID_PhongBanBN);
            ViewBag.ID_Xuong = ID_Xuong ?? 0;
            ViewBag.ID_XuongBG = ID_XuongBG??0;
            ViewBag.ID_XuongBN = ID_XuongBN??0;
            //res = res.Where(x => x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay).ToList();

            if (ID_PhongBan != null) res = res.Where(x => x.ID_PhongBan_BG == ID_PhongBan || x.ID_PhongBan_BN == ID_PhongBan).ToList();
            if (ID_Xuong != null) res = res.Where(x => x.ID_Xuong_BG == ID_Xuong || x.ID_Xuong_BN == ID_Xuong).ToList();

            if (ID_PhongBanBG != null ) res = res.Where(x => x.ID_PhongBan_BG == ID_PhongBanBG).ToList();
            if (ID_XuongBG != null) res = res.Where(x => x.ID_Xuong_BG == ID_XuongBG).ToList();

            if (ID_PhongBanBN != null) res = res.Where(x => x.ID_PhongBan_BN == ID_PhongBanBN).ToList();
            if (ID_XuongBN != null) res = res.Where(x => x.ID_Xuong_BN == ID_XuongBN).ToList();

            if (ID_QuyTrinh != null) res = res.Where(x => x.ID_QuyTrinh == ID_QuyTrinh).ToList();
            if (search != null) res =res.Where(x=>x.SoPhieu.Contains(search) || !string.IsNullOrEmpty(x.NoiDungTrichYeu) && x.NoiDungTrichYeu.Contains(search)).ToList();
            var selectedValues = new List<string>();
            if (Kip != null) {
                List<string> ls = Kip.Split(",").ToList();
                if(ls != null)
                {
                    var BBGNFilter = new List<Tbl_BienBanGiaoNhan>();
                    foreach(var item in ls)
                    {
                        var ad = res.Where(x => x.SoPhieu.Contains(item)).ToList();
                        BBGNFilter.AddRange(ad);
                        selectedValues.Add(item);
                    }
                    res = BBGNFilter;
                }
                
            }
            
            ViewBag.KipSelec = selectedValues.ToArray();
            var items = new List<SelectListItem>
            {
                new SelectListItem { Text = "Tạo phiếu", Value = "1" },
                new SelectListItem { Text = "Bổ sung phiếu", Value = "2" },
                new SelectListItem { Text = "Đề nghị hiệu chỉnh", Value = "3" }
            };
            ViewBag.QuyTrinh = new SelectList(items, "Value", "Text", ID_QuyTrinh);
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

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x=>x.PhongBan.Contains(TenBP) && x.ID_TrangThai ==1)
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
        public async Task<IActionResult> TaoPhieu(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, IFormFile FileDinhKem)
        {
            int IDTaiKhoan = 0;
            int IDKip = 0;
            string XacNhan = "";
            int BBGN_ID = 0;
            string ID_Day = "";
            string ID_ca = "";
            string NoiDungTrichYeu = "";
            string filedk = "";
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
                        NoiDungTrichYeu = formCollection["NoiDungTrichYeu"];
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "IDCa" && key.Key != "FileDinhKem" && key.Key != "NoiDungTrichYeu" && key.Key != "NoiDungTrichYeu" && key.Key != "IDTaiKhoan" && key.Key != "xacnhan" && key.Key == "ghichu_" + key.Key.Split('_')[1] )
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
                    int Kip = Convert.ToInt32(ID_ca); // 1 ngày 2 đêm
                    //var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == Kip).FirstOrDefault();
                    var ID_Kip = _context.Tbl_Kip.Where(x => x.TenCa == ID_ca && x.NgayLamViec == NgayXuLy).FirstOrDefault();
                    // Kiểm tra lại thông tin ca kíp làm việc 
                    if (ID_Kip == null)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại ca kíp làm việc');</script>";
                        return RedirectToAction("TaoPhieu", "BM_11");
                    }

                    // Kiểm tra điều kiện không nhập dữ liệu 
                    if (Tbl_ChiTiet_BienBanGiaoNhan.Count == 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng điền thông tin vật tư giao nhận');</script>";
                        return RedirectToAction("TaoPhieu", "BM_11");
                    }
                    var checkList = Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.KhoiLuong_BG == 0 ).ToList();
                    if(checkList.Count() != 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Khối lượng bên giao không để trống');</script>";
                        return RedirectToAction("TaoPhieu", "BM_11");
                    }


                    //DateTime Day = DateTime.Now;
                    //string Day_Convert = Day.ToString("dd-MM-yyyy");
                    DateTime ThoiGianXuLyBG = DateTime.ParseExact(day_bs, "dd-MM-yyyy", CultureInfo.InvariantCulture); // ngày chọn

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

                        //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},@ID_BBGN OUTPUT",
                        //                                       ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                        //                                       ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_Kip.ID_Kip, 0, 1, ID_Kip.TenKip, ID_Kip.TenCa, Output_ID_BBGN);
                        //BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        // // insert chi tiết BBGN
                        //foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        //{
                        //    double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        //    //double KL_QuyKho = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        //    double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                        //    if (BBGN_ID != 0)
                        //    { 
                        //        if(item.MaLo != "" && item.MaLo != null)
                        //        {

                        //            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        //                                                     item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);

                        //        }  
                        //        else
                        //        {
                        //            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        //                                                   item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                        //        }    
                              

                        //    }

                        //}
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                    else if (XacNhan == "1" && XacNhan != "") // gửi phiếu
                    {
                        if(NoiDungTrichYeu == "" || NoiDungTrichYeu == null)
                        {
                            var vattu = _context.Tbl_VatTu.Where(x => x.ID_VatTu == Tbl_ChiTiet_BienBanGiaoNhan[0].ID_VatTu).FirstOrDefault();
                            NoiDungTrichYeu = vattu.TenVatTu;
                        }
                        // file đính kèm
                        if (FileDinhKem != null && FileDinhKem.Length > 0)
                        {
                            // Lấy tên file gốc và phần mở rộng
                            var originalFileName = Path.GetFileNameWithoutExtension(FileDinhKem.FileName);
                            var fileExtension = Path.GetExtension(FileDinhKem.FileName);

                            // Tạo tên file mới với thời gian
                            var newFileName = $"{originalFileName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                            // Đường dẫn lưu file (thư mục cần tồn tại trước)
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", newFileName);

                            // Lưu file vào server
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                FileDinhKem.CopyTo(stream);
                            }

                            // Lưu path vào DB
                            if (path != "" && path != null)
                            {
                                // lưu vào đường dẫn tương đối
                                filedk = $"/uploads/{newFileName}";
                            }

                            //ViewBag.Message = "File uploaded successfully: " + fileName;
                        }

                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},@ID_BBGN OUTPUT",
                                                               ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 1,
                                                               ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_Kip.ID_Kip, 0, 1, ID_Kip.TenKip, ID_Kip.TenCa, NoiDungTrichYeu,filedk, Output_ID_BBGN);
                        BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        {
                            double QuyKho = item.KhoiLuong_BG * 100 - item.DoAm_W* item.KhoiLuong_BG;
                            //double KL_QuyKho = Math.Round(dividedNumber, 3, MidpointRounding.AwayFromZero);
                            // Chia cho 100 và làm tròn chính xác đến 3 chữ số thập phân
                            double dividedNumber = QuyKho / 100;
                            double KL_QuyKho = RoundLikeExcel(dividedNumber, 3);
                            if (BBGN_ID != 0)
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {

                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu, BBGN_ID);
                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu, BBGN_ID);
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


            var MaLo = await (from a in _context.Tbl_MaLo
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
                        //double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        //// Làm tròn đến 4 chữ số thập phân
                        //double roundedTo4 = Math.Round(QuyKho, 4, MidpointRounding.AwayFromZero);
                        //double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(roundedTo4, 3), 3, MidpointRounding.ToEven);
                        double QuyKho = item.KhoiLuong_BG * 100 - item.DoAm_W * item.KhoiLuong_BG;
                        double dividedNumber = QuyKho / 100;
                        double KL_QuyKho = RoundLikeExcel(dividedNumber, 3);
                        if (ID_CT == null)
                        {
                            if (item.MaLo != "" && item.MaLo != null)
                            {
                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                            item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BN, item.KL_QuyKho_BN, item.GhiChu, id);

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
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                           ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);

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

                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                        item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);

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

                        //double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        //// Làm tròn đến 4 chữ số thập phân
                        //double roundedTo4 = Math.Round(QuyKho, 4, MidpointRounding.AwayFromZero);
                        //double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(roundedTo4, 3), 3, MidpointRounding.ToEven);
                        double QuyKho = item.KhoiLuong_BG * 100 - item.DoAm_W * item.KhoiLuong_BG;
                        double dividedNumber = QuyKho / 100;
                        double KL_QuyKho = RoundLikeExcel(dividedNumber, 3);
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


            var NhanVien_TT = await (from a in _context.Tbl_ThongKeXuong.Where(x=>x.ID_Xuong ==TaiKhoan.ID_PhanXuong)
                                     join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                  }).ToListAsync();

            ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");



            //var NhanVien_TT_View = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
            //                         select new Tbl_TaiKhoan
            //                         {
            //                             ID_TaiKhoan = a.ID_TaiKhoan,
            //                             HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
            //                         }).ToListAsync();

            //var NhanVien_TT_View = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong)
            //                         join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
            //                         select new Tbl_TaiKhoan
            //                         {
            //                             ID_TaiKhoan = a.ID_TaiKhoan,
            //                             HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
            //                         }).ToListAsync();


            ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP) && x.ID_TrangThai == 1)
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
        public async Task<IActionResult> BoSungPhieu(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, IFormFile FileDinhKem)
        {
            int IDTaiKhoan = 0;
            int IDNhanVienTT = 0;
            int IDNhanVien_TT_View = 0;
            string ID_Day = "";
            string ID_ca = "";
            //string ID_KipLV = "";
            string XacNhan = "";
            int BBGN_ID = 0;
            string NoiDungTrichYeu = "";
            string filedk = "";
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
                        NoiDungTrichYeu = formCollection["NoiDungTrichYeu"];
                    }
                    if (key.Key != "__RequestVerificationToken" &&  key.Key != "FileDinhKem" && key.Key != "NoiDungTrichYeu" && key.Key != "IDTaiKhoan" && key.Key != "NhanVienTT"
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
                var checkList = Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.KhoiLuong_BG == 0).ToList();
                if (checkList.Count() != 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Khối lượng bên giao không để trống');</script>";
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

                    // Kiểm tra thời gian được tạo bổ sung phiếu không
                    var ThoigianXL = _context.Tbl_ThoiGianKhoa.Where(x => x.Nam == ThoiGianXuLyBG.Year && x.Thang == ThoiGianXuLyBG.Month && !x.IsLock).FirstOrDefault();
                    if(ThoigianXL != null)
                    {
                        TempData["msgSuccess"] = "<script>alert('P.KH đã khóa tạo phiếu trong thời gian này');</script>";
                        return RedirectToAction("BoSungPhieu", "BM_11");
                    }

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
                                        NgayXuLy.ToString("dd") + NgayXuLy.ToString("MM") + NgayXuLy.ToString("yy") + stt+"-BS";

                    var Output_ID_BBGN = new SqlParameter
                    {
                        ParameterName = "ID_BBGN",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = System.Data.ParameterDirection.Output,
                    };
                    if (XacNhan == "0" && XacNhan != "")
                    {

                        //// Thông tin phê duyệt nhân viên thống kê 
                        //if (IDNhanVienTT == 0)
                        //{
                        //    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                        //    return RedirectToAction("BoSungPhieu", "BM_11");
                        //}
                        //// insert BBGN mới từ BBGN cũ
                        //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},@ID_BBGN OUTPUT",
                        //                                       ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                        //                                       ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, Kip, 0, 2, ID_Kip.TenKip,ID_Kip.TenCa, Output_ID_BBGN);
                        //BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        //foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        //{
                        //    if (BBGN_ID != 0)
                        //    {
                        //        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        //        //double KL_QuyKho = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        //        double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);

                        //        if (item.MaLo != "" && item.MaLo != null)
                        //        {

                        //            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        //                                                    item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);

                        //        }
                        //        else
                        //        {
                        //            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        //                                                   item.ID_VatTu,"", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                        //        }
                        //    }

                        //}
                        //// Thêm mới thông tin trình ký bổ sung
                        //var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", BBGN_ID, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 0);

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
                        if (NoiDungTrichYeu == "" || NoiDungTrichYeu == null)
                        {
                            var vattu = _context.Tbl_VatTu.Where(x => x.ID_VatTu == Tbl_ChiTiet_BienBanGiaoNhan[0].ID_VatTu).FirstOrDefault();
                            NoiDungTrichYeu = vattu.TenVatTu;
                        }
                        // file đính kèm
                        if (FileDinhKem != null && FileDinhKem.Length > 0)
                        {
                            // Lấy tên file gốc và phần mở rộng
                            var originalFileName = Path.GetFileNameWithoutExtension(FileDinhKem.FileName);
                            var fileExtension = Path.GetExtension(FileDinhKem.FileName);

                            // Tạo tên file mới với thời gian
                            var newFileName = $"{originalFileName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                            // Đường dẫn lưu file (thư mục cần tồn tại trước)
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", newFileName);

                            // Lưu file vào server
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                FileDinhKem.CopyTo(stream);
                            }

                            // Lưu path vào DB
                            if (path != "" && path != null)
                            {
                                // lưu vào đường dẫn tương đối
                                filedk = $"/uploads/{newFileName}";
                            }

                            //ViewBag.Message = "File uploaded successfully: " + fileName;
                        }
                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},@ID_BBGN OUTPUT",
                                                               ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 1,
                                                               ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, Kip, 0, 2, ID_Kip.TenKip, ID_Kip.TenCa,NoiDungTrichYeu,filedk, Output_ID_BBGN);
                        BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        {
                            if (BBGN_ID != 0)
                            {
                                //double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                                //// Làm tròn đến 4 chữ số thập phân
                                //double roundedTo4 = Math.Round(QuyKho, 4, MidpointRounding.AwayFromZero);
                                //double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(roundedTo4, 3), 3, MidpointRounding.ToEven);
                                double QuyKho = item.KhoiLuong_BG * 100 - item.DoAm_W * item.KhoiLuong_BG;
                                double dividedNumber = QuyKho / 100;
                                double KL_QuyKho = RoundLikeExcel(dividedNumber, 3);


                                if (item.MaLo != "" && item.MaLo != null)
                                {

                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                             item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu, BBGN_ID);

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                             item.ID_VatTu,"", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu, BBGN_ID);
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
                            var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", BBGN_ID, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 0);
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

            return RedirectToAction("Index_Detai", "XuLyPhieu", new { id = BBGN_ID });
        }


        public async Task<IActionResult> TaoPhieuHieuChinh(int id)
        {
            int BBGN_ID = 0;
            try
            {
                // Phát sinh phiếu mới
                var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                var checkSPhieu = ID_BBGN.SoPhieu.Split("_")[0];
                var ID_BBGNHC = _context.Tbl_BienBanGiaoNhan.Where(x => x.SoPhieu.Contains(checkSPhieu+"_HC")).ToList();
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
                //string SoPhieu = ThongTin_BP_BG.TenNgan + "-" + ThongTin_BP_BN.TenNgan + "-" + ID_Kip.TenCa + ID_Kip.TenKip + "-" +
                //                    ThoiGianXuLyBG.ToString("dd") + ThoiGianXuLyBG.ToString("MM") + ThoiGianXuLyBG.ToString("yy") + stt+"-HC.";
                int so = ID_BBGNHC.Count() + 1;
                string SoPhieu = checkSPhieu.Trim() + "_HC." + so;

                var Output_ID_BBGN = new SqlParameter
                {
                    ParameterName = "ID_BBGN",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output,
                };
                var result_new = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},@ID_BBGN OUTPUT",
                                                          ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                                                          ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_BBGN.ID_Kip, 0, 3, ID_Kip.TenKip, ID_Kip.TenCa, ID_BBGN.NoiDungTrichYeu, Output_ID_BBGN);
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
                //update BBGN cũ
                //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 3);
                var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 3); // cập nhật tình trạng phiếu cũ thành ĐNHC
                var check = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == BBGN_ID).FirstOrDefault(); // check bo sung phieu mới
                if (check == null) // insert ký duyệt P.KH
                {
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", BBGN_ID, 0, 0, DateTime.Now, 0);
                }
                else
                {
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", check.ID_TrinhKy, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Đề nghị hiệu chỉnh thất bại');</script>";
            }


            return RedirectToAction("YCauHieuChinh", "XuLyPhieu", new { id = BBGN_ID});
        }

        static double RoundLikeExcel(double value, int decimals)
        {
            // Chuyển đổi sang decimal để đảm bảo độ chính xác
            decimal decimalValue = (decimal)value;

            // Nhân giá trị với 10^decimals để loại bỏ phần thập phân
            decimal multiplier = (decimal)Math.Pow(10, decimals);
            decimal scaledValue = decimalValue * multiplier;

            // Làm tròn giá trị scaledValue về số nguyên gần nhất (MidpointRounding.AwayFromZero)
            decimal roundedValue = Math.Round(scaledValue, 0, MidpointRounding.AwayFromZero);

            // Chia ngược lại để đưa về dạng thập phân với đúng số chữ số
            return (double)(roundedValue / multiplier);
        }

        static double AdjustIfLastDigitIsFive(double number, int precision)
        {
            // Tăng độ chính xác (dịch dấu thập phân để lấy chữ số cuối)
            //double multiplier = Math.Pow(10, precision + 1);
            //double shiftedNumber = number * multiplier;

            //// Lấy phần nguyên để kiểm tra chữ số cuối
            //int lastDigit = (int)Math.Abs(shiftedNumber) % 10;

            //// Nếu chữ số cuối là 5, cộng thêm một lượng nhỏ
            //if (lastDigit == 5)
            //{
            //    double adjustment = 1 / multiplier;
            //    number += adjustment; // Cộng thêm lượng nhỏ
            //}
            // Tính hệ số để làm tròn
            //double factor = Math.Pow(10, precision);
            //// Làm tròn số nhân lên, rồi chia ngược lại
            ////double roundedValue = Math.Round(number * factor, MidpointRounding.ToEven) / factor;
            //// Xác định giá trị trung điểm
            //double multiplied = number * factor;
            //double midpoint = Math.Truncate(multiplied) + 0.5;

            //// Kiểm tra nếu lớn hơn hoặc bằng trung điểm, làm tròn lên
            //if (multiplied >= midpoint)
            //    return Math.Ceiling(multiplied) / factor;
            //else
            //    return Math.Floor(multiplied) / factor;

            double factor = Math.Pow(10, precision + 1);
            return Math.Round(number * factor) / factor;

            //return roundedValue;
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
        public async Task<IActionResult> Xuong(int IDPhongBan)
        {
            if (IDPhongBan == null) IDPhongBan = 0;

            var xuong = _context.Tbl_Xuong.Where(x => x.ID_PhongBan == IDPhongBan).ToList();

            return Json(xuong);
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

            var NguyenLieu = await (from a in _context.Tbl_VatTu.Where(x=>x.ID_TrangThai ==1)                 
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

        public async Task<IActionResult> TaiKhoanTK(int IDTaiKhoan)
        {
            var taikhoan = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();
            var res = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == taikhoan.ID_PhanXuong)
                                          join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                          select new Tbl_TaiKhoan
                                          {
                                              ID_TaiKhoan = a.ID_TaiKhoan,
                                              HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                          }).ToListAsync();
            return Json(res);
        }

        public async Task<IActionResult> KipN(DateTime? Ngay, string Ca)
        {
            DateTime day_datetime = (DateTime)Ngay;
            string Day = day_datetime.ToString("dd-MM-yyyy");
            DateTime Day_Convert = DateTime.ParseExact(Day, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
            var CaKip = await (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == Day_Convert && x.TenCa == Ca)
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
            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            var chitiet_BBGN = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).ToList();
            ViewBag.ChiTietBBGN = chitiet_BBGN;

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

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP) && x.ID_TrangThai == 1)
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
        public async Task<IActionResult> SaoChepPhieu(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, IFormFile FileDinhKem)
        {
            int IDTaiKhoan = 0;
            int IDKip = 0;
            string XacNhan = "";
            int BBGN_ID = 0;
            string ID_Day = "";
            string ID_ca = "";
            string NoiDungTrichYeu = "";
            string filedk = "";
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
                        NoiDungTrichYeu = formCollection["NoiDungTrichYeu"];
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "IDCa" && key.Key != "FileDinhKem" && key.Key != "NoiDungTrichYeu" && key.Key != "NoiDungTrichYeu" && key.Key != "IDTaiKhoan" && key.Key != "xacnhan" && key.Key == "ghichu_" + key.Key.Split('_')[1])
                    {
                        Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                        {
                            ID_VatTu = Convert.ToInt32(formCollection["VatTu_" + key.Key.Split('_')[1]]),
                            MaLo = formCollection["lo_" + key.Key.Split('_')[1]],
                            DoAm_W = double.TryParse(formCollection["doam_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0,
                            KhoiLuong_BG = double.TryParse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 0,
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
                    int Kip = Convert.ToInt32(ID_ca); // 1 ngày 2 đêm
                    //var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == Kip).FirstOrDefault();
                    var ID_Kip = _context.Tbl_Kip.Where(x => x.TenCa == ID_ca && x.NgayLamViec == NgayXuLy).FirstOrDefault();
                    // Kiểm tra lại thông tin ca kíp làm việc 
                    if (ID_Kip == null)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại ca kíp làm việc');</script>";
                        return RedirectToAction("TaoPhieu", "BM_11");
                    }

                    // Kiểm tra điều kiện không nhập dữ liệu 
                    if (Tbl_ChiTiet_BienBanGiaoNhan.Count == 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng điền thông tin vật tư giao nhận');</script>";
                        return RedirectToAction("TaoPhieu", "BM_11");
                    }
                    var checkList = Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.KhoiLuong_BG == 0).ToList();
                    if (checkList.Count() != 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Khối lượng bên giao không để trống');</script>";
                        return RedirectToAction("TaoPhieu", "BM_11");
                    }


                    //DateTime Day = DateTime.Now;
                    //string Day_Convert = Day.ToString("dd-MM-yyyy");
                    DateTime ThoiGianXuLyBG = DateTime.ParseExact(day_bs, "dd-MM-yyyy", CultureInfo.InvariantCulture); // ngày chọn

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

                        //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},@ID_BBGN OUTPUT",
                        //                                       ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 0,
                        //                                       ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_Kip.ID_Kip, 0, 1, ID_Kip.TenKip, ID_Kip.TenCa, Output_ID_BBGN);
                        //BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        // // insert chi tiết BBGN
                        //foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        //{
                        //    double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        //    //double KL_QuyKho = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        //    double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                        //    if (BBGN_ID != 0)
                        //    { 
                        //        if(item.MaLo != "" && item.MaLo != null)
                        //        {

                        //            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        //                                                     item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);

                        //        }  
                        //        else
                        //        {
                        //            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        //                                                   item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, BBGN_ID);
                        //        }    


                        //    }

                        //}
                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                    else if (XacNhan == "1" && XacNhan != "") // gửi phiếu
                    {
                        if (NoiDungTrichYeu == "" || NoiDungTrichYeu == null)
                        {
                            var vattu = _context.Tbl_VatTu.Where(x => x.ID_VatTu == Tbl_ChiTiet_BienBanGiaoNhan[0].ID_VatTu).FirstOrDefault();
                            NoiDungTrichYeu = vattu.TenVatTu;
                        }
                        // file đính kèm
                        if (FileDinhKem != null && FileDinhKem.Length > 0)
                        {
                            // Lấy tên file gốc và phần mở rộng
                            var originalFileName = Path.GetFileNameWithoutExtension(FileDinhKem.FileName);
                            var fileExtension = Path.GetExtension(FileDinhKem.FileName);

                            // Tạo tên file mới với thời gian
                            var newFileName = $"{originalFileName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                            // Đường dẫn lưu file (thư mục cần tồn tại trước)
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", newFileName);

                            // Lưu file vào server
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                FileDinhKem.CopyTo(stream);
                            }

                            // Lưu path vào DB
                            if (path != "" && path != null)
                            {
                                // lưu vào đường dẫn tương đối
                                filedk = $"/uploads/{newFileName}";
                            }

                            //ViewBag.Message = "File uploaded successfully: " + fileName;
                        }

                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},@ID_BBGN OUTPUT",
                                                               ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 1,
                                                               ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_Kip.ID_Kip, 0, 1, ID_Kip.TenKip, ID_Kip.TenCa, NoiDungTrichYeu, filedk, Output_ID_BBGN);
                        BBGN_ID = Convert.ToInt32(Output_ID_BBGN.Value);

                        foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                        {
                            //double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                            //// Làm tròn đến 4 chữ số thập phân
                            //double roundedTo4 = Math.Round(QuyKho, 4, MidpointRounding.AwayFromZero);
                            //double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(roundedTo4, 3), 3, MidpointRounding.ToEven);
                            double QuyKho = item.KhoiLuong_BG * 100 - item.DoAm_W * item.KhoiLuong_BG;
                            double dividedNumber = QuyKho / 100;
                            double KL_QuyKho = RoundLikeExcel(dividedNumber, 3);
                            if (BBGN_ID != 0)
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {

                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu, BBGN_ID);
                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu, BBGN_ID);
                                }
                            }

                        }
                        TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
                        return RedirectToAction("Index_Detai", "BM_11", new { id = BBGN_ID });
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
                        Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 2);
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
                        Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 2);
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
        public async Task<IActionResult> ExportToExcel_All(string search, int? ID_QuyTrinh, DateTime? begind, DateTime? endd, int? ID_PhongBan, int? ID_Xuong, int? ID_PhongBanBG, int? ID_XuongBG, int? ID_PhongBanBN, int? ID_XuongBN, string Kip)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("BBGN");

                DateTime Now = DateTime.Now;
                DateTime startDay = Now.AddDays(-1);
                DateTime endDay = Now;
                if (begind != null) startDay = (DateTime)begind;
                if (endd != null) endDay = (DateTime)endd;

                int row = 8, stt = 0, icol = 1;
                var BBGN = _context.Tbl_BienBanGiaoNhan.Where(x=>!x.IsDelete).ToList();
                if (startDay != null & endDay != null)
                {
                    BBGN = BBGN.Where(x => x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay).ToList();
                }
                if (ID_QuyTrinh != null)
                {
                    BBGN = BBGN.Where(x => x.ID_QuyTrinh == ID_QuyTrinh).ToList();
                }
                if (ID_PhongBan != null) BBGN = BBGN.Where(x => x.ID_PhongBan_BG == ID_PhongBan || x.ID_PhongBan_BN == ID_PhongBan).ToList();
                if (ID_Xuong != null) BBGN = BBGN.Where(x => x.ID_Xuong_BG == ID_Xuong || x.ID_Xuong_BN == ID_Xuong).ToList();
                if (ID_PhongBanBG != null) BBGN = BBGN.Where(x => x.ID_PhongBan_BG == ID_PhongBanBG).ToList();
                if (ID_XuongBG != null) BBGN = BBGN.Where(x => x.ID_Xuong_BG == ID_XuongBG).ToList();
                if (ID_PhongBanBN != null) BBGN = BBGN.Where(x => x.ID_PhongBan_BN == ID_PhongBanBN).ToList();
                if (ID_XuongBN != null) BBGN = BBGN.Where(x => x.ID_Xuong_BN == ID_XuongBN).ToList();
                //if (Kip != null) BBGN = BBGN.Where(x => x.SoPhieu.Contains(Kip)).ToList();
                if (Kip != null)
                {
                    List<string> ls = Kip.Split(",").ToList();
                    if (ls != null)
                    {
                        var BBGNFilter = new List<Tbl_BienBanGiaoNhan>();
                        foreach (var item in ls)
                        {
                            var ad = BBGN.Where(x => x.SoPhieu.Contains(item)).ToList();
                            BBGNFilter.AddRange(ad);
                        }
                        BBGN = BBGNFilter;
                    }

                }

                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
                // check phòng ban list
                List<int> ListPBInt = new List<int>();
                List<string> ListPB = new List<string>();
                if (TaiKhoan.PhongBan_Them != null)
                {
                    ListPB = TaiKhoan.PhongBan_Them.Split(',').Select(item => item.Trim()).ToList();
                    foreach (var item in ListPB)
                    {
                        var pb = _context.Tbl_PhongBan.Where(x => x.TenNgan == item).FirstOrDefault();
                        if (pb != null) ListPBInt.Add(pb.ID_PhongBan);
                    }
                }
                if (TaiKhoan.ID_Quyen == 4)
                {
                    BBGN = BBGN.Where(x => x.ID_PhongBan_BG == TaiKhoan.ID_PhongBan || x.ID_PhongBan_BN == TaiKhoan.ID_PhongBan).ToList();
                }
                if(TaiKhoan.ID_Quyen == 7)
                {
                    BBGN = BBGN.Where(x => x.ID_PhongBan_BG == TaiKhoan.ID_PhongBan || x.ID_PhongBan_BN == TaiKhoan.ID_PhongBan || ListPBInt.Contains(x.ID_PhongBan_BG)|| ListPBInt.Contains(x.ID_PhongBan_BN)).ToList();
                }
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
                        Worksheet.Cell(row, icol).Value = ID_VT?.TenVatTu;
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
                        Worksheet.Cell(row, icol).Value = ID_VT?.DonViTinh;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KhoiLuong_BN;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        //Worksheet.Cell(row, icol).Style.NumberFormat.Format = "0.000";

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.DoAm_W;// "'" + Math.Round(item.DoAm_W, 2);
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
                        Worksheet.Cell(row, icol).Value = ID_XBN?.TenXuong;
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
                        Worksheet.Cell(row, icol).Value = item.DoAm_W;// "'" + Math.Round(item.DoAm_W, 2);
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
                        Worksheet.Cell(row, icol).Value = ID_XBG?.TenXuong;
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

                        icol++;
                        if (ID_BBGN.ID_TrangThai_BBGN == 0)
                        {
                            Worksheet.Cell(row, icol).Value = "Đang xử lý";
                        }
                        else if (ID_BBGN.ID_TrangThai_BBGN == 1)
                        {
                            Worksheet.Cell(row, icol).Value = "Hoàn thành";
                        }
                        else if (ID_BBGN.ID_TrangThai_BBGN == 2)
                        {
                            Worksheet.Cell(row, icol).Value = "BN Hủy Phiếu";
                        }
                        else if (ID_BBGN.ID_TrangThai_BBGN == 3)
                        {
                            Worksheet.Cell(row, icol).Value = "Đề nghị hiệu chỉnh";
                        }
                        else if (ID_BBGN.ID_TrangThai_BBGN == 4)
                        {
                            Worksheet.Cell(row, icol).Value = "Xóa Phiếu";
                        }
                        else if (ID_BBGN.ID_TrangThai_BBGN == 5)
                        {
                            Worksheet.Cell(row, icol).Value = "PKH Hủy phiếu";
                        }
                        //Worksheet.Cell(row, icol).Value = "Hoàn thành";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }

                }

                Worksheet.Range("A7:T" + (row)).Style.Font.SetFontName("Times New Roman");
                Worksheet.Range("A7:T" + (row)).Style.Font.SetFontSize(13);
                Worksheet.Range("A7:T" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                Worksheet.Range("A7:T" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                // Áp dụng định dạng cho một cột
                //Worksheet.Columns(8,10).Style.NumberFormat.Format = "0.000";
                Worksheet.Column(8).Style.NumberFormat.Format = "0.000";
                Worksheet.Column(9).Style.NumberFormat.Format = "0.00";
                Worksheet.Column(10).Style.NumberFormat.Format = "0.000";
                Worksheet.Column(13).Style.NumberFormat.Format = "0.000";
                Worksheet.Column(14).Style.NumberFormat.Format = "0.00";
                Worksheet.Column(15).Style.NumberFormat.Format = "0.000";
                //Worksheet.Columns(13, 15).Style.NumberFormat.Format = "0.000";

                Workbook.SaveAs(fileNamemaunew);
                byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                string fileName = "Thống kê BBGN" + ".xlsx";
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index_All", "BM_11");
            }
        }

        public async Task<IActionResult> KhoaPhieu()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaPhieu(IFormFile file, DateTime? monthPicker, bool CheckUnlock)
        {
            try {
                if (monthPicker != null && CheckUnlock == false)
                {
                    DateTime tg = (DateTime)monthPicker.Value;
                    var checkTg = _context.Tbl_ThoiGianKhoa.Where(x => x.Thang == tg.Month && x.Nam == tg.Year).FirstOrDefault();
                    if(checkTg == null)
                    {
                        Tbl_ThoiGianKhoa khoa = new Tbl_ThoiGianKhoa()
                        {
                            Nam = tg.Year,
                            Thang = tg.Month,
                            NgayXuLy = DateTime.Now
                        };
                        _context.Tbl_ThoiGianKhoa.Add(khoa);
                        _context.SaveChanges();
                        // khóa tất cả phiếu tháng 11
                        var listPhieu = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBG.Month == tg.Month && x.ThoiGianXuLyBG.Year == tg.Year).ToList();
                        // Cập nhật giá trị cho các trường
                        foreach (var entity in listPhieu)
                        {
                            entity.IsLock = true;
                        }

                        // Lưu thay đổi vào database
                        _context.SaveChanges();
                        TempData["msgError"] = "<script>alert('Xử lý khóa thành công');</script>";
                    }
                    else
                    {
                        TempData["msgError"] = "<script>alert('Thời gian này đã được xử lý trước đó');</script>";
                    }
                }
                else if (monthPicker != null && CheckUnlock == true) // Unlock All
                {
                    DateTime tg = (DateTime)monthPicker.Value;
                    var checkTg = _context.Tbl_ThoiGianKhoa.Where(x => x.Thang == tg.Month && x.Nam == tg.Year).FirstOrDefault();
                    if (checkTg != null)
                    {
                        _context.Tbl_ThoiGianKhoa.Remove(checkTg);
                        _context.SaveChanges();
                        // mở khóa tất cả phiếu tháng
                        var listPhieu = _context.Tbl_BienBanGiaoNhan.Where(x => x.ThoiGianXuLyBG.Month == tg.Month && x.ThoiGianXuLyBG.Year == tg.Year).ToList();
                        // Cập nhật giá trị cho các trường
                        foreach (var entity in listPhieu)
                        {
                            entity.IsLock = false;
                        }
                        // Lưu thay đổi vào database
                        _context.SaveChanges();
                        TempData["msgError"] = "<script>alert('Xử lý mở khóa thành công');</script>";
                    }
                    else
                    {
                        TempData["msgError"] = "<script>alert('Thời gian này chưa được xử lý');</script>";
                    }
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xử lý thất bại');</script>";
            }

            return RedirectToAction("Index_All", "BM_11");
        }

        public async Task<IActionResult> Lock(int id, int page)
        {
            try
            {

                //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_lock {0},{1}", id, 0);
                var phieu = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                phieu.IsLock = true;
                _context.SaveChanges();

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }

            var ID_PhongBanBG = Request.Query["ID_PhongBanBG"];
            var ID_XuongBG = Request.Query["ID_XuongBG"];
            var begind = Request.Query["begind"];
            var endd = Request.Query["endd"];
            var ID_PhongBanBN = Request.Query["ID_PhongBanBN"];
            var ID_XuongBN = Request.Query["ID_XuongBN"];
            var ID_QuyTrinh = Request.Query["ID_QuyTrinh"];
            var Kip = Request.Query["Kip"];
            var search = Request.Query["search"];
            return RedirectToAction("Index_All", "BM_11", new
            {
                ID_PhongBanBG = ID_PhongBanBG,
                ID_XuongBG = ID_XuongBG,
                begind = begind,
                endd = endd,
                ID_PhongBanBN = ID_PhongBanBN,
                ID_XuongBN = ID_XuongBN,
                ID_QuyTrinh = ID_QuyTrinh,
                Kip = Kip,
                search = search
            }); // Quay lại trang danh sách
            //return RedirectToAction("Index_All", "BM_11", new { page = page });
        }
        public async Task<IActionResult> Unlock(int id, int page)
        {
            try
            {

                //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_lock {0},{1}", id, 1);

                var phieu = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                phieu.IsLock = false;
                _context.SaveChanges();

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }
            var ID_PhongBanBG = Request.Query["ID_PhongBanBG"];
            var ID_XuongBG = Request.Query["ID_XuongBG"];
            var begind = Request.Query["begind"];
            var endd = Request.Query["endd"];
            var ID_PhongBanBN = Request.Query["ID_PhongBanBN"];
            var ID_XuongBN = Request.Query["ID_XuongBN"];
            var ID_QuyTrinh = Request.Query["ID_QuyTrinh"];
            var Kip = Request.Query["Kip"];
            var search = Request.Query["search"];
            return RedirectToAction("Index_All", "BM_11", new
            {
                ID_PhongBanBG = ID_PhongBanBG,
                ID_XuongBG = ID_XuongBG,
                begind = begind,
                endd = endd,
                ID_PhongBanBN = ID_PhongBanBN,
                ID_XuongBN = ID_XuongBN,
                ID_QuyTrinh = ID_QuyTrinh,
                Kip = Kip,
                search = search
            }); // Quay lại trang danh sách

            //return RedirectToAction("Index_All", "BM_11", new { page = page });
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
            private readonly DataContext _context;

            public FooterHandler(DataContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }

            //public void HandleEvent(Event @event)
            //{
            //    var httpContext = new HttpContextAccessor().HttpContext;
            //    var pdfEvent = (PdfDocumentEvent)@event;
            //    var pdfPage = pdfEvent.GetPage();
            //    var canvas = new PdfCanvas(pdfPage);
            //    var pageSize = pdfPage.GetPageSize();

            //    // Lấy số trang hiện tại
            //    int pageNumber = pdfEvent.GetDocument().GetPageNumber(pdfPage);
            //    // lấy IDBBGN và mã hóa
            //    var bbgnID = "BIENBANGIAONHAN_BM11.QT05_" + httpContext.Request.RouteValues["id"]?.ToString()+DateTime.Now.ToString("dd/MM/yyyy");

            //    byte[] idBytes = System.Text.Encoding.UTF8.GetBytes(bbgnID.ToString());

            //    // Chuỗi cần tính chiều rộng
            //    string text = Convert.ToBase64String(idBytes);

            //    // Kích thước font
            //    float fontSize = 8;

            //    // Font sử dụng (Helvetica)
            //    var pdfFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

            //    // Tính chiều rộng chuỗi
            //    float textWidth = pdfFont.GetWidth(text, fontSize);

            //    // Vị trí Footer
            //    float x = pageSize.GetWidth() - (textWidth +10); // Vị trí giữa trang
            //    float y = pageSize.GetBottom() + 20; // Cách mép dưới 20 đơn vị

            //    // Vẽ Footer
            //    canvas.BeginText()
            //          .SetFontAndSize(PdfFontFactory.CreateFont("C:/Windows/Fonts/times.ttf", PdfEncodings.IDENTITY_H), 8)
            //          .MoveText(x, y)
            //          .ShowText($"{Convert.ToBase64String(idBytes)}") // Nội dung Footer
            //          .EndText()
            //          .Release();
            //}
            public void HandleEvent(Event @event)
            {
                
                var httpContext = new HttpContextAccessor().HttpContext;
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                var pageSize = page.GetPageSize();
                int IDBBGN = int.Parse(httpContext.Request.RouteValues["id"].ToString());
                var bbgn = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == IDBBGN).FirstOrDefault();
                // lấy IDBBGN và mã hóa
                var bbgnID = bbgn.SoPhieu+"_"+ "BIENBANGIAONHAN_BM11.QT05_" + httpContext.Request.RouteValues["id"]?.ToString() +"_"+ DateTime.Now.ToString("dd/MM/yyyy");
                // Tạo mã QR
                BarcodeQRCode barcodeQRCode = new BarcodeQRCode(bbgnID);
                //ImageData qrImageData = barcodeQRCode.CreateFormXObject(page.GetResources(), pdfDoc);
                PdfFormXObject qrCodeObject = barcodeQRCode.CreateFormXObject(null, pdfDoc);

                // Vị trí và kích thước mã QR
                float qrSize = 50; // Kích thước mã QR
                float x = pageSize.GetRight() - 70; // Cách mép phải 70 đơn vị
                float y = pageSize.GetBottom() + 20; // Cách mép dưới 20 đơn vị

                // Vẽ mã QR lên trang
                PdfCanvas canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);
                //canvas.AddImage(qrImageData, x, y, qrSize, false);
                // Tính toán tỉ lệ (scale) từ qrSize
               
                PdfArray bbox = qrCodeObject.GetBBox();
                float width = bbox.GetAsNumber(2).FloatValue() - bbox.GetAsNumber(0).FloatValue();
                float height = bbox.GetAsNumber(3).FloatValue() - bbox.GetAsNumber(1).FloatValue();
                float scale = qrSize / width;
                // Định vị đối tượng trên canvas
                canvas.ConcatMatrix(scale, 0, 0, scale, x, y);
                canvas.AddXObject(qrCodeObject);
            }
        }

        [HttpPost]
        public IActionResult ProcessSelected(List<int> selectedItems)
        {
            if (selectedItems != null && selectedItems.Any())
            {
                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
                // Xử lý danh sách ID được chọn
                foreach (var id in selectedItems)
                {
                    // Lưu vào database
                    _context.Tbl_PKHXuLyPhieu.Add(new Tbl_PKHXuLyPhieu
                    {
                        ID_BBGN = id,
                        ID_TaiKhoan = TaiKhoan.ID_TaiKhoan,
                        NgayXuLy = DateTime.Now,
                        TinhTrang =true
                    });
                }
                _context.SaveChanges();
            }
            var ID_PhongBanBG = Request.Query["ID_PhongBanBG"];
            var ID_XuongBG = Request.Query["ID_XuongBG"];
            var begind = Request.Query["begind"];
            var endd = Request.Query["endd"];
            var ID_PhongBanBN = Request.Query["ID_PhongBanBN"];
            var ID_XuongBN = Request.Query["ID_XuongBN"];
            var ID_QuyTrinh = Request.Query["ID_QuyTrinh"];
            var Kip = Request.Query["Kip"];
            var search = Request.Query["search"];
            return RedirectToAction("Index_All", "BM_11",new { ID_PhongBanBG = ID_PhongBanBG ,
                ID_XuongBG = ID_XuongBG , begind = begind , endd = endd, ID_PhongBanBN= ID_PhongBanBN,
                ID_XuongBN= ID_XuongBN,
                ID_QuyTrinh= ID_QuyTrinh,
                Kip= Kip,
                search= search
            }); // Quay lại trang danh sách
        }

    }
}
