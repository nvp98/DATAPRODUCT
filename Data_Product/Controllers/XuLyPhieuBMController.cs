using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Layout.Font;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using iText.Barcodes;
using iText.Kernel.Pdf.Xobject;
namespace Data_Product.Controllers
{
    public class XuLyPhieuBMController : Controller
    {
        private readonly DataContext _context;
        //private readonly BM_11Controller _11Controller;
        private readonly ICompositeViewEngine _viewEngine;
        public XuLyPhieuBMController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        //public XuLyPhieuBMController(DataContext _context)
        //{
        //    this._context = _context;
        //}

        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = Now;
            //DateTime startDay = Now.AddDays(-1);
            //DateTime endDay = Now;
            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;
            //var res =_context.Tbl_BBGangLong_GangThoi.Where(x=>x.ID_BBGL==)
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BN = TaiKhoan.ID_TaiKhoan;
            int idtk = TaiKhoan.ID_TaiKhoan;
            ViewBag.IDTest_ = idtk;
            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai_PheDuyet.ToList(), "ID_TrangThai_PheDuyet", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_NhanVien_HRC == TaiKhoan.ID_TaiKhoan || x.ID_NhanVien_QLCL == TaiKhoan.ID_TaiKhoan)
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
        public async Task<IActionResult> PhieudentoiBM(int? id)
        {
            DateTime today = DateTime.Today;
            DateTime yesterday = today.AddDays(-1);

            ViewBag.DefaultDate = today.ToString("yyyy-MM-dd");
            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            var res = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == id).FirstOrDefault();

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();
            if (res.TinhTrang_HRC == 1 && res.TinhTrang_QLCL == 1) ;

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
            ViewBag.TingtrangHRC = res.TinhTrang_HRC;
            var TaiKhoan_QLCL = (from a in _context.Tbl_TaiKhoan
                                 select new Tbl_TaiKhoan
                                 {
                                     ID_TaiKhoan = a.ID_TaiKhoan,
                                     HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                 }).ToList();

            ViewBag.IDTaiKhoan = new SelectList(TaiKhoan_QLCL, "ID_TaiKhoan", "HoVaTen");

            ViewBag.TingtrangQLCL = res.TinhTrang_QLCL;

            ViewBag.Data = id;

