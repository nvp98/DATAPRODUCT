using ClosedXML.Excel;
using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace Data_Product.Controllers
{
    public class MaLoController : Controller
    {
        private readonly DataContext _context;

        public MaLoController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_MaLo
                             select new Tbl_MaLo
                             {
                                 ID_MaLo = a.ID_MaLo,
                                 TenMaLo = a.TenMaLo,
                                 PhongBan = a.PhongBan,
                                 ID_TinhTrang = (int)a.ID_TinhTrang,
                                 VatTu = (from v in _context.Tbl_VatTuMaLo.Where(x => x.ID_MaLo == a.ID_MaLo)
                                          join pb in _context.Tbl_VatTu on v.ID_VatTu equals pb.ID_VatTu
                                          select new Tbl_VatTu { ID_VatTu = pb.ID_VatTu, TenVatTu = pb.TenVatTu }).ToList(),
                             }).ToListAsync();
          
            //List<int> selectedValues = _context.Tbl_VatTuMaLo.Where(x => x.ID_MaLo == id).Select(p => p.ID_VatTu).ToList();
            if (search != null)
            {
                res = res.Where(x => (x?.TenMaLo ?? string.Empty).Contains(search)).ToList();

            }
            //const int pageSize = 20;
            //if (page < 1)
            //{
            //    page = 1;
            //}
            //int resCount = res.Count;
            //var pager = new Pager(resCount, page, pageSize);
            //int recSkip = (page - 1) * pageSize;
            //var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            //this.ViewBag.Pager = pager;
            return View(res);
        }
        public async Task<IActionResult> Create()
        {
            //List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            //ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            List<Tbl_VatTu> vt = _context.Tbl_VatTu.ToList();
            ViewBag.ID_VatTu = new SelectList(vt, "ID_VatTu", "TenVatTu");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_MaLo _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                Tbl_MaLo malo = new Tbl_MaLo();
                malo.TenMaLo = _DO.TenMaLo;
                malo.ID_TinhTrang = 1;
                // kiểm tra mã lô
                if(malo.TenMaLo == null || malo.TenMaLo == "")
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại tên mã lô');</script>";
                    return RedirectToAction("Index", "MaLo");
                }
                _context.Tbl_MaLo.Add(malo);
                _context.SaveChanges();
                

                if (_DO.ID_VatTu != null)
                {
                    foreach (var bp in _DO.ID_VatTu)
                    {
                        int vt =Convert.ToInt32(bp) ;
                        var Value = _context.Tbl_VatTu.Where(x=>x.ID_VatTu == vt).FirstOrDefault();
                        Tbl_VatTuMaLo ha = new Tbl_VatTuMaLo
                            {
                            ID_VatTu = vt,
                            ID_MaLo = malo.ID_MaLo
                            };
                        _context.Tbl_VatTuMaLo.Add(ha);
                        _context.SaveChanges();
                    }
                  
                }
                else
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Vật tư');</script>";
                    return RedirectToAction("Index", "MaLo");
                }



                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "MaLo");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "MaLo");
            }

            var res = await (from a in _context.Tbl_MaLo.Where(x=>x.ID_MaLo == id)
                             select new Tbl_MaLo
                             {
                                 ID_MaLo = a.ID_MaLo,
                                 TenMaLo = a.TenMaLo,
                                 //PhongBan = a.PhongBan,
                                 //ID_TinhTrang = (int)a.ID_TinhTrang
                                 VatTu = (from v in _context.Tbl_VatTuMaLo.Where(x => x.ID_MaLo == a.ID_MaLo)
                                          join pb in _context.Tbl_VatTu on v.ID_VatTu equals pb.ID_VatTu
                                          select new Tbl_VatTu { ID_VatTu = pb.ID_VatTu, TenVatTu = pb.TenVatTu }).ToList(),
                             }).ToListAsync();

            Tbl_MaLo DO = new Tbl_MaLo();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_MaLo = a.ID_MaLo;
                    DO.TenMaLo = a.TenMaLo;
                    //DO.PhongBan = a.PhongBan;
                    DO.VatTu = a.VatTu;
                    DO.ID_TinhTrang = (int)a.ID_TinhTrang;
                }

                //List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
                //ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

                List<Tbl_VatTu> vt = _context.Tbl_VatTu.ToList();
                // Các ID được chọn ban đầu
                List<string> selectedValues = (from v in _context.Tbl_VatTuMaLo.Where(x => x.ID_MaLo == id)
                                            join pb in _context.Tbl_VatTu on v.ID_VatTu equals pb.ID_VatTu
                                            select new Tbl_VatTu { ID_VatTu = pb.ID_VatTu, TenVatTu = pb.TenVatTu }).Select(p=>p.TenVatTu).ToList();
                var selectedValuesStr = new List<string>();
                foreach (var item in _context.Tbl_VatTuMaLo.Where(x => x.ID_MaLo == id))
                {
                    if (item.ID_VatTu != 0) { selectedValuesStr.Add(item.ID_VatTu.ToString()); }
                }
                // Tạo SelectList và đánh dấu các mục đã chọn
                ViewBag.ID_VatTu = new SelectList(vt, "ID_VatTu", "TenVatTu");
                ViewBag.ID_VatTuSelec = selectedValues;
                ViewBag.selectedValues = selectedValuesStr.ToArray();
                ViewBag.ID_MaLo = id;
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_MaLo _DO)
        {
            try
            {
                //Tbl_MaLo malo = new Tbl_MaLo();
                //malo.TenMaLo = _DO.TenMaLo;
                //malo.ID_TinhTrang = 1;
                var ml =_context.Tbl_MaLo.Find(id);
                ml.TenMaLo = _DO.TenMaLo;
                // kiểm tra mã lô
                if (ml.TenMaLo == null || ml.TenMaLo == "")
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại tên mã lô');</script>";
                    return RedirectToAction("Index", "MaLo");
                }
                _context.SaveChanges();


                if (_DO.ID_VatTu != null)
                {
                    foreach (var bp in _DO.ID_VatTu)
                    {
                        int vt = Convert.ToInt32(bp);
                        var Value = _context.Tbl_VatTu.Where(x => x.ID_VatTu == vt).FirstOrDefault();
                        Tbl_VatTuMaLo ha = new Tbl_VatTuMaLo
                        {
                            ID_VatTu = vt,
                            ID_MaLo = id
                        };
                        // Check if the record already exists
                        bool exists = _context.Tbl_VatTuMaLo
                                             .Any(x => x.ID_MaLo == id && x.ID_VatTu ==vt);

                        if (!exists)
                        {
                            _context.Tbl_VatTuMaLo.Add(ha);
                            _context.SaveChanges();
                            // Record !exists, return false or handle accordingly
                        }
                    }

                }
                //else
                //{
                //    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn Vật tư');</script>";
                //    return RedirectToAction("Index", "MaLo");
                //}

                //if (_DO.ID_MaLo != null)
                //{
                //    var Name = new List<string>();
                //    foreach (var bp in _DO.ID_VatTu)
                //    {
                //        int ID_PhongBan = Convert.ToInt32(bp);
                //        var Value = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_PhongBan).FirstOrDefault();
                //        Name.Add(Value.TenNgan);
                //    }
                //    var ListGate = string.Join(", ", Name);

                //    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_MaLo_update {0},{1},{2}",
                //    _DO.ID_MaLo,_DO.TenMaLo, ListGate);
                //}
                //else
                //{
                //    var DO = _context.Tbl_MaLo.Where(x => x.ID_MaLo == _DO.ID_MaLo).FirstOrDefault();

                //    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_MaLo_update {0},{1},{2}",
                //    _DO.ID_MaLo, _DO.TenMaLo, DO.PhongBan);

                //}
                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }



            return RedirectToAction("Index", "MaLo");
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_MaLo_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "MaLo");
        }
        public async Task<IActionResult> Lock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_MaLo_lock {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "MaLo");
        }
        public async Task<IActionResult> Unlock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_MaLo_lock {0},{1}", id, 1);

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "MaLo");
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
                    return RedirectToAction("Index", "MaLo");
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

                            string MaLo = serviceDetails.Rows[i][2].ToString().Trim();
                            // Check thông tin phòng ban
                            string TenVT = serviceDetails.Rows[i][1].ToString().Trim();
                            var check_vt = _context.Tbl_VatTu.Where(x => x.TenVatTu == TenVT).FirstOrDefault();
                            if (check_vt == null)
                            {
                                TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra tên Vật tư: " + TenVT + "');</script>";
                                return RedirectToAction("Index", "MaLo");
                            }
                            // Check thông tin mã lô trùng
                            var ID_MaLo = _context.Tbl_MaLo.Where(x => x.TenMaLo == MaLo).FirstOrDefault();
                            if (ID_MaLo == null )
                            {
                                Tbl_MaLo malo = new Tbl_MaLo();
                                malo.TenMaLo = MaLo;
                                malo.ID_TinhTrang = 1;
                                // kiểm tra mã lô
                                if (malo.TenMaLo == null || malo.TenMaLo == "")
                                {
                                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại tên mã lô " + malo.TenMaLo + "');</script>";
                                    return RedirectToAction("Index", "MaLo");
                                }
                                _context.Tbl_MaLo.Add(malo);
                                _context.SaveChanges();


                                if (check_vt.ID_VatTu != null)
                                {
                                    int vt = Convert.ToInt32(check_vt.ID_VatTu);
                                    var Value = _context.Tbl_VatTu.Where(x => x.ID_VatTu == vt).FirstOrDefault();
                                    Tbl_VatTuMaLo ha = new Tbl_VatTuMaLo
                                    {
                                        ID_VatTu = vt,
                                        ID_MaLo = malo.ID_MaLo
                                    };
                                    _context.Tbl_VatTuMaLo.Add(ha);
                                    _context.SaveChanges();
                                }
                            }
                            else
                            {
                                TempData["msgSuccess"] = "<script>alert('Mã lô đã tồn tại : " + MaLo + "');</script>";
                                return RedirectToAction("Index", "MaLo");
                            }

                            

                            //var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_MaLo_insert {0},{1},{2}",
                            //                                     MaLo, TenBP, 1);
                        }
                    }
                }
                TempData["msgSuccess"] = "<script>alert('Import thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "MaLo");
        }

        public IActionResult ExportToExcel()
        {
            var data = (from a in _context.Tbl_MaLo
                        select new Tbl_MaLo
                        {
                            ID_MaLo = a.ID_MaLo,
                            TenMaLo = a.TenMaLo,
                            PhongBan = a.PhongBan,
                            ID_TinhTrang = (int)a.ID_TinhTrang,
                            VatTu = (from v in _context.Tbl_VatTuMaLo.Where(x => x.ID_MaLo == a.ID_MaLo)
                                     join pb in _context.Tbl_VatTu on v.ID_VatTu equals pb.ID_VatTu
                                     select new Tbl_VatTu { ID_VatTu = pb.ID_VatTu, TenVatTu = pb.TenVatTu }).ToList(),
                        }).ToList();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("MaLo");
                //Header
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Tên mã lô";
                worksheet.Cell(1, 3).Value = "Mã Vật tư";
                //value
                int row = 2; int stt = 1;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = stt;
                    worksheet.Cell(row, 2).Value = item.TenMaLo;
                    worksheet.Cell(row, 3).Value = item.VatTu.Select(x=>x.TenVatTu).ToArray().ToString();
                    if (item.VatTu.Count() != 0)
                    {
                        var selectedValues = new List<string>(); // ID các giá trị mặc định

                        foreach (var vattu in item.VatTu)
                        {
                            selectedValues.Add(vattu.TenVatTu.ToString());
                        }
                        worksheet.Cell(row, 3).Value = string.Join(", ", selectedValues);
                    }
                    
                        row++; stt++;
                }

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0; // Reset con trỏ stream về đầu

                return File(stream, System.Net.Mime.MediaTypeNames.Application.Octet, "DanhSachMaLo.xlsx");
            }
        }

    }
}
