using ClosedXML.Excel;
using Data_Product.Models;
using Data_Product.Repositorys;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Xml.Linq;

namespace Data_Product.Controllers
{
    public class VatTuController : Controller
    {
        private readonly DataContext _context;

        public VatTuController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_VatTu
                             join vt in _context.Tbl_NhomVatTu on a.ID_NhomVatTu equals vt.ID_NhomVatTu into joinedT
                             from vt in joinedT.DefaultIfEmpty()
                             select new Tbl_VatTu
                             {
                                 ID_VatTu = a.ID_VatTu,
                                 TenVatTu = a.TenVatTu,
                                 MaVatTu_Sap = a.MaVatTu_Sap??default,
                                 TenVatTu_Sap = a.TenVatTu_Sap,
                                 DonViTinh = a.DonViTinh,
                                 ID_NhomVatTu = (int)a.ID_NhomVatTu,
                                 TenNhomVatTu = vt.TenNhomVatTu,
                                 PhongBan = a.PhongBan,
                                 ID_TrangThai = (int?)a.ID_TrangThai ?? default,
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.TenVatTu.Contains(search) || (x?.TenVatTu_Sap ?? string.Empty).Contains(search)).ToList();

            }
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
        public async Task<IActionResult> Create()
        {
            List<Tbl_NhomVatTu> vt = _context.Tbl_NhomVatTu.ToList();
            ViewBag.VTList = new SelectList(vt, "ID_NhomVatTu", "TenNhomVatTu");

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_VatTu _DO)
        {   
            DateTime NgayTao = DateTime.Now;
            try
            {
                var Name = new List<string>();
                if (_DO.ID_PhongBan != null)
                {
                    foreach (var bp in _DO.ID_PhongBan)
                    {
                        int ID_PhongBan = Convert.ToInt32(bp);
                        var Value = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_PhongBan).FirstOrDefault();
                        Name.Add(Value.TenNgan);
                    }
                    var ListGate = string.Join(", ", Name);

                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_insert {0},{1},{2},{3},{4},{5},{6}",
                    _DO.TenVatTu, _DO.MaVatTu_Sap, _DO.TenVatTu_Sap, _DO.DonViTinh, _DO.ID_NhomVatTu, ListGate, 1);
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn BP/NM');</script>";
                    return RedirectToAction("Index", "VatTu");
                }    

              

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "VatTu");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "VatTu");
            }

            var res = await (from a in _context.Tbl_VatTu.Where(x=>x.ID_VatTu == id)
                             join vt in _context.Tbl_NhomVatTu on a.ID_NhomVatTu equals vt.ID_NhomVatTu into joinedT
                             from vt in joinedT.DefaultIfEmpty()
                             select new Tbl_VatTu
                             {
                                 ID_VatTu = a.ID_VatTu,
                                 TenVatTu = a.TenVatTu,
                                 MaVatTu_Sap = a.MaVatTu_Sap??default,
                                 TenVatTu_Sap = a.TenVatTu_Sap,
                                 DonViTinh = a.DonViTinh,
                                 ID_NhomVatTu = (int)a.ID_NhomVatTu,
                                 TenNhomVatTu = vt.TenNhomVatTu,
                                 PhongBan = a.PhongBan,
                                 ID_TrangThai = (int?)a.ID_TrangThai ?? default
                             }).ToListAsync();

            Tbl_VatTu DO = new Tbl_VatTu();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_VatTu = a.ID_VatTu;
                    DO.TenVatTu = a.TenVatTu;
                    DO.MaVatTu_Sap = a.MaVatTu_Sap?? default;
                    DO.TenVatTu_Sap = a.TenVatTu_Sap;
                    DO.DonViTinh = a.DonViTinh;
                    DO.ID_NhomVatTu = a?.ID_NhomVatTu;
                    DO.PhongBan = a.PhongBan;
                    DO.ID_TrangThai = (int?)a.ID_TrangThai ?? default;
                }
                List<Tbl_NhomVatTu> vt = _context.Tbl_NhomVatTu.ToList();
                ViewBag.VTList = new SelectList(vt, "ID_NhomVatTu", "TenNhomVatTu", DO.ID_NhomVatTu);

                List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
                ViewBag.ID_PhongBan = new SelectList(pb, "TenNgan", "TenPhongBan");
                ViewBag.ID_VatTu = id;

                //var selectedValues = new List<string>(); // ID các giá trị mặc định

                //foreach (var item in res1)
                //{
                //    var idquyen = item.ID_Xuong != 0 ? item.ID_Xuong : 0;
                //    if (item.ID_Xuong != 0) { selectedValues.Add(item.ID_Xuong.ToString()); }
                //}
                //// Tạo SelectList và đánh dấu giá trị mặc định
                //var selectList = xuong.Select(role => new SelectListItem
                //{
                //    Value = role.ID_Xuong.ToString(),
                //    Text = role.TenXuong,
                //}).ToList();
                //ViewBag.SelectedValue = selectedValues.ToArray();
                //ViewBag.ListQuyen_Them = selectList;

            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_VatTu _DO)
        {
            try
            {
                if (_DO.ID_PhongBan != null)
                {
                    var Name = new List<string>();
                    var DO = _context.Tbl_VatTu.Where(x => x.ID_VatTu == id).FirstOrDefault();
                    if (DO != null)
                    {
                        foreach (var item in DO.PhongBan.Split(","))
                        {
                            Name.Add(item);
                        }
                    }
                    foreach (var bp in _DO.ID_PhongBan)
                    {
                        if (!Name.Contains(bp))
                        {
                            Name.Add(bp);
                        }
                       
                    }
                    var ListGate = string.Join(",", Name);
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_update {0},{1},{2},{3},{4},{5},{6}", id,
                             _DO.TenVatTu, _DO.MaVatTu_Sap, _DO.TenVatTu_Sap, _DO.DonViTinh, _DO.ID_NhomVatTu, ListGate);
                }
                else
                {
                    var DO = _context.Tbl_VatTu.Where(x => x.ID_VatTu == _DO.ID_VatTu).FirstOrDefault();

                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_update {0},{1},{2},{3},{4},{5},{6}", id,
                        _DO.TenVatTu, _DO.MaVatTu_Sap, _DO.TenVatTu_Sap, _DO.DonViTinh, _DO.ID_NhomVatTu, DO.PhongBan);

                }
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }



            return RedirectToAction("Index", "VatTu");
        }
        public async Task<IActionResult> Lock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_lock {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "VatTu");
        }
        public async Task<IActionResult> Unlock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_lock {0},{1}", id, 1);

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "VatTu");
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "VatTu");
        }
        public async Task<IActionResult> ImportExcel()
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
                    return RedirectToAction("Index", "VatTu");
                }


                // Create the Directory if it is not exist
                string webRootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string dirPath = Path.Combine(webRootPath, "ReceivedReports");
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // MAke sure that only Excel file is used 
                string dataFileName = Path.GetFileName(DateTime.Now.ToString("yyyyMMddHHmm"));

                string extension = Path.GetExtension(dataFileName);

                string[] allowedExtsnions = new string[] { ".xls", ".xlsx" };
                // Make a Copy of the Posted File from the Received HTTP Request
                string saveToPath = Path.Combine(dirPath, dataFileName);

                using (FileStream stream = new FileStream(saveToPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // USe this to handle Encodeing differences in .NET Core
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                // read the excel file
                IExcelDataReader reader = null;
                using (var stream = new FileStream(saveToPath, FileMode.Open))
                {
                    if (extension == ".xlsx")
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    else
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                    DataSet ds = new DataSet();
                    ds = reader.AsDataSet();
                    reader.Close();
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        System.Data.DataTable serviceDetails = ds.Tables[0];

                        for (int i = 1; i < serviceDetails.Rows.Count; i++)
                        {
                            string TenNguyenLieu = serviceDetails.Rows[i][1].ToString().Trim();
                            string TenBP = serviceDetails.Rows[i][2].ToString().Trim();
                            string[] arrList = TenBP.Trim().Split(',');
                            foreach (var ar in arrList)
                            {
                                if (ar != "")
                                {
                                    var check_bp = _context.Tbl_PhongBan.Where(x => x.TenNgan == ar.Trim()).FirstOrDefault();
                                    if (check_bp == null)
                                    {
                                        TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên BP/NM: " + TenNguyenLieu + "');</script>";
                                        return RedirectToAction("Index", "VatTu");
                                    }
                                }

                            }
         
                            string TenNhom = serviceDetails.Rows[i][3].ToString().Trim();
                            var check_nhom = _context.Tbl_NhomVatTu.Where(x => x.TenNhomVatTu == TenNhom).FirstOrDefault();
                            if (TenNhom != "")
                            {
                                //var check_nhom = _context.Tbl_NhomVatTu.Where(x => x.TenNhomVatTu == TenNhom).FirstOrDefault();
                                if (check_nhom == null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên nhóm vật tư: " + TenNguyenLieu + "');</script>";
                                    return RedirectToAction("Index", "VatTu");
                                }
                            }



                            string DonViTinh = serviceDetails.Rows[i][4].ToString().Trim();
                            string MaVatTu_Sap  = serviceDetails.Rows[i][5].ToString().Trim();
                            string TenVatTu_Sap = serviceDetails.Rows[i][6].ToString().Trim();
                            if(MaVatTu_Sap == "")
                            {
                                if(TenNhom == "")
                                {
                                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_insert {0},{1},{2},{3},{4},{5},{6}",
                                TenNguyenLieu, null, TenVatTu_Sap, DonViTinh, null, TenBP, 1);
                                }
                                else
                                {
                                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_insert {0},{1},{2},{3},{4},{5},{6}",
                               TenNguyenLieu, null, TenVatTu_Sap, DonViTinh, check_nhom.ID_NhomVatTu, TenBP, 1);
                                }
                               
                            }   
                            else
                            {
                                if(TenNhom == "")
                                {
                                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_insert {0},{1},{2},{3},{4},{5},{6}",
                            TenNguyenLieu, MaVatTu_Sap, TenVatTu_Sap, DonViTinh, null, TenBP, 1);
                                }
                                else
                                {
                                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_VatTu_insert {0},{1},{2},{3},{4},{5},{6}",
                            TenNguyenLieu, MaVatTu_Sap, TenVatTu_Sap, DonViTinh, check_nhom.ID_NhomVatTu, TenBP, 1);
                                }
                               
                            }    
                         
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "VatTu");
        }

        public IActionResult ExportToExcel()
        {
            var data = (from a in _context.Tbl_VatTu
                        join vt in _context.Tbl_NhomVatTu on a.ID_NhomVatTu equals vt.ID_NhomVatTu into joinedT
                        from vt in joinedT.DefaultIfEmpty()
                        select new Tbl_VatTu
                        {
                            ID_VatTu = a.ID_VatTu,
                            TenVatTu = a.TenVatTu,
                            MaVatTu_Sap = a.MaVatTu_Sap ?? default,
                            TenVatTu_Sap = a.TenVatTu_Sap,
                            DonViTinh = a.DonViTinh,
                            ID_NhomVatTu = (int)a.ID_NhomVatTu,
                            TenNhomVatTu = vt.TenNhomVatTu,
                            PhongBan = a.PhongBan,
                            ID_TrangThai = (int?)a.ID_TrangThai ?? default,
                        }).ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("VatTu");
                //Header
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Tên vật tư";
                worksheet.Cell(1, 3).Value = "Tên vật tư (SAP)";
                worksheet.Cell(1, 4).Value = "Mã vật tư (SAP)";
                worksheet.Cell(1, 5).Value = "Tên BP/NM";
                worksheet.Cell(1, 6).Value = "Đơn vị tính";
                worksheet.Cell(1, 7).Value = "Nhóm vật tư";
                //value
                int row = 2; int stt = 1;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = stt;
                    worksheet.Cell(row, 2).Value = item.TenVatTu;
                    worksheet.Cell(row, 3).Value = item.TenVatTu_Sap;
                    worksheet.Cell(row, 4).Value = item.MaVatTu_Sap;
                    worksheet.Cell(row, 5).Value = item.PhongBan;
                    worksheet.Cell(row, 6).Value = item.DonViTinh;
                    worksheet.Cell(row, 7).Value = item.TenNhomVatTu;
                    row++; stt++;
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0; // Reset con trỏ stream về đầu

                return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, "DanhSachVatTu.xlsx");
            }
        }
    }
}
