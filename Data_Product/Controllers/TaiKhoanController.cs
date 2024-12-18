using ClosedXML.Excel;
using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;

namespace Data_Product.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly DataContext _context;

        public TaiKhoanController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int? ID_PhongBan, int page = 1)
        {

            List<Tbl_PhongBan> lspb = _context.Tbl_PhongBan.ToList();
            ViewBag.PBList = new SelectList(lspb, "ID_PhongBan", "TenPhongBan",ID_PhongBan);

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
                                 ID_TrangThai = (int)a.ID_TrangThai,
                                 PhongBan_Them = a.PhongBan_Them,
                                 Quyen_Them = a.Quyen_Them
                             }).OrderBy(x=>x.TenTaiKhoan).ToListAsync();

            if (search != null)
            {
                res = res.Where(x => x.HoVaTen.Contains(search) || x.TenTaiKhoan.Contains(search)).ToList();

            }
            if (ID_PhongBan != null) res = res.Where(x => x.ID_PhongBan == ID_PhongBan).ToList();


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

            ViewBag.ListPhongBan_Them = new SelectList(pb, "TenNgan", "TenPhongBan");
            ViewBag.ListQuyen_Them = new SelectList(q, "ID_Quyen", "TenQuyen");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create (Tbl_TaiKhoan _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var checktk = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == _DO.TenTaiKhoan).FirstOrDefault();
                if (checktk != null) {
                    TempData["msgError"] = "<script>alert('Tài khoản đã tồn tại');</script>";
                    return RedirectToAction("Index", "TaiKhoan");
                }
                if(_DO.ID_PhanXuong == null || _DO.ID_Quyen == null || _DO.ID_ChucVu == null)
                {
                    TempData["msgError"] = "<script>alert('Kiểm tra lại thông tin Xưởng);</script>";
                    return RedirectToAction("Index", "TaiKhoan");
                }
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                                                               _DO.TenTaiKhoan, Encryptor.MD5Hash(_DO.MatKhau), _DO.HoVaTen, _DO.ID_PhongBan, _DO.ID_PhanXuong, _DO.ID_ChucVu, 
                                                               _DO.Email, _DO.SoDienThoai, NgayTao, _DO.ID_Quyen, null, 1);
                
                if (_DO.ListPhongBan_Them != null)
                {
                    var NamePB = new List<string>();
                    foreach (var bp in _DO.ListPhongBan_Them)
                    {
                        NamePB.Add(bp);
                    }
                    var ListGate = string.Join(", ", NamePB);
                    var tk = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == _DO.TenTaiKhoan).FirstOrDefault();
                    tk.PhongBan_Them = ListGate;
                    _context.SaveChanges();
                }
                if (_DO.ListQuyen_Them != null)
                {
                    var NamePB = new List<string>();
                    foreach (var bp in _DO.ListQuyen_Them)
                    {
                        NamePB.Add(bp);
                    }
                    var ListGate = string.Join(", ", NamePB);
                    var tk = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == _DO.TenTaiKhoan).FirstOrDefault();
                    tk.Quyen_Them = ListGate;
                    _context.SaveChanges();
                }

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
                                 ID_TrangThai = (int)a.ID_TrangThai,
                                 PhongBan_Them = a.PhongBan_Them,
                                 Quyen_Them = a.Quyen_Them,
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
                    DO.Quyen_Them = a.Quyen_Them;
                    DO.PhongBan_Them     = a.PhongBan_Them;
                }
                List<Tbl_Quyen> q = _context.Tbl_Quyen.ToList();
                ViewBag.QList = new SelectList(q, "ID_Quyen", "TenQuyen",DO.ID_Quyen);
                List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
                ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan", DO.ID_PhongBan);

                List<Tbl_Xuong> x = _context.Tbl_Xuong.ToList();
                ViewBag.XList = new SelectList(x, "ID_Xuong", "TenXuong", DO.ID_PhanXuong);

                List<Tbl_ViTri> vt = _context.Tbl_ViTri.ToList();
                ViewBag.VTList = new SelectList(vt, "ID_ViTri", "TenViTri", DO.ID_ChucVu);

                ViewBag.ListPhongBan_Them = new SelectList(pb, "TenNgan", "TenPhongBan");
               
                var NameQuyen = new List<string>();
                var selectedValues = new List<string>(); // ID các giá trị mặc định
               
                if (DO.Quyen_Them != null && DO.Quyen_Them != "")
                {
                    List<string> result = DO.Quyen_Them.Split(',').ToList();
                    
                    foreach (var item in result)
                    {
                        var idquyen = item != "" ? int.Parse(item) : 0;
                        var ck = _context.Tbl_Quyen.Where(x => x.ID_Quyen == idquyen).FirstOrDefault();
                        if (ck != null) { NameQuyen.Add(ck.TenQuyen); selectedValues.Add(ck.ID_Quyen.ToString()); }
                    }
                }
                var selectedValuePB = new List<string>(); // ID các giá trị mặc định
                if (DO.PhongBan_Them != null && DO.PhongBan_Them != "")
                {
                    List<string> result = DO.PhongBan_Them.Split(',').ToList();
                    foreach (var item in result)
                    {
                        //var idquyen = item != "" ? int.Parse(item) : 0;
                        var ck = _context.Tbl_PhongBan.Where(x => x.TenNgan.Contains(item.Trim())).FirstOrDefault();
                        if (ck != null) { selectedValuePB.Add(ck.TenNgan); }
                    }
                }
                var ListGate = string.Join(", ", NameQuyen);
                ViewBag.ListQuyen_Them_View = ListGate;

                // Tạo SelectList và đánh dấu giá trị mặc định
                var selectList = q.Select(role => new SelectListItem
                {
                    Value = role.ID_Quyen.ToString(),
                    Text = role.TenQuyen,
                }).ToList();
                ViewBag.SelectedValue = selectedValues.ToArray();
                ViewBag.SelectedValuePB = selectedValuePB.ToArray();
                ViewBag.ListQuyen_Them = selectList;
                //ViewBag.ListQuyen_Them = new SelectList(q, "ID_Quyen", "TenQuyen");

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
                                                                             _DO.Email, _DO.SoDienThoai, ID.NgayTao, _DO.ID_Quyen, _DO.ChuKy, 1);

                if (_DO.ListPhongBan_Them != null)
                {
                    var NamePB = new List<string>();
                    foreach (var bp in _DO.ListPhongBan_Them)
                    {
                        NamePB.Add(bp);
                    }
                    var ListGate = string.Join(", ", NamePB);
                    //var tk = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == _DO.TenTaiKhoan).FirstOrDefault();
                    ID.PhongBan_Them = ListGate;
                    _context.SaveChanges();
                }
                if (_DO.ListQuyen_Them != null)
                {
                    var NamePB = new List<string>();
                    foreach (var bp in _DO.ListQuyen_Them)
                    {
                        NamePB.Add(bp);
                    }
                    var ListGate = string.Join(", ", NamePB);
                    //var tk = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == _DO.TenTaiKhoan).FirstOrDefault();
                    ID.Quyen_Them = ListGate;
                    _context.SaveChanges();
                }

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

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_TaiKhoan_delete {0}", id);

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
                                var ID_PhanXuong = _context.Tbl_Xuong.Where(x => x.TenXuong == PhanXuong && x.ID_PhongBan == ID_PhongBan.ID_PhongBan).FirstOrDefault();
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
        public async Task<IActionResult> Edit_ThongKeXuong(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "TaiKhoan");
            }

           
            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            List<Tbl_Xuong> xuong = (from a in _context.Tbl_Xuong
                                 join b in _context.Tbl_PhongBan on a.ID_PhongBan equals b.ID_PhongBan
                                 select new Tbl_Xuong
                                 {
                                     ID_Xuong = a.ID_Xuong,
                                     ID_PhongBan = b.ID_PhongBan,
                                     TenXuong = b.TenPhongBan + "-" + a.TenXuong
                                 }
                                 ).ToList();
            ViewBag.XList = new SelectList(xuong, "ID_Xuong", "TenXuong");
            var res1 = (from a in _context.Tbl_Xuong
                       join b in _context.Tbl_ThongKeXuong.Where(x => x.ID_TaiKhoan == id) on a.ID_Xuong equals b.ID_Xuong
                       join c in _context.Tbl_PhongBan on a.ID_PhongBan equals c.ID_PhongBan
                       select new Tbl_Xuong
                       {
                           ID_Xuong = a.ID_Xuong,
                           ID_PhongBan = a.ID_PhongBan,
                           TenXuong = c.TenPhongBan + "-" + a.TenXuong,
                       }).ToList();
            var res = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == id)
                             select new Tbl_TaiKhoan
                             {
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 TenTaiKhoan = a.TenTaiKhoan,
                                 HoVaTen = a.HoVaTen
                             }).FirstOrDefaultAsync();

            if (res != null)
            {
                var selectedValues = new List<string>(); // ID các giá trị mặc định

                foreach (var item in res1)
                {
                    var idquyen = item.ID_Xuong != 0 ? item.ID_Xuong : 0;
                    if (item.ID_Xuong != 0) { selectedValues.Add(item.ID_Xuong.ToString()); }
                }
                // Tạo SelectList và đánh dấu giá trị mặc định
                var selectList = xuong.Select(role => new SelectListItem
                {
                    Value = role.ID_Xuong.ToString(),
                    Text = role.TenXuong,
                }).ToList();
                ViewBag.SelectedValue = selectedValues.ToArray();
                ViewBag.ListQuyen_Them = selectList;

            }
            else
            {
                return NotFound();
            }
            return PartialView(res);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit_ThongKeXuong(int id, Tbl_TaiKhoan _DO)
        {
            var ID = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == id).FirstOrDefault();
            try
            {
                List<Tbl_ThongKeXuong> Thongke = _context.Tbl_ThongKeXuong.Where(x => x.ID_TaiKhoan == id).ToList();
                if(Thongke != null)
                {
                    _context.Tbl_ThongKeXuong.RemoveRange(Thongke);
                    _context.SaveChanges();
                }
                if (_DO.ListQuyen_Them != null)
                {
                    var NamePB = new List<string>();
                    foreach (var bp in _DO.ListQuyen_Them)
                    {
                        Tbl_ThongKeXuong tk = new Tbl_ThongKeXuong()
                        {
                            ID_TaiKhoan = id,
                            ID_Xuong = int.Parse(bp)
                        };
                        // Kiểm tra dữ liệu trùng lặp
                        bool isDuplicate = _context.Tbl_ThongKeXuong
                            .Any(x => x.ID_TaiKhoan == tk.ID_TaiKhoan && x.ID_Xuong == tk.ID_Xuong);
                        if (!isDuplicate)
                        {
                            _context.Tbl_ThongKeXuong.Add(tk);
                            _context.SaveChanges();
                        }
                           
                    }
                   
                }

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }



            return RedirectToAction("Index", "TaiKhoan",new { ID_PhongBan =ID.ID_PhongBan });
        }

        public  IActionResult ExportToExcel()
        {
            var TaiKhoan = (from a in _context.Tbl_TaiKhoan
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
                                ID_TrangThai = (int)a.ID_TrangThai,
                                PhongBan_Them = a.PhongBan_Them,
                                Quyen_Them = a.Quyen_Them
                            }).OrderBy(x => x.TenTaiKhoan).ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("TaiKhoan");
                //Header
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Họ và tên";
                worksheet.Cell(1, 3).Value = "Phòng Ban";
                worksheet.Cell(1, 4).Value = "Chức vụ";
                worksheet.Cell(1, 5).Value = "Quyền đăng nhập";
                //value
                //worksheet.Cell(2, 1).Value = 1;
                //worksheet.Cell(2, 2).Value = "John Doe";
                //worksheet.Cell(2, 3).Value = 30;
                int row = 2;int stt = 1;
                foreach (var item in TaiKhoan)
                {
                    worksheet.Cell(row, 1).Value = stt;
                    worksheet.Cell(row, 2).Value = item.HoVaTen;
                    worksheet.Cell(row, 3).Value = item.TenPhongBan;
                    worksheet.Cell(row, 4).Value = item.TenChucVu;
                    worksheet.Cell(row, 5).Value = item.TenQuyen;
                    row++;stt++;
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0; // Reset con trỏ stream về đầu

                return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, "DanhSachTaiKhoan.xlsx");
            }

        }
    }
}
