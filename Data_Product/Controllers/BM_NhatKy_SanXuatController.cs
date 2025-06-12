using ClosedXML.Excel;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BM_NhatKy_SanXuatController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        //public PdfController(IConverter converter)
        //{
        //    _converter = converter;
        //}

        public BM_NhatKy_SanXuatController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = Now;
            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BG = TaiKhoan.ID_TaiKhoan;
            var trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Đang lưu" },
                new SelectListItem { Value = "1", Text = "Hoàn thành" }
            };

            ViewBag.TTList = new SelectList(trangThaiList, "Value", "Text", ID_TrangThai);
            var res = await(from a in _context.Tbl_NhatKy_SanXuat.Where(x => x.ID_NhanVien_SX == ID_NhanVien_BG && !x.IsDelete)
                            join b in _context.Tbl_TaiKhoan on a.ID_NhanVien_SX equals b.ID_TaiKhoan
                            select new Tbl_NhatKy_SanXuat
                            {
                                ID = a.ID,
                                Ca = a.Ca,
                                Kip = a.Kip,
                                NgayDungSX = a.NgayDungSX,
                                TinhTrang = a.TinhTrang,
                                SoPhieu = a.SoPhieu,
                                NgayTao = a.NgayTao,
                                Tbl_TaiKhoan = b,
                                IsLock = a.IsLock,
                            }).OrderByDescending(x => x.NgayDungSX).ToListAsync();
            if (ID_TrangThai != null) res = res.Where(x => x.TinhTrang == ID_TrangThai).ToList();
            if (begind != null && endd != null) res = res.Where(x => x.NgayDungSX >= startDay && x.NgayDungSX <= endDay).ToList();
           
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

        public async Task<IActionResult> Index_All(DateTime? begind, DateTime? endd, int? ID_TrangThai, string noidungDung,int? IDPhongBan, int? IDXuong, int? LyDoDung, string? IDCa, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = Now;
            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BG = TaiKhoan.ID_TaiKhoan;
            ViewBag.TaiKhoan = TaiKhoan;
            var trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Đang lưu" },
                new SelectListItem { Value = "1", Text = "Hoàn thành" }
            };

            ViewBag.ID_TrangThai = new SelectList(trangThaiList, "Value", "Text", ID_TrangThai);

            var ca = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Ngày" },
                new SelectListItem { Value = "2", Text = "Đêm" }
            };
            ViewBag.IDCa = new SelectList(ca, "Value", "Text", IDCa);

            ViewBag.IDPhongBan = new SelectList(_context.Tbl_PhongBan.ToList(), "ID_PhongBan", "TenPhongBan", IDPhongBan);
            ViewBag.LyDoDung = new SelectList(_context.Tbl_NhatKy_LyDo.ToList(), "ID", "TenLyDo", LyDoDung);
            ViewBag.noidungDung = noidungDung;
            ViewBag.IDXuong =  new SelectList(_context.Tbl_Xuong.ToList(), "ID_Xuong", "TenXuong", IDXuong);
            ViewBag.XuongSelect = IDXuong??0;
            
            // Bắt đầu bằng IQueryable (chưa gọi DB)
            var query = from a in _context.Tbl_NhatKy_SanXuat.Where(x => !x.IsDelete)
                             join b in _context.Tbl_TaiKhoan on a.ID_NhanVien_SX equals b.ID_TaiKhoan
                             let c = _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x=>x.ID_NhatKy == a.ID).ToList()
                             select new Tbl_NhatKy_SanXuat
                             {
                                 ID = a.ID,
                                 Ca = a.Ca,
                                 Kip = a.Kip,
                                 NgayDungSX = a.NgayDungSX,
                                 TinhTrang = a.TinhTrang,
                                 SoPhieu = a.SoPhieu,
                                 NgayTao = a.NgayTao,
                                 Tbl_TaiKhoan = b,
                                 NhatKy_SanXuat_ChiTiet = c,
                                 ID_PhongBan_SX = a.ID_PhongBan_SX,
                                 IsLock = a.IsLock,
                                 TinhTrangCheckPhieu = _context.Tbl_PKHXuLyPhieu.FirstOrDefault(x=>x.ID_NKDSX == a.ID && x.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan) != null?1:0
                             };
            //Set quyền 
            if(TaiKhoan.ID_Quyen ==9 || TaiKhoan.Quyen_Them.Contains("9"))
            {
                query = query.Where(x => x.ID_PhongBan_SX == TaiKhoan.ID_PhongBan);
            }
            // Áp dụng điều kiện lọc nếu có
            if (ID_TrangThai != null) query = query.Where(x => x.TinhTrang == ID_TrangThai);
            if (IDCa != null) query = query.Where(x => x.Ca == IDCa);
            if (IDPhongBan != null) query = query.Where(x => x.ID_PhongBan_SX == IDPhongBan);
            if (IDXuong != null) query = query.Where(x => x.NhatKy_SanXuat_ChiTiet.Any(a=>a.ID_Xuong == IDXuong));
            if (LyDoDung != null) query = query.Where(x => x.NhatKy_SanXuat_ChiTiet.Any(a => a.LyDo_DungThietBi == LyDoDung));
            if (startDay != default && endDay != default) query = query.Where(x => x.NgayDungSX >= startDay && x.NgayDungSX <= endDay);
            if (noidungDung != null) query = query.Where(x => x.NhatKy_SanXuat_ChiTiet.Any(a=> (!string.IsNullOrEmpty(a.NoiDungDung) && a.NoiDungDung.ToLower().Contains(noidungDung.ToLower()))));

            // Thực thi truy vấn tại đây
            var res = await query.OrderByDescending(x => x.NgayDungSX).ToListAsync();


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
            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
            var PhongBan = _context.Tbl_PhongBan.FirstOrDefault(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan);
            string TenBP = PhongBan.TenNgan.ToString();
            ViewBag.TenPhongBan = TenBP;

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            var NhanVien = await(from a in _context.Tbl_TaiKhoan
                                 select new Tbl_TaiKhoan
                                 {
                                     ID_TaiKhoan = a.ID_TaiKhoan,
                                     HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                 }).ToListAsync();
            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen");

            ViewBag.IDXuong = new SelectList(_context.Tbl_Xuong.Where(x=>x.ID_PhongBan == PhongBan.ID_PhongBan), "ID_Xuong", "TenXuong");

            var CaKip = await(from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == NgayLamViec)
                              select new Tbl_Kip
                              {
                                  ID_Kip = a.ID_Kip,
                                  TenCa = a.TenCa
                              }).ToListAsync();
            ViewBag.IDKip = new SelectList(CaKip, "ID_Kip", "TenCa");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_NhatKy_SanXuat _DO, IFormCollection formCollection, IFormFile FileDinhKem)
        {
            //int IDTaiKhoan = 0;
            int IDKip = 0;
            string XacNhan = "";
            string Luu = "";
            int IDNKSX = 0;
            string ID_Day = "";
            string ID_ca = "";

            List<Tbl_NhatKy_SanXuat_ChiTiet> Tbl_NhatKy_SanXuat_ChiTiet = new List<Tbl_NhatKy_SanXuat_ChiTiet>();
            try
            {
                //IDTaiKhoan = Convert.ToInt32(formCollection["IDTaiKhoan"]);
                IDKip = Convert.ToInt32(formCollection["IDKip"]);
                XacNhan = formCollection["luu"];
                ID_Day = formCollection["ID_Day"];
                ID_ca = formCollection["IDCa"];

                //Thông tin NhatKy_SanXuat
                var MaNV = User.FindFirstValue(ClaimTypes.Name);
                var ThongTin_NV = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MaNV).FirstOrDefault();
                var ThongTin_BP = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_NV.ID_PhongBan).FirstOrDefault();
                // Thông tin ca kíp làm việc
                DateTime date = DateTime.Parse(ID_Day);
                int Kip = Convert.ToInt32(ID_ca); // 1 ngày 2 đêm
                //var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == Kip).FirstOrDefault();
                var ID_Kip = _context.Tbl_Kip.Where(x => x.TenCa == ID_ca && x.NgayLamViec == date).FirstOrDefault();
                // Kiểm tra lại thông tin ca kíp làm việc 
                if (ID_Kip == null)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại ca kíp làm việc');</script>";
                    return RedirectToAction("Create", "BM_NhatKy_SanXuat");
                }
                string cakip = ID_Kip.TenCa + ID_Kip.TenKip;
                string checkPhieu = $"NKSX-{cakip}-{ThongTin_BP?.TenNgan}";
                var sttHomNay = _context.Tbl_NhatKy_SanXuat.Where(x=>x.SoPhieu.Contains(checkPhieu)).Count() + 1;
                string soPhieu = $"NKSX-{cakip}-{ThongTin_BP?.TenNgan}-{date.ToString("ddMMyyyy")}{sttHomNay.ToString("D3")}";

               
                var NhatKyNew = new Tbl_NhatKy_SanXuat()
                {
                    ID_NhanVien_SX = ThongTin_NV.ID_TaiKhoan,
                    ID_PhongBan_SX = ThongTin_BP.ID_PhongBan,
                    ID_Kip = ID_Kip.ID_Kip,
                    Ca = ID_Kip.TenCa,
                    Kip = ID_Kip.TenKip,
                    NgayDungSX = date,
                    NgayTao = DateTime.Now,
                    SoPhieu = soPhieu,
                    IsDelete = false,
                };
                if(XacNhan != null)
                {
                    NhatKyNew.TinhTrang = 0; // lưu
                }
                else NhatKyNew.TinhTrang = 1; // gửi
                _context.Tbl_NhatKy_SanXuat.Add(NhatKyNew);
                _context.SaveChanges();
                // Danh sach Tbl_NhatKy_SanXuat_ChiTiet
                if (_DO.NhatKy_SanXuat_ChiTiet != null && _DO.NhatKy_SanXuat_ChiTiet.Any())
                {
                    foreach (var item in _DO.NhatKy_SanXuat_ChiTiet)
                    {
                        var start = DateTime.Today.Add(item.ThoiDiemDung);
                        var end = DateTime.Today.Add(item.ThoiDiemChay);
                        if (end < start)
                        {
                            end = end.AddDays(1);
                        }
                        var nhatkychitiet =  new Tbl_NhatKy_SanXuat_ChiTiet()
                        {
                            
                            ID_Xuong = item.ID_Xuong,
                            ID_NhatKy = NhatKyNew.ID, // ID parent
                            LyDo_DungThietBi = item.LyDo_DungThietBi,
                            ThoiDiemDung = item.ThoiDiemDung,
                            ThoiDiemChay = item.ThoiDiemChay,
                            NoiDungDung = item.NoiDungDung,
                            GhiChu = item.GhiChu,
                            ThoiGianDung = (int)(end - start).TotalMinutes
                        };
                        _context.Tbl_NhatKy_SanXuat_ChiTiet.Add(nhatkychitiet);
                    }
                    _context.SaveChanges();
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                if (XacNhan != null)
                    return RedirectToAction("Edit", "BM_NhatKy_SanXuat", new { IDNKSX = NhatKyNew.ID });
                else return RedirectToAction("Edit", "BM_NhatKy_SanXuat", new { IDNKSX = NhatKyNew.ID });


            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại, lỗi "+e+" ');</script>";
                return RedirectToAction("Create", "BM_NhatKy_SanXuat");
            }
        }

        public async Task<IActionResult> Edit(int IDNKSX)
        {

            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
            var PhongBan = _context.Tbl_PhongBan.FirstOrDefault(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan);
            string TenBP = PhongBan.TenNgan.ToString();
            ViewBag.TenPhongBan = TenBP;

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();
            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen");

            ViewBag.IDXuong = new SelectList(_context.Tbl_Xuong.Where(x => x.ID_PhongBan == PhongBan.ID_PhongBan), "ID_Xuong", "TenXuong");

            

            var res = _context.Tbl_NhatKy_SanXuat.FirstOrDefault(x => x.ID == IDNKSX);
            res.NhatKy_SanXuat_ChiTiet = _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x=>x.ID_NhatKy ==  IDNKSX).ToList();

            var CaKip = await (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == NgayLamViec)
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenCa = a.TenCa
                               }).ToListAsync();
            ViewBag.TenKip = res.Kip;
            ViewBag.ID_Day = res.NgayDungSX.ToString("yyyy-MM-dd");

            return PartialView(res);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tbl_NhatKy_SanXuat _DO, IFormCollection formCollection, IFormFile FileDinhKem)
        {
            //int IDTaiKhoan = 0;
            int IDKip = 0;
            string XacNhan = "";
            string Luu = "";
            int IDNKSX = 0;
            string ID_Day = "";
            string ID_ca = "";

            List<Tbl_NhatKy_SanXuat_ChiTiet> Tbl_NhatKy_SanXuat_ChiTiet = new List<Tbl_NhatKy_SanXuat_ChiTiet>();
            try
            {
                //IDTaiKhoan = Convert.ToInt32(formCollection["IDTaiKhoan"]);
                IDKip = Convert.ToInt32(formCollection["IDKip"]);
                XacNhan = formCollection["luu"];
                ID_Day = formCollection["ID_Day"];
                ID_ca = formCollection["IDCa"];

                //Thông tin NhatKy_SanXuat
                var MaNV = User.FindFirstValue(ClaimTypes.Name);
                var ThongTin_NV = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == MaNV).FirstOrDefault();
                var ThongTin_BP = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_NV.ID_PhongBan).FirstOrDefault();
                // Thông tin ca kíp làm việc
                DateTime date = DateTime.Parse(ID_Day);
                int Kip = Convert.ToInt32(ID_ca); // 1 ngày 2 đêm
                //var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == Kip).FirstOrDefault();
                var ID_Kip = _context.Tbl_Kip.Where(x => x.TenCa == ID_ca && x.NgayLamViec == date).FirstOrDefault();
                // Kiểm tra lại thông tin ca kíp làm việc 
                //if (ID_Kip == null)
                //{
                //    TempData["msgSuccess"] = "<script>alert('Vui lòng kiểm tra lại ca kíp làm việc');</script>";
                //    return RedirectToAction("Create", "BM_NhatKy_SanXuat");
                //}

                var NhatKy = _context.Tbl_NhatKy_SanXuat.FirstOrDefault(x => x.ID == _DO.ID);
                //NhatKy.NgayDungSX = date;
                //NhatKy.ID_Kip = ID_Kip.ID_Kip;
                //NhatKy.Ca = ID_Kip.TenCa;
                //NhatKy.Kip = ID_Kip.TenKip;

                if (XacNhan != null)
                {
                    NhatKy.TinhTrang = 0; // lưu
                }
                else NhatKy.TinhTrang = 1; // gửi
                _context.SaveChanges(); // update Nhật ký
                // xóa danh sách chitiet cũ 
                var listchitiet = _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x => x.ID_NhatKy == _DO.ID).ToList();
                _context.Tbl_NhatKy_SanXuat_ChiTiet.RemoveRange(listchitiet);
                _context.SaveChanges();
                // Danh sach Tbl_NhatKy_SanXuat_ChiTiet
                if (_DO.NhatKy_SanXuat_ChiTiet != null && _DO.NhatKy_SanXuat_ChiTiet.Any())
                {
                    foreach (var item in _DO.NhatKy_SanXuat_ChiTiet)
                    {
                        var start = DateTime.Today.Add(item.ThoiDiemDung);
                        var end = DateTime.Today.Add(item.ThoiDiemChay);
                        if (end < start)
                        {
                            end = end.AddDays(1);
                        }
                        var nhatkychitiet = new Tbl_NhatKy_SanXuat_ChiTiet()
                        {

                            ID_Xuong = item.ID_Xuong,
                            ID_NhatKy = _DO.ID, // ID parent
                            LyDo_DungThietBi = item.LyDo_DungThietBi,
                            ThoiDiemDung = item.ThoiDiemDung,
                            ThoiDiemChay = item.ThoiDiemChay,
                            NoiDungDung = item.NoiDungDung,
                            GhiChu = item.GhiChu,
                            ThoiGianDung = (int)(end - start).TotalMinutes
                        };
                        _context.Tbl_NhatKy_SanXuat_ChiTiet.Add(nhatkychitiet);
                    }
                    _context.SaveChanges();
                }
                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
                if (XacNhan != null)
                    return RedirectToAction("Edit", "BM_NhatKy_SanXuat", new { IDNKSX = _DO.ID });
                else return RedirectToAction("View_Details", "BM_NhatKy_SanXuat", new { IDNKSX = _DO.ID });


            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại, lỗi " + e + " ');</script>";
                return RedirectToAction("Edit", "BM_NhatKy_SanXuat", new { IDNKSX = _DO.ID });
            }
        }

        public async Task<IActionResult> View_Details(int IDNKSX)
        {

            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
            var PhongBan = _context.Tbl_PhongBan.FirstOrDefault(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan);
            string TenBP = PhongBan.TenNgan.ToString();
            ViewBag.TenPhongBan = TenBP;

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();
            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen");

            //ViewBag.IDXuong = new SelectList(_context.Tbl_Xuong.Where(x => x.ID_PhongBan == PhongBan.ID_PhongBan), "ID_Xuong", "TenXuong");



            var res = _context.Tbl_NhatKy_SanXuat.FirstOrDefault(x => x.ID == IDNKSX);
            res.NhatKy_SanXuat_ChiTiet = _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x => x.ID_NhatKy == IDNKSX).ToList();

            var CaKip = await (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == NgayLamViec)
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenCa = a.TenCa
                               }).ToListAsync();
            ViewBag.TenKip = res.Kip;
            ViewBag.ID_Day = res.NgayDungSX.ToString("yyyy-MM-dd");

            ViewBag.taikhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x=>x.ID_TaiKhoan == res.ID_NhanVien_SX)?.HoVaTen;
            ViewBag.ID_PhongBan = _context.Tbl_PhongBan.FirstOrDefault(x=>x.ID_PhongBan == res.ID_PhongBan_SX)?.TenPhongBan;

            TimeSpan tgdungThietbi = TimeSpan.FromMinutes(res.NhatKy_SanXuat_ChiTiet.Where(x => x.LyDo_DungThietBi == 1).Sum(x => x.ThoiGianDung)??0);
            TimeSpan tgdungCongNge = TimeSpan.FromMinutes(res.NhatKy_SanXuat_ChiTiet.Where(x => x.LyDo_DungThietBi == 2).Sum(x => x.ThoiGianDung) ?? 0);
            TimeSpan DungSuCoCN = TimeSpan.FromMinutes(res.NhatKy_SanXuat_ChiTiet.Where(x => x.LyDo_DungThietBi == 3).Sum(x => x.ThoiGianDung) ?? 0);
            TimeSpan DungKhachQuan = TimeSpan.FromMinutes(res.NhatKy_SanXuat_ChiTiet.Where(x => x.LyDo_DungThietBi == 4).Sum(x => x.ThoiGianDung) ?? 0);
            TimeSpan TongTgianDung = TimeSpan.FromMinutes(res.NhatKy_SanXuat_ChiTiet.Sum(x => x.ThoiGianDung) ?? 0);


            ViewBag.DungThietBi = tgdungThietbi.TotalHours.ToString("F2");
            ViewBag.DungCongNghe = tgdungCongNge.TotalHours.ToString("F2");
            ViewBag.DungSuCoCN = DungSuCoCN.TotalHours.ToString("F2");
            ViewBag.DungKhachQuan = DungKhachQuan.TotalHours.ToString("F2");
            ViewBag.TongTgianDung = TongTgianDung.TotalHours.ToString("F2");

            return PartialView(res);
        }
        public async Task<IActionResult> ExportToExcel(DateTime? begind, DateTime? endd, string noidungDung, int? ID_TrangThai, int? IDXuong, int? IDPhongBan, int? LyDoDung, string? IDCa)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_NhatKyDungSanXuat.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_NhatKyDungSanXuat_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("BM");
                var res = from a in _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x => (!IDXuong.HasValue || x.ID_Xuong == IDXuong))
                          join b in _context.Tbl_NhatKy_SanXuat.Where(x => (!begind.HasValue || x.NgayDungSX >= begind) && (!endd.HasValue || x.NgayDungSX <= endd) && (!ID_TrangThai.HasValue || x.TinhTrang == ID_TrangThai)) on a.ID_NhatKy equals b.ID
                          join c in _context.Tbl_PhongBan.Where(x => (!IDPhongBan.HasValue || x.ID_PhongBan == IDPhongBan)) on b.ID_PhongBan_SX equals c.ID_PhongBan
                          join d in _context.Tbl_Xuong on a.ID_Xuong equals d.ID_Xuong
                          select new Tbl_NhatKy_SanXuat_ChiTietExport
                          {
                              IDCT = a.IDCT,
                              ID_Xuong = a.ID_Xuong,
                              TenXuong = d.TenXuong,
                              Ca = b.Ca,
                              Kip= b.Kip,
                              SoPhieu = b.SoPhieu,
                              NgayDungSX = b.NgayDungSX,
                              TenPhongBan = c.TenPhongBan,
                              ThoiDiemDung = a.ThoiDiemDung,
                              ThoiDiemChay = a.ThoiDiemChay,
                              ThoiGianDung = a.ThoiGianDung,
                              LyDo_DungThietBi = a.LyDo_DungThietBi,
                              NoiDungDung = a.NoiDungDung,
                              GhiChu = a.GhiChu,
                              TinhTrang = b.TinhTrang

                          };

                if (IDCa != null) res = res.Where(x => x.Ca == IDCa);
                if (LyDoDung != null) res = res.Where(x => x.LyDo_DungThietBi == LyDoDung);
                if (noidungDung != null) res = res.Where(x => (!string.IsNullOrEmpty(x.NoiDungDung) && x.NoiDungDung.ToLower().Contains(noidungDung.ToLower())));


                // Thực thi truy vấn tại đây
                var Data = await res.ToListAsync();



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
                        Worksheet.Cell(row, icol).Value = item.NgayDungSX;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd/MM/yyyy";
                      
                        icol++;
                        Worksheet.Cell(row, icol).Value = item.Kip;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        if (item.Ca == "1")
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


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;



                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TenXuong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.ThoiDiemDung.ToString(@"hh\:mm");
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.ThoiDiemChay.ToString(@"hh\:mm");
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        TimeSpan tgdungThietbi = TimeSpan.FromMinutes(item.ThoiGianDung??0);
                        icol++;
                        Worksheet.Cell(row, icol).Value = tgdungThietbi.TotalHours.ToString("F2");
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.LyDo_DungThietBi ==1?"X":"";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.LyDo_DungThietBi == 2 ? "X" : "";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.LyDo_DungThietBi == 3 ? "X" : "";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;





                        icol++;
                        Worksheet.Cell(row, icol).Value = item.LyDo_DungThietBi == 4 ? "X" : "";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.NoiDungDung;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.GhiChu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.SoPhieu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.TinhTrang ==0?"Đang lưu":"Hoàn tất";
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }

                    Worksheet.Range("A7:Q" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:Q" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A7:Q" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:Q" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "NhatKyDungSX - " + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "NhatKyDungSX - " + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index_All", "BM_NhatKy_SanXuat");
            }
        }

        public async Task<IActionResult> XoaDuLieu(int? id)
        {
            try {
                var nkdsx = _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x => x.ID_NhatKy == id);
                if (nkdsx.Any())
                {
                    _context.Tbl_NhatKy_SanXuat_ChiTiet.RemoveRange(nkdsx);
                    _context.SaveChanges();
                    TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công!');</script>";
                }
                return RedirectToAction("Edit", "BM_NhatKy_SanXuat", new { IDNKSX = id });
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi "+ex+" khi xóa dữ liệu.');</script>";

                return RedirectToAction("Edit", "BM_NhatKy_SanXuat", new { IDNKSX = id });
            }
        }

        [HttpPost]
        public IActionResult CheckXuLy([FromBody] List<int> selectedItems)
        {
            if (selectedItems == null || !selectedItems.Any())
                return BadRequest("Không có dữ liệu cần xóa.");

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            // Xử lý danh sách ID được chọn
            foreach (var id in selectedItems)
            {
                // Lưu vào database
                _context.Tbl_PKHXuLyPhieu.Add(new Tbl_PKHXuLyPhieu
                {
                    ID_NKDSX = id,
                    ID_TaiKhoan = TaiKhoan.ID_TaiKhoan,
                    NgayXuLy = DateTime.Now,
                    TinhTrang = true
                });
            }
            _context.SaveChanges();

            return Ok();
        }

        public async Task<IActionResult> KhoaPhieu()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KhoaPhieu(DateTime? monthPicker, bool CheckUnlock)
        {
            try
            {
                // Xử lý logic tại đây
                if (monthPicker == null)
                    return Json(new { success = false, message = "Chưa chọn tháng cần xóa." });
                if (monthPicker != null && CheckUnlock == false)
                {
                    DateTime tg = (DateTime)monthPicker.Value;
                    var checkTg = _context.Tbl_ThoiGianKhoa.Where(x => x.Thang == tg.Month && x.Nam == tg.Year && x.MaBB == "NKDSX").FirstOrDefault();
                    if (checkTg == null)
                    {
                        Tbl_ThoiGianKhoa khoa = new Tbl_ThoiGianKhoa()
                        {
                            Nam = tg.Year,
                            Thang = tg.Month,
                            NgayXuLy = DateTime.Now,
                            MaBB = "NKDSX"
                        };
                        _context.Tbl_ThoiGianKhoa.Add(khoa);
                        _context.SaveChanges();
                        // khóa tất cả phiếu tháng 11
                        var listPhieu = _context.Tbl_NhatKy_SanXuat.Where(x => x.NgayDungSX.Month == tg.Month && x.NgayDungSX.Year == tg.Year).ToList();
                        // Cập nhật giá trị cho các trường
                        foreach (var entity in listPhieu)
                        {
                            entity.IsLock = true;
                        }

                        // Lưu thay đổi vào database
                        _context.SaveChanges();
                        return Json(new { success = true, message = "Khóa phiếu thành công " });
                    }
                    else
                    {
                        return Json(new { success = true, message = "Thời gian đã được khóa trước đó " });
                    }
                }
                else if (monthPicker != null && CheckUnlock == true) // Unlock All
                {
                    DateTime tg = (DateTime)monthPicker.Value;
                    var checkTg = _context.Tbl_ThoiGianKhoa.Where(x => x.Thang == tg.Month && x.Nam == tg.Year && x.MaBB == "NKDSX").FirstOrDefault();
                    if (checkTg != null)
                    {
                        _context.Tbl_ThoiGianKhoa.Remove(checkTg);
                        _context.SaveChanges();
                        // mở khóa tất cả phiếu tháng
                        var listPhieu = _context.Tbl_NhatKy_SanXuat.Where(x => x.NgayDungSX.Month == tg.Month && x.NgayDungSX.Year == tg.Year).ToList();
                        // Cập nhật giá trị cho các trường
                        foreach (var entity in listPhieu)
                        {
                            entity.IsLock = false;
                        }
                        // Lưu thay đổi vào database
                        _context.SaveChanges();
                        return Json(new { success = true, message = "Xử lý mở khóa thành công " });
                    }
                    else
                    {
                        return Json(new { success = true, message = "Thời gian này chưa được xử lý " });
                    }
                }
                return Json(new { success = true, message = "Thành công! " });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                // _logger.LogError(ex, "Lỗi khi khóa phiếu");

                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult XuLyKhoa(int id)
        {
            if (id == 0 )
                return BadRequest("Không có dữ liệu cần khóa.");
            // mở khóa tất cả phiếu tháng
            var Phieu = _context.Tbl_NhatKy_SanXuat.Where(x => x.ID == id).FirstOrDefault();
            // Cập nhật giá trị cho các trường
            Phieu.IsLock = true;
            // Lưu thay đổi vào database
            _context.SaveChanges();
            return Json(new { success = true, message = "Khóa phiếu thành công " });

        }
        [HttpPost]
        public IActionResult XuLyMoKhoa(int id)
        {
            if (id == 0)
                return BadRequest("Không có dữ liệu cần khóa.");

            // mở khóa tất cả phiếu tháng
            var Phieu = _context.Tbl_NhatKy_SanXuat.Where(x => x.ID == id).FirstOrDefault();
            // Cập nhật giá trị cho các trường
            Phieu.IsLock = false;
            // Lưu thay đổi vào database
            _context.SaveChanges();
            return Json(new { success = true, message = "Mở khóa phiếu thành công " });

        }

    }
}
