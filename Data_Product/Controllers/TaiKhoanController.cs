using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Data_Product.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly DataContext _context;

        public TaiKhoanController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_TaiKhoan
                             join pb in _context.Tbl_PhongBan on a.ID_PhongBan equals pb.ID_PhongBan
                             join x in _context.Tbl_Xuong on a.ID_PhanXuong equals x.ID_Xuong
                             join vt in _context.Tbl_ViTri on a.ID_ChucVu equals vt.ID_ViTri
                             join q in _context.Tbl_Quyen on a.ID_Quyen equals q.ID_Quyen
                             select new Tbl_TaiKhoan
                             {
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 TenTaiKhoan = a.TenTaiKhoan,
                                 MatKhau = a.MatKhau,
                                 HoVaTen = a.HoVaTen,
                                 ID_PhongBan = a.ID_PhongBan,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_PhanXuong = a.ID_PhanXuong,
                                 TenXuong = x.TenXuong,
                                 ID_ChucVu = a.ID_ChucVu,
                                 TenChucVu = vt.TenViTri,
                                 Email = a.Email,
                                 SoDienThoai = a.SoDienThoai,
                                 NgayTao = (DateTime)a.NgayTao,
                                 ID_Quyen = (int?)a.ID_Quyen ?? default,
                                 TenQuyen = q.TenQuyen,
                                 ChuKy = a.ChuKy,
                                 ID_TrangThai = (int)a.ID_TrangThai
                             }).OrderBy(x=>x.TenTaiKhoan).ToListAsync();

            if (search != null)
            {
                res = res.Where(x => x.HoVaTen.Contains(search) || x.TenTaiKhoan.Contains(search)).ToList();

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

            List<Tbl_Quyen> q = _context.Tbl_Quyen.ToList();
            ViewBag.QList = new SelectList(q, "ID_Quyen", "TenQuyen");

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            List<Tbl_Xuong> x = _context.Tbl_Xuong.ToList();
            ViewBag.XList = new SelectList(x, "ID_Xuong", "TenXuong");

            List<Tbl_ViTri> vt = _context.Tbl_ViTri.ToList();
            ViewBag.VTList = new SelectList(vt, "ID_ViTri", "TenViTri");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (Tbl_TaiKhoan _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                               _DO.TenTaiKhoan, Encryptor.MD5Hash(_DO.MatKhau), _DO.HoVaTen, _DO.ID_PhongBan, _DO.ID_PhanXuong, _DO.ID_ChucVu, 
                                                               _DO.Email, _DO.SoDienThoai, NgayTao, _DO.ID_Quyen, null, 1);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "TaiKhoan");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "TaiKhoan");
            }

            var res = await (from a in _context.Tbl_TaiKhoan.Where(x=>x.ID_TaiKhoan == id)
                             join pb in _context.Tbl_PhongBan on a.ID_PhongBan equals pb.ID_PhongBan
                             join x in _context.Tbl_Xuong on a.ID_PhanXuong equals x.ID_Xuong
                             join vt in _context.Tbl_ViTri on a.ID_ChucVu equals vt.ID_ViTri
                             join q in _context.Tbl_Quyen on a.ID_Quyen equals q.ID_Quyen
                             select new Tbl_TaiKhoan
                             {
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 TenTaiKhoan = a.TenTaiKhoan,
                                 MatKhau = a.MatKhau,
                                 HoVaTen = a.HoVaTen,
                                 ID_PhongBan = a.ID_PhongBan,
                                 TenPhongBan = pb.TenPhongBan,
                                 ID_PhanXuong = a.ID_PhanXuong,
                                 TenXuong = x.TenXuong,
                                 ID_ChucVu = a.ID_ChucVu,
                                 Email = a.Email,
                                 SoDienThoai = a.SoDienThoai,
                                 NgayTao = (DateTime)a.NgayTao,
                                 ID_Quyen = (int?)a.ID_Quyen ?? default,
                                 ChuKy = a.ChuKy,
                                 ID_TrangThai = (int)a.ID_TrangThai
                             }).ToListAsync();

            Tbl_TaiKhoan DO = new Tbl_TaiKhoan();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_TaiKhoan = a.ID_TaiKhoan;
                    DO.TenTaiKhoan = a.TenTaiKhoan;
                    DO.MatKhau = a.MatKhau;
                    DO.HoVaTen = a.HoVaTen;
                    DO.ID_PhongBan = a.ID_PhongBan;
                    DO.ID_PhanXuong = a.ID_PhanXuong;
                    DO.ID_ChucVu = a.ID_ChucVu;
                    DO.Email = a.Email;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.NgayTao = (DateTime)a.NgayTao;
                    DO.ID_Quyen = (int?)a.ID_Quyen ?? default;
                    DO.ChuKy = a.ChuKy;
                    DO.ID_TrangThai = (int)a.ID_TrangThai;
                }
                List<Tbl_Quyen> q = _context.Tbl_Quyen.ToList();
                ViewBag.QList = new SelectList(q, "ID_Quyen", "TenQuyen",DO.ID_Quyen);
                List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
                ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan", DO.ID_PhongBan);

                List<Tbl_Xuong> x = _context.Tbl_Xuong.ToList();
                ViewBag.XList = new SelectList(x, "ID_Xuong", "TenXuong", DO.ID_PhanXuong);

                List<Tbl_ViTri> vt = _context.Tbl_ViTri.ToList();
                ViewBag.VTList = new SelectList(vt, "ID_ViTri", "TenViTri", DO.ID_ChucVu);
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_TaiKhoan _DO) 
        {
            try
            {
                var ID = _context.Tbl_TaiKhoan.Where(x=>x.ID_TaiKhoan == id).FirstOrDefault();
                string MatKhau = ID.MatKhau;
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_update {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", id,
                                                                             _DO.TenTaiKhoan, MatKhau, _DO.HoVaTen, _DO.ID_PhongBan, _DO.ID_PhanXuong, _DO.ID_ChucVu,
                                                                             _DO.Email, _DO.SoDienThoai, ID.NgayTao, _DO.ID_Quyen, null, 1);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }



            return RedirectToAction("Index", "TaiKhoan");
        }
        public async Task<IActionResult> Lock(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_lock {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "TaiKhoan", new { page = page });
        }
        public async Task<IActionResult> resetPass(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_pass {0},{1}", id, Encryptor.MD5Hash("HPDQ@1234"));

                TempData["msgSuccess"] = "<script>alert('pass HPDQ@1234');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('thất bại');</script>";
            }


            return RedirectToAction("Index", "TaiKhoan", new { page = page });
        }

        public async Task<IActionResult> Delete(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_delete {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Xóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "TaiKhoan", new { page = page });
        }

        public async Task<IActionResult> Unlock(int id, int page)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_lock {0},{1}", id, 1);

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "TaiKhoan", new { page = page });
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
                    return RedirectToAction("Index", "TaiKhoan");
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
                            string MNV = serviceDetails.Rows[i][1].ToString().Trim();
                            string MatKhau = "HPDQ@1234";
                            string HoVaTen = serviceDetails.Rows[i][2].ToString().Trim();
                            string PhongBan = serviceDetails.Rows[i][3].ToString().Trim();
                            // check nhân viên
                            var check_nv = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MNV).FirstOrDefault();
                            if(check_nv == null)
                            {

                                var ID_PhongBan = _context.Tbl_PhongBan.Where(x => x.TenPhongBan == PhongBan || x.TenNgan == PhongBan).FirstOrDefault();
                                if (ID_PhongBan == null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên BP/NM nhân viên : " + MNV + "');</script>";
                                    return RedirectToAction("Index", "TaiKhoan");
                                }
                                string PhanXuong = serviceDetails.Rows[i][4].ToString().Trim();
                                var ID_PhanXuong = _context.Tbl_Xuong.Where(x => x.TenXuong == PhanXuong).FirstOrDefault();
                                if (ID_PhanXuong == null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên Xưởng nhân viên : " + MNV + "');</script>";
                                    return RedirectToAction("Index", "TaiKhoan");
                                }
                                string ChucVu = serviceDetails.Rows[i][5].ToString().Trim();
                                var ID_ChucVu = _context.Tbl_ViTri.Where(x => x.TenViTri == ChucVu).FirstOrDefault();
                                if (ID_ChucVu == null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên chức vụ nhân viên : " + MNV + "');</script>";
                                    return RedirectToAction("Index", "TaiKhoan");
                                }
                                string Email = serviceDetails.Rows[i][6].ToString().Trim();
                                string SoDienThoai = serviceDetails.Rows[i][7].ToString().Trim();
                                string TenQuyen = serviceDetails.Rows[i][8].ToString().Trim();
                                var check_g = _context.Tbl_Quyen.Where(x => x.TenQuyen == TenQuyen).FirstOrDefault();
                                if (check_g == null)
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra quyền đăng nhập nhân viên: " + MNV + "');</script>";
                                    return RedirectToAction("Index", "TaiKhoan");
                                }


                                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                                     MNV, Encryptor.MD5Hash(MatKhau), HoVaTen, ID_PhongBan.ID_PhongBan, ID_PhanXuong.ID_Xuong, ID_ChucVu.ID_ViTri,
                                                                      Email, SoDienThoai, NgayTao, check_g.ID_Quyen, null, 1);

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

            return RedirectToAction("Index", "TaiKhoan");
        }
    }
}
