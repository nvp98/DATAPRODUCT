using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Data_Product.Controllers
{
    public class ChucVuController : Controller
    {
        private readonly DataContext _context;

        public ChucVuController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_ViTri
                             select new Tbl_ViTri
                             {
                                 ID_ViTri = a.ID_ViTri,
                                 TenViTri = a.TenViTri,
                                 ID_TrangThai = (int?)a.ID_TrangThai ?? default
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.TenViTri.Contains(search)).ToList();

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

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_ViTri _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_ViTri_insert {0},{1}", _DO.TenViTri, 1);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "ChucVu");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "PhongBan");
            }

            var res = await (from a in _context.Tbl_ViTri.Where(x=>x.ID_ViTri == id)
                             select new Tbl_ViTri
                             {
                                 ID_ViTri = a.ID_ViTri,
                                 TenViTri = a.TenViTri,
                                 ID_TrangThai = (int?)a.ID_TrangThai ?? default
                             }).ToListAsync();

            Tbl_ViTri DO = new Tbl_ViTri();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_ViTri = a.ID_ViTri;
                    DO.TenViTri = a.TenViTri;
                    DO.ID_TrangThai = (int?)a.ID_TrangThai ?? default;
                }
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_ViTri _DO)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_ViTri_update {0},{1}", id, _DO.TenViTri);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }



            return RedirectToAction("Index", "ChucVu");
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_ViTri_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "ChucVu");
        }
        public async Task<IActionResult> Lock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_ViTri_lock {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "ChucVu");
        }
        public async Task<IActionResult> Unlock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_ViTri_lock {0},{1}", id, 1);

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "ChucVu");
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
                    return RedirectToAction("Index", "ChucVu");
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
                            DateTime NgayTao = DateTime.Now;
                            string TenChucVu = serviceDetails.Rows[i][1].ToString().Trim();
                            var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_ViTri_insert {0},{1}", TenChucVu, 1);
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "ChucVu");
        }
    }
}
