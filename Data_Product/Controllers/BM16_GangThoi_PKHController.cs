using ClosedXML.Excel;
using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BM16_GangThoi_PKHController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public BM16_GangThoi_PKHController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public async Task<IActionResult> Index()
        {
            // Lò cao
            var loCaoList = await _context.Tbl_LoCao.ToListAsync();
            ViewBag.LoCaoList = loCaoList;

            // Lò Thổi
            var loThoiList = await _context.Tbl_LoThoi.ToListAsync();
            ViewBag.LoThoiList = loThoiList;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchDto dto)
        {
            var data = await SearchByPayload(dto);
            return Ok(data);
        }


        [HttpPost]
        public async Task<IActionResult> ChotThung([FromBody] List<ChotThungDto> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return BadRequest("Danh sách ID trống.");

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            if (TaiKhoan == null) return Unauthorized();

            var Ids = selectedIds
                                .Select(x => x.id)
                                .ToList();

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                                       .Where(x => Ids.Contains(x.ID) && x.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                                       .ToListAsync();

            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");
            foreach (var t in thungs)
            {
                t.ID_TrangThai = (int)TinhTrang.DaChot;
                t.ID_NguoiChot = TaiKhoan.ID_TaiKhoan;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<List<Tbl_BM_16_GangLong>> SearchByPayload(SearchDto dto)
        {
            var query = _context.Tbl_BM_16_GangLong.AsQueryable();

            if (dto.ID_LoCao.HasValue)
            {
                query = query.Where(x => x.ID_Locao == dto.ID_LoCao.Value);
            }

            if (dto.ID_LoThoi.HasValue)
            {
                query = query.Where(x => x.ID_LoThoi == dto.ID_LoThoi.Value);
            }

            if (dto.TuNgay_LG.HasValue && dto.DenNgay_LG.HasValue)
            {
                query = query.Where(x => x.NgayLuyenGang >= dto.TuNgay_LG.Value && x.NgayLuyenGang <= dto.DenNgay_LG.Value);

            }

            if (dto.TuNgay_LT.HasValue && dto.DenNgay_LT.HasValue)
            {
                query = query.Where(x => x.NgayLuyenThep >= dto.TuNgay_LT.Value && x.NgayLuyenThep <= dto.DenNgay_LT.Value);

            }
            if (dto.Ca_LT.HasValue)
            {
                query = query.Where(x => x.T_Ca == dto.Ca_LT.Value);
            }
            if (dto.Ca_LG.HasValue)
            {
                query = query.Where(x => x.G_Ca == dto.Ca_LG.Value);
            }

            if (!string.IsNullOrEmpty(dto.ID_Kip_LT))
            {
                query = query.Where(thung =>
                                    thung.T_ID_Kip != null &&
                                    _context.Tbl_Kip
                                        .Where(k => k.TenKip == dto.ID_Kip_LT)
                                        .Select(k => k.ID_Kip)
                                        .Contains(thung.T_ID_Kip.Value)
                                );
            }

            if (!string.IsNullOrEmpty(dto.ID_Kip_LG))
            {
                query = query.Where(thung =>
                                    thung.G_ID_Kip != null &&
                                    _context.Tbl_Kip
                                        .Where(k => k.TenKip == dto.ID_Kip_LG)
                                        .Select(k => k.ID_Kip)
                                        .Contains(thung.G_ID_Kip.Value)
                                );
            }

            if (!string.IsNullOrEmpty(dto.ChuyenDen))
            {
                query = query.Where(x => x.ChuyenDen.Contains(dto.ChuyenDen));
            }
            if (!string.IsNullOrEmpty(dto.ThungSo))
            {
                query = query.Where(x => x.BKMIS_ThungSo.Contains(dto.ThungSo));
            }
            if (dto.ID_TinhTrang.HasValue)
            {
                query = query.Where(x => x.ID_TrangThai == dto.ID_TinhTrang.Value);
            }
            if (dto.ID_TinhTrang_LT.HasValue)
            {
                query = query.Where(x => x.T_ID_TrangThai == dto.ID_TinhTrang_LT.Value);
            }
            if (dto.ID_TinhTrang_LG.HasValue)
            {
                query = query.Where(x => x.G_ID_TrangThai == dto.ID_TinhTrang_LG.Value);
            }
            if (!string.IsNullOrEmpty(dto.MaThungGang))
            {
                query = query.Where(x => x.MaThungGang.Contains(dto.MaThungGang));
            }
            if (!string.IsNullOrEmpty(dto.MaThungThep))
            {
                query = query.Where(x => x.MaThungThep.Contains(dto.MaThungThep));
            }

            var res = await (from a in query
                             join trangThai in _context.Tbl_BM_16_TrangThai on a.ID_TrangThai equals trangThai.ID into g_tt
                             from trangThai in g_tt.DefaultIfEmpty()

                             join trangThaiLG in _context.Tbl_BM_16_TrangThai on a.G_ID_TrangThai equals trangThaiLG.ID into g_TrangThai
                             from trangThaiLG in g_TrangThai.DefaultIfEmpty()

                             join trangThaiLT in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThaiLT.ID into t_TrangThai
                             from trangThaiLT in t_TrangThai.DefaultIfEmpty()

                             join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID into g_lc
                             from loCao in g_lc.DefaultIfEmpty()

                             join methoi in _context.Tbl_MeThoi on a.ID_MeThoi equals methoi.ID into g_mt
                             from methoi in g_mt.DefaultIfEmpty()

                             join kipG in _context.Tbl_Kip on a.G_ID_Kip equals kipG.ID_Kip into g_kipG
                             from kipG in g_kipG.DefaultIfEmpty()

                             join kipT in _context.Tbl_Kip on a.T_ID_Kip equals kipT.ID_Kip into g_kipT
                             from kipT in g_kipT.DefaultIfEmpty()

                             join thungUser in _context.Tbl_BM_16_TaiKhoan_Thung on a.MaThungThep equals thungUser.MaThungThep into g_thungUser
                             from thungUser in g_thungUser.DefaultIfEmpty()

                             join user in _context.Tbl_TaiKhoan on thungUser.ID_taiKhoan equals user.ID_TaiKhoan into g_user
                             from user in g_user.DefaultIfEmpty()

                             join phongban in _context.Tbl_PhongBan on user.ID_PhongBan equals phongban.ID_PhongBan into g_phongban
                             from phongban in g_phongban.DefaultIfEmpty()

                             orderby a.MaThungGang, a.MaThungThep
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = a.ID,
                                 NgayLuyenGang = a.NgayLuyenGang,
                                 G_Ca = a.G_Ca,
                                 G_TenKip = kipG != null ? kipG.TenKip : null,
                                 MaThungGang = a.MaThungGang,
                                 ID_Locao = a.ID_Locao,
                                 BKMIS_SoMe = a.BKMIS_SoMe,
                                 BKMIS_ThungSo = a.BKMIS_ThungSo,
                                 BKMIS_Gio = a.BKMIS_Gio,
                                 BKMIS_PhanLoai = a.BKMIS_PhanLoai,
                                 KL_XeGoong = a.KL_XeGoong,
                                 G_KLThungChua = a.G_KLThungChua,
                                 G_KLThungVaGang = a.G_KLThungVaGang,
                                 G_KLGangLong = a.G_KLGangLong,
                                 ChuyenDen = a.ChuyenDen,
                                 Gio_NM = a.Gio_NM,
                                 G_ID_TrangThai = a.G_ID_TrangThai,
                                 NgayLuyenThep = a.NgayLuyenThep,
                                 T_Ca = a.T_Ca,
                                 T_TenKip = kipT != null ? kipT.TenKip : null,
                                 MaThungThep = a.MaThungThep != null ? a.MaThungThep : null,
                                 ID_LoThoi = a.ID_LoThoi,
                                 KR = a.KR,
                                 T_KLThungVaGang = a.T_KLThungVaGang,
                                 T_KLThungChua = a.T_KLThungChua,
                                 T_KLGangLong = a.T_KLGangLong,
                                 ThungTrungGian = a.ThungTrungGian,
                                 T_KLThungVaGang_Thoi = a.T_KLThungVaGang_Thoi,
                                 T_KLThungChua_Thoi = a.T_KLThungChua_Thoi,
                                 T_KLGangLongThoi = a.T_KLGangLongThoi,
                                 ID_MeThoi = a.ID_MeThoi,
                                 MaMeThoi = methoi.MaMeThoi,
                                 T_ID_TrangThai = a.T_ID_TrangThai,
                                 ID_TrangThai = a.ID_TrangThai,
                                 TenLoCao = loCao.TenLoCao,
                                 T_KL_phe = a.T_KL_phe,
                                 TrangThai = trangThai.TenTrangThai,
                                 TrangThaiLG = trangThaiLG.TenTrangThai,
                                 TrangThaiLT = trangThaiLT.TenTrangThai,

                                 HoVaTen = user.HoVaTen,
                                 TenPhongBan = phongban.TenNgan
                             }).ToListAsync();
            return res;
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] SearchDto dto)
        {
            try
            {
                var thungList = await SearchByPayload(dto);
                if (thungList == null || !thungList.Any())
                    return BadRequest("Danh sách trống.");

                // Đường dẫn đến template
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "QTGN_Gang_Long_PKH.xlsx");
                using (var ms = new MemoryStream())
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet("Sheet1");

                        // Xóa dữ liệu cũ (nếu có)
                        var lastRow = Math.Max(worksheet.LastRowUsed()?.RowNumber() ?? 8, 8);
                        if (lastRow >= 8)
                        {
                            var rangeClear = worksheet.Range($"A8:AE{lastRow}");
                            rangeClear.Clear(XLClearOptions.Contents | XLClearOptions.NormalFormats);
                        }

                        int row = 8, stt = 1;

                        foreach (var item in thungList)
                        {
                            int icol = 1;

                            worksheet.Cell(row, icol++).Value = stt++;
                            worksheet.Cell(row, icol++).Value = item.NgayLuyenGang?.Day.ToString();
                            worksheet.Cell(row, icol++).Value = item.G_Ca != null && item.G_Ca == 1 ? "N" : "Đ";
                            worksheet.Cell(row, icol++).Value = item.G_TenKip;
                            worksheet.Cell(row, icol++).Value = item.MaThungGang;
                            worksheet.Cell(row, icol++).Value = item.ID_Locao;
                            worksheet.Cell(row, icol++).Value = item.BKMIS_SoMe;
                            worksheet.Cell(row, icol++).Value = item.BKMIS_ThungSo;
                            worksheet.Cell(row, icol++).Value = item.BKMIS_Gio;
                            worksheet.Cell(row, icol++).Value = item.BKMIS_PhanLoai;
                            worksheet.Cell(row, icol++).Value = item.KL_XeGoong;
                            worksheet.Cell(row, icol++).Value = item.G_KLThungChua;
                            worksheet.Cell(row, icol++).Value = item.G_KLThungVaGang;
                            // Cột 13: T_KLGangLong
                            var cell13 = worksheet.Cell(row, icol++);
                            if (item.G_KLGangLong.HasValue)
                            {
                                cell13.Value = item.G_KLGangLong.Value;
                                cell13.Style.NumberFormat.Format = "0.00";
                                cell13.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell13.Style.Font.Bold = true;
                            }
                            worksheet.Cell(row, icol++).Value = item.ChuyenDen;
                            worksheet.Cell(row, icol++).Value = item.Gio_NM;

                            var tinhTrangG_cell= worksheet.Cell(row, icol++);
                            RenderTrangThaiCell(tinhTrangG_cell, item.TrangThaiLG, item.G_ID_TrangThai);

                            worksheet.Cell(row, icol++).Value = item.NgayLuyenThep?.Day.ToString();
                            worksheet.Cell(row, icol++).Value = item.T_Ca != null && item.T_Ca == 1 ? "N" : "Đ";
                            worksheet.Cell(row, icol++).Value = item.T_TenKip;
                            worksheet.Cell(row, icol++).Value = item.MaThungThep;
                            worksheet.Cell(row, icol++).Value = item.ID_LoThoi;
                            worksheet.Cell(row, icol++).Value = item.ThungTrungGian;
                            worksheet.Cell(row, icol++).Value = item.T_KLThungVaGang_Thoi;
                            worksheet.Cell(row, icol++).Value = item.T_KLThungChua_Thoi;

                            // Cột 25: T_KLGangLongThoi
                            var cell25 = worksheet.Cell(row, icol++);
                            if (item.T_KLGangLongThoi.HasValue)
                            {
                                cell25.Value = item.T_KLGangLongThoi.Value;
                                cell25.Style.NumberFormat.Format = "0.00";
                                cell25.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell25.Style.Font.Bold = true;
                            }

                            worksheet.Cell(row, icol++).Value = item.MaMeThoi;

                            var tinhTrangT_cell = worksheet.Cell(row, icol++);
                            RenderTrangThaiCell(tinhTrangT_cell, item.TrangThaiLT, item.T_ID_TrangThai);

                            worksheet.Cell(row, icol++).Value = item.TenPhongBan;
                            worksheet.Cell(row, icol++).Value = item.HoVaTen;

                            var tinhTrang_cell = worksheet.Cell(row, icol++);
                            RenderTrangThaiCell(tinhTrang_cell, item.TrangThai, item.ID_TrangThai);

                            worksheet.Row(row).Height = 25;
                            for (int col = 1; col <= 31; col++)
                            {
                                if (col != 14 && col != 26)
                                    worksheet.Cell(row, col).Style.NumberFormat.SetNumberFormatId(0); // General
                            }

                            row++;
                        }

                        // Dòng tổng
                        int sumRow = row;
                        var totalLabel = worksheet.Range($"A{sumRow}:M{sumRow}");
                        totalLabel.Merge();
                        totalLabel.Value = "Tổng:";
                        totalLabel.Style.Font.SetBold();
                        totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột 14
                        worksheet.Cell(sumRow, 14).FormulaA1 = $"=SUM(N8:N{row - 1})";
                        worksheet.Cell(sumRow, 14).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 14).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge các cột O -> Y (15 -> 25)
                        var mergeRange1 = worksheet.Range(sumRow, 15, sumRow, 25);
                        mergeRange1.Merge();
                        mergeRange1.Value = ""; 
                        mergeRange1.Style.Fill.BackgroundColor = XLColor.White; 

                        // Tổng cột 26
                        worksheet.Cell(sumRow, 26).FormulaA1 = $"=SUM(Z8:Z{row - 1})";
                        worksheet.Cell(sumRow, 26).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 26).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 26).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge các cột AA -> AE (27 -> 31)
                        var mergeRange2 = worksheet.Range(sumRow, 27, sumRow, 31);
                        mergeRange2.Merge();
                        mergeRange2.Value = ""; 
                        mergeRange2.Style.Fill.BackgroundColor = XLColor.White;

                        // Format toàn bảng
                        var usedRange = worksheet.Range($"A7:AE{sumRow}");
                        usedRange.Style.Font.SetFontName("Arial").Font.SetFontSize(11);
                        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        usedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        usedRange.Style.Alignment.WrapText = true;


                        // Chiều cao dòng
                        for (int i = 8; i <= sumRow; i++)
                        {
                            worksheet.Row(i).Height = 25;
                        }

                        // Ghi workbook vào MemoryStream (không lưu ra ổ đĩa)
                        workbook.SaveAs(ms);
                    }
                    // Trả file ra MemoryStream
                    ms.Position = 0;

                    string outputName = $"QTGN_Gang_Long_Gang_Thoi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(ms.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                outputName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index", "BM_GangThoi_PKH");
            }
        }
        private void RenderTrangThaiCell(IXLCell cell, string trangThaiText, int? idTrangThai)
        {
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
    }
}
