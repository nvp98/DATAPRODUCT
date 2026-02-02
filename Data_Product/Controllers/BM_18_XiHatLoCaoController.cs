using System.Security.Claims;
using ClosedXML.Excel;
using Data_Product.Common.Enums;
using Data_Product.DTO.BM_18_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using Data_Product.Services;
using iText.Barcodes;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout.Font;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class BM_18_XiHatLoCaoController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ILogger<BM_18_XiHatLoCaoController> _logger;
        public BM_18_XiHatLoCaoController(DataContext _context, ICompositeViewEngine viewEngine, ILogger<BM_18_XiHatLoCaoController> logger)
        {
            this._context = _context;
            _viewEngine = viewEngine;
            _logger = logger;
        }
        public async Task<IActionResult> GetKipFromCa(DateTime? Ngay, string Ca)
        {
            DateTime day_datetime = (DateTime)Ngay;
            string Day = day_datetime.ToString("dd-MM-yyyy");
            DateTime Day_Convert = DateTime.ParseExact(Day, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
            var CaKip = await (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == Day_Convert && x.TenCa == Ca)
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenKip = a.TenKip,
                               }).ToListAsync();

            return Json(CaKip);
        }
        public async Task<SelectList> GetLoCaoList()
        {
            var loCaos = await _context.Tbl_LoCao.OrderBy(l => l.TenLoCao).ToListAsync();

            return new SelectList(loCaos, "ID", "TenLoCao");
        }
        public async Task<SelectList> GetLoCaoWithAuth()
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = await _context.Tbl_TaiKhoan
                         .FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);

            if (TaiKhoan == null)
            {
                return new SelectList(Enumerable.Empty<object>());
            }

            var quyenLo = await (from map in _context.Tbl_BM_18_PhanQuyenXiHat_TaiKhoan
                                 join lo in _context.Tbl_BM_18_PhanQuyenXiHat on map.ID_LoSanXuat equals lo.ID
                                 where map.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan && lo.IsActived == true
                                 select new
                                 {
                                     ID_BoPhan = lo.ID_BoPhan,
                                     MaLo = lo.MaLo
                                 }).ToListAsync();

            // Group theo ID_BoPhan
            var quyenGroup = quyenLo
                .GroupBy(x => x.ID_BoPhan)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.MaLo).Distinct().ToList()
                );


            // Danh sách lò cao cuối cùng
            List<Tbl_LoCao> loCaos = new();

            foreach (var kvp in quyenGroup)
            {
                int idBoPhan = kvp.Key;
                var dsMaLo = kvp.Value;

                var loCaoTrongBoPhan = await _context.Tbl_LoCao
                    .Where(x => x.ID_PhongBan == idBoPhan && dsMaLo.Contains(x.ID))
                    .ToListAsync();

                loCaos.AddRange(loCaoTrongBoPhan);
            }
            return new SelectList(loCaos, "ID", "TenLoCao");
        }
        public async Task<IActionResult> Danhsachphieu(string maPhieu, DateTime? ngay, DateTime? tuNgaySX,
              DateTime? denNgaySX, string ca, string locao, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;
            var loCaos = await _context.Tbl_LoCao.OrderBy(l => l.TenLoCao).ToListAsync();
            var loCaoList = await GetLoCaoWithAuth();
            ViewBag.LoCaoList = loCaoList;
            var data = new List<DanhSachPhieuDto>();
            var pager = new Pager();
            if (loCaoList.Any())
            {
                //var query = _context.Tbl_BM_16_Phieu.OrderByDescending(p => p.NgayPhieuGang)
                var query = _context.Tbl_BM_18_PhieuXiHat
                    .OrderByDescending(p => p.NgaySanXuat.Date) // Ngày mới trước
            .Select(p => new DanhSachPhieuDto
                {
                    MaPhieu = p.MaPhieu,
                    NgayTaoPhieu = p.NgayTaoPhieu,
                    NgaySanXuat = p.NgaySanXuat,
                    ID_LoCao = p.ID_Locao,
                    TenNguoiTao = _context.Tbl_TaiKhoan
                                    .Where(tk => tk.ID_TaiKhoan == p.ID_NguoiTao)
                                    .Select(tk => tk.TenTaiKhoan + " " + "-" + " " + tk.HoVaTen).FirstOrDefault(),
                    TenCa = _context.Tbl_Kip
                                .Where(k => k.ID_Kip == p.ID_Kip)
                                .Select(k => k.TenKip)
                                .FirstOrDefault(),
                    TenLoCao = _context.Tbl_LoCao
                                .Where(lc => lc.ID == p.ID_Locao)
                                .Select(lc => lc.TenLoCao)
                                .FirstOrDefault(),
                    TrangThai = p.TrangThai,
                });

                // Lọc theo Mã Phiếu (chuỗi)
                if (!string.IsNullOrEmpty(maPhieu))
                {
                    string maPhieuLower = maPhieu.ToLower();
                    query = query.Where(s => s.MaPhieu.ToLower().Contains(maPhieuLower));
                }
                // Lọc theo ngày phiếu gang
                if (tuNgaySX.HasValue)
                {
                    query = query.Where(s => s.NgaySanXuat >= tuNgaySX.Value.Date);
                }

                if (denNgaySX.HasValue)
                {
                    query = query.Where(s => s.NgaySanXuat < denNgaySX.Value.Date.AddDays(1));
                }


                // Lọc theo Ca (chuỗi)
                if (!string.IsNullOrEmpty(ca))
                {
                    string caLower = ca.ToLower();
                    query = query.Where(s => s.TenCa.ToLower() == caLower);
                }

                if (!string.IsNullOrEmpty(locao))
                {
                    if (int.TryParse(locao, out int locaoId))
                    {
                        query = query.Where(s => _context.Tbl_LoCao
                                                  .Where(lc => lc.ID == locaoId)
                                                  .Select(lc => lc.TenLoCao)
                                                  .FirstOrDefault() == s.TenLoCao);
                    }
                }
                else
                {
                    if (loCaoList.Any())
                    {
                        var idList = loCaoList.Items.Cast<Tbl_LoCao>().Select(x => x.ID).ToList();
                        query = query.Where(s => idList.Contains(s.ID_LoCao));
                    }
                }

                int resCount = await query.CountAsync();

                data = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                pager = new Pager(resCount, page, pageSize);
            }

            ViewBag.Pager = pager;

            // Truyền lại giá trị tìm kiếm cho view
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Ngay = ngay?.ToString("dd-MM-yyyy") ?? "";
            ViewBag.TuNgaySX = tuNgaySX?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.DenNgaySX = denNgaySX?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Ca = ca;
            ViewBag.TenLoCao = locao;

            return View(data);
        }
        public async Task<IActionResult> Index_All(string maPhieu, DateTime? ngay, DateTime? ngaysx, string ca, string locao, int page = 1)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BN = TaiKhoan.ID_TaiKhoan;

            const int pageSize = 10;
            if (page < 1) page = 1;
            var loCaos = await _context.Tbl_LoCao.OrderBy(l => l.TenLoCao).ToListAsync();
            var loCaoList = await GetLoCaoWithAuth();
            ViewBag.LoCaoList = loCaoList;
            var data = new List<DanhSachPhieuDto>();
            var pager = new Pager();
            if (loCaoList.Any())
            {
                //var query = _context.Tbl_BM_16_Phieu.OrderByDescending(p => p.NgayPhieuGang)
                var query = _context.Tbl_BM_18_PhieuXiHat
                    .OrderByDescending(p => p.NgaySanXuat.Date) // Ngày mới trước
            .Select(p => new DanhSachPhieuDto
            {
                MaPhieu = p.MaPhieu,
                NgayTaoPhieu = p.NgayTaoPhieu,
                NgaySanXuat = p.NgaySanXuat,
                ID_LoCao = p.ID_Locao,
                TenNguoiTao = _context.Tbl_TaiKhoan
                                    .Where(tk => tk.ID_TaiKhoan == p.ID_NguoiTao)
                                    .Select(tk => tk.TenTaiKhoan + " " + "-" + " " + tk.HoVaTen).FirstOrDefault(),
                TenCa = _context.Tbl_Kip
                                .Where(k => k.ID_Kip == p.ID_Kip)
                                .Select(k => k.TenKip)
                                .FirstOrDefault(),
                TenLoCao = _context.Tbl_LoCao
                                .Where(lc => lc.ID == p.ID_Locao)
                                .Select(lc => lc.TenLoCao)
                                .FirstOrDefault(),
                TrangThai = p.TrangThai,
                ID_NguoiNhan = p.ID_NguoiNhan,
             });
                query = query.Where(p => p.ID_NguoiNhan == ID_NhanVien_BN);
                // Lọc theo Mã Phiếu (chuỗi)
                if (!string.IsNullOrEmpty(maPhieu))
                {
                    string maPhieuLower = maPhieu.ToLower();
                    query = query.Where(s => s.MaPhieu.ToLower().Contains(maPhieuLower));
                }
                // Lọc theo ngày phiếu gang
                if (ngaysx.HasValue)
                {
                    query = query.Where(s => s.NgaySanXuat.Date == ngaysx.Value.Date);
                }
                // Lọc theo Ca (chuỗi)
                if (!string.IsNullOrEmpty(ca))
                {
                    string caLower = ca.ToLower();
                    query = query.Where(s => s.TenCa.ToLower() == caLower);
                }

                if (!string.IsNullOrEmpty(locao))
                {
                    if (int.TryParse(locao, out int locaoId))
                    {
                        query = query.Where(s => _context.Tbl_LoCao
                                                  .Where(lc => lc.ID == locaoId)
                                                  .Select(lc => lc.TenLoCao)
                                                  .FirstOrDefault() == s.TenLoCao);
                    }


                }
                else
                {
                    if (loCaoList.Any())
                    {
                        var idList = loCaoList.Items.Cast<Tbl_LoCao>().Select(x => x.ID).ToList();
                        query = query.Where(s => idList.Contains(s.ID_LoCao));
                    }
                }

                int resCount = await query.CountAsync();

                data = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                pager = new Pager(resCount, page, pageSize);
            }

            ViewBag.Pager = pager;

            // Truyền lại giá trị tìm kiếm cho view
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Ngay = ngay?.ToString("dd-MM-yyyy") ?? "";
            ViewBag.NgaySanXuat = ngaysx?.ToString("dd-MM-yyyy") ?? "";
            ViewBag.Ca = ca;
            ViewBag.TenLoCao = locao;

            return View(data);
        }
        public async Task<IActionResult> Index_All_PKH(string maPhieu, DateTime? ngay, DateTime? tuNgaySX, DateTime? denNgaySX, string ca, string locao, int? trangThai, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;
            var loCaos = await _context.Tbl_LoCao.OrderBy(l => l.TenLoCao).ToListAsync();
            var loCaoList = await GetLoCaoList();
            ViewBag.LoCaoList = loCaoList;
            var data = new List<DanhSachPhieuDto>();
            var pager = new Pager();
            if (loCaoList.Any())
            {
                //var query = _context.Tbl_BM_16_Phieu.OrderByDescending(p => p.NgayPhieuGang)
                var query = _context.Tbl_BM_18_PhieuXiHat
                    .OrderByDescending(p => p.NgaySanXuat.Date) // Ngày mới trước
            .Select(p => new DanhSachPhieuDto
                {
                    MaPhieu = p.MaPhieu,
                    NgayTaoPhieu = p.NgayTaoPhieu,
                    NgaySanXuat = p.NgaySanXuat,
                    ID_LoCao = p.ID_Locao,
                    TenNguoiTao = _context.Tbl_TaiKhoan
                                    .Where(tk => tk.ID_TaiKhoan == p.ID_NguoiTao)
                                    .Select(tk => tk.TenTaiKhoan + " " + "-" + " " + tk.HoVaTen).FirstOrDefault(),
                    TenCa = _context.Tbl_Kip
                                .Where(k => k.ID_Kip == p.ID_Kip)
                                .Select(k => k.TenKip)
                                .FirstOrDefault(),
                    TenLoCao = _context.Tbl_LoCao
                                .Where(lc => lc.ID == p.ID_Locao)
                                .Select(lc => lc.TenLoCao)
                                .FirstOrDefault(),
                    TrangThai = p.TrangThai,
                });

                // Lọc theo Mã Phiếu (chuỗi)
                if (!string.IsNullOrEmpty(maPhieu))
                {
                    string maPhieuLower = maPhieu.ToLower();
                    query = query.Where(s => s.MaPhieu.ToLower().Contains(maPhieuLower));
                }
                // Lọc theo ngày phiếu gang
                //if (ngaysx.HasValue)
                //{
                //    query = query.Where(s => s.NgaySanXuat.Date == ngaysx.Value.Date);
                //}
                if (tuNgaySX.HasValue && denNgaySX.HasValue)
                {
                    query = query.Where(s =>
                        s.NgaySanXuat.Date >= tuNgaySX.Value.Date &&
                        s.NgaySanXuat.Date <= denNgaySX.Value.Date
                    );
                }
                else if (tuNgaySX.HasValue)
                {
                    query = query.Where(s => s.NgaySanXuat.Date >= tuNgaySX.Value.Date);
                }
                else if (denNgaySX.HasValue)
                {
                    query = query.Where(s => s.NgaySanXuat.Date <= denNgaySX.Value.Date);
                }

                // Lọc theo Ca (chuỗi)
                if (!string.IsNullOrEmpty(ca))
                {
                    string caLower = ca.ToLower();
                    query = query.Where(s => s.TenCa.ToLower() == caLower);
                }

                if (!string.IsNullOrEmpty(locao))
                {
                    if (int.TryParse(locao, out int locaoId))
                    {
                        query = query.Where(s => _context.Tbl_LoCao
                                                  .Where(lc => lc.ID == locaoId)
                                                  .Select(lc => lc.TenLoCao)
                                                  .FirstOrDefault() == s.TenLoCao);
                    }
                }
                else
                {
                    if (loCaoList.Any())
                    {
                        var idList = loCaoList.Items.Cast<Tbl_LoCao>().Select(x => x.ID).ToList();
                        query = query.Where(s => idList.Contains(s.ID_LoCao));
                    }
                }
                if (trangThai.HasValue)
                {
                    query = query.Where(x => x.TrangThai == trangThai.Value);
                }
                int resCount = await query.CountAsync();

                data = await query
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync();
                pager = new Pager(resCount, page, pageSize);
            }

            ViewBag.Pager = pager;
            ViewBag.TrangThai = trangThai;

            // Truyền lại giá trị tìm kiếm cho view
            ViewBag.MaPhieu = maPhieu;
            ViewBag.Ngay = ngay?.ToString("dd-MM-yyyy") ?? "";
            ViewBag.TuNgaySX = tuNgaySX?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.DenNgaySX = denNgaySX?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Ca = ca;
            ViewBag.TenLoCao = locao;

            return View(data);
        }
        [HttpGet]
        public async Task<IActionResult> TaoPhieu()
        {

            ViewBag.LoCaoList = await GetLoCaoList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> TaoPhieu([FromBody] TaoPhieuDto model)
        {
            if (model == null)
                return BadRequest(new { success = false, message = "Dữ liệu rỗng." });

            if (model.NgaySanXuat == default)
                return BadRequest(new { success = false, message = "Ngày phiếu không hợp lệ." });

            if (model.ID_Kip <= 0)
                return BadRequest(new { success = false, message = "Kíp không hợp lệ." });

            if (model.ID_LoCaos == null || model.ID_LoCaos.Count == 0)
                return BadRequest(new { success = false, message = "Vui lòng chọn ít nhất 1 lò cao." });

            if (model.LoaiCan < 1 || model.LoaiCan > 3)
                return BadRequest(new { success = false, message = "Loại cân không hợp lệ (1-3)." });

            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(tenTaiKhoan))
                return Unauthorized(new { success = false, message = "Phiên đăng nhập không hợp lệ." });

            var taiKhoan = await _context.Tbl_TaiKhoan.FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
            if (taiKhoan == null)
                return Unauthorized(new { success = false, message = "Tài khoản không tồn tại." });

            // Lấy thông tin Ca/Kíp (để sinh mã phiếu)
            var kipInfo = await _context.Tbl_Kip
                .Where(x => x.ID_Kip == model.ID_Kip)
                .Select(x => new { x.TenCa, x.TenKip })
                .FirstOrDefaultAsync();

            var results = new List<object>();
            int createdCount = 0;
            int duplicateCount = 0;
            int errorCount = 0;

            foreach (var loId in model.ID_LoCaos.Distinct())
            {
                try
                {
                    bool existed = await _context.Tbl_BM_18_PhieuXiHat.AnyAsync(p =>
                        p.ID_Locao == loId &&
                        p.ID_Kip == model.ID_Kip &&
                        p.NgaySanXuat.Date == model.NgaySanXuat.Date
                    );

                    if (existed)
                    {
                        duplicateCount++;
                        results.Add(new
                        {
                            ID_LoCao = loId,
                            status = "duplicate",
                            message = "Đã tồn tại phiếu cho lò/kíp/ngày này."
                        });
                        continue;
                    }

                    string maPhieu = GenerateMaPhieuBM18(loId, kipInfo?.TenCa, kipInfo?.TenKip, model.NgaySanXuat);

                    // Lấy khối lượng dựa trên loại cân đã chọn
                    var giaTriResult = await LayBaGiaTriGang(model.Ca, model.ID_Kip, model.NgaySanXuat, loId, model.LoaiCan);
                    decimal giaTriGang = 0m;
                    
                    if (giaTriResult is OkObjectResult okResult && okResult.Value != null)
                    {
                        var resultValue = okResult.Value;
                        var successProp = resultValue.GetType().GetProperty("success");
                        var giaTriProp = resultValue.GetType().GetProperty("giaTriGang");
                        
                        if (successProp?.GetValue(resultValue) is bool success && success && giaTriProp != null)
                        {
                            giaTriGang = Convert.ToDecimal(giaTriProp.GetValue(resultValue));
                        }
                    }

                    var header = new Tbl_BM_18_PhieuXiHat
                    {
                        MaPhieu = maPhieu,
                        NgayTaoPhieu = DateTime.Now,
                        ID_Locao = loId,
                        ID_Kip = model.ID_Kip,
                        ID_NguoiTao = taiKhoan.ID_TaiKhoan,
                        NgaySanXuat = model.NgaySanXuat,
                        ID_NguoiGiao = 0,
                        ID_NguoiNhan = 0,
                        ID_TrangThaiBG = (int)TrangThaiXuLy.ChuaXuLy,
                        ID_TrangThaiBN = (int)TrangThaiXuLy.ChuaXuLy,
                        TrangThai = (int)TrangThaiXuLy.ChuaXuLy,
                        LoaiCan = model.LoaiCan // Lưu loại cân đã chọn
                    };

                    _context.Tbl_BM_18_PhieuXiHat.Add(header);
                    await _context.SaveChangesAsync();

                    // Thêm dòng chi tiết mặc định vào Tbl_BM_18_XiHatLoCao
                    var xiHat = new Tbl_BM_18_XiHatLoCao
                    {
                        Ca = null,
                        Kip = header.ID_Kip,
                        NgaySanXuat = header.NgaySanXuat,
                        MaPhieu = header.MaPhieu,
                        ID_LoCao = header.ID_Locao,
                        Ten_NVL = "Xỉ hạt lò cao",
                        DVT = "Tấn",
                        ID_Lo = null,
                        HeSo = null,
                        KL_Gang_Giao = giaTriGang // Gán KL Gang theo loại cân đã chọn
                    };
                    _context.Tbl_BM_18_XiHatLoCao.Add(xiHat);
                    await _context.SaveChangesAsync();

                    createdCount++;
                    results.Add(new
                    {
                        ID_LoCao = loId,
                        status = "created",
                        id = header.ID,
                        maPhieu = header.MaPhieu,
                        redirectUrl = Url.Action("DetailPhieu", "BM_18_XiHatLoCao", new { id = header.ID })
                    });
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex, "Lỗi tạo phiếu BM18 cho lò {LoId}", loId);
                    results.Add(new
                    {
                        ID_LoCao = loId,
                        status = "error",
                        message = ex.Message
                    });
                }
            }

            return Json(new
            {
                success = true,
                createdCount,
                duplicateCount,
                errorCount,
                results
            });
        }

        private string GenerateMaPhieuBM18(int idLoCao, string? tenCa, string? tenKip, DateTime ngay)
        {
            string ca = tenCa ?? "";
            string kip = tenKip ?? "";
            return $"XHLC-L{idLoCao}-{ca}{kip}-{ngay:ddMMyy}";
        }

        [HttpGet("DetailPhieu")]
        public async Task<IActionResult> DetailPhieu(string maPhieu)
        {
            if (string.IsNullOrWhiteSpace(maPhieu))
                return BadRequest("Thiếu mã phiếu.");

            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
            var PhongBan = _context.Tbl_PhongBan.FirstOrDefault(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan);
            string TenBP = PhongBan.TenNgan;

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);

            if (phieu == null)
                return NotFound($"Không tìm thấy phiếu với mã: {maPhieu}");

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", phieu.ID_NguoiNhan);
            ViewBag.HasNguoiNhan = phieu.ID_NguoiNhan.HasValue;
            ViewBag.ID_NguoiNhan = phieu.ID_NguoiNhan;

            var kip = await _context.Tbl_Kip.FirstOrDefaultAsync(k => k.ID_Kip == phieu.ID_Kip);

            var chiTiet = await _context.Tbl_BM_18_XiHatLoCao
                .AsNoTracking()
                .Where(x => x.MaPhieu == maPhieu)
                .OrderBy(x => x.ID)
                .ToListAsync();

            var MaLo = await _context.Tbl_MaLo
                .Select(a => new Tbl_MaLo
                {
                    ID_MaLo = a.ID_MaLo,
                    TenMaLo = a.TenMaLo,
                    ID_TinhTrang = 1
                }).ToListAsync();

            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");
            ViewBag.MaPhieu = phieu.MaPhieu;
            ViewBag.NgaySanXuat = phieu.NgaySanXuat.ToString("yyyy-MM-dd");
            ViewBag.TenKip = phieu.ID_Kip;
            ViewBag.ID_Locao = phieu.ID_Locao;
            ViewBag.ID_Kip = phieu.ID_Kip;
            ViewBag.TenKip = kip?.TenKip;
            ViewBag.TenCa = kip?.TenCa;
            ViewBag.Phieu = phieu;
            ViewBag.PhongBan = PhongBan?.TenNgan;

            // ===== Build ViewModel chỉ để đưa vào view =====

            var vm = new Data_Product.DTO.BM_18_DTO.BM18DetailViewModel
            {
                Phieu = phieu,
                ChiTiet = chiTiet
            };

            // ViewBag giữ nguyên, chỉ đổi return
            return View("DetailPhieu", vm);
        }

        [HttpPost]
        public async Task<IActionResult> SaveChiTiet([FromBody] BM18Request req)
        {
            if (req == null || req.ChiTiet == null || req.ChiTiet.Count == 0)
                return BadRequest("Dữ liệu không hợp lệ!");

            // Kiểm tra hợp lệ người nhận
            if (req.ID_NguoiNhan == null || req.ID_NguoiNhan <= 0)
                return BadRequest("Vui lòng chọn người nhận!");

            foreach (var item in req.ChiTiet)
            {
                // Kiểm tra hợp lệ Lô
                if (item.ID_Lo == null || item.ID_Lo <= 0)
                    return BadRequest("Vui lòng chọn Lô!");
                // Kiểm tra hợp lệ hệ số quy đổi
                if (item.HeSo == null || item.HeSo <= 0)
                    return BadRequest("Vui lòng nhập hệ số quy đổi!");

                if (item.ID == 0)
                {
                    // THÊM MỚI
                    var newItem = new Tbl_BM_18_XiHatLoCao
                    {
                        MaPhieu = item.MaPhieu,
                        ID_LoCao = item.ID_LoCao,
                        Ca = item.ID_Ca,
                        Kip = item.ID_Kip,
                        Ten_NVL = item.Ten_NVL,
                        DVT = item.DVT,
                        ID_Lo = item.ID_Lo,
                        HeSo = item.HeSo,
                        NgaySanXuat = item.NgaySanXuat,
                        KL_Gang_Giao = item.KL_Gang_Giao,
                        KL_Xi_Giao = item.KL_Xi_Giao,
                        KL_Gang_Nhan = item.KL_Gang_Nhan,
                        KL_Xi_Nhan = item.KL_Xi_Nhan,
                        GhiChu = item.GhiChu
                    };
                    _context.Tbl_BM_18_XiHatLoCao.Add(newItem);
                }
                else
                {
                    // UPDATE
                    var updateItem = await _context.Tbl_BM_18_XiHatLoCao
                        .FirstOrDefaultAsync(x => x.ID == item.ID);

                    if (updateItem != null)
                    {
                        updateItem.Ca = item.ID_Ca;
                        updateItem.Kip = item.ID_Kip;
                        updateItem.Ten_NVL = item.Ten_NVL;
                        updateItem.DVT = item.DVT;
                        updateItem.ID_Lo = item.ID_Lo;
                        updateItem.HeSo = item.HeSo;
                        updateItem.KL_Gang_Giao = item.KL_Gang_Giao;
                        updateItem.KL_Xi_Giao = item.KL_Xi_Giao;
                        updateItem.KL_Gang_Nhan = item.KL_Gang_Nhan;
                        updateItem.KL_Xi_Nhan = item.KL_Xi_Nhan;
                        updateItem.GhiChu = item.GhiChu;
                    }
                }
            }

            // Cập nhật ID_NguoiGiao và ID_NguoiNhan vào phiếu
            if (!string.IsNullOrEmpty(req.MaPhieu) && req.ID_NguoiGiao > 0 && req.ID_NguoiNhan > 0)
            {
                var phieu = await _context.Tbl_BM_18_PhieuXiHat.FirstOrDefaultAsync(x => x.MaPhieu == req.MaPhieu);
                if (phieu != null)
                {
                    
                    phieu.ID_NguoiGiao = req.ID_NguoiGiao;
                    phieu.ID_NguoiNhan = req.ID_NguoiNhan;
                    phieu.ID_TrangThaiBG = (int)TrangThaiXuLy.HoanThanh;
                    phieu.ID_TrangThaiBN = (int)TrangThaiXuLy.DangXuLy;
                    phieu.TrangThai = (int)TrangThaiXuLy.DangXuLy; ;
                    await _context.SaveChangesAsync();
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Đã lưu thành công!" });
        }
        public async Task<IActionResult> Index_Detail(string maPhieu)
        {
            // 1. Lấy thông tin phiếu
            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);

            if (phieu == null)
                return NotFound("Không tìm thấy phiếu BM18 với mã này.");

            // 2. Lấy chi tiết phiếu với join MaLo
            var chiTiet = await (
                from ct in _context.Tbl_BM_18_XiHatLoCao
                    .Where(x => x.MaPhieu == phieu.MaPhieu)
                join ml in _context.Tbl_MaLo
                    on ct.ID_Lo equals ml.ID_MaLo into g
                from ml in g.DefaultIfEmpty()
                    // Join thêm bảng Kip
                join kp in _context.Tbl_Kip
                    on ct.Kip equals kp.ID_Kip into g2
                from kp in g2.DefaultIfEmpty()
                select new Tbl_BM_18_XiHatLoCao
                {
                    ID = ct.ID,
                    MaPhieu = ct.MaPhieu,
                    ID_LoCao = ct.ID_LoCao,
                    Ca = ct.Ca,
                    Kip = ct.Kip,
                    Ten_NVL = ct.Ten_NVL,
                    DVT = ct.DVT,
                    ID_Lo = ct.ID_Lo,
                    HeSo = ct.HeSo,
                    NgaySanXuat = ct.NgaySanXuat,
                    KL_Gang_Giao = ct.KL_Gang_Giao,
                    KL_Xi_Giao = ct.KL_Xi_Giao,
                    KL_Gang_Nhan = ct.KL_Gang_Nhan,
                    KL_Xi_Nhan = ct.KL_Xi_Nhan,
                    GhiChu = ct.GhiChu,
                    TenMaLo = ml != null ? ml.TenMaLo : "",
                    CaKip = kp != null ? $"{kp.TenCa}{kp.TenKip}" : ""
                }
            ).ToListAsync();

            // 3. Lấy thông tin bên giao
            // 3. Lấy thông tin bên giao (GỘP QUERY – AN TOÀN)
            Tbl_TaiKhoan thongTinBG = null;
            Tbl_PhongBan phongBanBG = null;
            Tbl_Xuong phanXuongBG = null;
            Tbl_ViTri viTriBG = null;

            if (phieu.ID_NguoiGiao.HasValue && phieu.ID_NguoiGiao.Value > 0)
            {
                var benGiao = await (
                    from tk in _context.Tbl_TaiKhoan.AsNoTracking()

                    join pbBG in _context.Tbl_PhongBan
                        on tk.ID_PhongBan equals pbBG.ID_PhongBan into pbj
                    from pbBG in pbj.DefaultIfEmpty()

                    join pxBG in _context.Tbl_Xuong
                        on tk.ID_PhanXuong equals pxBG.ID_Xuong into pxj
                    from pxBG in pxj.DefaultIfEmpty()

                    join vtBG in _context.Tbl_ViTri
                        on tk.ID_ChucVu equals vtBG.ID_ViTri into vtj
                    from vtBG in vtj.DefaultIfEmpty()

                    where tk.ID_TaiKhoan == phieu.ID_NguoiGiao.Value
                    select new
                    {
                        TaiKhoan = tk,
                        PhongBan = pbBG,
                        PhanXuong = pxBG,
                        ViTri = vtBG
                    }
                ).FirstOrDefaultAsync();

                if (benGiao != null)
                {
                    thongTinBG = benGiao.TaiKhoan;
                    phongBanBG = benGiao.PhongBan;
                    phanXuongBG = benGiao.PhanXuong;
                    viTriBG = benGiao.ViTri;
                }
            }
            Tbl_TaiKhoan thongTinBN = null;
            Tbl_PhongBan phongBanBN = null;
            Tbl_Xuong phanXuongBN = null;
            Tbl_ViTri viTriBN = null;
            if (phieu.ID_NguoiNhan.HasValue && phieu.ID_NguoiNhan.Value > 0)
            {
                thongTinBN = await _context.Tbl_TaiKhoan
                    .FirstOrDefaultAsync(x => x.ID_TaiKhoan == phieu.ID_NguoiNhan.Value);
                if (thongTinBN != null)
                {
                    phongBanBN = await _context.Tbl_PhongBan
                        .FirstOrDefaultAsync(x => x.ID_PhongBan == thongTinBN.ID_PhongBan);
                    phanXuongBN = await _context.Tbl_Xuong
                        .FirstOrDefaultAsync(x => x.ID_Xuong == thongTinBN.ID_PhanXuong);
                    viTriBN = await _context.Tbl_ViTri
                        .FirstOrDefaultAsync(x => x.ID_ViTri == thongTinBN.ID_ChucVu);
                }
            }
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
            var PhongBan = _context.Tbl_PhongBan.FirstOrDefault(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan);
            string TenBP = PhongBan.TenNgan;

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");
            ViewBag.DsMaLo = await _context.Tbl_MaLo
                .AsNoTracking()
                .OrderBy(x => x.TenMaLo)
                .ToListAsync();

            // 5. Tạo ViewModel
            var viewModel = new BM18DetailViewModel
            {
                Phieu = phieu,
                ChiTiet = chiTiet,
                ThongTinBenGiao = thongTinBG,
                PhongBanBenGiao = phongBanBG,
                PhanXuongBenGiao = phanXuongBG,
                ViTriBenGiao = viTriBG,
                ThongTinBenNhan = thongTinBN,
                PhongBanBenNhan = phongBanBN,
                PhanXuongBenNhan = phanXuongBN,
                ViTriBenNhan = viTriBN,
            };
            ViewBag.PhongBan = PhongBan?.TenNgan;
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> XacNhanPhieuBN([FromBody] XacNhanPhieuBNRequest req)
        {
            if (string.IsNullOrEmpty(req.MaPhieu))
                return BadRequest(new { success = false, message = "Mã phiếu không hợp lệ!" });

            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var taiKhoan = await _context.Tbl_TaiKhoan
                .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);

            if (taiKhoan == null)
                return Unauthorized(new { success = false, message = "Không xác định được tài khoản!" });

            int idNhanVienBN = taiKhoan.ID_TaiKhoan;

            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                .FirstOrDefaultAsync(x => x.MaPhieu == req.MaPhieu);

            if (phieu == null)
                return NotFound(new { success = false, message = "Không tìm thấy phiếu!" });

            if (phieu.ID_NguoiNhan != idNhanVienBN)
                return Forbid();

            phieu.ID_TrangThaiBN = req.TrangThai;
            if (req.TrangThai == (int)TrangThaiXuLy.HoanThanh)
            {
                phieu.ID_TrangThaiBN = (int)TrangThaiXuLy.HoanThanh;
                phieu.TrangThai = (int)TrangThaiXuLy.HoanThanh;
            }
            else if (req.TrangThai == (int)TrangThaiXuLy.TuChoi)
            {
                phieu.ID_TrangThaiBN = (int)TrangThaiXuLy.TuChoi;
                phieu.TrangThai = (int)TrangThaiXuLy.TuChoi;
            }
            else
            {
                phieu.ID_TrangThaiBN = req.TrangThai;
                phieu.TrangThai = (int)TrangThaiXuLy.DangXuLy;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Xác nhận thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPhieu([FromBody] ResetPhieuRequest request)
        {
            if (string.IsNullOrEmpty(request.MaPhieu))
                return BadRequest(new { success = false, message = "Mã phiếu không hợp lệ" });

            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                .FirstOrDefaultAsync(x => x.MaPhieu == request.MaPhieu);
            
            if (phieu == null)
                return NotFound(new { success = false, message = "Không tìm thấy phiếu!" });

            // Kiểm tra quyền: chỉ người tạo hoặc admin mới được reset
            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var taiKhoan = await _context.Tbl_TaiKhoan
                .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
            
            if (taiKhoan == null)
                return Unauthorized(new { success = false, message = "Không xác định được tài khoản!" });

            //// Kiểm tra: phải là người tạo mới được reset
            //if (phieu.ID_NguoiTao != taiKhoan.ID_TaiKhoan)
            //    return Forbid();

            // Kiểm tra trạng thái (không cho reset phiếu đã hoàn thành)
            //if (phieu.TrangThai == (int)TrangThaiXuLy.HoanThanh)
            //    return BadRequest(new { success = false, message = "Không thể reset phiếu đã hoàn thành!" });

            // Reset phiếu header
            phieu.ID_NguoiGiao = null;
            phieu.ID_TrangThaiBG = (int)TrangThaiXuLy.ChuaXuLy;
            phieu.ID_NguoiNhan = null;
            phieu.ID_TrangThaiBN = (int)TrangThaiXuLy.ChuaXuLy;
            phieu.TrangThai = (int)TrangThaiXuLy.ChuaXuLy;
            phieu.LoaiCan = null; // Reset loại cân về null để có thể chọn lại

            // Reset chi tiết phiếu
            var chiTietList = await _context.Tbl_BM_18_XiHatLoCao
                .Where(x => x.MaPhieu == request.MaPhieu)
                .ToListAsync();
                
            foreach (var ct in chiTietList)
            {
                ct.HeSo = null;
                ct.ID_Lo = null;
                ct.KL_Gang_Giao = null; // Reset KL Gang để có thể set lại theo loại cân mới
                ct.KL_Xi_Giao = null;
                ct.KL_Gang_Nhan = null;
                ct.KL_Xi_Nhan = null;
                ct.GhiChu = null;
            }
            
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã reset phiếu thành công! Bạn có thể chọn lại loại cân." });
        }

        /// <summary>
        /// Cập nhật loại cân và khối lượng gang cho phiếu đã tạo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CapNhatLoaiCan([FromBody] CapNhatLoaiCanRequest request)
        {
            if (string.IsNullOrEmpty(request.MaPhieu))
                return BadRequest(new { success = false, message = "Mã phiếu không hợp lệ" });

            if (request.LoaiCan < 1 || request.LoaiCan > 3)
                return BadRequest(new { success = false, message = "Loại cân không hợp lệ (1-3)" });

            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                .FirstOrDefaultAsync(x => x.MaPhieu == request.MaPhieu);
            
            if (phieu == null)
                return NotFound(new { success = false, message = "Không tìm thấy phiếu!" });

            //// Kiểm tra quyền
            //var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            //var taiKhoan = await _context.Tbl_TaiKhoan
            //    .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
            
            //if (taiKhoan == null)
            //    return Unauthorized(new { success = false, message = "Không xác định được tài khoản!" });

            //if (phieu.ID_NguoiTao != taiKhoan.ID_TaiKhoan)
            //    return Forbid();

            // Chỉ cho phép cập nhật khi phiếu ở trạng thái "Chưa xử lý"
            //if (phieu.TrangThai != (int)TrangThaiXuLy.ChuaXuLy)
            //    return BadRequest(new { success = false, message = "Chỉ có thể cập nhật loại cân cho phiếu chưa xử lý!" });

            // Lấy thông tin Ca từ Kíp
            var kipInfo = await _context.Tbl_Kip
                .Where(x => x.ID_Kip == phieu.ID_Kip)
                .Select(x => new { x.TenCa, TenCa_Int = x.TenCa == "1" ? 1 : 2 })
                .FirstOrDefaultAsync();

            if (kipInfo == null)
                return BadRequest(new { success = false, message = "Không tìm thấy thông tin kíp!" });

            // Gọi API lấy giá trị gang theo loại cân
            var giaTriResult = await LayBaGiaTriGang(
                kipInfo.TenCa_Int, 
                phieu.ID_Kip.Value, 
                phieu.NgaySanXuat, 
                phieu.ID_Locao, 
                request.LoaiCan
            );

            decimal giaTriGang = 0m;
            if (giaTriResult is OkObjectResult okResult && okResult.Value != null)
            {
                var resultValue = okResult.Value;
                var successProp = resultValue.GetType().GetProperty("success");
                var giaTriProp = resultValue.GetType().GetProperty("giaTriGang");
                
                if (successProp?.GetValue(resultValue) is bool success && success && giaTriProp != null)
                {
                    giaTriGang = Convert.ToDecimal(giaTriProp.GetValue(resultValue));
                }
            }

            if (giaTriGang == 0m)
                return BadRequest(new { success = false, message = "Không tính được khối lượng gang cho loại cân này!" });

            // Cập nhật loại cân cho phiếu
            phieu.LoaiCan = request.LoaiCan;

            // Cập nhật KL_Gang_Giao cho dòng đầu tiên của chi tiết
            var chiTietDauTien = await _context.Tbl_BM_18_XiHatLoCao
                .Where(x => x.MaPhieu == request.MaPhieu)
                .OrderBy(x => x.ID)
                .FirstOrDefaultAsync();

            if (chiTietDauTien != null)
            {
                chiTietDauTien.KL_Gang_Giao = giaTriGang;
                chiTietDauTien.KL_Gang_Nhan = giaTriGang;
            }

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                success = true, 
                message = "Đã cập nhật loại cân thành công!",
                loaiCan = request.LoaiCan,
                khoiLuongGang = giaTriGang
            });
        }

        [HttpGet]
        public async Task<IActionResult> KLGangTrongCaJson(int ca, int idKip, DateTime ngaySanXuat, int idLoCao, string maPhieu = null)
        {
            // Nếu có mã phiếu, kiểm tra trạng thái phiếu
            if (!string.IsNullOrEmpty(maPhieu))
            {
                var phieu = await _context.Tbl_BM_18_PhieuXiHat.FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);
                if (phieu != null && phieu.ID_TrangThaiBG != (int)TrangThaiXuLy.ChuaXuLy)
                {
                    // Lấy giá trị KL_Gang đã lưu trong DB (dòng đầu tiên của phiếu)
                    var chiTiet = await _context.Tbl_BM_18_XiHatLoCao.Where(x => x.MaPhieu == maPhieu).OrderBy(x => x.ID).FirstOrDefaultAsync();
                    decimal tongKLGangFromDb = chiTiet?.KL_Gang_Giao ?? 0;
                    return Ok(new { tongKLGang = tongKLGangFromDb });
                }
            }
            // ===== Lấy danh sách thùng theo điều kiện mới =====
            var dsThung = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngaySanXuat.Date &&
                    t.ID_Locao == idLoCao &&
                    t.T_copy == false
                )
                .ToListAsync();

            if (!dsThung.Any())
                return NotFound("Không tìm thấy dữ liệu theo điều kiện lọc.");

            // ===== Lấy % đúc =====
            decimal ptDuc = await _context.Tbl_BM_16_PhanTramDuc
                .Where(x => x.ID == 1)
                .Select(x => x.PhanTram)
                .FirstOrDefaultAsync();

            // ===== 1) TÍNH TỔNG KL THEO MẺ =====
            var tongTheoMe = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngaySanXuat.Date &&
                    t.ID_Locao == idLoCao &&
                    !string.IsNullOrEmpty(t.BKMIS_SoMe)
                )
                .GroupBy(t => t.BKMIS_SoMe)
                .Select(g => new
                {
                    SoMe = g.Key,
                    TongKL = g.Sum(x => (decimal?)(x.KLGangChia ?? x.T_KLGangLong ?? 0)) ?? 0m
                })
                .ToDictionaryAsync(x => x.SoMe, x => x.TongKL);
            // Tổng tất cả KL theo mẻ
            decimal tongKL_TheoMe = tongTheoMe.Values.Sum();

            // ===== 2) TÍNH KL ĐÚC THEO MẺ =====
            var klDucTheoMeRaw = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngaySanXuat.Date &&
                    t.ID_Locao == idLoCao &&
                    !string.IsNullOrEmpty(t.BKMIS_SoMe) &&
                    t.ChuyenDen != "HRC1" &&
                    t.ChuyenDen != "HRC2"
                )
                .GroupBy(t => t.BKMIS_SoMe)
                .Select(g => new
                {
                    SoMe = g.Key,

                    SumG = g.Where(x => x.T_copy == false)
                            .Sum(x => (decimal?)(x.G_KLGangLong ?? 0)) ?? 0m,

                    SumT = g.Sum(x => (decimal?)(x.T_KLGangLong ?? 0)) ?? 0m,

                    SumChiaRaw = g.Sum(x => (decimal?)(x.KLGangChia ?? 0)) ?? 0m,

                    HasChia = g.Any(x => x.KLGangChia != null && x.KLGangChia > 0)
                })
                .ToListAsync();

            var klDucTheoMe = klDucTheoMeRaw.ToDictionary(
                x => x.SoMe,
                x =>
                {
                    var baseValue = x.HasChia ? x.SumChiaRaw : x.SumT;
                    var kld = (x.SumG - baseValue) * (ptDuc / 100m);
                    return Math.Round(kld, 2);
                }
            );
            // Tổng tất cả KL đúc
            decimal tongKLDuc = klDucTheoMe.Values.Sum();
            decimal tongKLGang = tongKL_TheoMe + tongKLDuc;

            return Ok(new
            {
                tongKLGang,
                tongKL_TheoMe = tongTheoMe,
                klDuc_TheoMe = klDucTheoMe
            });
        }

        /// <summary>
        /// Lấy 3 giá trị chính hoặc 1 giá trị cụ thể: 
        /// 1. Tổng KL Gang lỏng theo Phiếu (từ G_KL_GangLong)
        /// 2. Tổng Cân cẩu trục + xỉ (G_KLGangLong - bao gồm xỉ)
        /// 3. Tổng KL Gang theo Cân cẩu trục (KL Gang ròng đã trừ xỉ và tính đúc)
        /// </summary>
        /// <param name="loaiCan">Nếu có giá trị (1-3), trả về giá trị cụ thể. Nếu null, trả về cả 3 giá trị</param>
        [HttpGet]
        public async Task<IActionResult> LayBaGiaTriGang(int ca, int idKip, DateTime ngaySanXuat, int idLoCao, int? loaiCan = null)
        {
            // ===== Lấy danh sách thùng theo điều kiện =====
            var dsThung = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngaySanXuat.Date &&
                    t.ID_Locao == idLoCao &&
                    t.T_copy == false
                )
                .ToListAsync();

            if (!dsThung.Any())
            {
                return NotFound(new 
                { 
                    success = false, 
                    message = "Không tìm thấy dữ liệu gang lỏng theo điều kiện lọc." 
                });
            }

            // ===== Lấy % đúc =====
            decimal ptDuc = await _context.Tbl_BM_16_PhanTramDuc
                .Where(x => x.ID == 1)
                .Select(x => x.PhanTram)
                .FirstOrDefaultAsync();

            // ===== 1) TÍNH TỔNG KL GANG LỎNG THEO PHIẾU (từ G_KL_GangLong) =====
            var tongKLTheoMe = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngaySanXuat.Date &&
                    t.ID_Locao == idLoCao &&
                    !string.IsNullOrEmpty(t.BKMIS_SoMe)
                )
                .GroupBy(t => t.BKMIS_SoMe)
                .Select(g => new
                {
                    SoMe = g.Key!,
                    SumG = g.Sum(x => (decimal?)(x.G_KLGangLong ?? 0)) ?? 0m,
                    SumChia = g.Sum(x => (decimal?)(x.KLGangChia ?? 0)) ?? 0m,
                    SumT = g.Sum(x => (decimal?)(x.T_KLGangLong ?? 0)) ?? 0m
                })
                .ToListAsync();

            var tongTheoMe = tongKLTheoMe.ToDictionary(
                x => x.SoMe, 
                x => x.SumChia > 0m ? x.SumChia : x.SumT
            );

            decimal tongKLGangLongCanCauTruc = tongTheoMe.Values.Sum();

            // ===== TÍNH KHỐI LƯỢNG ĐÚC (ChuyenDen = DUC1 hoặc DUC2) =====
            var meChuyenDuc = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngaySanXuat.Date &&
                    t.ID_Locao == idLoCao &&
                    !string.IsNullOrEmpty(t.BKMIS_SoMe) &&
                    (t.ChuyenDen == "DUC1" || t.ChuyenDen == "DUC2")
                )
                .Select(x => x.BKMIS_SoMe)
                .Distinct()
                .ToListAsync();

            var klDucByMe = tongKLTheoMe
                .Where(x => meChuyenDuc.Contains(x.SoMe))
                .ToDictionary(
                    x => x.SoMe,
                    x =>
                    {
                        var baseValue = x.SumChia > 0m ? x.SumChia : x.SumT;
                        var value = (x.SumG - baseValue) * (ptDuc / 100m);
                        return Math.Round(value, 2);
                    }
                );

            decimal tongKLDuc = klDucByMe.Values.Sum();

            // ===== 2) TÍNH TỔNG CÂN CẨU TRỤC + XỈ (KLGangCCTVaXi) =====
            var canCauTrucVaXiRaw = await _context.Tbl_BM_16_GangLong
                    .Where(t =>
                        t.G_Ca == ca &&
                        t.G_ID_Kip == idKip &&
                        t.NgayTao == ngaySanXuat.Date &&
                        t.ID_Locao == idLoCao &&
                        !string.IsNullOrEmpty(t.BKMIS_SoMe)
                    )
                    .GroupBy(t => t.BKMIS_SoMe)
                    .Select(g => new
                    {
                        SoMe = g.Key!,
                        TongKL = g.Sum(x => (decimal?)(x.KLGangCCTVaXi ?? x.KLGangChia ?? x.T_KLGangLong)) ?? 0m
                    })
                    .ToListAsync();

            var canCauTrucVaXiTheoMe = canCauTrucVaXiRaw.ToDictionary(x => x.SoMe, x => x.TongKL);

            decimal tongCanCauTrucVaXi = canCauTrucVaXiTheoMe.Values.Sum() + tongKLDuc;

            // ===== 3) TÍNH TỔNG KL GANG THEO CÂN CẨU TRỤC (chỉ sum G_KLGangLong) =====
            var tongKLGangTheoCanRay = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngaySanXuat.Date &&
                    t.ID_Locao == idLoCao &&
                    t.T_copy == false &&
                    !string.IsNullOrEmpty(t.BKMIS_SoMe)
                )
                .SumAsync(x => (decimal?)(x.G_KLGangLong ?? 0) ?? 0m);

            // ===== TRẢ VỀ GIÁ TRỊ =====
            // Nếu có chọn loại cân cụ thể, chỉ trả về giá trị đó
            if (loaiCan.HasValue)
            {
                decimal giaTriChon = loaiCan.Value switch
                {
                    1 => Math.Round(tongKLGangLongCanCauTruc, 2),
                    2 => Math.Round(tongCanCauTrucVaXi, 2),
                    3 => Math.Round(tongKLGangTheoCanRay, 2),
                    _ => 0m
                };

                return Ok(new
                {
                    success = true,
                    giaTriGang = giaTriChon,
                    loaiCan = loaiCan.Value
                });
            }

            // Nếu không chọn loại cân, trả về cả 3 giá trị
            return Ok(new
            {
                success = true,
                data = new
                {
                    // Giá trị 1: Tổng KL Gang lỏng theo Phiếu
                    tongKLGangLongCanCauTruc = Math.Round(tongKLGangLongCanCauTruc, 2),
                    
                    // Giá trị 2: Tổng Cân cẩu trục + xỉ (từ KLGangCCTVaXi)
                    tongCanCauTrucVaXi = Math.Round(tongCanCauTrucVaXi, 2),

                    // Giá trị 3: Tổng KL Gang theo Cân cẩu trục (từ G_KLGangLong)
                    tongKLGangTheoCanRay = Math.Round(tongKLGangTheoCanRay, 2),

                    // Giá trị 4: Tổng KL Đúc (ChuyenDen = DUC1 hoặc DUC2)
                    tongKLDuc = Math.Round(tongKLDuc, 2),
                    
                    // Chi tiết bổ sung
                    chiTiet = new
                    {
                        phanTramDuc = ptDuc,
                        soLuongMe = tongTheoMe.Count,
                        danhSachMeVaKL = tongTheoMe,
                        canCauTrucVaXiTheoMe = canCauTrucVaXiTheoMe,
                        klDucTheoMe = klDucByMe,
                        soLuongMeDuc = klDucByMe.Count,
                        tongKLDuc = tongKLDuc
                    }
                }
            });
        }

        [HttpGet]

        public async Task<IActionResult> ExportPDF()
        {
            string testMaPhieu = "XHLC-L1-1A-251101";

            // Lấy phiếu tổng
            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                 .FirstOrDefaultAsync(x => x.MaPhieu == testMaPhieu);

            if (phieu == null)
                return NotFound("Không tìm thấy phiếu.");

            // Lấy chi tiết (có TenMaLo)
            var chiTiet = await (
                 from ct in _context.Tbl_BM_18_XiHatLoCao
                     .Where(x => x.MaPhieu == phieu.MaPhieu)
                 join ml in _context.Tbl_MaLo
                     on ct.ID_Lo equals ml.ID_MaLo into g
                 from ml in g.DefaultIfEmpty()
                     // Join thêm bảng Kip
                 join kp in _context.Tbl_Kip
                     on ct.Kip equals kp.ID_Kip into g2
                 from kp in g2.DefaultIfEmpty()
                 select new Tbl_BM_18_XiHatLoCao
                 {
                     ID = ct.ID,
                     MaPhieu = ct.MaPhieu,
                     ID_LoCao = ct.ID_LoCao,
                     Ca = ct.Ca,
                     Kip = ct.Kip,
                     Ten_NVL = ct.Ten_NVL,
                     DVT = ct.DVT,
                     ID_Lo = ct.ID_Lo,
                     HeSo = ct.HeSo,
                     NgaySanXuat = ct.NgaySanXuat,
                     KL_Gang_Giao = ct.KL_Gang_Giao,
                     KL_Xi_Giao = ct.KL_Xi_Giao,
                     KL_Gang_Nhan = ct.KL_Gang_Nhan,
                     KL_Xi_Nhan = ct.KL_Xi_Nhan,
                     GhiChu = ct.GhiChu,
                     TenMaLo = ml != null ? ml.TenMaLo : "",
                     CaKip = kp != null ? $"{kp.TenCa}{kp.TenKip}" : ""
                 }
             ).ToListAsync();

            var vm = new Data_Product.DTO.BM_18_DTO.BM18DetailViewModel
            {
                Phieu = phieu,
                ChiTiet = chiTiet
            };

            return View(vm); // truyền đúng ViewModel
        }
        public async Task<IActionResult> ExportDanhSachExcel(string maPhieu, DateTime? tuNgaySX,DateTime? denNgaySX, string ca, string locao, string trangThai)
        {
            string fileNamemaunew = null;
            try
            {

                // Query danh sách phiếu với filter
                var query = _context.Tbl_BM_18_PhieuXiHat.AsQueryable();

                if (!string.IsNullOrEmpty(maPhieu))
                {
                    query = query.Where(x => x.MaPhieu.Contains(maPhieu));
                }

                if (tuNgaySX.HasValue && denNgaySX.HasValue)
                {
                    query = query.Where(s =>
                        s.NgaySanXuat.Date >= tuNgaySX.Value.Date &&
                        s.NgaySanXuat.Date <= denNgaySX.Value.Date
                    );
                }
                else if (tuNgaySX.HasValue)
                {
                    query = query.Where(s => s.NgaySanXuat.Date >= tuNgaySX.Value.Date);
                }
                else if (denNgaySX.HasValue)
                {
                    query = query.Where(s => s.NgaySanXuat.Date <= denNgaySX.Value.Date);
                }


                if (!string.IsNullOrEmpty(ca) && int.TryParse(ca, out int caInt))
                {
                    query = query.Where(x => x.Ca.HasValue && x.Ca.Value == caInt);
                }

                if (!string.IsNullOrEmpty(locao))
                {
                    if (int.TryParse(locao, out int loCaoId))
                    {
                        query = query.Where(x => x.ID_Locao == loCaoId);
                    }
                }

                if (!string.IsNullOrEmpty(trangThai))
                {
                    if (int.TryParse(trangThai, out int ttValue))
                    {
                        query = query.Where(x => x.TrangThai == ttValue);
                    }
                }

                var danhSachPhieu = await query.OrderByDescending(x => x.NgaySanXuat).ToListAsync();

                if (danhSachPhieu.Count == 0)
                {
                    TempData["msgError"] = "<script>alert('Không có dữ liệu để xuất!');</script>";
                    return RedirectToAction("Index_All_PKH");
                }

                // Lấy tất cả mã phiếu
                var maPhieuList = danhSachPhieu.Select(x => x.MaPhieu).ToList();

                var nguoiIds = danhSachPhieu
                .Where(x => x.ID_NguoiGiao.HasValue || x.ID_NguoiNhan.HasValue)
                .SelectMany(x => new int?[] { x.ID_NguoiGiao, x.ID_NguoiNhan })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

                var nhanSuList = await (
                    from nv in _context.Tbl_TaiKhoan
                    join pb in _context.Tbl_PhongBan
                        on nv.ID_PhongBan equals pb.ID_PhongBan
                    join x in _context.Tbl_Xuong
                        on nv.ID_PhanXuong equals x.ID_Xuong
                    where nguoiIds.Contains(nv.ID_TaiKhoan)
                    select new
                    {
                        NhanVienId = nv.ID_TaiKhoan,
                        TenPhongBan = pb.TenPhongBan,
                        TenXuong = x.TenXuong
                    }
                ).ToListAsync();


                var nhanSuDict = nhanSuList
                    .GroupBy(x => x.NhanVienId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First()
                    );

                // Load chi tiết của tất cả phiếu
                var chiTietList = await _context.Tbl_BM_18_XiHatLoCao
                    .Where(x => maPhieuList.Contains(x.MaPhieu))
                    .OrderBy(x => x.MaPhieu)
                    .ThenBy(x => x.ID)
                    .ToListAsync();

                if (chiTietList.Count == 0)
                {
                    TempData["msgError"] = "<script>alert('Không có chi tiết dữ liệu!');</script>";
                    return RedirectToAction("Index_All_PKH");
                }

                // Load dữ liệu liên quan
                var kipIds = danhSachPhieu.Select(x => x.ID_Kip).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToArray();
                var kips = await _context.Tbl_Kip
                    .Where(x => kipIds.Contains(x.ID_Kip))
                    .ToDictionaryAsync(x => x.ID_Kip);

                var loCaoIds = chiTietList.Select(x => x.ID_LoCao).Distinct().ToArray();
                var loCaos = await _context.Tbl_LoCao
                    .Where(x => loCaoIds.Contains(x.ID))
                    .ToDictionaryAsync(x => x.ID);

                var loIds = chiTietList.Select(x => x.ID_Lo).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToArray();
                var los = await _context.Tbl_MaLo
                    .Where(x => loIds.Contains(x.ID_MaLo))
                    .ToDictionaryAsync(x => x.ID_MaLo);

                // Tạo dictionary phiếu để lookup nhanh
                var phieuDict = danhSachPhieu.ToDictionary(x => x.MaPhieu);

                // Tạo file Excel từ template hoặc tạo mới
                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_TongHopXiHatLoCao.xlsx";
                fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BM_TongHopXiHatLoCao" + Guid.NewGuid() + ".xlsx";

                // ✅ KHỞI TẠO Ở ĐÂY – BẮT BUỘC
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet(1);

                string tuNgayText = tuNgaySX.HasValue ? tuNgaySX.Value.ToString("dd/MM/yyyy") : "....";
                string denNgayText = denNgaySX.HasValue ? denNgaySX.Value.ToString("dd/MM/yyyy") : ".......";

                string filterNgay = $"Từ ngày: {tuNgayText} đến ngày: {denNgayText}";

                Worksheet.Range("A4:T4").Merge();
                Worksheet.Cell("A4").Value = filterNgay;

                Worksheet.Range("A4:T4").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                Worksheet.Range("A4:T4").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                Worksheet.Range("A4:T4").Style.Font.Italic = true;
                Worksheet.Range("A4:T4").Style.Font.SetFontSize(11);

                if (!System.IO.File.Exists(fileNamemau))
                {
                    TempData["msgError"] =
                        "<script>alert('Không tìm thấy file template BM_TongHopXiHatLoCao.xlsx');</script>";
                    return RedirectToAction("Index_All_PKH");
                }

                // Bắt đầu ghi dữ liệu từ dòng 8
                int currentRow = 9;
                int stt = 0;

                foreach (var item in chiTietList)
                {
                    stt++;
                    int col = 1;

                    // Lấy thông tin phiếu
                    var phieu = phieuDict.ContainsKey(item.MaPhieu) ? phieuDict[item.MaPhieu] : null;

                    // STT
                    SetCellValue(Worksheet.Cell(currentRow, col++), stt, XLAlignmentHorizontalValues.Center);

                    // Ngày
                    if (item.NgaySanXuat.HasValue)
                    {
                        var cell = Worksheet.Cell(currentRow, col++);
                        cell.Value = item.NgaySanXuat.Value;
                        cell.Style.DateFormat.Format = "dd/MM/yyyy";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    else
                    {
                        SetCellValue(Worksheet.Cell(currentRow, col++), "", XLAlignmentHorizontalValues.Center);
                    }

                    //// Lò cao
                    //string tenLoCao = "";
                    //if (loCaos.ContainsKey(item.ID_LoCao))
                    //{
                    //    tenLoCao = loCaos[item.ID_LoCao].TenLoCao ?? "";
                    //}
                    //SetCellValue(Worksheet.Cell(currentRow, col++), tenLoCao, XLAlignmentHorizontalValues.Center);

                    // Kíp
                    string tenKip = "";
                    if (phieu != null && phieu.ID_Kip.HasValue && kips.ContainsKey(phieu.ID_Kip.Value))
                    {
                        tenKip = kips[phieu.ID_Kip.Value].TenKip ?? "";
                    }
                    SetCellValue(Worksheet.Cell(currentRow, col++), tenKip, XLAlignmentHorizontalValues.Center);

                    // Ca
                    string tenCa = item.Ca == 1 ? "Ngày" : item.Ca == 2 ? "Đêm" : (item.Ca?.ToString() ?? "");
                    SetCellValue(Worksheet.Cell(currentRow, col++), tenCa, XLAlignmentHorizontalValues.Center);

                    // Tên N/VL
                    SetCellValue(Worksheet.Cell(currentRow, col++), item.Ten_NVL ?? "");

                    // Tên lô
                    string tenLo = "";
                    if (item.ID_Lo.HasValue && los.ContainsKey(item.ID_Lo.Value))
                    {
                        tenLo = los[item.ID_Lo.Value].TenMaLo ?? "";
                    }
                    SetCellValue(Worksheet.Cell(currentRow, col++), tenLo);

                    //// Hệ số quy đổi
                    //SetCellValue(Worksheet.Cell(currentRow, col++), item.HeSo ?? 0, XLAlignmentHorizontalValues.Center);

                    // ĐVT
                    SetCellValue(Worksheet.Cell(currentRow, col++), item.DVT ?? "", XLAlignmentHorizontalValues.Center);
                    // === KL bên nhận ===
                    // KL gang
                    //SetCellValue(Worksheet.Cell(currentRow, col++), item.KL_Gang_Nhan ?? 0, XLAlignmentHorizontalValues.Right);
                    var cellGangNhan = Worksheet.Cell(currentRow, col++);
                    cellGangNhan.Value = item.KL_Gang_Nhan ?? 0;
                    cellGangNhan.Style.NumberFormat.Format = "0.000";
                    cellGangNhan.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cellGangNhan.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    //// Hệ số quy đổi
                    var cellHeSoNhan = Worksheet.Cell(currentRow, col++);
                    cellHeSoNhan.Value = item.HeSo ?? 0;
                    cellHeSoNhan.Style.NumberFormat.Format = "0.000";
                    cellHeSoNhan.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cellHeSoNhan.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    // KL Xi

                    // SetCellValue(Worksheet.Cell(currentRow, col++), item.KL_Xi_Nhan ?? 0, XLAlignmentHorizontalValues.Right);
                    var cellXiNhan = Worksheet.Cell(currentRow, col++);
                    cellXiNhan.Value = item.KL_Xi_Nhan ?? 0;
                    cellXiNhan.Style.NumberFormat.Format = "0.000";
                    cellXiNhan.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cellXiNhan.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                    string xuongNhan = "";
                    string boPhanNhan = "";

                    if (phieu?.ID_NguoiNhan.HasValue == true &&
                        nhanSuDict.TryGetValue(phieu.ID_NguoiNhan.Value, out var nsNhan))
                    {
                        xuongNhan = nsNhan.TenXuong ?? "";
                        boPhanNhan = nsNhan.TenPhongBan ?? "";
                    }

                    // Xưởng nhận
                    SetCellValue(Worksheet.Cell(currentRow, col++), xuongNhan);

                    // Bộ phận nhận
                    SetCellValue(Worksheet.Cell(currentRow, col++), boPhanNhan);

                    // === KL bên giao ===
                    // KL gang
                    //SetCellValue(Worksheet.Cell(currentRow, col++), item.KL_Gang_Giao ?? 0, XLAlignmentHorizontalValues.Right);
                    var cellGang = Worksheet.Cell(currentRow, col++);
                    cellGang.Value = item.KL_Gang_Giao ?? 0;
                    cellGang.Style.NumberFormat.Format = "0.000";
                    cellGang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cellGang.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    //// Hệ số quy đổi
                    var cellHeSoGiao = Worksheet.Cell(currentRow, col++);
                    cellHeSoGiao.Value = item.HeSo ?? 0;
                    cellHeSoGiao.Style.NumberFormat.Format = "0.000";
                    cellHeSoGiao.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cellHeSoGiao.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    // KL xỉ
                    // SetCellValue(Worksheet.Cell(currentRow, col++), item.KL_Xi_Giao ?? 0, XLAlignmentHorizontalValues.Right);
                    var cellXi = Worksheet.Cell(currentRow, col++);
                    cellXi.Value = item.KL_Xi_Giao ?? 0;
                    cellXi.Style.NumberFormat.Format = "0.000";
                    cellXi.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    cellXi.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    string xuongGiao = "";
                    string boPhanGiao = "";

                    if (phieu?.ID_NguoiGiao.HasValue == true &&
                        nhanSuDict.TryGetValue(phieu.ID_NguoiGiao.Value, out var nsGiao))
                    {
                        xuongGiao = nsGiao.TenXuong ?? "";
                        boPhanGiao = nsGiao.TenPhongBan ?? "";
                    }

                    // Xưởng giao
                    SetCellValue(Worksheet.Cell(currentRow, col++), xuongGiao);

                    // Bộ phận giao
                    SetCellValue(Worksheet.Cell(currentRow, col++), boPhanGiao);


                    // Ghi chú
                    SetCellValue(Worksheet.Cell(currentRow, col++), item.GhiChu ?? "");

                    // Mã phiếu
                    SetCellValue(Worksheet.Cell(currentRow, col++), item.MaPhieu ?? "");

                    // Tình trạng phiếu
                    string trangThaiText = "";
                    if (phieu != null)
                    {
                        switch (phieu.TrangThai)
                        {
                            case 0: trangThaiText = "Chưa xử lý"; break;
                            case 1: trangThaiText = "Đang xử lý"; break;
                            case 2: trangThaiText = "Hoàn thành"; break;
                            case 3: trangThaiText = "Từ chối"; break;
                            default: trangThaiText = ""; break;
                        }
                    }
                    SetCellValue(Worksheet.Cell(currentRow, col++), trangThaiText, XLAlignmentHorizontalValues.Center);

                    currentRow++;
                }

                // Format toàn bộ vùng dữ liệu
                int lastRow = currentRow - 1;
                if (lastRow >= 8)
                {
                    var dataRange = Worksheet.Range($"A7:T{lastRow}");
                    dataRange.Style.Font.SetFontName("Times New Roman");
                    dataRange.Style.Font.SetFontSize(11);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                // Lưu file
                Workbook.SaveAs(fileNamemaunew);

                // Đọc và trả về
                byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                string fileName = $"TongHop_XiHatLoCao_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xuất Excel danh sách xỉ hạt");
                TempData["msgError"] = "<script>alert('Có lỗi khi xuất file.  Vui lòng thử lại!');</script>";
                return RedirectToAction("Index_All_PKH");
            }
            finally
            {
                if (!string.IsNullOrEmpty(fileNamemaunew) && System.IO.File.Exists(fileNamemaunew))
                {
                    try
                    {
                        System.IO.File.Delete(fileNamemaunew);
                    }
                    catch { }
                }
            }
        }

        // Hàm helper
        private void SetCellValue(IXLCell cell, object value, XLAlignmentHorizontalValues horizontal = XLAlignmentHorizontalValues.Left)
        {
            cell.SetValue(value?.ToString() ?? string.Empty);
            cell.Style.Alignment.Horizontal = horizontal;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;
        }


        public async Task<IActionResult> GeneratePdf(string maPhieu)
        {
            // Lấy phiếu
            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);

            if (phieu == null)
                return NotFound("Không tìm thấy phiếu.");

            // Lấy chi tiết (có TenMaLo)
            var chiTiet = await (
                 from ct in _context.Tbl_BM_18_XiHatLoCao
                     .Where(x => x.MaPhieu == phieu.MaPhieu)
                 join ml in _context.Tbl_MaLo
                     on ct.ID_Lo equals ml.ID_MaLo into g
                 from ml in g.DefaultIfEmpty()
                     // Join thêm bảng Kip
                 join kp in _context.Tbl_Kip
                     on ct.Kip equals kp.ID_Kip into g2
                 from kp in g2.DefaultIfEmpty()
                 select new Tbl_BM_18_XiHatLoCao
                 {
                     ID = ct.ID,
                     MaPhieu = ct.MaPhieu,
                     ID_LoCao = ct.ID_LoCao,
                     Ca = ct.Ca,
                     Kip = ct.Kip,
                     Ten_NVL = ct.Ten_NVL,
                     DVT = ct.DVT,
                     ID_Lo = ct.ID_Lo,
                     HeSo = ct.HeSo,
                     NgaySanXuat = ct.NgaySanXuat,
                     KL_Gang_Giao = ct.KL_Gang_Giao,
                     KL_Xi_Giao = ct.KL_Xi_Giao,
                     KL_Gang_Nhan = ct.KL_Gang_Nhan,
                     KL_Xi_Nhan = ct.KL_Xi_Nhan,
                     GhiChu = ct.GhiChu,
                     TenMaLo = ml != null ? ml.TenMaLo : "",
                     CaKip = kp != null ? $"{kp.TenCa}{kp.TenKip}" : ""
                 }
             ).ToListAsync();

            // Đưa vào ViewModel
            var vm = new BM18DetailViewModel
            {
                Phieu = phieu,
                ChiTiet = chiTiet
            };

            // Render View -> HTML
            string htmlContent = await RenderViewToStringAsync("ExportPDF", vm);

            // HTML -> PDF
            byte[] pdfBytes = ConvertHtmlToPdf(htmlContent);

            string filename = phieu.MaPhieu + "_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".pdf";

            return File(pdfBytes, "application/pdf", filename);
        }

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

                if (!viewResult.Success)
                    throw new FileNotFoundException($"Không tìm thấy view: {viewName}");

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
        private byte[] ConvertHtmlToPdf(string html)
        {
            using (var ms = new MemoryStream())
            {
                var writer = new iText.Kernel.Pdf.PdfWriter(ms);
                var pdf = new iText.Kernel.Pdf.PdfDocument(writer);

                // A4 ngang
                pdf.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4.Rotate());

                // Font
                var fontProvider = new FontProvider();
                fontProvider.AddFont("C:/Windows/Fonts/times.ttf");
                fontProvider.AddFont("C:/Windows/Fonts/timesbd.ttf");

                var props = new ConverterProperties();
                props.SetFontProvider(fontProvider);

                HtmlConverter.ConvertToPdf(html, pdf, props);

                return ms.ToArray();
            }
        }
        [HttpPost]
        public async Task<IActionResult> CapNhatMaLo([FromBody] UpdateMaLoDto model)
        {
            if (model == null || model.MaPhieu == null || model.ID_MaLo == 0)
                return BadRequest("Dữ liệu không hợp lệ.");

            // 1. Lấy user đang đăng nhập
            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var user = await _context.Tbl_TaiKhoan
                .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);

            if (user == null)
                return Unauthorized("Không xác định người dùng.");

            // 2. Lấy chi tiết BM18
            var chiTiet = await _context.Tbl_BM_18_XiHatLoCao
                .FirstOrDefaultAsync(x => x.MaPhieu == model.MaPhieu);

            if (chiTiet == null)
                return NotFound("Không tìm thấy dòng chi tiết.");

            // 3. Lấy phiếu BM18
            var phieu = await _context.Tbl_BM_18_PhieuXiHat
                .FirstOrDefaultAsync(x => x.MaPhieu == chiTiet.MaPhieu);

            if (phieu == null)
                return NotFound("Không tìm thấy phiếu.");

            // 4. CHECK NGHIỆP VỤ
            // Chỉ cho sửa khi:
            // - Phiếu đang xử lý
            // - User là BÊN NHẬN
            if (phieu.TrangThai != (int)TrangThaiXuLy.DangXuLy)
                return Forbid("Phiếu đã hoàn thành, không được chỉnh sửa.");

            if (!phieu.ID_NguoiNhan.HasValue || phieu.ID_NguoiNhan.Value != user.ID_TaiKhoan)
                return Forbid("Bạn không có quyền chỉnh sửa mã lô.");

            // 5. Check mã lô tồn tại
            var maLoMoi = await _context.Tbl_MaLo
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ID_MaLo == model.ID_MaLo);

            if (maLoMoi == null)
                return BadRequest("Mã lô không tồn tại.");

            // 6. Không update nếu không thay đổi
            if (chiTiet.ID_Lo == model.ID_MaLo)
                return Ok(new { message = "Mã lô không thay đổi." });
            // 8. Cập nhật mã lô
            chiTiet.ID_Lo = model.ID_MaLo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật mã lô thành công",
                MaLoMoi = maLoMoi.TenMaLo
            });
        }
    }
}