            return PartialView();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhieudentoiBM(int? id, IFormCollection formCollection)
        {
            if (id == null)
            {
                return BadRequest(new { success = false, error = "ID không hợp lệ" });
            }

            try
            {
                string xacnhan = formCollection["xacnhan"];


                if (xacnhan == "1")
                {
                    var res = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == id).FirstOrDefault();
                    if (res == null)
                    {
                        return Json(new { success = false, error = "Không tìm thấy bản ghi" });
                    }

                    var MBVN_BG = User.FindFirstValue(ClaimTypes.Name);
                    var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MBVN_BG).FirstOrDefault();
                    var idtaiKhoan = ThongTin_BG.ID_TaiKhoan;

                    var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();
                    string sqlUpdate = "";
                    // Câu lệnh SQL Update đã sửa
                    if (idtaiKhoan == res.ID_NhanVien_HRC)
                    {
                        sqlUpdate = @"
                        UPDATE Tbl_BBGangLong_GangThoi 
                        SET 
                            ID_NhanVien_BG = {0},
                            ID_NhanVien_HRC = {1},
                            ID_PhongBan_HRC = {2},
                            ID_ViTri_HRC = {3},
                            ID_Xuong_HRC = {4},
                            ID_NhanVien_QLCL = {5},
                            ID_PhongBan_QLCL = {6},
                            ID_Xuong_QLCL = {7},
                            ID_ViTri_QLCL = {8},
                            NgayXuly_BG = {9},
                            ID_QuyTrinh = {10},
                            TinhTrang_BBGN = {11},
                            NgayXuLy_HRC={12},
                            NgayXuLy_QLCL={13},
                            TinhTrang_HRC={14}
                            
                        WHERE ID_BBGL = '" + id + " '";

                        await _context.Database.ExecuteSqlRawAsync(sqlUpdate,
                        res.ID_NhanVien_BG,
                        res.ID_NhanVien_HRC,
                        res.ID_PhongBan_HRC,
                        res.ID_ViTri_HRC,
                        res.ID_Xuong_HRC,
                        res.ID_NhanVien_QLCL,
                        res.ID_PhongBan_QLCL,
                        res.ID_Xuong_QLCL,
                        res.ID_ViTri_QLCL,
                        res.NgayXuly_BG,
                        res.ID_QuyTrinh,
                        1, // TinhTrang_BBGN
                        DateTime.Now,
                        DateTime.Now,
                        1,

                        id.Value);
                    }
                    else
                    {
                        sqlUpdate = @"
                        UPDATE Tbl_BBGangLong_GangThoi 
                        SET 
                            ID_NhanVien_BG = {0},
                            ID_NhanVien_HRC = {1},
                            ID_PhongBan_HRC = {2},
                            ID_ViTri_HRC = {3},
                            ID_Xuong_HRC = {4},
                            ID_NhanVien_QLCL = {5},
                            ID_PhongBan_QLCL = {6},
                            ID_Xuong_QLCL = {7},
                            ID_ViTri_QLCL = {8},
                            NgayXuly_BG = {9},
                            ID_QuyTrinh = {10},
                            TinhTrang_BBGN = {11},
                            NgayXuLy_HRC={12},
                            NgayXuLy_QLCL={13},
                            
                            TinhTrang_QLCL={14}
                        WHERE ID_BBGL = '" + id + " '";

                        await _context.Database.ExecuteSqlRawAsync(sqlUpdate,
                        res.ID_NhanVien_BG,
                        res.ID_NhanVien_HRC,
                        res.ID_PhongBan_HRC,
                        res.ID_ViTri_HRC,
                        res.ID_Xuong_HRC,
                        res.ID_NhanVien_QLCL,
                        res.ID_PhongBan_QLCL,
                        res.ID_Xuong_QLCL,
                        res.ID_ViTri_QLCL,
                        res.NgayXuly_BG,
                        res.ID_QuyTrinh,
                        1, // TinhTrang_BBGN
                        DateTime.Now,
                        DateTime.Now,

                        1,
                        id.Value);
                    }





                    TempData["msgSuccess"] = "<script>alert('xác nhận thành công');</script>";
                    return RedirectToAction("Index", "XuLyPhieuBM");
                }
                else if (xacnhan == "0")
                {
                    var res = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == id).FirstOrDefault();
                    if (res == null)
                    {
                        return Json(new { success = false, error = "Không tìm thấy bản ghi" });
                    }

