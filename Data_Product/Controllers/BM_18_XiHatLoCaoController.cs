using System.Security.Claims;
using ClosedXML.Excel;
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
        public async Task<IActionResult> Danhsachphieu(string maPhieu, DateTime? ngay, DateTime? ngaysx, string ca, string locao, int page = 1)
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
                    .OrderByDescending(p => p.NgayTaoPhieu.Date) // Ngày mới trước
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
        public async Task<IActionResult> Index_All(string maPhieu, DateTime? ngay, DateTime? ngaysx, string ca, string locao, int page = 1)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BN = TaiKhoan.ID_TaiKhoan;

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
                    .OrderByDescending(p => p.NgayTaoPhieu.Date) // Ngày mới trước
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
                        ID_TrangThaiBG = 0,
                        ID_TrangThaiBN = 0,
                        TrangThai = 0
                    };

                    _context.Tbl_BM_18_PhieuXiHat.Add(header);
                    await _context.SaveChangesAsync();

                    // Thêm dòng chi tiết mặc định vào Tbl_BM_18_XiHatLoCao
                    var xiHat = new Tbl_BM_18_XiHatLoCao
                    {
                        Ca = header.ID_Kip,
                        Kip = header.ID_Kip,
                        NgaySanXuat = header.NgaySanXuat,
                        MaPhieu = header.MaPhieu,
                        ID_LoCao = header.ID_Locao,
                        Ten_NVL = "Xỉ hạt lò cao",
                        DVT = "Tấn",
                        ID_Lo = null,
                        HeSo = null,
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
            return $"XHLC-L{idLoCao}-{ca}{kip}-{ngay:yyMMdd}";
        }

        // [HttpGet("DetailPhieu")]
        //public async Task<IActionResult> DetailPhieu(string maPhieu)
        //{
        //    if (string.IsNullOrWhiteSpace(maPhieu))
        //        return BadRequest("Thiếu mã phiếu.");

        //    DateTime DayNow = DateTime.Now;
        //    String Day = DayNow.ToString("dd/MM/yyyy");
        //    DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

        //    var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
        //    var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
        //    var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
        //    string TenBP = PhongBan.TenNgan.ToString();

        //    List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
        //    ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

        //    var NhanVien = await (from a in _context.Tbl_TaiKhoan
        //                          select new Tbl_TaiKhoan
        //                          {
        //                              ID_TaiKhoan = a.ID_TaiKhoan,
        //                              HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
        //                          }).ToListAsync();

        //    var phieu = await _context.Set<Tbl_BM_18_PhieuXiHat>()
        //        .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);

        //    if (phieu == null)
        //        return NotFound($"Không tìm thấy phiếu với mã: {maPhieu}");

        //    // Tạo SelectList với giá trị được chọn sẵn nếu có ID_NguoiNhan
        //    ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", phieu.ID_NguoiNhan);

        //    // Thêm ViewBag để biết có ID_NguoiNhan hay không
        //    ViewBag.HasNguoiNhan = phieu.ID_NguoiNhan.HasValue;
        //    ViewBag.ID_NguoiNhan = phieu.ID_NguoiNhan;

        //    var kip = await _context.Tbl_Kip.FirstOrDefaultAsync(k => k.ID_Kip == phieu.ID_Kip);
        //    var chiTiet = await _context.Set<Tbl_BM_18_XiHatLoCao>()
        //        .AsNoTracking()
        //        .Where(x => x.MaPhieu == maPhieu)
        //        .OrderBy(x => x.ID)
        //        .ToListAsync();

        //    var MaLo = await (from a in _context.Tbl_MaLo
        //                      select new Tbl_MaLo
        //                      {
        //                          ID_MaLo = a.ID_MaLo,
        //                          TenMaLo = a.TenMaLo,
        //                          ID_TinhTrang = 1
        //                      }).ToListAsync();

        //    ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");
        //    ViewBag.MaPhieu = phieu.MaPhieu;
        //    ViewBag.NgaySanXuat = phieu.NgaySanXuat.ToString("yyyy-MM-dd");
        //    ViewBag.TenKip = phieu.ID_Kip;
        //    ViewBag.ID_Locao = phieu.ID_Locao;
        //    ViewBag.ID_Kip = phieu.ID_Kip;
        //    ViewBag.TenKip = kip?.TenKip;
        //    ViewBag.TenCa = kip?.TenCa;
        //    ViewBag.Phieu = phieu;

        //    return View("DetailPhieu", chiTiet);
        //}
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
                    phieu.ID_TrangThaiBG = 1;
                    phieu.TrangThai = 0;
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
             Tbl_TaiKhoan thongTinBG = null;
             Tbl_PhongBan phongBanBG = null;
             Tbl_Xuong phanXuongBG = null;
             Tbl_ViTri viTriBG = null;
             if (phieu.ID_NguoiGiao.HasValue && phieu.ID_NguoiGiao.Value > 0)
             {
                 thongTinBG = await _context.Tbl_TaiKhoan
                     .FirstOrDefaultAsync(x => x.ID_TaiKhoan == phieu.ID_NguoiGiao.Value);
                 if (thongTinBG != null)
                 {
                     phongBanBG = await _context.Tbl_PhongBan
                         .FirstOrDefaultAsync(x => x.ID_PhongBan == thongTinBG.ID_PhongBan);
                     phanXuongBG = await _context.Tbl_Xuong
                         .FirstOrDefaultAsync(x => x.ID_Xuong == thongTinBG.ID_PhanXuong);
                     viTriBG = await _context.Tbl_ViTri
                         .FirstOrDefaultAsync(x => x.ID_ViTri == thongTinBG.ID_ChucVu);
                 }
             }

             // 4. Lấy thông tin bên nhận
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

             return View(viewModel);
         }
        [HttpPost]
        public async Task<IActionResult> XacNhanPhieuBN([FromBody] XacNhanPhieuBNRequest req)
        {
            if (string.IsNullOrEmpty(req.MaPhieu))
                return BadRequest("Mã phiếu không hợp lệ!");
            // if (req.TrangThai != 1 && req.TrangThai != 2)
            //     return BadRequest("Trạng thái không hợp lệ!");

            var phieu = await _context.Tbl_BM_18_PhieuXiHat.FirstOrDefaultAsync(x => x.MaPhieu == req.MaPhieu);
            if (phieu == null)
                return NotFound("Không tìm thấy phiếu!");

            // 1 = Đã xử lý, 2 = Hủy phiếu
            phieu.ID_TrangThaiBN = req.TrangThai;
            phieu.TrangThai = (req.TrangThai == 1) ? 1 : 2;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Xác nhận thành công!" });
        }
        [HttpPost]
        public IActionResult ResetPhieu([FromBody] ResetPhieuRequest request)
        {
            if (string.IsNullOrEmpty(request.MaPhieu))
                return BadRequest("Mã phiếu không hợp lệ");

            var phieu = _context.Tbl_BM_18_PhieuXiHat.FirstOrDefault(x => x.MaPhieu == request.MaPhieu);
            phieu.ID_NguoiGiao = null;
            phieu.ID_TrangThaiBG = 0;
            phieu.ID_NguoiNhan = null;
            phieu.ID_TrangThaiBN = 0;
            phieu.TrangThai = 0;  

            // Xóa chi tiết phiếu
            var chiTietList = _context.Tbl_BM_18_XiHatLoCao
                                  .Where(x => x.MaPhieu == request.MaPhieu).ToList();
            foreach (var ct in chiTietList)
            {
                ct.ID_Lo = 0;
                ct.KL_Gang_Giao = 0;
                ct.KL_Xi_Giao = 0;
                ct.KL_Gang_Nhan = 0;
                ct.KL_Xi_Nhan = 0;
                ct.GhiChu = null;
            }
            _context.SaveChanges();

            return Ok(new { success = true });
        }
        // [HttpGet]
        // public async Task<IActionResult> KLGangTrongCa(int ca, int idKip, DateTime ngayLuyenGang, int idLoCao)
        // {
            
        //     var dsThung = await _context.Tbl_BM_16_GangLong
        //         .Where(t =>
        //             t.G_Ca == ca &&
        //             t.G_ID_Kip == idKip &&
        //             t.NgayTao == ngayLuyenGang.Date &&
        //             t.ID_Locao == idLoCao &&
        //             t.T_copy == false
        //         )
        //         .ToListAsync();

        //     if (!dsThung.Any())
        //         return NotFound("Không tìm thấy dữ liệu theo điều kiện lọc.");


        //     decimal ptDuc = await _context.Tbl_BM_16_PhanTramDuc
        //         .Where(x => x.ID == 1)
        //         .Select(x => x.PhanTram)
        //         .FirstOrDefaultAsync();


        //     var tongTheoMe = await _context.Tbl_BM_16_GangLong
        //         .Where(t =>
        //             t.G_Ca == ca &&
        //             t.G_ID_Kip == idKip &&
        //             t.NgayTao == ngayLuyenGang.Date &&
        //             t.ID_Locao == idLoCao &&
        //             !string.IsNullOrEmpty(t.BKMIS_SoMe)
        //         )
        //         .GroupBy(t => t.BKMIS_SoMe)
        //         .Select(g => new
        //         {
        //             SoMe = g.Key,
        //             TongKL = g.Sum(x => (decimal?)(x.KLGangChia ?? x.T_KLGangLong ?? 0)) ?? 0m
        //         })
        //         .ToDictionaryAsync(x => x.SoMe, x => x.TongKL);
          
        //     decimal tongKL_TheoMe = tongTheoMe.Values.Sum();
   
        //     var klDucTheoMeRaw = await _context.Tbl_BM_16_GangLong
        //         .Where(t =>
        //             t.G_Ca == ca &&
        //             t.G_ID_Kip == idKip &&
        //             t.NgayTao == ngayLuyenGang.Date &&
        //             t.ID_Locao == idLoCao &&
        //             !string.IsNullOrEmpty(t.BKMIS_SoMe) &&
        //             t.ChuyenDen != "HRC1" &&
        //             t.ChuyenDen != "HRC2"
        //         )
        //         .GroupBy(t => t.BKMIS_SoMe)
        //         .Select(g => new
        //         {
        //             SoMe = g.Key,

        //             SumG = g.Where(x => x.T_copy == false)
        //                     .Sum(x => (decimal?)(x.G_KLGangLong ?? 0)) ?? 0m,

        //             SumT = g.Sum(x => (decimal?)(x.T_KLGangLong ?? 0)) ?? 0m,

        //             SumChiaRaw = g.Sum(x => (decimal?)(x.KLGangChia ?? 0)) ?? 0m,

        //             HasChia = g.Any(x => x.KLGangChia != null && x.KLGangChia > 0)
        //         })
        //         .ToListAsync();

        //     var klDucTheoMe = klDucTheoMeRaw.ToDictionary(
        //         x => x.SoMe,
        //         x =>
        //         {
        //             var baseValue = x.HasChia ? x.SumChiaRaw : x.SumT;
        //             var kld = (x.SumG - baseValue) * (ptDuc / 100m);
        //             return Math.Round(kld, 2);
        //         }
        //     );
       
        //     decimal tongKLDuc = klDucTheoMe.Values.Sum();

        //     decimal tongKLGang = tongKLDuc + tongKL_TheoMe;
     
        //     ViewBag.TongKL_TheoMe = tongTheoMe;
        //     ViewBag.KLDuc_TheoMe = klDucTheoMe;
        //     ViewBag.TongKLGang = tongKLGang;
        //     return View();
        // }

        [HttpGet]
        public async Task<IActionResult> KLGangTrongCaJson(int ca, int idKip, DateTime ngaySanXuat, int idLoCao)
        {
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

        public async Task<IActionResult> ExportToExcel(int BBGN_ID)
        {
            try
            {

                string fileNamemau = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN.xlsx";
                string fileNamemaunew = AppDomain.CurrentDomain.DynamicDirectory + @"App_Data\BBGN_Temp.xlsx";
                XLWorkbook Workbook = new XLWorkbook(fileNamemau);
                IXLWorksheet Worksheet = Workbook.Worksheet("BBGN");
                var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == BBGN_ID).FirstOrDefault();
                var Data = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == BBGN_ID).ToList();
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
                        Worksheet.Cell(row, icol).Value = ID_BBGN.ThoiGianXuLyBG;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        Worksheet.Cell(row, icol).Style.DateFormat.Format = "dd/MM/yyyy";


                        var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == ID_BBGN.ID_Kip).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_Kip.TenKip;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        if (ID_Kip.TenCa == "1")
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

                        var ID_VT = _context.Tbl_VatTu.Where(x => x.ID_VatTu == item.ID_VatTu).FirstOrDefault();

                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_VT.TenVatTu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_Lo = _context.Tbl_MaLo.Where(x => x.TenMaLo == item.MaLo).FirstOrDefault();
                        icol++;
                        if (ID_Lo != null)
                        {
                            Worksheet.Cell(row, icol).Value = ID_Lo.TenMaLo;
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        }
                        else
                        {
                            Worksheet.Cell(row, icol).Value = "";
                            Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;
                        }


                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_VT.DonViTinh;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KhoiLuong_BN;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 2);
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KL_QuyKho_BN;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_XBN = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BN).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_XBN.TenXuong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_BPBN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BN).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_BPBN.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;





                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KhoiLuong_BG;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = Math.Round(item.DoAm_W, 2);
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = item.KL_QuyKho_BG;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_XBG = _context.Tbl_Xuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BG).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_XBG.TenXuong;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        var ID_BPBG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ID_BBGN.ID_PhongBan_BG).FirstOrDefault();
                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_BPBG.TenPhongBan;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;

                        icol++;
                        Worksheet.Cell(row, icol).Value = item.GhiChu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                        icol++;
                        Worksheet.Cell(row, icol).Value = ID_BBGN.SoPhieu;
                        Worksheet.Cell(row, icol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        Worksheet.Cell(row, icol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        Worksheet.Cell(row, icol).Style.Alignment.WrapText = true;


                    }

                    Worksheet.Range("A7:T" + (row)).Style.Font.SetFontName("Times New Roman");
                    Worksheet.Range("A7:T" + (row)).Style.Font.SetFontSize(13);
                    Worksheet.Range("A7:T" + (row)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    Worksheet.Range("A7:T" + (row)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "BBGN - " + ID_BBGN.SoPhieu + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
                else
                {


                    Workbook.SaveAs(fileNamemaunew);
                    byte[] fileBytes = System.IO.File.ReadAllBytes(fileNamemaunew);
                    string fileName = "BBGN - " + ID_BBGN.SoPhieu + ".xlsx";
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index_Detai", "BM_11", new { id = BBGN_ID });
            }
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

    }
}
