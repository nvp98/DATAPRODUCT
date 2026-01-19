using ClosedXML.Excel;
using Data_Product.Models;
using Data_Product.Repositorys;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Data_Product.Controllers
{
    public class CumTBController : Controller
    {
        private readonly DataContext _context;

        public CumTBController(DataContext context)
        {
            _context = context;
        }

        // 🟢 Trang danh sách cụm thiết bị
        public async Task<IActionResult> Index(string search)
        {
            // Lấy danh sách cụm thiết bị
            var data = await _context.Tbl_NhatKy_CumTB
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.ID)
                .ToListAsync();

            // Lọc theo tên
            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(x => x.TenCumTB.Contains(search)).ToList();
            }

            // Nạp danh sách xưởng cho từng cụm
            var mapping = await _context.Tbl_NhatKy_CumTB_Xuong.ToListAsync();
            var xuongs = await _context.Tbl_Xuong.ToListAsync();

            foreach (var item in data)
            {
                var xuongIds = mapping
                    .Where(m => m.CumTB_ID == item.ID)
                    .Select(m => m.Xuong_ID)
                    .ToList();

                item.Xuong = xuongs
                    .Where(x => xuongIds.Contains(x.ID_Xuong))
                    .ToList();
            }

            return View(data);
        }

        // 🟢 Hiển thị modal (cả thêm + sửa)
        [HttpGet]
        public async Task<IActionResult> CumTBModal(int? id)
        {
            Tbl_NhatKy_CumTB model;

            if (id == null) // Tạo mới
            {
                model = new Tbl_NhatKy_CumTB();
                ViewBag.Mode = "Create";
                ViewBag.Title = "THÊM MỚI CỤM THIẾT BỊ";
            }
            else // Sửa
            {
                model = await _context.Tbl_NhatKy_CumTB.FindAsync(id);
                if (model == null) return NotFound();

                // Gán danh sách xưởng đang có
                var xuongMap = await _context.Tbl_NhatKy_CumTB_Xuong
                    .Where(x => x.CumTB_ID == id)
                    .Select(x => x.Xuong_ID)
                    .ToListAsync();

                ViewBag.SelectedXuong = xuongMap;
                ViewBag.Mode = "Edit";
                ViewBag.Title = "CHỈNH SỬA CỤM THIẾT BỊ";
            }

            // Danh sách xưởng chọn
            List<Tbl_Xuong> xuong = await (from a in _context.Tbl_Xuong
                                           join b in _context.Tbl_PhongBan on a.ID_PhongBan equals b.ID_PhongBan
                                           select new Tbl_Xuong
                                           {
                                               ID_Xuong = a.ID_Xuong,
                                               ID_PhongBan = b.ID_PhongBan,
                                               TenXuong = b.TenPhongBan + "-" + a.TenXuong
                                           }
                                ).ToListAsync();
            ViewBag.XuongList = new MultiSelectList(xuong, "ID_Xuong", "TenXuong", ViewBag.SelectedXuong);

            return PartialView("CumTBModal", model);
        }

        // 🧩 Xử lý submit (thêm/sửa)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCumTB(Tbl_NhatKy_CumTB model, int[] Xuong_ID, List<string> TenCumTBs)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TenCumTB))
                {
                    TempData["msgError"] = "<script>alert('Tên cụm thiết bị không được để trống');</script>";
                    return RedirectToAction("Index");
                }

                if (model.ID == 0)
                {
                    // Nếu form có danh sách nhiều cụm (TenCumTBs)
                    if (TenCumTBs != null && TenCumTBs.Count > 0)
                    {
                        foreach (var ten in TenCumTBs)
                        {
                            if (string.IsNullOrWhiteSpace(ten))
                                continue;

                            var newCum = new Tbl_NhatKy_CumTB
                            {
                                TenCumTB = ten.Trim(),
                                IsLock = false,
                                IsDelete = false
                            };

                            _context.Tbl_NhatKy_CumTB.Add(newCum);
                            await _context.SaveChangesAsync(); // cần lưu trước để có ID

                            if (Xuong_ID != null && Xuong_ID.Length > 0)
                            {
                                foreach (var xuong in Xuong_ID)
                                {
                                    _context.Tbl_NhatKy_CumTB_Xuong.Add(new Tbl_NhatKy_CumTB_Xuong
                                    {
                                        CumTB_ID = newCum.ID,
                                        Xuong_ID = xuong
                                    });
                                }
                            }
                        }

                        TempData["msgSuccess"] = "<script>alert('Đã thêm mới nhiều cụm thiết bị thành công');</script>";
                    }
                    if (model.TenCumTB != "")
                    {
                        // ➕ Thêm mới 1 cụm như logic cũ
                        model.IsLock = false;
                        model.IsDelete = false;
                        _context.Tbl_NhatKy_CumTB.Add(model);
                        await _context.SaveChangesAsync();

                        if (Xuong_ID != null && Xuong_ID.Length > 0)
                        {
                            foreach (var xuong in Xuong_ID)
                            {
                                _context.Tbl_NhatKy_CumTB_Xuong.Add(new Tbl_NhatKy_CumTB_Xuong
                                {
                                    CumTB_ID = model.ID,
                                    Xuong_ID = xuong
                                });
                            }
                        }

                        TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                    }
                }

                else
                {
                    // ✏️ Cập nhật
                    var old = await _context.Tbl_NhatKy_CumTB.FindAsync(model.ID);
                    if (old == null) return NotFound();

                    old.TenCumTB = model.TenCumTB;
                    old.IsLock = model.IsLock;
                    await _context.SaveChangesAsync();

                    // Cập nhật lại mapping
                    var oldMap = _context.Tbl_NhatKy_CumTB_Xuong.Where(x => x.CumTB_ID == model.ID);
                    _context.Tbl_NhatKy_CumTB_Xuong.RemoveRange(oldMap);

                    if (Xuong_ID != null)
                    {
                        foreach (var xuong in Xuong_ID)
                        {
                            _context.Tbl_NhatKy_CumTB_Xuong.Add(new Tbl_NhatKy_CumTB_Xuong
                            {
                                CumTB_ID = model.ID,
                                Xuong_ID = xuong
                            });
                        }
                    }

                    TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Lưu thất bại');</script>";
            }

            return RedirectToAction("Index");
        }



        // 🟢 Form thêm mới cụm thiết bị
        public IActionResult Create()
        {
            List<Tbl_Xuong> xuongs = _context.Tbl_Xuong.ToList();
            ViewBag.XuongList = new SelectList(xuongs, "ID", "TenXuong");
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_NhatKy_CumTB model, int[] Xuong_ID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.TenCumTB))
                {
                    TempData["msgError"] = "<script>alert('Vui lòng nhập tên cụm thiết bị');</script>";
                    return RedirectToAction("Index");
                }

                model.IsLock = false;
                model.IsDelete = false;
                _context.Tbl_NhatKy_CumTB.Add(model);
                await _context.SaveChangesAsync();

                // Gán cụm thiết bị vào xưởng
                if (Xuong_ID != null && Xuong_ID.Length > 0)
                {
                    foreach (var id in Xuong_ID)
                    {
                        var map = new Tbl_NhatKy_CumTB_Xuong
                        {
                            CumTB_ID = model.ID,
                            Xuong_ID = id
                        };
                        _context.Tbl_NhatKy_CumTB_Xuong.Add(map);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }
            return RedirectToAction("Index");
        }

        // 🟠 Form chỉnh sửa cụm thiết bị
        public async Task<IActionResult> Edit(int id)
        {
            var cum = await _context.Tbl_NhatKy_CumTB.FindAsync(id);
            if (cum == null) return NotFound();

            List<Tbl_Xuong> xuongs = _context.Tbl_Xuong.ToList();
            List<int> selectedXuong = _context.Tbl_NhatKy_CumTB_Xuong
                .Where(x => x.CumTB_ID == id)
                .Select(x => x.Xuong_ID)
                .ToList();

            ViewBag.XuongList = new MultiSelectList(xuongs, "ID", "TenXuong", selectedXuong);
            return PartialView(cum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_NhatKy_CumTB model, int[] Xuong_ID)
        {
            try
            {
                var cum = await _context.Tbl_NhatKy_CumTB.FindAsync(id);
                if (cum == null)
                {
                    TempData["msgError"] = "<script>alert('Không tìm thấy cụm thiết bị');</script>";
                    return RedirectToAction("Index");
                }

                cum.TenCumTB = model.TenCumTB;
                cum.IsLock = model.IsLock;
                await _context.SaveChangesAsync();

                // Cập nhật lại danh sách xưởng
                var old = _context.Tbl_NhatKy_CumTB_Xuong.Where(x => x.CumTB_ID == id);
                _context.Tbl_NhatKy_CumTB_Xuong.RemoveRange(old);
                await _context.SaveChangesAsync();

                if (Xuong_ID != null && Xuong_ID.Length > 0)
                {
                    foreach (var xuong in Xuong_ID)
                    {
                        _context.Tbl_NhatKy_CumTB_Xuong.Add(new Tbl_NhatKy_CumTB_Xuong
                        {
                            CumTB_ID = id,
                            Xuong_ID = xuong
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
            }
            catch (Exception)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }

            return RedirectToAction("Index");
        }

        // 🔴 Xóa cụm thiết bị (logic delete)
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cum = await _context.Tbl_NhatKy_CumTB.FindAsync(id);
                if (cum != null)
                {
                    cum.IsDelete = true;
                    await _context.SaveChangesAsync();
                }
                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch
            {
                TempData["msgError"] = "<script>alert('Xóa thất bại');</script>";
            }

            return RedirectToAction("Index");
        }

        //Khóa / Mở khóa cụm thiết bị
        public async Task<IActionResult> ToggleLock(int id)
        {
            try
            {
                var cum = await _context.Tbl_NhatKy_CumTB.FindAsync(id);
                if (cum != null)
                {
                    cum.IsLock = !cum.IsLock;
                    await _context.SaveChangesAsync();
                    TempData["msgSuccess"] = "<script>alert('Cập nhật trạng thái thành công');</script>";
                }
            }
            catch
            {
                TempData["msgError"] = "<script>alert('Lỗi cập nhật khóa');</script>";
            }

            return RedirectToAction("Index");
        }

        // Import Excel
        public IActionResult ImportExcel()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    TempData["msgError"] = "<script>alert('Vui lòng chọn file Excel');</script>";
                    return RedirectToAction("Index");
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using var reader = ExcelReaderFactory.CreateReader(stream);
                var ds = reader.AsDataSet();
                var table = ds.Tables[0];

                for (int i = 1; i < table.Rows.Count; i++)
                {
                    string ten = table.Rows[i][0].ToString().Trim();
                    if (string.IsNullOrEmpty(ten)) continue;

                    bool exists = _context.Tbl_NhatKy_CumTB.Any(x => x.TenCumTB == ten);
                    if (!exists)
                    {
                        _context.Tbl_NhatKy_CumTB.Add(new Tbl_NhatKy_CumTB
                        {
                            TenCumTB = ten,
                            IsDelete = false,
                            IsLock = false
                        });
                    }
                }
                await _context.SaveChangesAsync();

                TempData["msgSuccess"] = "<script>alert('Import thành công');</script>";
            }
            catch
            {
                TempData["msgError"] = "<script>alert('Import thất bại');</script>";
            }

            return RedirectToAction("Index");
        }

        // 📤 Export Excel
        public async Task<IActionResult> ExportToExcel()
        {
            var data = await (
                from ctb in _context.Tbl_NhatKy_CumTB.AsNoTracking()
                join ctbx in _context.Tbl_NhatKy_CumTB_Xuong.AsNoTracking()
                    on ctb.ID equals ctbx.CumTB_ID
                join x in _context.Tbl_Xuong.AsNoTracking()
                    on ctbx.Xuong_ID equals x.ID_Xuong
                where !ctb.IsDelete
                select new
                {
                    TenCumTB = ctb.TenCumTB,
                    TenXuong = x.TenXuong,
                    TrangThai = ctb.IsLock ? "Đã khóa" : "Hoạt động"
                }
            ).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("CumThietBi");

            /* ===============================
             * 1️⃣ TIÊU ĐỀ LỚN
             * =============================== */
            worksheet.Cell(1, 1).Value = "DANH SÁCH CỤM THIẾT BỊ";
            worksheet.Range(1, 1, 1, 4).Merge();

            var titleCell = worksheet.Cell(1, 1);
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 16;
            titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            titleCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            titleCell.Style.Font.FontColor = XLColor.White;

            worksheet.Row(1).Height = 35;

            /* ===============================
             * 2️⃣ HEADER CỘT
             * =============================== */
            worksheet.Cell(2, 1).Value = "STT";
            worksheet.Cell(2, 2).Value = "Tên Cụm Thiết Bị";
            worksheet.Cell(2, 3).Value = "Xưởng";
            worksheet.Cell(2, 4).Value = "Trạng Thái";

            var headerRange = worksheet.Range(2, 1, 2, 4);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#5B9BD5");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Row(2).Height = 25;

            /* ===============================
             * 3️⃣ DATA
             * =============================== */
            int row = 3, stt = 1;
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = stt++;
                worksheet.Cell(row, 2).Value = item.TenCumTB;
                worksheet.Cell(row, 3).Value = item.TenXuong;
                worksheet.Cell(row, 4).Value = item.TrangThai;

                worksheet.Range(row, 1, row, 4)
                    .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                row++;
            }

            /* ===============================
             * 4️⃣ FORMAT CHUNG
             * =============================== */
            worksheet.Columns().AdjustToContents();
            worksheet.SheetView.FreezeRows(2);

            /* ===============================
             * 5️⃣ EXPORT
             * =============================== */
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DanhSachCumThietBi.xlsx"
            );

        }
    }
       
}