                    var MBVN_BG = User.FindFirstValue(ClaimTypes.Name);
                    var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MBVN_BG).FirstOrDefault();
                    var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();

                    // Câu lệnh SQL Update đã sửa
                    string sqlUpdate = @"
                    UPDATE Tbl_BBGangLong_GangThoi 
                    SET 
                        ID_NhanVien_BG = {0},
                        ID_NhanVien_HRC = {1},
                        ID_PhongBan_HRC = {2},
                        ID_ViTri_HRC = {3},
                        ID_Xuong_HRC = {4},
                        ID_NhanVien_QLCL = {5},
                        ID_PhongBan_QLCL = {6},
                        ID_Xuong_QLCL = {7},
                        ID_ViTri_QLCL = {8},
                        NgayXuly_BG = {9},
                        ID_QuyTrinh = {10},
                        TinhTrang_BBGN = {11},
                        NgayXuLy_HRC={12},
                        NgayXuLy_QLCL={13}
                    WHERE ID_BBGL = '" + id + " '";

                    await _context.Database.ExecuteSqlRawAsync(sqlUpdate,
                        res.ID_NhanVien_BG,
                        res.ID_NhanVien_HRC,
                        res.ID_PhongBan_HRC,
                        res.ID_ViTri_HRC,
                        res.ID_Xuong_HRC,
                        res.ID_NhanVien_QLCL,
                        res.ID_PhongBan_QLCL,
                        res.ID_Xuong_QLCL,
                        res.ID_ViTri_QLCL,
                        res.NgayXuly_BG,
                        res.ID_QuyTrinh,
                        2, // TinhTrang_BBGN
                        DateTime.Now,
                        DateTime.Now,
                        id.Value);


                    TempData["msgSuccess"] = "<script>alert('Hủy phiếu thành công');</script>";
                    return RedirectToAction("Index", "XuLyPhieuBM");
                }

                return Json(new { success = false, error = "Xác nhận không hợp lệ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }

        public async Task<IActionResult> ExportToExcel(int BBGN_ID)
        {
            try
            {
                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGNGLGT.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGNGLGT.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("Sheet1");
                var ID_BBGN = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == BBGN_ID).FirstOrDefault();
                var Data = _context.Tbl_ChiTiet_BBGangLong_GangThoi.Where(x => x.Id_BBGL == BBGN_ID).ToList();
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
                        Worksheet.Cell(row, icol).Value = item.SoMe;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.ThungSo;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KhoiLuongXeGoong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KhoiLuongThung;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KLThungGangLong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KLGangLongCanRay;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.VanChuyenHRC1;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.VanChuyenHRC2;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.PhanLoai;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.Gio;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.GhiChu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }
                    Worksheet.Range("A7:T" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:T" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A7:T" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:T" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "BBGNGLGT - " + ID_BBGN.SoPhieu + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {
                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "BBGNGLGT - " + ID_BBGN.SoPhieu + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<Script>alert('Có lỗi vui lòng kiểm tra lại ');</Srcipt>";
                return RedirectToAction("Index_detail", "BM", new { id = BBGN_ID });
            }
        }
        public async Task<IActionResult> GenerateAndSavePdf(int? id)
        {
            var bbgn = _context.Tbl_BBGangLong_GangThoi.Find(id);
            if (bbgn == null)
            {
                return null;
            }
            // 1. Render Razor View thành chuỗi HTML
            string htmlContent = await RenderViewToStringAsync("ExportPdfView", new { id = id });

            // 2. Chuyển đổi HTML sang PDF
            byte[] pdfBytes = ConvertHtmlToPdf(htmlContent);
            string filename = $"{bbgn.SoPhieu}.pdf";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");

            // Lưu file vào server và trả về path
            string pathsave = SavePdfToFile(pdfBytes, folderPath, filename);
            if (pathsave != "" && pathsave != null)
            {
                bbgn.FileBBGL = pathsave;
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

            public void HandleEvent(Event @event)
            {

                var httpContext = new HttpContextAccessor().HttpContext;
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                var pageSize = page.GetPageSize();
                int IDBBGN = int.Parse(httpContext.Request.RouteValues["id"].ToString());
                var bbgn = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == IDBBGN).FirstOrDefault();
                // lấy IDBBGN và mã hóa
                var bbgnID = bbgn.SoPhieu + "_" + "BIENBANGANGLONG-GANGTHOI BM16.QT05_" + httpContext.Request.RouteValues["id"]?.ToString() + "_" + DateTime.Now.ToString("dd/MM/yyyy");
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
        public async Task<IActionResult> HuyPhieu_PheDuyet(int id)
        {
            try
            {
                // Kiểm tra user đăng nhập
                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                // Lấy thông tin tài khoản
                var TaiKhoan = await _context.Tbl_TaiKhoan
                    .FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);
                // Lấy thông tin BBGN
                var BBGN = await _context.Tbl_BBGangLong_GangThoi
                    .FirstOrDefaultAsync(x => x.ID_BBGL == id);

                var NhanVien_TT = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong)
                                         join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                         select new Tbl_TaiKhoan
                                         {
                                             ID_TaiKhoan = a.ID_TaiKhoan,
                                             HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                         }).ToListAsync();

                ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");

                var NhanVien_TTView = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == BBGN.ID_Xuong_HRC)
                                             join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                             select new Tbl_TaiKhoan
                                             {
                                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                                 HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                             }).ToListAsync();

                ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TTView, "ID_TaiKhoan", "HoVaTen");
                ViewBag.NhanVienBN_HRC = new SelectList(_context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == BBGN.ID_NhanVien_HRC), "ID_TaiKhoan", "HoVaTen");
                ViewBag.NhanVienBN_QLCL = new SelectList(_context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == BBGN.ID_NhanVien_QLCL), "ID_TaiKhoan", "HoVaTen");
                return PartialView();
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây
                // _logger.LogError(ex, "Lỗi trong HuyPhieu_PheDuyet");
                return Json(new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu_PheDuyet(Tbl_XuLyXoaPhieu _DO, int id)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var bbgn = _context.Tbl_BBGangLong_GangThoi.Where(x => x.ID_BBGL == id).FirstOrDefault();
                if (bbgn.TinhTrang_BBGN != 1)
                {
                    TempData["msgError"] = "<script>alert('Phiếu đang được hiệu chỉnh, không thể xóa phiếu này');</script>";
                    return RedirectToAction("Index_Detail", "BM", new { id = id });
                }
                if (_DO.ID_TaiKhoanKH != 0 && _DO.ID_TaiKhoanKH_View != null)
                {
                    var check = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    if (check != null)
                    {
                        check.ID_TaiKhoanBN = _DO.ID_TaiKhoanBN;
                        check.ID_TaiKhoanKH = _DO.ID_TaiKhoanKH;
                        check.TinhTrang_BN = 0;
                        check.TinhTrang_KH = 0;
                        check.ID_BBGN = id;
                        check.ID_TrangThai = 0;
                        check.ID_TaiKhoanKH_View = _DO.ID_TaiKhoanKH_View;
                    }
                    else
                    {
                        Tbl_XuLyXoaPhieu phieu = new Tbl_XuLyXoaPhieu()
                        {
                            ID_BBGN = id,
                            ID_TaiKhoanBN = _DO.ID_TaiKhoanBN,
                            ID_TaiKhoanKH = _DO.ID_TaiKhoanKH,
                            TinhTrang_BN = 0,
                            TinhTrang_KH = 0,
                            ID_TrangThai = 0,
                            ID_TaiKhoanKH_View = _DO.ID_TaiKhoanKH_View
                        };
                        _context.Tbl_XuLyXoaPhieu.Add(phieu);
                    }
                    _context.SaveChanges();
                    var result_BBGN = @"Update Tbl_BBGangLong_GangThoi 
                                         Set
                                                
                                                TinhTrang_BBGN={0}
                                         WHERE ID_BBGL = '" + id + "'";
                    await _context.Database.ExecuteSqlRawAsync(result_BBGN,
                            5);

                }
                else
                {
                    TempData["msgError"] = "<script>alert('Vui lòng chọn Thống kê phê duyệt và nhận thông tin');</script>";
                    return RedirectToAction("confirm_details", "BM", new { id = id });
                }

                TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Trình ký thất bại');</script>";
            }

            return RedirectToAction("Index_Started", "BM");


        }
        public async Task<IActionResult> BN_XoaPhieu(int id, int tinhtrang)
        {
            try
            {
                if (tinhtrang == 0) //không xóa
                {
                    var result_BBGN = @"Update Tbl_BBGangLong_GangThoi 
                                         Set
                                                
                                                TinhTrang_BBGN={0}
                                         WHERE ID_BBGL = '" + id + "'";
                    await _context.Database.ExecuteSqlRawAsync(result_BBGN,
                            5);
                    Tbl_XuLyXoaPhieu xoaphieu = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    _context.Tbl_XuLyXoaPhieu.Remove(xoaphieu);
                    //xoaphieu.TinhTrang_BN = 0;
                    //xoaphieu.NgayXuLy_BN = DateTime.Now;
                    _context.SaveChanges();
                }
                else
                {
                    //var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                    Tbl_XuLyXoaPhieu xoaphieu = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    xoaphieu.TinhTrang_BN = 1;
                    xoaphieu.NgayXuLy_BN = DateTime.Now;
                    _context.SaveChanges();
                }

                TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Hủy phiếu thất bại');</script>";
            }
            return RedirectToAction("Index", "XuLyPhieu");
        }
    }
}
