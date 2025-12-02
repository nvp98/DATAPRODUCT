using System.Security.Claims;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.DTO.BM_18_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using Data_Product.Services;
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
        public async Task<IActionResult> Danhsachphieu(string maPhieu, DateTime? ngay, DateTime? ngaypg, string ca, string locao, int page = 1)
        {
            const int pageSize = 10;
            if (page < 1) page = 1;
            var loCaos = await _context.Tbl_LoCao.OrderBy(l => l.TenLoCao).ToListAsync();
            var loCaoList = await GetLoCaoList();
            ViewBag.LoCaoList = loCaoList;
            var data = new List<PhieuViewModel>();
            var pager = new Pager();
            if (loCaoList.Any())
            {
                //var query = _context.Tbl_BM_16_Phieu.OrderByDescending(p => p.NgayPhieuGang)
                var query = _context.Tbl_BM_18_Phieu
                    .OrderByDescending(p => p.NgayTaoPhieu.Date) // Ngày mới trước
            .Select(p => new PhieuViewModel
            {
                MaPhieu = p.MaPhieu,
                NgayTaoPhieu = p.NgayTaoPhieu,
                NgayPhieuGang = p.NgayPhieu,
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
                if (ngaypg.HasValue)
                {
                    query = query.Where(s => s.NgayPhieuGang.Date == ngaypg.Value.Date);
                }
                // Lọc theo ngày
                if (ngay.HasValue)
                {
                    query = query.Where(s => s.NgayTaoPhieu.Date == ngay.Value.Date);
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
            ViewBag.NgayPG = ngaypg?.ToString("dd-MM-yyyy") ?? "";
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { success = false, errors });
            }

            // Lấy tài khoản hiện hành
            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(tenTaiKhoan))
                return Unauthorized("Phiên đăng nhập không hợp lệ.");

            var taiKhoan = await _context.Tbl_TaiKhoan
                                         .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
            if (taiKhoan == null)
                return Unauthorized("Tài khoản không tồn tại.");

            int idNhanVienTao = taiKhoan.ID_TaiKhoan;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Kiểm tra trùng theo Lò cao/Kíp/Ngày
                bool phieuDaTonTai = await _context.Tbl_BM_18_Phieu.AnyAsync(p =>
                    p.ID_Locao == model.ID_LoCao &&
                    p.ID_Kip == model.ID_Kip &&
                    p.NgayPhieu != null &&
                    p.NgayPhieu.Date == model.NgayPhieu.Date
                );

                if (phieuDaTonTai)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Đã tồn tại phiếu cho lò cao này trong ngày được chọn."
                    });
                }

                // Lấy tên Ca/Kíp nếu cần ghép vào mã phiếu
                var caKip = await _context.Tbl_Kip
                    .Where(x => x.ID_Kip == model.ID_Kip)
                    .Select(x => new { x.TenCa, x.TenKip })
                    .FirstOrDefaultAsync();

                string maPhieu = GenerateMaPhieuBM18(model.ID_LoCao, caKip?.TenCa, caKip?.TenKip, model.NgayPhieu);

                var phieu = new Tbl_BM_18_Phieu
                {
                    // ID là INT IDENTITY (DB sinh ra)
                    MaPhieu = maPhieu,
                    NgayTaoPhieu = DateTime.Now,
                    ID_Locao = model.ID_LoCao,
                    ID_Kip = model.ID_Kip,
                    ID_NguoiTao = idNhanVienTao,
                    NgayPhieu = model.NgayPhieu
                };

                await _context.Tbl_BM_18_Phieu.AddAsync(phieu);
                await _context.SaveChangesAsync();

                await tx.CommitAsync();

                // Điều hướng tới trang chi tiết theo ID identity vừa sinh
                string url = Url.Action("DetailPhieu", "BM18_XiHatLoCao", new { id = phieu.ID });
                return Json(new { success = true, redirectUrl = url });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // Rule sinh mã phiếu BM18: XI-LC-L{Locao}-{Ca}{Kip}-{yyMMdd}
        private string GenerateMaPhieuBM18(int idLoCao, string? tenCa, string? tenKip, DateTime ngayPhieuGang)
        {
            string ca = tenCa ?? "";
            string kip = tenKip ?? "";
            return $"XHLC-L{idLoCao}-{ca}{kip}-{ngayPhieuGang:yyMMdd}";
        }
    }
}
