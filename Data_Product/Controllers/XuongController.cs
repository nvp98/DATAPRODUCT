using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Data_Product.Controllers
{
    public class XuongController : Controller
    {
        private readonly DataContext _context;

        public XuongController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_Xuong
                             join pb in _context.Tbl_PhongBan on a.ID_PhongBan equals pb.ID_PhongBan
                             select new Tbl_Xuong
                             {
                                 ID_Xuong = a.ID_Xuong,
                                 TenXuong = a.TenXuong,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_TrangThai = (int?)a.ID_TrangThai ?? default
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.TenXuong.Contains(search)).ToList();

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
            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_Xuong _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Xuong_insert {0},{1},{2}", _DO.TenXuong, _DO.ID_PhongBan, 1);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "Xuong");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "TaiKhoan");
            }

            var res = await (from a in _context.Tbl_Xuong.Where(x=>x.ID_Xuong == id)
                             join pb in _context.Tbl_PhongBan on a.ID_PhongBan equals pb.ID_PhongBan
                             select new Tbl_Xuong
                             {
                                 ID_Xuong = a.ID_Xuong,
                                 TenXuong = a.TenXuong,
                                 ID_PhongBan = (int)a.ID_PhongBan,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_TrangThai = (int?)a.ID_TrangThai ?? default
                             }).ToListAsync();

            Tbl_Xuong DO = new Tbl_Xuong();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_Xuong = a.ID_Xuong;
                    DO.TenXuong = a.TenXuong;
                    DO.ID_PhongBan = (int)a.ID_PhongBan;
                    DO.ID_TrangThai = (int?)a.ID_TrangThai ?? default;
                }
                List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
                ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan", DO.ID_PhongBan);
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_Xuong _DO)
        {
            try
            {
                var ID = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == id).FirstOrDefault();
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Xuong_update {0},{1},{2}", id,_DO.TenXuong,_DO.ID_PhongBan);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }



            return RedirectToAction("Index", "Xuong");
        }
        public async Task<IActionResult> Lock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Xuong_lock {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "Xuong");
        }
        public async Task<IActionResult> Unlock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Xuong_lock {0},{1}", id, 1);

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "Xuong");
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Xuong_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "Xuong");
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
                    return RedirectToAction("Index", "Xuong");
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
                            string PhanXuong = serviceDetails.Rows[i][1].ToString().Trim();
                            string TenBP = serviceDetails.Rows[i][2].ToString().Trim();
                            var check_bp = _context.Tbl_PhongBan.Where(x => x.TenNgan == TenBP).FirstOrDefault();
                            if (check_bp == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên BP/NM: " + PhanXuong + "');</script>";
                                return RedirectToAction("Index", "Xuong");
                            }

                            var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Xuong_insert {0},{1},{2}", PhanXuong, check_bp.ID_PhongBan, 1);
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "Xuong");
        }
    }
}
