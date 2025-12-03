using System.Security.Claims;
using Data_Product.DTO.BM_18_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using Data_Product.Services;
using Data_Product.Views.BM_18_XiHatLoCao;
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
                var query = _context.Tbl_BM_18_Phieu
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
                //// Lọc theo ngày
                //if (ngay.HasValue)
                //{
                //    query = query.Where(s => s.NgayTaoPhieu.Date == ngay.Value.Date);
                //}

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
                    bool existed = await _context.Tbl_BM_18_Phieu.AnyAsync(p =>
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

                    var header = new Tbl_BM_18_Phieu
                    {
                        MaPhieu = maPhieu,
                        NgayTaoPhieu = DateTime.Now,
                        ID_Locao = loId,
                        ID_Kip = model.ID_Kip,
                        ID_NguoiTao = taiKhoan.ID_TaiKhoan,
                        NgaySanXuat = model.NgaySanXuat,
                        ID_NguoiGiao = 0,
                        ID_NguoiNhan = 0
                    };

                    _context.Tbl_BM_18_Phieu.Add(header);
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

        [HttpGet("DetailPhieu")]
        public async Task<IActionResult> DetailPhieu(string maPhieu)
        {
            if (string.IsNullOrWhiteSpace(maPhieu))
                return BadRequest("Thiếu mã phiếu.");

            var phieu = await _context.Set<Tbl_BM_18_Phieu>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);

            if (phieu == null)
                return NotFound($"Không tìm thấy phiếu với mã: {maPhieu}");
            var kip = await _context.Tbl_Kip.FirstOrDefaultAsync(k => k.ID_Kip == phieu.ID_Kip);
            var chiTiet = await _context.Set<Tbl_BM_18_XiHatLoCao>()
                .AsNoTracking()
                .Where(x => x.MaPhieu == maPhieu)
                .OrderBy(x => x.ID)
                .ToListAsync();

            // Header fields for view
            ViewBag.MaPhieu = phieu.MaPhieu;
            ViewBag.NgaySanXuat = phieu.NgaySanXuat.ToString("yyyy-MM-dd");
            ViewBag.TenKip = phieu.ID_Kip; 
           // ViewBag.TenCa = phieu.ID_Kip;
            ViewBag.ID_Locao = phieu.ID_Locao;
            ViewBag.ID_Kip = phieu.ID_Kip;
            ViewBag.TenKip = kip?.TenKip;
            ViewBag.TenCa = kip?.TenCa;
            return View("DetailPhieu", chiTiet);
        }
        [HttpPost]
        public async Task<IActionResult> SaveChiTiet([FromBody] BM18Request req)
        {
            if (req == null || req.ChiTiet == null || req.ChiTiet.Count == 0)
                return BadRequest("Dữ liệu không hợp lệ!");

            foreach (var item in req.ChiTiet)
            {
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
                        Lo = item.Lo,
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
                        updateItem.Lo = item.Lo;
                        updateItem.HeSo = item.HeSo;

                        updateItem.KL_Gang_Giao = item.KL_Gang_Giao;
                        updateItem.KL_Xi_Giao = item.KL_Xi_Giao;
                        updateItem.KL_Gang_Nhan = item.KL_Gang_Nhan;
                        updateItem.KL_Xi_Nhan = item.KL_Xi_Nhan;
                        updateItem.GhiChu = item.GhiChu;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã lưu thành công!" });
        }

        [HttpPost]
        public IActionResult ResetPhieu([FromBody] ResetPhieuRequest request)
        {
            if (string.IsNullOrEmpty(request.MaPhieu))
                return BadRequest("Mã phiếu không hợp lệ");

            // Xóa chi tiết phiếu
            var chiTiet = _context.Tbl_BM_18_XiHatLoCao
                                  .Where(x => x.MaPhieu == request.MaPhieu);
            _context.Tbl_BM_18_XiHatLoCao.RemoveRange(chiTiet);
            _context.SaveChanges();

            return Ok(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> KLGangTrongCa(int ca, int idKip, DateTime ngayLuyenGang, int idLoCao)
        {
            // ===== Lấy danh sách thùng theo điều kiện mới =====
            var dsThung = await _context.Tbl_BM_16_GangLong
                .Where(t =>
                    t.G_Ca == ca &&
                    t.G_ID_Kip == idKip &&
                    t.NgayTao == ngayLuyenGang.Date &&
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
                    t.NgayTao == ngayLuyenGang.Date &&
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
                    t.NgayTao == ngayLuyenGang.Date &&
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

            decimal tongKLGang = tongKLDuc + tongKL_TheoMe;
            // ===== TRẢ DATA =====
            ViewBag.TongKL_TheoMe = tongTheoMe;
            ViewBag.KLDuc_TheoMe = klDucTheoMe;
            ViewBag.TongKLGang = tongKLGang;
            return View();
        }

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
    }
}
