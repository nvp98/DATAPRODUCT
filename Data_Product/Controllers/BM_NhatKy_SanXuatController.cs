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

using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Font;
using iText.Kernel.Pdf;
using iText.Barcodes;
using iText.Kernel.Pdf.Xobject;
using iText.StyledXmlParser.Node;

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
                new SelectListItem { Value = "-1", Text = "Không xác nhận" },
                new SelectListItem { Value = "0", Text = "Đã gửi" },
                new SelectListItem { Value = "1", Text = "Hoàn thành" },
                new SelectListItem { Value = "2", Text = "Đang xóa phiếu" },
                new SelectListItem { Value = "3", Text = "Đã xóa phiếu" },
            };
            ViewBag.TTList = new SelectList(trangThaiList, "Value", "Text", ID_TrangThai);
            var res = await(from a in _context.Tbl_NhatKy_SanXuat.Where(x => x.ID_NhanVien_SX == ID_NhanVien_BG && !x.IsDelete)
                            join b in _context.Tbl_TaiKhoan on a.ID_NhanVien_SX equals b.ID_TaiKhoan
                            let c = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.ID_TaiKhoan == a.ID_NhanVien_BTBD)
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
                                ID_NhanVien_BTBD = a.ID_NhanVien_BTBD,
                                HoTen_NhanVien_BTBD = (c.TenTaiKhoan ?? "") + " - " + (c.HoVaTen ?? "")
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

        public async Task<IActionResult> Index_BTBD(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime now = DateTime.Now;
            DateTime startDay = begind ?? new DateTime(now.Year, now.Month, 1);
            DateTime endDay = endd ?? now;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
            int ID_NhanVien = TaiKhoan?.ID_TaiKhoan ?? 0;
            var checkQuyenBTBD = _context.Tbl_QuyenXuLy.Where(x => x.MaXL == "BTBD" && x.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan).Select(x => x.ID_XuongXL).Distinct().ToList();
            var trangThaiList = new List<SelectListItem>
            {
                new SelectListItem { Value = "-1", Text = "Không xác nhận" },
                new SelectListItem { Value = "0", Text = "Chờ xử lý" },
                new SelectListItem { Value = "1", Text = "Hoàn thành" },
                new SelectListItem { Value = "2", Text = "Đang xóa phiếu" },
                new SelectListItem { Value = "3", Text = "Đã xóa phiếu" }
            };
            ViewBag.TTList = new SelectList(trangThaiList, "Value", "Text", ID_TrangThai);

            // Query gốc, chưa gọi ToListAsync
            var query = from a in _context.Tbl_NhatKy_SanXuat.Where(x=> x.ID_NhanVien_BTBD == TaiKhoan.ID_TaiKhoan)
                        join b in _context.Tbl_TaiKhoan on a.ID_NhanVien_SX equals b.ID_TaiKhoan
                        where !a.IsDelete
                        let c = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.ID_TaiKhoan == a.ID_NhanVien_BTBD)
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
                            ID_NhanVien_BTBD = a.ID_NhanVien_BTBD,
                            HoTen_NhanVien_BTBD = (c.TenTaiKhoan ?? "") + " - " + (c.HoVaTen ?? "")
                        };

            // Áp dụng lọc trang thái
            if (ID_TrangThai != null)
            {
                query = query.Where(x => x.TinhTrang == ID_TrangThai);
            }

            // Lọc theo ngày (nếu có cả 2)
            query = query.Where(x => x.NgayDungSX >= startDay && x.NgayDungSX <= endDay);

            // Tổng số dòng
            int resCount = await query.CountAsync();

            // Phân trang
            const int pageSize = 20;
            page = page < 1 ? 1 : page;
            int recSkip = (page - 1) * pageSize;
            var pager = new Pager(resCount, page, pageSize);

            var data = await query.OrderBy(x => x.TinhTrang)
                                  .Skip(recSkip)
                                  .Take(pager.PageSize)
                                  .ToListAsync();

            ViewBag.Pager = pager;
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
                new SelectListItem { Value = "-1", Text = "Không xác nhận" },
                new SelectListItem { Value = "0", Text = "Đã gửi" },
                new SelectListItem { Value = "1", Text = "Hoàn thành" },
                new SelectListItem { Value = "2", Text = "Đang xóa phiếu" },
                new SelectListItem { Value = "3", Text = "Đã xóa phiếu" }
            };
            ViewBag.ID_TrangThai = new SelectList(trangThaiList, "Value", "Text", ID_TrangThai);

            var ca = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Ngày" },
                new SelectListItem { Value = "2", Text = "Đêm" }
            };
            ViewBag.IDCa = new SelectList(ca, "Value", "Text", IDCa);

            var dsPhongBan = _context.Tbl_PhongBan.ToList();
            var dsXuongXL = _context.Tbl_QuyenXuLy.Where(x => x.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan && x.MaXL == "BTBD").Select(x=>x.ID_XuongXL).ToList();
            var dsPhongBanXL = _context.Tbl_Xuong.Where(x => dsXuongXL.Contains(x.ID_Xuong)).Select(x => x.ID_PhongBan).ToList();
            var quyenThem = (TaiKhoan.Quyen_Them ?? "").Split(',');
            var phongBanThem = (TaiKhoan.PhongBan_Them ?? "").Split(',');
            if (TaiKhoan.ID_Quyen == 9 || TaiKhoan.ID_Quyen == 10 ||  quyenThem.Contains("9") || quyenThem.Contains("10"))
            {
                dsPhongBan = dsPhongBan.Where(x =>x.ID_PhongBan == TaiKhoan.ID_PhongBan || dsPhongBanXL.Any(k => k == x.ID_PhongBan) ||  phongBanThem.Any(a=>a.Contains(x?.TenNgan??""))).ToList();
            }

            ViewBag.IDPhongBan = new SelectList(dsPhongBan, "ID_PhongBan", "TenPhongBan", IDPhongBan);
            ViewBag.LyDoDung = new SelectList(_context.Tbl_NhatKy_LyDo.ToList(), "ID", "TenLyDo", LyDoDung);
            ViewBag.noidungDung = noidungDung;
            ViewBag.IDXuong =  new SelectList(_context.Tbl_Xuong.ToList(), "ID_Xuong", "TenXuong", IDXuong);
            ViewBag.XuongSelect = IDXuong??0;
            
            // Bắt đầu bằng IQueryable (chưa gọi DB)
            var query = from a in _context.Tbl_NhatKy_SanXuat.Where(x => !x.IsDelete)
                             join b in _context.Tbl_TaiKhoan on a.ID_NhanVien_SX equals b.ID_TaiKhoan
                             let c = _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x=>x.ID_NhatKy == a.ID).ToList()
                             let btbd = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.ID_TaiKhoan == a.ID_NhanVien_BTBD)
                             let xuong = _context.Tbl_Xuong.FirstOrDefault(x => x.ID_Xuong == a.ID_Xuong_SX)
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
                                 TinhTrangCheckPhieu = _context.Tbl_PKHXuLyPhieu.FirstOrDefault(x=>x.ID_NKDSX == a.ID && x.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan) != null?1:0,
                                 ID_NhanVien_BTBD = a.ID_NhanVien_BTBD,
                                 HoTen_NhanVien_BTBD = (btbd.TenTaiKhoan ?? "") + " - " + (btbd.HoVaTen ?? ""),
                                 TenXuong_SX = xuong.TenXuong??"",
                                 GhiChu = a.GhiChu
                             };
            //Set quyền 
            List<string> ListPB = new List<string>();
            List<int> ListPBInt = new List<int>();
            if (TaiKhoan.PhongBan_Them != null)
            {
                ListPB = TaiKhoan.PhongBan_Them.Split(',').Select(item => item.Trim()).ToList();
                foreach (var item in ListPB)
                {
                    var pb = _context.Tbl_PhongBan.Where(x => x.TenNgan == item).FirstOrDefault();
                    if (pb != null) ListPBInt.Add(pb.ID_PhongBan);
                }
            }
            // Filter Phòng ban Bổ sung
            if (TaiKhoan.ID_Quyen > 3)
            {
                if (TaiKhoan.ID_Quyen != 8)
                {
                    query = query.Where(x => x.ID_PhongBan_SX == TaiKhoan.ID_PhongBan || dsPhongBanXL.Any(k => k == x.ID_PhongBan_SX) || ListPBInt.Any(k =>k ==x.ID_PhongBan_SX) || x.ID_NhanVien_BTBD == TaiKhoan.ID_TaiKhoan);
                }
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

            ViewBag.IDXuong = new SelectList(_context.Tbl_Xuong.Where(x=>x.ID_Xuong == TaiKhoan.ID_PhanXuong), "ID_Xuong", "TenXuong");

            var CaKip = await(from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == NgayLamViec)
                              select new Tbl_Kip
                              {
                                  ID_Kip = a.ID_Kip,
                                  TenCa = a.TenCa
                              }).ToListAsync();
            ViewBag.IDKip = new SelectList(CaKip, "ID_Kip", "TenCa");
            var nvBTBD = _context.Tbl_QuyenXuLy.Where(x => x.ID_XuongXL == TaiKhoan.ID_PhanXuong && x.MaXL == "BTBD").Select(x => x.ID_TaiKhoan).ToList();
            ViewBag.IDNhanVienBTBD = new SelectList(NhanVien.Where(x=>nvBTBD.Contains(x.ID_TaiKhoan)), "ID_TaiKhoan", "HoVaTen");
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_NhatKy_SanXuat _DO, IFormCollection formCollection, IFormFile FileDinhKem)
        {
            int IDTaiKhoanBTBD = 0;
            int IDKip = 0;
            string XacNhan = "";
            string Luu = "";
            int IDNKSX = 0;
            string ID_Day = "";
            string ID_ca = "";

            List<Tbl_NhatKy_SanXuat_ChiTiet> Tbl_NhatKy_SanXuat_ChiTiet = new List<Tbl_NhatKy_SanXuat_ChiTiet>();
            try
            {
                if (formCollection["IDNhanVienBTBD"].ToString() != null)
                {
                    IDTaiKhoanBTBD = Convert.ToInt32(formCollection["IDNhanVienBTBD"]);
                }
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
                if (IDTaiKhoanBTBD == 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên xử lý BTBD');</script>";
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
                    ID_Xuong_SX = ThongTin_NV?.ID_PhanXuong??0,
                    ID_Kip = ID_Kip.ID_Kip,
                    Ca = ID_Kip.TenCa,
                    Kip = ID_Kip.TenKip,
                    NgayDungSX = date,
                    NgayTao = DateTime.Now,
                    SoPhieu = soPhieu,
                    IsDelete = false,
                    ID_NhanVien_BTBD = IDTaiKhoanBTBD
                };
                //if(XacNhan != null)
                //{
                //    NhatKyNew.TinhTrang = 0; // lưu
                //}
                //else NhatKyNew.TinhTrang = 1; // gửi
                NhatKyNew.TinhTrang = 0; // gửi dữ liệu
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
                    return RedirectToAction("View_Details", "BM_NhatKy_SanXuat", new { IDNKSX = NhatKyNew.ID });
                else return RedirectToAction("View_Details", "BM_NhatKy_SanXuat", new { IDNKSX = NhatKyNew.ID });


            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại, lỗi "+e+" ');</script>";
                return RedirectToAction("Create", "BM_NhatKy_SanXuat");
            }
        }

        public async Task<IActionResult> Edit(int IDNKSX, int? isHieuChinh)
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

            ViewBag.IDXuong = new SelectList(_context.Tbl_Xuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong), "ID_Xuong", "TenXuong");


            ViewBag.IsHieuChinh = isHieuChinh;
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
            var nvBTBD = _context.Tbl_QuyenXuLy.Where(x => x.ID_XuongXL == TaiKhoan.ID_PhanXuong && x.MaXL == "BTBD").Select(x => x.ID_TaiKhoan).ToList();
            ViewBag.IDNhanVienBTBD = new SelectList(NhanVien.Where(x => x.ID_TaiKhoan == res.ID_NhanVien_BTBD ), "ID_TaiKhoan", "HoVaTen",res.ID_NhanVien_BTBD);
            return PartialView(res);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tbl_NhatKy_SanXuat _DO, IFormCollection formCollection, IFormFile FileDinhKem)
        {
            int IDTaiKhoanBTBD = 0;
            int IDKip = 0;
            string XacNhan = "";
            string Luu = "";
            int IDNKSX = 0;
            string ID_Day = "";
            string ID_ca = "";

            List<Tbl_NhatKy_SanXuat_ChiTiet> Tbl_NhatKy_SanXuat_ChiTiet = new List<Tbl_NhatKy_SanXuat_ChiTiet>();
            try
            {
                if (formCollection["IDNhanVienBTBD"].ToString() != null)
                {
                    IDTaiKhoanBTBD = Convert.ToInt32(formCollection["IDNhanVienBTBD"]);
                }
                IDKip = Convert.ToInt32(formCollection["IDKip"]);
                XacNhan = formCollection["xacnhan"];
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
                NhatKy.ID_NhanVien_BTBD = IDTaiKhoanBTBD;
                //NhatKy.ID_Kip = ID_Kip.ID_Kip;
                //NhatKy.Ca = ID_Kip.TenCa;
                //NhatKy.Kip = ID_Kip.TenKip;

                //kiem tra neu hieu chinh
                NhatKy.TinhTrang = 0; // Gửi dữ liệu
                //if (XacNhan =="0")
                //{
                //    NhatKy.ID_NhanVien_BTBD = null;
                //}
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
                return RedirectToAction("View_Details", "BM_NhatKy_SanXuat", new { IDNKSX = _DO.ID });


            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại, lỗi " + e + " ');</script>";
                return RedirectToAction("View_Details", "BM_NhatKy_SanXuat", new { IDNKSX = _DO.ID });
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


            // Người tạo và phòng ban ghi nhận
            ViewBag.taikhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.ID_TaiKhoan == res.ID_NhanVien_SX);
            ViewBag.taikhoanBTBD = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.ID_TaiKhoan == res.ID_NhanVien_BTBD);
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

        public async Task<IActionResult> View_BTBD(int IDNKSX)
        {

            DateTime ngayHienTai = DateTime.Today;

            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);

            var phongBan = await _context.Tbl_PhongBan.FindAsync(taiKhoan?.ID_PhongBan);
            ViewBag.TenPhongBan = phongBan?.TenNgan ?? "Không xác định";

            // Danh sách phòng ban nếu cần
            List<Tbl_PhongBan> pb = await _context.Tbl_PhongBan.ToListAsync();

            // Danh sách nhân viên (chỉ ID + Tên hiển thị)
            var nhanVienList = await _context.Tbl_TaiKhoan
                .Select(x => new
                {
                    x.ID_TaiKhoan,
                    HoVaTen = x.TenTaiKhoan + " - " + x.HoVaTen
                }).ToListAsync();
            ViewBag.IDTaiKhoan = new SelectList(nhanVienList, "ID_TaiKhoan", "HoVaTen");

            // Lấy nhật ký và chi tiết
            var res = _context.Tbl_NhatKy_SanXuat.FirstOrDefault(x => x.ID == IDNKSX);
            res.NhatKy_SanXuat_ChiTiet = _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x => x.ID_NhatKy == IDNKSX).ToList();

            // Ca/kíp trong ngày
            var caKip = await _context.Tbl_Kip
                .Where(x => x.NgayLamViec == ngayHienTai)
                .Select(x => new { x.ID_Kip, x.TenCa })
                .ToListAsync();
            ViewBag.TenKip = res?.Kip;
            ViewBag.ID_Day = res?.NgayDungSX.ToString("yyyy-MM-dd");

            // Người tạo và phòng ban ghi nhận
            ViewBag.taikhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.ID_TaiKhoan == res.ID_NhanVien_SX);
            ViewBag.taikhoanBTBD = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.ID_TaiKhoan == res.ID_NhanVien_BTBD);

            ViewBag.ID_PhongBan = await _context.Tbl_PhongBan
                .Where(x => x.ID_PhongBan == res.ID_PhongBan_SX)
                .Select(x => x.TenPhongBan)
                .FirstOrDefaultAsync();

            // Tính thời gian dừng theo từng lý do
            double SumMinutes(int lyDo) =>
                res.NhatKy_SanXuat_ChiTiet
                    .Where(x => x.LyDo_DungThietBi == lyDo)
                    .Sum(x => (double?)x.ThoiGianDung) ?? 0;

            TimeSpan tgdungThietbi = TimeSpan.FromMinutes(SumMinutes(1));
            TimeSpan tgdungCongNge = TimeSpan.FromMinutes(SumMinutes(2));
            TimeSpan DungSuCoCN = TimeSpan.FromMinutes(SumMinutes(3));
            TimeSpan DungKhachQuan = TimeSpan.FromMinutes(SumMinutes(4));
            TimeSpan TongTgianDung = TimeSpan.FromMinutes(res.NhatKy_SanXuat_ChiTiet.Sum(x => (double?)x.ThoiGianDung) ?? 0);

            // Gán ViewBag theo giờ (2 chữ số sau dấu phẩy)
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

                TimeSpan tgdungTB = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 1).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan tgdungCongNge = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 2).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan DungSuCoCN = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 3).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan DungKhachQuan = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 4).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan TongTgianDung = TimeSpan.FromMinutes(Data.Sum(x => x.ThoiGianDung) ?? 0);


                string DungTB = tgdungTB.TotalHours.ToString("F2");
                string DungCN = tgdungCongNge.TotalHours.ToString("F2");
                string DungChoCN = DungSuCoCN.TotalHours.ToString("F2");
                string DungKQuan = DungKhachQuan.TotalHours.ToString("F2");
                string TongTgian = TongTgianDung.TotalHours.ToString("F2");

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
                        string tinhtrang="";
                        if(item.TinhTrang == -1)
                        {
                            tinhtrang = "Không xác nhận";
                        }
                        else if (item.TinhTrang == 1)
                        {
                            tinhtrang = "Hoàn tất";
                        }
                        else if (item.TinhTrang == 0)
                        {
                            tinhtrang = "Đã gửi";
                        }
                        else if (item.TinhTrang == 2)
                        {
                            tinhtrang = "Đang xóa phiếu";
                        }
                        else if (item.TinhTrang == 3)
                        {
                            tinhtrang = "Đã xóa phiếu";
                        }
                        Worksheet.Cell(row, icol).Value = tinhtrang;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }
                    int rownext = row + 1;
                    Worksheet.Cell("I" + rownext).Value = "Tổng";
                    Worksheet.Cell("J" + rownext).Value = DungTB;
                    Worksheet.Cell("K" + rownext).Value = DungCN;
                    Worksheet.Cell("L" + rownext).Value = DungChoCN;
                    Worksheet.Cell("M" + rownext).Value = DungKQuan;
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

        public async Task<IActionResult> ExportToExcelPhieu( int? IDNKSX)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_NhatKyDungSanXuat.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_NhatKyDungSanXuat_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("BM");
                var res = from a in _context.Tbl_NhatKy_SanXuat_ChiTiet.Where(x => x.ID_NhatKy == IDNKSX)
                          join b in _context.Tbl_NhatKy_SanXuat on a.ID_NhatKy equals b.ID
                          join c in _context.Tbl_PhongBan on b.ID_PhongBan_SX equals c.ID_PhongBan
                          join d in _context.Tbl_Xuong on a.ID_Xuong equals d.ID_Xuong
                          select new Tbl_NhatKy_SanXuat_ChiTietExport
                          {
                              IDCT = a.IDCT,
                              ID_Xuong = a.ID_Xuong,
                              TenXuong = d.TenXuong,
                              Ca = b.Ca,
                              Kip = b.Kip,
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

                // Thực thi truy vấn tại đây
                var Data = await res.ToListAsync();

                TimeSpan tgdungTB = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 1).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan tgdungCongNge = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 2).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan DungSuCoCN = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 3).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan DungKhachQuan = TimeSpan.FromMinutes(Data.Where(x => x.LyDo_DungThietBi == 4).Sum(x => x.ThoiGianDung) ?? 0);
                TimeSpan TongTgianDung = TimeSpan.FromMinutes(Data.Sum(x => x.ThoiGianDung) ?? 0);


                string DungTB = tgdungTB.TotalHours.ToString("F2");
                string DungCN = tgdungCongNge.TotalHours.ToString("F2");
                string DungChoCN = DungSuCoCN.TotalHours.ToString("F2");
                string DungKQuan = DungKhachQuan.TotalHours.ToString("F2");
                string TongTgian = TongTgianDung.TotalHours.ToString("F2");


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

                        TimeSpan tgdungThietbi = TimeSpan.FromMinutes(item.ThoiGianDung ?? 0);
                        icol++;
                        Worksheet.Cell(row, icol).Value = tgdungThietbi.TotalHours.ToString("F2");
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.LyDo_DungThietBi == 1 ? "X" : "";
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
                        string tinhtrang = "";
                        if (item.TinhTrang == -1)
                        {
                            tinhtrang = "Không xác nhận";
                        }
                        else if (item.TinhTrang == 1)
                        {
                            tinhtrang = "Hoàn tất";
                        }
                        else if (item.TinhTrang == 0)
                        {
                            tinhtrang = "Đã gửi";
                        }
                        else if (item.TinhTrang == 2)
                        {
                            tinhtrang = "Đang xóa phiếu";
                        }
                        else if (item.TinhTrang == 3)
                        {
                            tinhtrang = "Đã xóa phiếu";
                        }
                        Worksheet.Cell(row, icol).Value = tinhtrang;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }
                    int rownext = row + 1;
                    Worksheet.Cell("I" + rownext).Value = "Tổng";
                    Worksheet.Cell("J" + rownext).Value = DungTB;
                    Worksheet.Cell("K" + rownext).Value = DungCN;
                    Worksheet.Cell("L" + rownext).Value = DungChoCN;
                    Worksheet.Cell("M" + rownext).Value = DungKQuan;
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

        [HttpPost]
        public JsonResult XacNhanPhieu(int id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var phieu = _context.Tbl_NhatKy_SanXuat.Find(id);
            if (phieu == null)
                return Json(new { success = false, message = "Phiếu không tồn tại" });

            phieu.TinhTrang = 1; // Hoặc trạng thái 'Đã xác nhận'
            phieu.ID_NhanVien_BTBD = TaiKhoan.ID_TaiKhoan;
            _context.SaveChanges();

            return Json(new { success = true, message = "Xác nhận thành công!" });
        }

        [HttpPost]
        public JsonResult TuChoiPhieu(int id, string ghichu)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var phieu = _context.Tbl_NhatKy_SanXuat.Find(id);
            if (phieu == null)
                return Json(new { success = false, message = "Phiếu không tồn tại" });

            phieu.TinhTrang = -1; // Hoặc trạng thái 'Không xác nhận'
            phieu.ID_NhanVien_BTBD = TaiKhoan.ID_TaiKhoan;
            phieu.GhiChu =ghichu;
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã từ chối xác nhận!" });
        }

        [HttpPost]
        public JsonResult XacNhanXoaPhieu(int id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var phieu = _context.Tbl_NhatKy_SanXuat.Find(id);
            if (phieu == null)
                return Json(new { success = false, message = "Phiếu không tồn tại" });

            phieu.TinhTrang = 3; // Hoặc trạng thái 'Đã xóa'
            phieu.ID_NhanVien_BTBD = TaiKhoan.ID_TaiKhoan;
            _context.SaveChanges();

            return Json(new { success = true, message = "Xác nhận thành công!" });
        }

        [HttpPost]
        public JsonResult TuChoiXoaPhieu(int id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var phieu = _context.Tbl_NhatKy_SanXuat.Find(id);
            if (phieu == null)
                return Json(new { success = false, message = "Phiếu không tồn tại" });

            phieu.TinhTrang = 1; // quay về hoàn thành
            phieu.ID_NhanVien_BTBD = TaiKhoan.ID_TaiKhoan;
            _context.SaveChanges();

            return Json(new { success = true, message = "Đã từ chối xác nhận!" });
        }

        [HttpPost]
        public IActionResult DeleteRequest(int id)
        {
            // Xử lý logic gửi yêu cầu xóa:
            // Ví dụ: Ghi nhận yêu cầu vào bảng log hoặc chuyển trạng thái phiếu

            var phieu = _context.Tbl_NhatKy_SanXuat.FirstOrDefault(x => x.ID == id);
            if (phieu == null)
            {
                return NotFound();
            }

            phieu.TinhTrang = 2; // Ví dụ: cập nhật trạng thái chờ xóa phiếu
            _context.SaveChanges();

            return Ok();
        }
        public async Task<IActionResult> ExportPdfView(int id)
        {
            return View("ExportPdfView", new {id = id });
        }

        public async Task<IActionResult> GenerateAndSavePdf(int? id)
        {
            var bbgn = _context.Tbl_NhatKy_SanXuat.Find(id);
            if (bbgn == null)
            {
                return null;
            }
            // 1. Render Razor View thành chuỗi HTML
            string htmlContent = await RenderViewToStringAsync("ExportPdfView", new { id = id });

            // 2. Chuyển đổi HTML sang PDF
            byte[] pdfBytes = ConvertHtmlToPdf(htmlContent, bbgn.SoPhieu + "_"+bbgn.ID+"_NKDSX");
            string filename = $"{bbgn.SoPhieu + DateTime.Now.ToString("yyyyMMddHHmm")}.pdf";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");

            // Lưu file vào server và trả về path
            string pathsave = SavePdfToFile(pdfBytes, folderPath, filename);
            if (pathsave != "" && pathsave != null)
            {
                bbgn.FileBB = pathsave;
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

        private byte[] ConvertHtmlToPdf(string htmlContent,string soPhieu)
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
                pdfDocument.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterHandler(_context, soPhieu));

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
            private readonly string _soPhieu;
            public FooterHandler(DataContext context,string soPhieu)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
                _soPhieu = soPhieu ?? throw new ArgumentNullException(nameof(soPhieu));
            }

           
            public void HandleEvent(Event @event)
            {

                var httpContext = new HttpContextAccessor().HttpContext;
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                var pageSize = page.GetPageSize();
                //int IDBBGN = int.Parse(httpContext.Request.RouteValues["id"].ToString());
                //var bbgn = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == IDBBGN).FirstOrDefault();
                // lấy IDBBGN và mã hóa
                //var bbgnID = bbgn.SoPhieu + "_" + "BIENBANGIAONHAN_BM11.QT05_" + httpContext.Request.RouteValues["id"]?.ToString() + "_" + DateTime.Now.ToString("dd/MM/yyyy");
                // Tạo mã QR
                BarcodeQRCode barcodeQRCode = new BarcodeQRCode(_soPhieu);
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


    }
}
