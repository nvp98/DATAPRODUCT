using ClosedXML.Excel;
using Data_Product.Common.Enums;
using Data_Product.DTO;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using Data_Product.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BM16_GangThoi_PKHController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IChiaGangService _chiaGangService;

        public BM16_GangThoi_PKHController(DataContext _context, ICompositeViewEngine viewEngine, IChiaGangService chiaGangService)
        {
            this._context = _context;
            this._chiaGangService = chiaGangService;
            _viewEngine = viewEngine;
        }
        public async Task<IActionResult> Index()
        {
            // Lò cao
            var loCaoList = await _context.Tbl_LoCao.ToListAsync();
            ViewBag.LoCaoList = loCaoList;

            // Lò Thổi
            var loThoiList = await _context.Tbl_LoThoi.ToListAsync();
            ViewBag.LoThoiList = loThoiList;

            var PhanTram = await _context.Tbl_BM_16_PhanTramDuc.Where(x => x.ID == 1).FirstOrDefaultAsync();
            ViewBag.PhanTram = PhanTram.PhanTram;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchDto dto)
        {
            var data = await SearchByPayload(dto);
            return Ok(data);
        }


        [HttpPost]
        public async Task<IActionResult> CheckChotThung([FromBody] List<ChotThungDto> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return BadRequest("Danh sách ID trống.");

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
            if (TaiKhoan == null) return Unauthorized();

            var Ids = selectedIds.Select(x => x.id).ToList();

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                .Where(x => Ids.Contains(x.ID) && x.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                .ToListAsync();

            if (thungs.Count == 0)
                return NotFound("Không tìm thấy thùng nào.");

            var invalidThungsGang = thungs
                .Where(x =>
                    x.KL_XeGoong == null ||
                    x.G_KLThungVaGang == null ||
                    x.G_KLThungChua == null ||
                    x.G_KLGangLong == null ||
                    x.ChuyenDen == null ||
                    x.Gio_NM == null 
                    // || x.T_KLThungVaGang == null ||
                    //x.T_KLThungChua == null ||
                    //x.T_KLGangLong == null ||
                    //x.ID_TTG == null
                )
                .Select(x => new { x.ID, x.MaThungGang })
                .ToList();

            if (invalidThungsGang.Any())
            {
                return Ok(new
                {
                    isValid = false,
                    invalidThungsGang,
                    invalidThungsTTG = new List<object>() 
                });
            }
            var allowedDestinations = new[] { "DUC1", "DUC2" };
            var thungsCanCheckTTG = thungs
                .Where(x => !allowedDestinations.Contains(x.ChuyenDen) && x.KLGangChia == null)
                .ToList();

            var invalidThungsTTG = new List<object>();

            if (thungsCanCheckTTG.Any())
            {
                var idTTGs = thungsCanCheckTTG
                    .Where(x => x.ID_TTG.HasValue)
                    .Select(x => x.ID_TTG.Value)
                    .Distinct()
                    .ToList();

                var allTTGs = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(x => idTTGs.Contains(x.ID))
                    .ToListAsync();

                var maThungTGs = allTTGs.Select(x => x.MaThungTG).Distinct().ToList();

                var relatedTTGs = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(x => maThungTGs.Contains(x.MaThungTG))
                    .ToListAsync();

                invalidThungsTTG = relatedTTGs
                    .Where(item =>
                        item.KLThungVaGang_Thoi == null ||
                        item.KLThung_Thoi == null ||
                        item.KL_phe == null ||
                        item.KLGang_Thoi == null ||
                        item.ID_MeThoi == null ||
                        item.GioChonMe == null
                    )
                    .Select(x => new { x.MaThungTG })
                    .ToList<object>();
            }

            if (invalidThungsTTG.Any())
            {
                return Ok(new
                {
                    isValid = false,
                    invalidThungsGang = new List<object>(), 
                    invalidThungsTTG
                });
            }

            return Ok(new { isValid = true });
        }


        [HttpPost]
        public async Task<IActionResult> LuuPhanTram([FromBody] int percent)
        {
            if (percent == null) return BadRequest("Không có dữ liệu.");
            try
            {
                var phantram = await _context.Tbl_BM_16_PhanTramDuc.Where(x => x.ID == 1).FirstOrDefaultAsync();
                if(phantram == null) return BadRequest("Không có dữ liệu.");

                phantram.PhanTram = percent;

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> ChotThung([FromBody] List<ChotThungDto> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return BadRequest("Danh sách ID trống.");

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            if (TaiKhoan == null) return Unauthorized();

            var Ids = selectedIds
                                .Select(x => x.id)
                                .ToList();

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                                       .Where(x => Ids.Contains(x.ID) && x.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                                       .ToListAsync();
            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");

            var idTTGs = thungs.Select(x => x.ID_TTG).Distinct().ToList();

            // Lấy tất cả thùng trung gian liên quan
            var allTTGs = await _context.Tbl_BM_16_ThungTrungGian
                .Where(x => idTTGs.Contains(x.ID))
                .ToListAsync();

            var maThungTGs = allTTGs.Select(x => x.MaThungTG).Distinct().ToList();

            // Lấy toàn bộ các thùng trung gian liên quan theo MaThungTG (gốc và bản sao)
            var relatedTTGs = await _context.Tbl_BM_16_ThungTrungGian
                .Where(x => maThungTGs.Contains(x.MaThungTG))
                .ToListAsync();

            var idMeThoiList = relatedTTGs
                .Where(x => x.ID_MeThoi.HasValue)
                .Select(x => x.ID_MeThoi.Value)
                .Distinct()
                .ToList();

            // Lấy toàn bộ mẻ thổi cần cập nhật
            var meThoiList = await _context.Tbl_MeThoi
                .Where(x => idMeThoiList.Contains(x.ID))
                .ToListAsync();

            // Cập nhật trạng thái mẻ thổi
            foreach (var me in meThoiList)
            {
                me.ID_TrangThai = (int)TinhTrang.DaChot;
            }

            foreach (var t in thungs)
            {

                t.ID_TrangThai = (int)TinhTrang.DaChot;
                t.ID_NguoiChot = TaiKhoan.ID_TaiKhoan;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> HuyChotThung([FromBody] List<ChotThungDto> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return BadRequest("Danh sách ID trống.");

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            if (TaiKhoan == null) return Unauthorized();

            var Ids = selectedIds
                                .Select(x => x.id)
                                .ToList();

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                                       .Where(x => Ids.Contains(x.ID) && x.ID_TrangThai == (int)TinhTrang.DaChot)
                                       .ToListAsync();
            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");

            var idTTGs = thungs.Select(x => x.ID_TTG).Distinct().ToList();

            // Lấy tất cả thùng trung gian liên quan
            var allTTGs = await _context.Tbl_BM_16_ThungTrungGian
                .Where(x => idTTGs.Contains(x.ID))
                .ToListAsync();

            var maThungTGs = allTTGs.Select(x => x.MaThungTG).Distinct().ToList();

            // Lấy toàn bộ các thùng trung gian liên quan theo MaThungTG (gốc và bản sao)
            var relatedTTGs = await _context.Tbl_BM_16_ThungTrungGian
                .Where(x => maThungTGs.Contains(x.MaThungTG))
                .ToListAsync();

            var idMeThoiList = relatedTTGs
                .Where(x => x.ID_MeThoi.HasValue)
                .Select(x => x.ID_MeThoi.Value)
                .Distinct()
                .ToList();

            // Lấy toàn bộ mẻ thổi cần cập nhật
            var meThoiList = await _context.Tbl_MeThoi
                .Where(x => idMeThoiList.Contains(x.ID))
                .ToListAsync();

            // Cập nhật trạng thái mẻ thổi
            foreach (var me in meThoiList)
            {
                me.ID_TrangThai = (int)TinhTrang.ChoXuLy;
            }

            foreach (var t in thungs)
            {

                t.ID_TrangThai = (int)TinhTrang.ChoXuLy;
                t.ID_NguoiChot = null;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GetDetailChiaGang([FromBody] string maThungThep)
        {
            try
            {
                var result = await _chiaGangService.GetDetailChiaGangAsync(maThungThep);
                return Ok(result);
            } catch(Exception ex){ 
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> GetDetailChiaGangCR([FromBody] DetailChiaGangCRDto dto)
        {
            try
            {
                var result = await _chiaGangService.GetDetailChiaGangCRAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

        //private async Task<decimal> SumLatestCRWithFallbackAsync(
        //    IQueryable<Tbl_BM_16_GangLong> gangQueryScope,
        //    List<int> ttgTargetIds)
        //{
        //    if (ttgTargetIds == null || ttgTargetIds.Count == 0)
        //        return 0m;

        //    // 1) Lấy thông tin gang trong scope
        //    var gangInfos = await gangQueryScope
        //        .Select(x => new
        //        {
        //            x.ID,
        //            x.MaThungGang,
        //            x.T_ID_TrangThai,
        //            x.G_KLGangLong
        //        })
        //        .ToListAsync();

        //    if (gangInfos.Count == 0) return 0m;

        //    var gangIds = gangInfos.Select(x => x.ID).ToList();
        //    var gklById = gangInfos.ToDictionary(k => k.ID, v => v.G_KLGangLong ?? 0m);
        //    var statusById = gangInfos.ToDictionary(k => k.ID, v => v.T_ID_TrangThai);
        //    var maGangById = gangInfos.ToDictionary(k => k.ID, v => v.MaThungGang ?? string.Empty);

        //    // 2) Đếm số lần nhận theo MaThungGang -> IsChiaCR
        //    var maGangs = gangInfos.Select(x => x.MaThungGang)
        //                           .Where(s => !string.IsNullOrWhiteSpace(s))
        //                           .Distinct(StringComparer.OrdinalIgnoreCase)
        //                           .ToList();

        //    var receiveCounts = await _context.Tbl_BM_16_TaiKhoan_Thung
        //        .Where(t => maGangs.Contains(t.MaThungGang))
        //        .GroupBy(t => t.MaThungGang)
        //        .Select(g => new { MaThungGang = g.Key, Count = g.Count() })
        //        .ToListAsync();

        //    var receiveMap = receiveCounts.ToDictionary(x => x.MaThungGang, x => x.Count, StringComparer.OrdinalIgnoreCase);

        //    bool IsChiaCR(string maThungGang)
        //        => !string.IsNullOrWhiteSpace(maThungGang)
        //           && receiveMap.TryGetValue(maThungGang, out var c)
        //           && c >= 2;

        //    // 3) Lấy phân bổ MỚI NHẤT theo từng cặp + kèm IsSaiChuyenDen để lọc "Sai"
        //    var latestKeysQuery =
        //        from pb in _context.Tbl_BM_16_PhanBoGangCR
        //        where gangIds.Contains(pb.ID_GangLong)
        //           && ttgTargetIds.Contains(pb.ID_TTG_Target)
        //        group pb by new { pb.ID_GangLong, pb.ID_TTG_Target } into g
        //        select new { g.Key.ID_GangLong, g.Key.ID_TTG_Target, MaxId = g.Max(x => x.ID) };

        //    var latestAllocRows = await (
        //        from pb in _context.Tbl_BM_16_PhanBoGangCR.AsNoTracking()
        //        join k in latestKeysQuery
        //          on new { pb.ID_GangLong, pb.ID_TTG_Target, pb.ID }
        //          equals new { k.ID_GangLong, k.ID_TTG_Target, ID = k.MaxId }
        //        select new
        //        {
        //            pb.ID_GangLong,
        //            pb.KL_PhanBo_CR,
        //            pb.IsSaiChuyenDen
        //        })
        //        .ToListAsync();

        //    // 4) Tổng CR hợp lệ (không "Sai") theo từng Gang
        //    var validCrByGang = latestAllocRows
        //        .Where(x => !(x.IsSaiChuyenDen ?? false))                 // bỏ các dòng "Sai"
        //        .GroupBy(x => x.ID_GangLong)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Sum(x => (x.KL_PhanBo_CR ?? 0m) > 0m ? (x.KL_PhanBo_CR ?? 0m) : 0m)
        //        );

        //    // 5) Cộng tổng theo đúng logic Excel
        //    decimal total = 0m;
        //    foreach (var gid in gangIds)
        //    {
        //        var ma = maGangById[gid];
        //        var daNhan = statusById.TryGetValue(gid, out var st) && st == 4;

        //        if (IsChiaCR(ma))
        //        {
        //            // Ưu tiên CR hợp lệ > 0; nếu không có thì fallback G_KLGangLong khi đã nhận
        //            if (validCrByGang.TryGetValue(gid, out var crSum) && crSum > 0m)
        //                total += crSum;
        //            else if (daNhan)
        //                total += gklById[gid];
        //        }
        //        else
        //        {
        //            // Không chia CR -> chỉ lấy G_KLGangLong khi đã nhận
        //            if (daNhan)
        //                total += gklById[gid];
        //        }
        //    }

        //    return total;
        //}



        //private async Task<PageResultViewModel<List<Tbl_BM_16_GangLong>>> SearchByPayload(SearchDto dto)
        //{
        //    var query = _context.Tbl_BM_16_GangLong
        //        .OrderByDescending(x => x.NgayTao)
        //        .ThenBy(x => x.ID_Locao)
        //        .ThenByDescending(x => x.G_Ca)
        //        .ThenByDescending(x => x.MaThungGang)
        //        .ThenBy(x => x.MaThungThep)
        //        .AsQueryable();

        //    decimal sumKLGang = 0;
        //    decimal sumKLGangLongThep = 0;
        //    decimal sumKLGangNhan = 0;
        //    decimal sumKLPhe = 0;
        //    decimal sumKLVaoLoThoi = 0;
        //    decimal sumKLGangChia = 0;
        //    decimal sumKLGangChiaCR = 0;

        //    if (dto.ID_HRC.HasValue)
        //    {
        //        query = query.Where(x =>
        //            _context.Tbl_TaiKhoan.Any(tk => tk.ID_TaiKhoan == x.T_ID_NguoiNhan && tk.ID_PhongBan == dto.ID_HRC));
        //    }

        //    if (dto.ID_LoCao.HasValue)
        //        query = query.Where(x => x.ID_Locao == dto.ID_LoCao.Value);

        //    if (dto.ID_LoThoi.HasValue)
        //        query = query.Where(x => x.ID_LoThoi == dto.ID_LoThoi.Value);

        //    if (dto.Ca_LT.HasValue)
        //        query = query.Where(x => x.T_Ca == dto.Ca_LT.Value);

        //    if (dto.Ca_LG.HasValue)
        //        query = query.Where(x => x.G_Ca == dto.Ca_LG.Value);

        //    if (!string.IsNullOrEmpty(dto.ID_Kip_LT))
        //    {
        //        query = query.Where(thung =>
        //            thung.T_ID_Kip != null &&
        //            _context.Tbl_Kip.Where(k => k.TenKip == dto.ID_Kip_LT)
        //                .Select(k => k.ID_Kip)
        //                .Contains(thung.T_ID_Kip.Value));
        //    }

        //    if (!string.IsNullOrEmpty(dto.ID_Kip_LG))
        //    {
        //        query = query.Where(thung =>
        //            thung.G_ID_Kip != null &&
        //            _context.Tbl_Kip.Where(k => k.TenKip == dto.ID_Kip_LG)
        //                .Select(k => k.ID_Kip)
        //                .Contains(thung.G_ID_Kip.Value));
        //    }

        //    if (!string.IsNullOrEmpty(dto.ChuyenDen))
        //        query = query.Where(x => x.ChuyenDen.Contains(dto.ChuyenDen));

        //    if (!string.IsNullOrEmpty(dto.ThungSo))
        //        query = query.Where(x => x.BKMIS_ThungSo.Contains(dto.ThungSo));

        //    if (dto.ID_TinhTrang.HasValue)
        //        query = query.Where(x => x.ID_TrangThai == dto.ID_TinhTrang.Value);

        //    if (dto.ID_TinhTrang_LT.HasValue)
        //        query = query.Where(x => x.T_ID_TrangThai == dto.ID_TinhTrang_LT.Value);

        //    if (dto.ID_TinhTrang_LG.HasValue)
        //        query = query.Where(x => x.G_ID_TrangThai == dto.ID_TinhTrang_LG.Value);

        //    if (!string.IsNullOrEmpty(dto.MaThungGang))
        //        query = query.Where(x => x.MaThungGang.Contains(dto.MaThungGang));

        //    if (!string.IsNullOrEmpty(dto.MaThungThep))
        //        query = query.Where(x => x.MaThungThep.Contains(dto.MaThungThep));

        //    if (!string.IsNullOrEmpty(dto.MaMeThoi))
        //    {
        //        var idTTGList = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(x => x.ID_MeThoi.HasValue &&
        //                        _context.Tbl_MeThoi
        //                            .Where(m => m.MaMeThoi.Contains(dto.MaMeThoi))
        //                            .Select(m => m.ID)
        //                            .Contains(x.ID_MeThoi.Value))
        //            .Select(x => x.ID)
        //            .ToListAsync();

        //        query = query.Where(x => x.ID_TTG.HasValue && idTTGList.Contains(x.ID_TTG.Value));
        //    }

        //    if (dto.IsChiaGang.HasValue)
        //        query = dto.IsChiaGang == true
        //            ? query.Where(x => x.KLGangChia.HasValue)
        //            : query.Where(x => !x.KLGangChia.HasValue);

        //    if (dto.TuNgay_LT.HasValue && dto.DenNgay_LT.HasValue)
        //    {
        //        var tuNgay = dto.TuNgay_LT.Value.Date;
        //        var denNgay = dto.DenNgay_LT.Value.Date.AddDays(1);
        //        query = query.Where(x => x.NgayLuyenThep >= tuNgay && x.NgayLuyenThep < denNgay);

        //        sumKLGang = query.Where(x => x.T_copy != true).Sum(x => x.G_KLGangLong ?? 0);
        //        sumKLGangLongThep = query.Sum(x => x.T_KLGangLong ?? 0);
        //        sumKLGangChia = query.Sum(x => (x.KLGangChia ?? x.T_KLGangLong) ?? 0);

        //        var maThungTGList = (
        //            from a in query
        //            join ttgRoot in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttgRoot.ID
        //            select ttgRoot.MaThungTG
        //        ).Distinct().ToList();

        //        var relatedThung = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(x => maThungTGList.Contains(x.MaThungTG))
        //            .ToListAsync();

        //        sumKLGangNhan = relatedThung.Sum(x => x.Tong_KLGangNhan ?? 0);
        //        sumKLPhe = relatedThung.Sum(x => x.KL_phe ?? 0);
        //        sumKLVaoLoThoi = relatedThung.Sum(x => x.KLGang_Thoi ?? 0);
        //        var ttgTargetIds_LT = relatedThung.Select(x => x.ID).ToList();

        //        sumKLGangChiaCR = await SumLatestCRWithFallbackAsync(query, ttgTargetIds_LT);
        //    }

        //    if (dto.TuNgay_LG.HasValue && dto.DenNgay_LG.HasValue)
        //    {
        //        var tuNgay = dto.TuNgay_LG.Value.Date;
        //        var denNgay = dto.DenNgay_LG.Value.Date.AddDays(1);
        //        query = query.Where(x => x.NgayTao >= tuNgay && x.NgayTao < denNgay);

        //        sumKLGang = query.Where(x => x.T_copy != true).Sum(x => x.G_KLGangLong ?? 0);
        //        sumKLGangLongThep = query.Sum(x => x.T_KLGangLong ?? 0);
        //        sumKLGangChia = query.Sum(x => (x.KLGangChia ?? x.T_KLGangLong) ?? 0);

        //        var maThungTGList = (
        //            from a in query
        //            join ttgRoot in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttgRoot.ID
        //            select ttgRoot.MaThungTG
        //        ).Distinct().ToList();

        //        var relatedThung = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(x => maThungTGList.Contains(x.MaThungTG))
        //            .ToListAsync();

        //        sumKLGangNhan = relatedThung.Sum(x => x.Tong_KLGangNhan ?? 0);
        //        sumKLPhe = relatedThung.Sum(x => x.KL_phe ?? 0);
        //        sumKLVaoLoThoi = relatedThung.Sum(x => x.KLGang_Thoi ?? 0);

        //        //var gangIdsFiltered_LG = await query.Select(x => x.ID).ToListAsync();
        //        //var ttgTargetIds_LG = relatedThung.Select(x => x.ID).ToList();

        //        //sumKLGangChiaCR = await SumLatestCRAsync(gangIdsFiltered_LG, ttgTargetIds_LG);
        //        var ttgTargetIds_LG = relatedThung.Select(x => x.ID).ToList();

        //        sumKLGangChiaCR = await SumLatestCRWithFallbackAsync(query, ttgTargetIds_LG);
        //    }

        //    // Tổng số bản ghi
        //    var totalRecords = await query.CountAsync();

        //    // Paging (nếu có)
        //    if (dto.PageNumber.HasValue && dto.PageSize.HasValue)
        //    {
        //        int page = dto.PageNumber.Value;
        //        int pageSize = dto.PageSize.Value;
        //        query = query.Skip((page - 1) * pageSize).Take(pageSize);
        //    }

        //    // Truy vấn dữ liệu hiển thị
        //    var gocData = await (from a in query
        //                         join trangThai in _context.Tbl_BM_16_TrangThai on a.ID_TrangThai equals trangThai.ID into g_tt
        //                         from trangThai in g_tt.DefaultIfEmpty()

        //                         join trangThaiLG in _context.Tbl_BM_16_TrangThai on a.G_ID_TrangThai equals trangThaiLG.ID into g_TrangThai
        //                         from trangThaiLG in g_TrangThai.DefaultIfEmpty()

        //                         join trangThaiLT in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThaiLT.ID into t_TrangThai
        //                         from trangThaiLT in t_TrangThai.DefaultIfEmpty()

        //                         join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID into g_lc
        //                         from loCao in g_lc.DefaultIfEmpty()

        //                         join kipG in _context.Tbl_Kip on a.G_ID_Kip equals kipG.ID_Kip into g_kipG
        //                         from kipG in g_kipG.DefaultIfEmpty()

        //                         join kipT in _context.Tbl_Kip on a.T_ID_Kip equals kipT.ID_Kip into g_kipT
        //                         from kipT in g_kipT.DefaultIfEmpty()

        //                         join thungUser in _context.Tbl_BM_16_TaiKhoan_Thung on a.MaThungThep equals thungUser.MaThungThep into g_thungUser
        //                         from thungUser in g_thungUser.DefaultIfEmpty()

        //                         join user in _context.Tbl_TaiKhoan on thungUser.ID_taiKhoan equals user.ID_TaiKhoan into g_user
        //                         from user in g_user.DefaultIfEmpty()

        //                         join phongban in _context.Tbl_PhongBan on user.ID_PhongBan equals phongban.ID_PhongBan into g_phongban
        //                         from phongban in g_phongban.DefaultIfEmpty()

        //                         join pkh_user in _context.Tbl_TaiKhoan on a.ID_NguoiChot equals pkh_user.ID_TaiKhoan into tk_user
        //                         from pkh_user in tk_user.DefaultIfEmpty()

        //                         join ttg in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttg.ID into t_ttg
        //                         from ttg in t_ttg.DefaultIfEmpty()

        //                         join methoi in _context.Tbl_MeThoi on ttg.ID_MeThoi equals methoi.ID into g_mt
        //                         from methoi in g_mt.DefaultIfEmpty()

        //                         select new Tbl_BM_16_GangLong
        //                         {
        //                             ID = a.ID,
        //                             NgayTao = a.NgayTao,
        //                             NgayLuyenGang = a.NgayLuyenGang,
        //                             G_Ca = a.G_Ca,
        //                             G_TenKip = kipG != null ? kipG.TenKip : null,
        //                             MaThungGang = a.MaThungGang,
        //                             ID_Locao = a.ID_Locao,
        //                             BKMIS_SoMe = a.BKMIS_SoMe,
        //                             BKMIS_ThungSo = a.BKMIS_ThungSo,
        //                             BKMIS_Gio = a.BKMIS_Gio,
        //                             BKMIS_PhanLoai = a.BKMIS_PhanLoai,
        //                             KL_XeGoong = a.KL_XeGoong,
        //                             G_KLThungChua = a.G_KLThungChua,
        //                             G_KLThungVaGang = a.G_KLThungVaGang,
        //                             G_KLGangLong = a.G_KLGangLong,
        //                             ChuyenDen = a.ChuyenDen,
        //                             Gio_NM = a.Gio_NM,
        //                             G_ID_TrangThai = a.G_ID_TrangThai,
        //                             NgayLuyenThep = a.NgayLuyenThep,
        //                             T_Ca = a.T_Ca,
        //                             T_TenKip = kipT != null ? kipT.TenKip : null,
        //                             MaThungThep = a.MaThungThep,
        //                             KR = a.KR,
        //                             T_KLThungVaGang = a.T_KLThungVaGang,
        //                             T_KLThungChua = a.T_KLThungChua,
        //                             T_KLGangLong = a.T_KLGangLong,
        //                             T_ID_TrangThai = a.T_ID_TrangThai,
        //                             ID_TrangThai = a.ID_TrangThai,
        //                             TenLoCao = loCao.TenLoCao,
        //                             T_KL_phe = a.T_KL_phe,
        //                             TrangThai = trangThai.TenTrangThai,
        //                             TrangThaiLG = trangThaiLG.TenTrangThai,
        //                             TrangThaiLT = trangThaiLT.TenTrangThai,
        //                             T_copy = a.T_copy,
        //                             KLGangChia = a.KLGangChia,
        //                             ID_NguoiChot = a.ID_NguoiChot,
        //                             HoTenNguoiChot = pkh_user.HoVaTen,

        //                             HoVaTen = user.HoVaTen,
        //                             TenPhongBan = phongban.TenNgan,

        //                             ID_TTG = a.ID_TTG,
        //                             MaThungTG = ttg != null ? ttg.MaThungTG : null,
        //                             ID_MeThoi = ttg != null ? ttg.ID_MeThoi : null,
        //                             IsCopy = ttg != null ? ttg.IsCopy : (bool?)null,
        //                             MaMeThoi = methoi != null ? methoi.MaMeThoi : null,
        //                             ID_LoThoi = ttg != null ? ttg.ID_LoThoi : (int?)null,
        //                             SoThungTG = ttg != null ? ttg.SoThungTG : null,
        //                             KLThungVaGang_Thoi = ttg != null ? ttg.KLThungVaGang_Thoi : null,
        //                             KLThung_Thoi = ttg != null ? ttg.KLThung_Thoi : null,
        //                             KLGang_Thoi = ttg != null ? ttg.KLGang_Thoi : null,
        //                             KL_phe = ttg != null ? ttg.KL_phe : null,
        //                             Tong_KLGangNhan = ttg != null ? ttg.Tong_KLGangNhan : null,
        //                             GioChonMe = ttg != null ? ttg.GioChonMe : null
        //                         }).ToListAsync();

        //    // Lấy danh sách TTG copy để nhân bản hiển thị
        //    var maTTGs = gocData.Where(x => !string.IsNullOrEmpty(x.MaThungTG))
        //                        .Select(x => x.MaThungTG)
        //                        .Distinct()
        //                        .ToList();

        //    var thungTG_Copies = await _context.Tbl_BM_16_ThungTrungGian
        //        .Where(x => x.IsCopy == true && maTTGs.Contains(x.MaThungTG))
        //        .ToListAsync();

        //    // Final result (gốc + copy TTG)
        //    var finalData = new List<Tbl_BM_16_GangLong>();

        //    foreach (var item in gocData)
        //    {
        //        // Thêm dòng gốc
        //        finalData.Add(item);

        //        // Bỏ qua nếu không có TTG gốc hoặc bản ghi gốc đã là copy
        //        if (!item.ID_TTG.HasValue || item.IsCopy == true) continue;

        //        // Lấy các bản TTG copy tương ứng
        //        var copies = thungTG_Copies.Where(x => x.MaThungTG == item.MaThungTG).ToList();

        //        foreach (var copy in copies)
        //        {
        //            var clone = CloneGangLong(item); // giả sử bạn đã có helper này

        //            var methoi = await _context.Tbl_MeThoi.FirstOrDefaultAsync(x => x.ID == copy.ID_MeThoi);

        //            clone.ID_TTG = copy.ID;
        //            clone.IsCopy = true;
        //            clone.SoThungTG = copy.SoThungTG;
        //            clone.KLThungVaGang_Thoi = copy.KLThungVaGang_Thoi;
        //            clone.KLThung_Thoi = copy.KLThung_Thoi;
        //            clone.KLGang_Thoi = copy.KLGang_Thoi;
        //            clone.KL_phe = copy.KL_phe;
        //            clone.ID_MeThoi = methoi?.ID;
        //            clone.MaMeThoi = methoi?.MaMeThoi;
        //            clone.GioChonMe = copy.GioChonMe;

        //            finalData.Add(clone);
        //        }
        //    }

        //    // ======= GÁN KL_GangChiaCR từ bảng phân bổ (KHÔNG ảnh hưởng logic khác) =======
        //    var gangIdsAll = finalData.Select(x => x.ID).Distinct().ToList();
        //    var ttgIdsAll = finalData.Where(x => x.ID_TTG.HasValue)
        //                             .Select(x => x.ID_TTG!.Value)
        //                             .Distinct()
        //                             .ToList();

        //    if (gangIdsAll.Count > 0 && ttgIdsAll.Count > 0)
        //    {

        //        // Subquery: lấy Max(ID) cho mỗi cặp
        //        var latestKeysQuery =
        //            from pb in _context.Tbl_BM_16_PhanBoGangCR
        //            where gangIdsAll.Contains(pb.ID_GangLong)
        //               && ttgIdsAll.Contains(pb.ID_TTG_Target)
        //            group pb by new { pb.ID_GangLong, pb.ID_TTG_Target } into g
        //            select new { g.Key.ID_GangLong, g.Key.ID_TTG_Target, MaxId = g.Max(x => x.ID) };

        //        // Join để lấy đúng dòng có ID = MaxId
        //        var allocRows = await (
        //            from pb in _context.Tbl_BM_16_PhanBoGangCR
        //            join k in latestKeysQuery
        //              on new { pb.ID_GangLong, pb.ID_TTG_Target, pb.ID }
        //              equals new { k.ID_GangLong, k.ID_TTG_Target, ID = k.MaxId }
        //            select new
        //            {
        //                pb.ID_GangLong,
        //                pb.ID_TTG_Target,
        //                pb.KL_PhanBo_CR,
        //                pb.IsSaiChuyenDen
        //            })
        //            .AsNoTracking()
        //            .ToListAsync();

        //        // Build dict như cũ
        //        var allocCrDict = allocRows.ToDictionary(
        //            k => (k.ID_GangLong, k.ID_TTG_Target),
        //            v => v.KL_PhanBo_CR
        //        );
        //        var allocErrDict = allocRows.ToDictionary(
        //            k => (k.ID_GangLong, k.ID_TTG_Target),
        //            v => v.IsSaiChuyenDen
        //        );

        //        foreach (var item in finalData)
        //        {
        //            if (item.ID_TTG.HasValue)
        //            {
        //                var key = (item.ID, item.ID_TTG.Value);

        //                if (allocCrDict.TryGetValue(key, out var kl))
        //                    item.KL_GangChiaCR = kl;
        //                else
        //                    item.KL_GangChiaCR = 0m;

        //                if (allocErrDict.TryGetValue(key, out var errFlag))
        //                    item.IsSaiChuyenDen = errFlag;
        //                else
        //                    item.IsSaiChuyenDen = false; // mặc định hợp lệ nếu không có dòng phân bổ
        //            }
        //            else
        //            {
        //                item.KL_GangChiaCR = 0m;
        //                item.IsSaiChuyenDen = null; // không thuộc TTG → không áp cờ
        //            }
        //        }
        //    }
        //    else
        //    {
        //        foreach (var item in finalData)
        //        {
        //            item.KL_GangChiaCR = 0m;
        //            item.IsSaiChuyenDen = item.ID_TTG.HasValue ? false : (bool?)null;
        //        }
        //    }
        //    // ============================================================================
        //    // ======= ĐÁNH DẤU IsChiaCR (NotMapped) =======
        //    var maThungGangSet = finalData
        //        .Where(x => !string.IsNullOrEmpty(x.MaThungGang))
        //        .Select(x => x.MaThungGang!)
        //        .Distinct(StringComparer.OrdinalIgnoreCase)
        //        .ToList();

        //    if (maThungGangSet.Count > 0)
        //    {
        //        var receiveCounts = await _context.Tbl_BM_16_TaiKhoan_Thung
        //            .Where(t => maThungGangSet.Contains(t.MaThungGang))
        //            .GroupBy(t => t.MaThungGang)
        //            .Select(g => new { MaThungGang = g.Key, Count = g.Count() })
        //            .ToListAsync();

        //        var receiveMap = receiveCounts.ToDictionary(x => x.MaThungGang, x => x.Count, StringComparer.OrdinalIgnoreCase);

        //        foreach (var item in finalData)
        //        {
        //            if (!string.IsNullOrEmpty(item.MaThungGang)
        //                && receiveMap.TryGetValue(item.MaThungGang!, out var cnt)
        //                && cnt >= 2)
        //            {
        //                item.IsChiaCR = true;
        //            }
        //            else
        //            {
        //                item.IsChiaCR = false;
        //            }
        //        }
        //    }
        //    // Group như cũ để hiển thị
        //    var groupedData = finalData
        //        .GroupBy(x => x.ID_TTG.HasValue ? x.ID_TTG.Value.ToString() : $"null_{x.ID}")
        //        .Select(g => g.ToList())
        //        .ToList();

        //    return new PageResultViewModel<List<Tbl_BM_16_GangLong>>
        //    {
        //        TotalRecords = totalRecords,
        //        SumKLGang = sumKLGang,
        //        SumKLGangLongThep = sumKLGangLongThep,
        //        SumKLGangNhan = sumKLGangNhan,
        //        SumKLPhe = sumKLPhe,
        //        SumKLVaoLoThoi = sumKLVaoLoThoi,
        //        SumKLGangChia = sumKLGangChia,
        //        SumKLGangChiaCR = sumKLGangChiaCR,
        //        Data = groupedData
        //    };
        //}
        // ===== Helper: Tính tổng CR theo quy tắc (mới nhất theo cặp, bỏ 'Sai', fallback G_KLGangLong) =====


        // ===== SearchByPayload: TÍNH TỔNG TRƯỚC PHÂN TRANG + GÁN KL_GangChiaCR_Display =====
        // ===== Helper: tính tổng CR theo đúng logic Excel, trên phạm vi đã lọc (trước paging) =====
        private async Task<decimal> SumLatestCRWithFallbackAsync(
            IQueryable<Tbl_BM_16_GangLong> gangQueryScope,
            List<int> ttgTargetIds)
        {
            if (ttgTargetIds == null || ttgTargetIds.Count == 0)
                return 0m;

            // 1) Lấy thông tin gang trong scope (đã Where đủ điều kiện ở ngoài)
            var gangInfos = await gangQueryScope
                .Select(x => new
                {
                    x.ID,
                    x.MaThungGang,
                    x.T_ID_TrangThai,
                    x.G_KLGangLong
                })
                .ToListAsync();

            if (gangInfos.Count == 0) return 0m;

            var gangIds = gangInfos.Select(x => x.ID).ToList();
            var gklById = gangInfos.ToDictionary(k => k.ID, v => v.G_KLGangLong ?? 0m);
            var statusById = gangInfos.ToDictionary(k => k.ID, v => v.T_ID_TrangThai);
            var maGangById = gangInfos.ToDictionary(k => k.ID, v => v.MaThungGang ?? string.Empty);

            // 2) Đếm số lần nhận theo MaThungGang -> IsChiaCR (>= 2 lần)
            var maGangs = gangInfos.Select(x => x.MaThungGang)
                                   .Where(s => !string.IsNullOrWhiteSpace(s))
                                   .Distinct(StringComparer.OrdinalIgnoreCase)
                                   .ToList();

            var receiveCounts = await _context.Tbl_BM_16_TaiKhoan_Thung
                .Where(t => maGangs.Contains(t.MaThungGang))
                .GroupBy(t => t.MaThungGang)
                .Select(g => new { MaThungGang = g.Key, Count = g.Count() })
                .ToListAsync();

            var receiveMap = receiveCounts.ToDictionary(
                    x => x.MaThungGang!,
                    x => x.Count,
                    StringComparer.OrdinalIgnoreCase
                );

            bool IsChiaCR(string maThungGang)
                => !string.IsNullOrWhiteSpace(maThungGang)
                   && receiveMap.TryGetValue(maThungGang, out var c)
                   && c >= 2;

            // 3) Lấy phân bổ MỚI NHẤT theo từng cặp + kèm IsSaiChuyenDen để bỏ "Sai"
            var latestKeysQuery =
                from pb in _context.Tbl_BM_16_PhanBoGangCR
                where gangIds.Contains(pb.ID_GangLong)
                   && ttgTargetIds.Contains(pb.ID_TTG_Target)
                group pb by new { pb.ID_GangLong, pb.ID_TTG_Target } into g
                select new { g.Key.ID_GangLong, g.Key.ID_TTG_Target, MaxId = g.Max(x => x.ID) };

            var latestAllocRows = await (
                from pb in _context.Tbl_BM_16_PhanBoGangCR.AsNoTracking()
                join k in latestKeysQuery
                  on new { pb.ID_GangLong, pb.ID_TTG_Target, pb.ID }
                  equals new { k.ID_GangLong, k.ID_TTG_Target, ID = k.MaxId }
                select new
                {
                    pb.ID_GangLong,
                    pb.KL_PhanBo_CR,
                    pb.IsSaiChuyenDen
                })
                .ToListAsync();

            // 4) Tổng CR hợp lệ (không "Sai", > 0) theo từng Gang
            var validCrByGang = latestAllocRows
                .Where(x => !(x.IsSaiChuyenDen ?? false))
                .GroupBy(x => x.ID_GangLong)
                .ToDictionary(g => g.Key,
                              g => g.Sum(x => (x.KL_PhanBo_CR ?? 0m) > 0m
                                               ? (x.KL_PhanBo_CR ?? 0m) : 0m));

            // 5) Cộng tổng theo logic Excel
            decimal total = 0m;
            foreach (var gid in gangIds)
            {
                var ma = maGangById[gid];
                var daNhan = statusById.TryGetValue(gid, out var st) && st == 4;

                if (IsChiaCR(ma))
                {
                    if (validCrByGang.TryGetValue(gid, out var crSum) && crSum > 0m)
                        total += crSum;
                    else if (daNhan)
                        total += gklById[gid];
                }
                else
                {
                    if (daNhan)
                        total += gklById[gid];
                }
            }

            return total;
        }

        private async Task<PageResultViewModel<List<Tbl_BM_16_GangLong>>> SearchByPayload(SearchDto dto)
        {
            // 0) Base query (chỉ Where; chưa OrderBy, chưa Skip/Take)
            IQueryable<Tbl_BM_16_GangLong> baseQuery = _context.Tbl_BM_16_GangLong.AsQueryable();

            if (dto.ID_HRC.HasValue)
            {
                baseQuery = baseQuery.Where(x =>
                    _context.Tbl_TaiKhoan.Any(tk => tk.ID_TaiKhoan == x.T_ID_NguoiNhan && tk.ID_PhongBan == dto.ID_HRC));
            }
            if (dto.ID_LoCao.HasValue) baseQuery = baseQuery.Where(x => x.ID_Locao == dto.ID_LoCao.Value);
            if (dto.ID_LoThoi.HasValue) baseQuery = baseQuery.Where(x => x.ID_LoThoi == dto.ID_LoThoi.Value);
            if (dto.Ca_LT.HasValue) baseQuery = baseQuery.Where(x => x.T_Ca == dto.Ca_LT.Value);
            if (dto.Ca_LG.HasValue) baseQuery = baseQuery.Where(x => x.G_Ca == dto.Ca_LG.Value);

            if (!string.IsNullOrEmpty(dto.ID_Kip_LT))
            {
                baseQuery = baseQuery.Where(thung =>
                    thung.T_ID_Kip != null &&
                    _context.Tbl_Kip.Where(k => k.TenKip == dto.ID_Kip_LT)
                        .Select(k => k.ID_Kip)
                        .Contains(thung.T_ID_Kip.Value));
            }
            if (!string.IsNullOrEmpty(dto.ID_Kip_LG))
            {
                baseQuery = baseQuery.Where(thung =>
                    thung.G_ID_Kip != null &&
                    _context.Tbl_Kip.Where(k => k.TenKip == dto.ID_Kip_LG)
                        .Select(k => k.ID_Kip)
                        .Contains(thung.G_ID_Kip.Value));
            }

            if (!string.IsNullOrEmpty(dto.ChuyenDen))
                baseQuery = baseQuery.Where(x => x.ChuyenDen.Contains(dto.ChuyenDen));
            if (!string.IsNullOrEmpty(dto.ThungSo))
                baseQuery = baseQuery.Where(x => x.BKMIS_ThungSo.Contains(dto.ThungSo));

            if (dto.ID_TinhTrang.HasValue)
                baseQuery = baseQuery.Where(x => x.ID_TrangThai == dto.ID_TinhTrang.Value);
            if (dto.ID_TinhTrang_LT.HasValue)
                baseQuery = baseQuery.Where(x => x.T_ID_TrangThai == dto.ID_TinhTrang_LT.Value);
            if (dto.ID_TinhTrang_LG.HasValue)
                baseQuery = baseQuery.Where(x => x.G_ID_TrangThai == dto.ID_TinhTrang_LG.Value);

            if (!string.IsNullOrEmpty(dto.MaThungGang))
                baseQuery = baseQuery.Where(x => x.MaThungGang.Contains(dto.MaThungGang));
            if (!string.IsNullOrEmpty(dto.MaThungThep))
                baseQuery = baseQuery.Where(x => x.MaThungThep.Contains(dto.MaThungThep));

            if (!string.IsNullOrEmpty(dto.MaMeThoi))
            {
                var idTTGList = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(x => x.ID_MeThoi.HasValue &&
                                _context.Tbl_MeThoi
                                    .Where(m => m.MaMeThoi.Contains(dto.MaMeThoi))
                                    .Select(m => m.ID)
                                    .Contains(x.ID_MeThoi.Value))
                    .Select(x => x.ID)
                    .ToListAsync();

                baseQuery = baseQuery.Where(x => x.ID_TTG.HasValue && idTTGList.Contains(x.ID_TTG.Value));
            }

            if (dto.IsChiaGang.HasValue)
                baseQuery = dto.IsChiaGang == true
                    ? baseQuery.Where(x => x.KLGangChia.HasValue)
                    : baseQuery.Where(x => !x.KLGangChia.HasValue);

            // 1) Áp filter ngày cho tập TÍNH TỔNG (pre-paging scope)
            IQueryable<Tbl_BM_16_GangLong> totalScope = baseQuery;
            bool hasLT = dto.TuNgay_LT.HasValue && dto.DenNgay_LT.HasValue;
            bool hasLG = dto.TuNgay_LG.HasValue && dto.DenNgay_LG.HasValue;
            bool hasDateFilter = hasLT || hasLG;

            if (hasLT)
            {
                var tuNgay = dto.TuNgay_LT.Value.Date;
                var denNgay = dto.DenNgay_LT.Value.Date.AddDays(1);
                totalScope = totalScope.Where(x => x.NgayLuyenThep >= tuNgay && x.NgayLuyenThep < denNgay);
            }
            if (hasLG)
            {
                var tuNgay = dto.TuNgay_LG.Value.Date;
                var denNgay = dto.DenNgay_LG.Value.Date.AddDays(1);
                totalScope = totalScope.Where(x => x.NgayTao >= tuNgay && x.NgayTao < denNgay);
            }

            // 2) Tính các tổng trên totalScope (trước paging)
            var totalRecords = await totalScope.CountAsync();

            //decimal sumKLGang = await totalScope.Where(x => x.T_copy != true).SumAsync(x => (decimal?)(x.G_KLGangLong ?? 0m)) ?? 0m;
            //decimal sumKLGangLongThep = await totalScope.SumAsync(x => (decimal?)(x.T_KLGangLong ?? 0m)) ?? 0m;
            //decimal sumKLGangChia = await totalScope.SumAsync(x => (decimal?)((x.KLGangChia ?? x.T_KLGangLong) ?? 0m)) ?? 0m;

            //// TTG liên quan để tính SumKLGangNhan, SumKLPhe, SumKLVaoLoThoi và sumKLGangChiaCR
            //var maThungTGListForSum = await (
            //    from a in totalScope
            //    join ttgRoot in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttgRoot.ID
            //    select ttgRoot.MaThungTG
            //).Distinct().ToListAsync();

            //var relatedThungForSum = await _context.Tbl_BM_16_ThungTrungGian
            //    .Where(x => maThungTGListForSum.Contains(x.MaThungTG))
            //    .ToListAsync();

            //decimal sumKLGangNhan = relatedThungForSum.Sum(x => x.Tong_KLGangNhan ?? 0m);
            //decimal sumKLPhe = relatedThungForSum.Sum(x => x.KL_phe ?? 0m);
            //decimal sumKLVaoLoThoi = relatedThungForSum.Sum(x => x.KLGang_Thoi ?? 0m);
            //scopeTtgIds = relatedThungForSum.Select(x => x.ID).ToList();

            //decimal sumKLGangChiaCR = 0m;
            //if (scopeTtgIds.Count > 0)
            //    sumKLGangChiaCR = await SumLatestCRWithFallbackAsync(totalScope, scopeTtgIds);
            // 2) Tính các tổng trên totalScope (TRƯỚC paging) — CHỈ khi có ít nhất 1 cặp ngày
            //int totalRecords;
            decimal sumKLGang = 0m;
            decimal sumKLGangLongThep = 0m;
            decimal sumKLGangNhan = 0m;
            decimal sumKLPhe = 0m;
            decimal sumKLVaoLoThoi = 0m;
            decimal sumKLGangChia = 0m;
            decimal sumKLGangChiaCR = 0m;

            List<int> scopeTtgIds = new();

            if (hasDateFilter)
            {
                totalRecords = await totalScope.CountAsync();

                sumKLGang = await totalScope.Where(x => x.T_copy != true)
                    .SumAsync(x => (decimal?)(x.G_KLGangLong ?? 0m)) ?? 0m;

                sumKLGangLongThep = await totalScope
                    .SumAsync(x => (decimal?)(x.T_KLGangLong ?? 0m)) ?? 0m;

                sumKLGangChia = await totalScope
                    .SumAsync(x => (decimal?)((x.KLGangChia ?? x.T_KLGangLong) ?? 0m)) ?? 0m;

                var maThungTGListForSum = await (
                    from a in totalScope
                    join ttgRoot in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttgRoot.ID
                    select ttgRoot.MaThungTG
                ).Distinct().ToListAsync();

                var relatedThungForSum = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(x => maThungTGListForSum.Contains(x.MaThungTG))
                    .ToListAsync();

                sumKLGangNhan = relatedThungForSum.Sum(x => x.Tong_KLGangNhan ?? 0m);
                sumKLPhe = relatedThungForSum.Sum(x => x.KL_phe ?? 0m);
                sumKLVaoLoThoi = relatedThungForSum.Sum(x => x.KLGang_Thoi ?? 0m);
                scopeTtgIds = relatedThungForSum.Select(x => x.ID).ToList();

                if (scopeTtgIds.Count > 0)
                    sumKLGangChiaCR = await SumLatestCRWithFallbackAsync(totalScope, scopeTtgIds);
            }

            // 3) Tạo ordered query rồi paging
            IQueryable<Tbl_BM_16_GangLong> orderedQuery = totalScope
                .OrderByDescending(x => x.NgayTao)
                .ThenBy(x => x.ID_Locao)
                .ThenByDescending(x => x.G_Ca)
                .ThenByDescending(x => x.MaThungGang)
                .ThenBy(x => x.MaThungThep); // vẫn là IQueryable

            if (dto.PageNumber.HasValue && dto.PageSize.HasValue)
            {
                int page = dto.PageNumber.Value;
                int pageSize = dto.PageSize.Value;
                orderedQuery = orderedQuery.Skip((page - 1) * pageSize).Take(pageSize);
            }

            // 4) Lấy dữ liệu hiển thị (page scope)
            var gocData = await (from a in orderedQuery
                                 join trangThai in _context.Tbl_BM_16_TrangThai on a.ID_TrangThai equals trangThai.ID into g_tt
                                 from trangThai in g_tt.DefaultIfEmpty()

                                 join trangThaiLG in _context.Tbl_BM_16_TrangThai on a.G_ID_TrangThai equals trangThaiLG.ID into g_TrangThai
                                 from trangThaiLG in g_TrangThai.DefaultIfEmpty()

                                 join trangThaiLT in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThaiLT.ID into t_TrangThai
                                 from trangThaiLT in t_TrangThai.DefaultIfEmpty()

                                 join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID into g_lc
                                 from loCao in g_lc.DefaultIfEmpty()

                                 join kipG in _context.Tbl_Kip on a.G_ID_Kip equals kipG.ID_Kip into g_kipG
                                 from kipG in g_kipG.DefaultIfEmpty()

                                 join kipT in _context.Tbl_Kip on a.T_ID_Kip equals kipT.ID_Kip into g_kipT
                                 from kipT in g_kipT.DefaultIfEmpty()

                                 join thungUser in _context.Tbl_BM_16_TaiKhoan_Thung on a.MaThungThep equals thungUser.MaThungThep into g_thungUser
                                 from thungUser in g_thungUser.DefaultIfEmpty()

                                 join user in _context.Tbl_TaiKhoan on thungUser.ID_taiKhoan equals user.ID_TaiKhoan into g_user
                                 from user in g_user.DefaultIfEmpty()

                                 join phongban in _context.Tbl_PhongBan on user.ID_PhongBan equals phongban.ID_PhongBan into g_phongban
                                 from phongban in g_phongban.DefaultIfEmpty()

                                 join pkh_user in _context.Tbl_TaiKhoan on a.ID_NguoiChot equals pkh_user.ID_TaiKhoan into tk_user
                                 from pkh_user in tk_user.DefaultIfEmpty()

                                 join ttg in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttg.ID into t_ttg
                                 from ttg in t_ttg.DefaultIfEmpty()

                                 join methoi in _context.Tbl_MeThoi on ttg.ID_MeThoi equals methoi.ID into g_mt
                                 from methoi in g_mt.DefaultIfEmpty()

                                 select new Tbl_BM_16_GangLong
                                 {
                                     ID = a.ID,
                                     NgayTao = a.NgayTao,
                                     NgayLuyenGang = a.NgayLuyenGang,
                                     G_Ca = a.G_Ca,
                                     G_TenKip = kipG != null ? kipG.TenKip : null,
                                     MaThungGang = a.MaThungGang,
                                     ID_Locao = a.ID_Locao,
                                     BKMIS_SoMe = a.BKMIS_SoMe,
                                     BKMIS_ThungSo = a.BKMIS_ThungSo,
                                     BKMIS_Gio = a.BKMIS_Gio,
                                     BKMIS_PhanLoai = a.BKMIS_PhanLoai,
                                     KL_XeGoong = a.KL_XeGoong,
                                     G_KLThungChua = a.G_KLThungChua,
                                     G_KLThungVaGang = a.G_KLThungVaGang,
                                     G_KLGangLong = a.G_KLGangLong,
                                     ChuyenDen = a.ChuyenDen,
                                     Gio_NM = a.Gio_NM,
                                     G_ID_TrangThai = a.G_ID_TrangThai,
                                     NgayLuyenThep = a.NgayLuyenThep,
                                     T_Ca = a.T_Ca,
                                     T_TenKip = kipT != null ? kipT.TenKip : null,
                                     MaThungThep = a.MaThungThep,
                                     KR = a.KR,
                                     T_KLThungVaGang = a.T_KLThungVaGang,
                                     T_KLThungChua = a.T_KLThungChua,
                                     T_KLGangLong = a.T_KLGangLong,
                                     T_ID_TrangThai = a.T_ID_TrangThai,
                                     ID_TrangThai = a.ID_TrangThai,
                                     TenLoCao = loCao.TenLoCao,
                                     T_KL_phe = a.T_KL_phe,
                                     TrangThai = trangThai.TenTrangThai,
                                     TrangThaiLG = trangThaiLG.TenTrangThai,
                                     TrangThaiLT = trangThaiLT.TenTrangThai,
                                     T_copy = a.T_copy,
                                     KLGangChia = a.KLGangChia,
                                     ID_NguoiChot = a.ID_NguoiChot,
                                     HoTenNguoiChot = pkh_user.HoVaTen,

                                     HoVaTen = user.HoVaTen,
                                     TenPhongBan = phongban.TenNgan,

                                     ID_TTG = a.ID_TTG,
                                     MaThungTG = ttg != null ? ttg.MaThungTG : null,
                                     ID_MeThoi = ttg != null ? ttg.ID_MeThoi : null,
                                     IsCopy = ttg != null ? ttg.IsCopy : (bool?)null,
                                     MaMeThoi = methoi != null ? methoi.MaMeThoi : null,
                                     ID_LoThoi = ttg != null ? ttg.ID_LoThoi : (int?)null,
                                     SoThungTG = ttg != null ? ttg.SoThungTG : null,
                                     KLThungVaGang_Thoi = ttg != null ? ttg.KLThungVaGang_Thoi : null,
                                     KLThung_Thoi = ttg != null ? ttg.KLThung_Thoi : null,
                                     KLGang_Thoi = ttg != null ? ttg.KLGang_Thoi : null,
                                     KL_phe = ttg != null ? ttg.KL_phe : null,
                                     Tong_KLGangNhan = ttg != null ? ttg.Tong_KLGangNhan : null,
                                     GioChonMe = ttg != null ? ttg.GioChonMe : null
                                 }).ToListAsync();

            // 5) Nhân bản TTG copy cho data hiển thị
            var maTTGs = gocData.Where(x => !string.IsNullOrEmpty(x.MaThungTG))
                                .Select(x => x.MaThungTG!)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();

            var thungTG_Copies = await _context.Tbl_BM_16_ThungTrungGian
                .Where(x => x.IsCopy == true && maTTGs.Contains(x.MaThungTG))
                .ToListAsync();

            var finalData = new List<Tbl_BM_16_GangLong>(gocData.Count + thungTG_Copies.Count * 2);

            foreach (var item in gocData)
            {
                finalData.Add(item);
                if (!item.ID_TTG.HasValue || item.IsCopy == true) continue;

                var copies = thungTG_Copies.Where(x => x.MaThungTG == item.MaThungTG).ToList();
                foreach (var copy in copies)
                {
                    var clone = CloneGangLong(item); // bạn đã có helper này
                    var methoi = await _context.Tbl_MeThoi.FirstOrDefaultAsync(x => x.ID == copy.ID_MeThoi);

                    clone.ID_TTG = copy.ID;
                    clone.IsCopy = true;
                    clone.SoThungTG = copy.SoThungTG;
                    clone.KLThungVaGang_Thoi = copy.KLThungVaGang_Thoi;
                    clone.KLThung_Thoi = copy.KLThung_Thoi;
                    clone.KLGang_Thoi = copy.KLGang_Thoi;
                    clone.KL_phe = copy.KL_phe;
                    clone.ID_MeThoi = methoi?.ID;
                    clone.MaMeThoi = methoi?.MaMeThoi;
                    clone.GioChonMe = copy.GioChonMe;

                    finalData.Add(clone);
                }
            }

            // 6) Lấy phân bổ CR mới nhất cho (GangID, TTG_ID) của finalData, đồng thời set IsSaiChuyenDen
            var gangIdsAll = finalData.Select(x => x.ID).Distinct().ToList();
            var ttgIdsAll = finalData.Where(x => x.ID_TTG.HasValue).Select(x => x.ID_TTG!.Value).Distinct().ToList();

            var allocCrDict = new Dictionary<(int gangId, int ttgId), decimal?>();
            var allocErrDict = new Dictionary<(int gangId, int ttgId), bool?>();

            if (gangIdsAll.Count > 0 && ttgIdsAll.Count > 0)
            {
                var latestKeysQuery2 =
                    from pb in _context.Tbl_BM_16_PhanBoGangCR
                    where gangIdsAll.Contains(pb.ID_GangLong)
                       && ttgIdsAll.Contains(pb.ID_TTG_Target)
                    group pb by new { pb.ID_GangLong, pb.ID_TTG_Target } into g
                    select new { g.Key.ID_GangLong, g.Key.ID_TTG_Target, MaxId = g.Max(x => x.ID) };

                var allocRows = await (
                    from pb in _context.Tbl_BM_16_PhanBoGangCR
                    join k in latestKeysQuery2
                      on new { pb.ID_GangLong, pb.ID_TTG_Target, pb.ID }
                      equals new { k.ID_GangLong, k.ID_TTG_Target, ID = k.MaxId }
                    select new
                    {
                        pb.ID_GangLong,
                        pb.ID_TTG_Target,
                        pb.KL_PhanBo_CR,
                        pb.IsSaiChuyenDen
                    })
                    .AsNoTracking()
                    .ToListAsync();

                foreach (var r in allocRows)
                {
                    allocCrDict[(r.ID_GangLong, r.ID_TTG_Target)] = r.KL_PhanBo_CR;
                    allocErrDict[(r.ID_GangLong, r.ID_TTG_Target)] = r.IsSaiChuyenDen;
                }
            }

            // 7) Đánh dấu IsChiaCR (>=2 lần nhận) & gán KL_GangChiaCR + KL_GangChiaCR_Display theo logic Excel
            var maThungGangSet = finalData
                .Where(x => !string.IsNullOrEmpty(x.MaThungGang))
                .Select(x => x.MaThungGang!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var receiveCounts2 = maThungGangSet.Count == 0
                ? new List<(string MaThungGang, int Count)>()
                : await _context.Tbl_BM_16_TaiKhoan_Thung
                    .Where(t => maThungGangSet.Contains(t.MaThungGang))
                    .GroupBy(t => t.MaThungGang)
                    .Select(g => new { MaThungGang = g.Key, Count = g.Count() })
                    .ToListAsync()
                    .ContinueWith(t => t.Result.Select(x => (x.MaThungGang!, x.Count)).ToList());

            var receiveMap2 = receiveCounts2.ToDictionary<(string MaThungGang, int Count), string, int>(
                x => x.MaThungGang, x => x.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var item in finalData)
            {
                // IsChiaCR
                if (!string.IsNullOrWhiteSpace(item.MaThungGang)
                    && receiveMap2.TryGetValue(item.MaThungGang!, out var cnt) && cnt >= 2)
                    item.IsChiaCR = true;
                else
                    item.IsChiaCR = false;

                // KL_GangChiaCR + IsSaiChuyenDen
                if (item.ID_TTG.HasValue)
                {
                    var key = (item.ID, item.ID_TTG.Value);

                    item.KL_GangChiaCR = allocCrDict.TryGetValue(key, out var cr) ? (cr ?? 0m) : 0m;
                    item.IsSaiChuyenDen = allocErrDict.TryGetValue(key, out var err) ? err : false;
                }
                else
                {
                    item.KL_GangChiaCR = 0m;
                    item.IsSaiChuyenDen = null; // không thuộc TTG
                }

                // KL_GangChiaCR_Display: chỉ là số để Excel render; phần "Sai" Excel sẽ tự ghi theo IsSaiChuyenDen
                if (item.IsChiaCR == true)
                {
                    if (item.IsSaiChuyenDen == true)
                    {
                        item.KL_GangChiaCR_Display = null; // Excel sẽ in "Sai"
                    }
                    else
                    {
                        // Nếu có CR > 0 lấy CR, nếu không mà đã nhận (T_ID_TrangThai == 4) thì fallback G_KLGangLong, ngược lại null
                        if (item.KL_GangChiaCR > 0m)
                            item.KL_GangChiaCR_Display = item.KL_GangChiaCR;
                        else if (item.T_ID_TrangThai == 4)
                            item.KL_GangChiaCR_Display = item.G_KLGangLong ?? 0m;
                        else
                            item.KL_GangChiaCR_Display = null;
                    }
                }
                else
                {
                    // Không chia CR: nếu đã nhận thì hiện G_KLGangLong, ngược lại null
                    if (item.T_ID_TrangThai == 4)
                        item.KL_GangChiaCR_Display = item.G_KLGangLong ?? 0m;
                    else
                        item.KL_GangChiaCR_Display = null;
                }
            }

            // 8) Group để trả ra UI
            var groupedData = finalData
                .GroupBy(x => x.ID_TTG.HasValue ? x.ID_TTG.Value.ToString() : $"null_{x.ID}")
                .Select(g => g.ToList())
                .ToList();

            // 9) Return (sumKLGangChiaCR đã tính trên totalScope trước paging)
            return new PageResultViewModel<List<Tbl_BM_16_GangLong>>
            {
                TotalRecords = totalRecords,
                SumKLGang = sumKLGang,
                SumKLGangLongThep = sumKLGangLongThep,
                SumKLGangNhan = sumKLGangNhan,
                SumKLPhe = sumKLPhe,
                SumKLVaoLoThoi = sumKLVaoLoThoi,
                SumKLGangChia = sumKLGangChia,
                SumKLGangChiaCR = sumKLGangChiaCR,
                Data = groupedData
            };
        }


        private Tbl_BM_16_GangLong CloneGangLong(Tbl_BM_16_GangLong original)
        {
            return new Tbl_BM_16_GangLong
            {
                ID = original.ID,
                NgayTao = original.NgayTao,
                NgayLuyenGang = original.NgayLuyenGang,
                G_Ca = original.G_Ca,
                G_TenKip = original.G_TenKip,
                MaThungGang = original.MaThungGang,
                ID_Locao = original.ID_Locao,
                BKMIS_SoMe = original.BKMIS_SoMe,
                BKMIS_ThungSo = original.BKMIS_ThungSo,
                BKMIS_Gio = original.BKMIS_Gio,
                BKMIS_PhanLoai = original.BKMIS_PhanLoai,
                //KL_XeGoong = original.KL_XeGoong,
                //G_KLThungChua = original.G_KLThungChua,
                //G_KLThungVaGang = original.G_KLThungVaGang,
                //G_KLGangLong = original.G_KLGangLong,
                KL_XeGoong = null,
                G_KLThungChua = null,
                G_KLThungVaGang = null,
                G_KLGangLong = null,
                ChuyenDen = original.ChuyenDen,
                Gio_NM = original.Gio_NM,
                G_ID_TrangThai = original.G_ID_TrangThai,
                NgayLuyenThep = original.NgayLuyenThep,
                T_Ca = original.T_Ca,
                T_TenKip = original.T_TenKip,
                MaThungThep = original.MaThungThep,
                T_copy = original.T_copy,
                KR = original.KR,
                //T_KLThungVaGang = original.T_KLThungVaGang,
                //T_KLThungChua = original.T_KLThungChua,
                //T_KLGangLong = original.T_KLGangLong,
                T_KLThungVaGang = null,
                T_KLThungChua =null,
                T_KLGangLong = null,

                T_ID_TrangThai = original.T_ID_TrangThai,
                ID_TrangThai = original.ID_TrangThai,
                TenLoCao = original.TenLoCao,
                T_KL_phe = original.T_KL_phe,
                TrangThai = original.TrangThai,
                TrangThaiLG = original.TrangThaiLG,
                TrangThaiLT = original.TrangThaiLT,
                HoVaTen = original.HoVaTen,
                TenPhongBan = original.TenPhongBan,

                ID_TTG = original.ID_TTG,
                MaThungTG = original.MaThungTG,
                ID_MeThoi = original.ID_MeThoi,
                IsCopy = original.IsCopy,
                MaMeThoi = original.MaMeThoi,
                ID_LoThoi = original.ID_LoThoi,
                SoThungTG = original.SoThungTG,
                //KLThungVaGang_Thoi = original.KLThungVaGang_Thoi,
                //KLThung_Thoi = original.KLThung_Thoi,
                //KLGang_Thoi = original.KLGang_Thoi,
                //KL_phe = original.KL_phe,
                //Tong_KLGangNhan = original.Tong_KLGangNhan,
                KLThungVaGang_Thoi = null,
                KLThung_Thoi = null,
                KLGang_Thoi = null,
                KL_phe = null,
                Tong_KLGangNhan = null,
                GioChonMe = null
            };
        }
        

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] SearchDto dto)
        {
            try
            {
                var res = await SearchByPayload(dto);
                var thungList = res.Data;
                var totalRecords = res.TotalRecords;
                var sumKLGang = res.SumKLGang;

                if (thungList == null || !thungList.Any())
                    return BadRequest("Danh sách trống.");

                var groupByTTG = thungList; // Nếu cần nhóm, chỉnh lại ở đây
                // Thì dùng groupBy thực sự:
                //var groupByTTG = thungList
                //    .GroupBy(x => x.ID_TTG.HasValue ? x.ID_TTG.Value.ToString() : $"null_{x.ID}")
                //    .OrderByDescending(g => g.Max(x => x.NgayTao))
                //    .ToList();

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "QTGN_Gang_Long_PKH.xlsx");
                using (var ms = new MemoryStream())
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet("Sheet1");

                        // Xóa dữ liệu cũ
                        var lastRow = Math.Max(worksheet.LastRowUsed()?.RowNumber() ?? 8, 8);
                        if (lastRow >= 8)
                        {
                            var rangeClear = worksheet.Range($"A8:AE{lastRow}");
                            rangeClear.Clear(XLClearOptions.Contents | XLClearOptions.NormalFormats);
                            // Set lại format General sau khi clear
                            rangeClear.Style.NumberFormat.SetFormat("General");
                        }

                        int row = 8, stt = 1;

                        foreach (var group in groupByTTG)
                        {
                            int rowspan = group.Count();
                            bool isFirst = true;
                            int mergedColumnCount = 0;

                            foreach (var item in group)
                            {
                                int colIndex = 1;

                                worksheet.Cell(row, colIndex++).Value = stt;
                                worksheet.Cell(row, colIndex++).Value = item.NgayTao?.Day.ToString();
                                worksheet.Cell(row, colIndex++).Value = item.G_Ca == 1 ? "N" : item.G_Ca == 2 ? "Đ" : "";
                                worksheet.Cell(row, colIndex++).Value = item.G_TenKip;
                                worksheet.Cell(row, colIndex++).Value = item.MaThungGang;

                                var LoCaocell = worksheet.Cell(row, colIndex++);
                                LoCaocell.Value = item.ID_Locao;
                                LoCaocell.Style.NumberFormat.NumberFormatId = 0;

                                var cellSoMe = worksheet.Cell(row, colIndex++);
                                cellSoMe.Value = item.BKMIS_SoMe;

                                var cellThungSo = worksheet.Cell(row, colIndex++);
                                cellThungSo.Value = item.BKMIS_ThungSo;

                                worksheet.Cell(row, colIndex++).Value = item.BKMIS_Gio;
                                worksheet.Cell(row, colIndex++).Value = item.BKMIS_PhanLoai;
                                worksheet.Cell(row, colIndex++).Value = item.KR == true ? "X" : "";
                                if (item.T_copy == true || item.IsCopy == true)
                                {
                                    cellSoMe.Style.Font.FontColor = XLColor.Red;
                                    cellThungSo.Style.Font.FontColor = XLColor.Red;

                                    // Thêm 4 ô trống cho các cột KL_XeGoong -> G_KLGangLong
                                    worksheet.Cell(row, colIndex++).Value = "";
                                    worksheet.Cell(row, colIndex++).Value = "";
                                    worksheet.Cell(row, colIndex++).Value = "";
                                    worksheet.Cell(row, colIndex++).Value = "";
                                }
                                else
                                {
                                    worksheet.Cell(row, colIndex++).Value = item.KL_XeGoong;
                                    worksheet.Cell(row, colIndex++).Value = item.G_KLThungVaGang;
                                    worksheet.Cell(row, colIndex++).Value = item.G_KLThungChua;

                                    var cellKLGangLong = worksheet.Cell(row, colIndex++);
                                    if (item.G_KLGangLong.HasValue)
                                    {
                                        cellKLGangLong.Value = item.G_KLGangLong.Value;
                                        cellKLGangLong.Style.NumberFormat.Format = "0.00";
                                        cellKLGangLong.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        cellKLGangLong.Style.Font.Bold = true;
                                    }
                                    else
                                    {
                                        cellKLGangLong.Value = "";
                                    }
                                }

                                worksheet.Cell(row, colIndex++).Value = item.ChuyenDen;
                                worksheet.Cell(row, colIndex++).Value = item.Gio_NM;

                                var tinhTrangG_cell = worksheet.Cell(row, colIndex++);
                                RenderTrangThaiCell(tinhTrangG_cell, item.TrangThaiLG, item.G_ID_TrangThai);

                                worksheet.Cell(row, colIndex++).Value = item.NgayLuyenThep?.Day.ToString();
                                worksheet.Cell(row, colIndex++).Value = item.T_Ca == 1 ? "N" : item.T_Ca == 2 ? "Đ" : "";
                                worksheet.Cell(row, colIndex++).Value = item.T_TenKip;
                                worksheet.Cell(row, colIndex++).Value = item.MaThungThep;

                                worksheet.Cell(row, colIndex++).Value = item.T_KLThungVaGang;
                                worksheet.Cell(row, colIndex++).Value = item.T_KLThungChua;
                                worksheet.Cell(row, colIndex++).Value = item.T_KLGangLong;

                                var cellKLGangChia = worksheet.Cell(row, colIndex++);
                                if (item.KLGangChia.HasValue)
                                {
                                    cellKLGangChia.Value = item.KLGangChia;
                                    cellKLGangChia.Style.Font.FontColor = XLColor.FromHtml("#ef2337");
                                }
                                else
                                {
                                    cellKLGangChia.Value = item.T_KLGangLong.HasValue ? item.T_KLGangLong : "";
                                }

                                var cellKLGangChiaCR = worksheet.Cell(row, colIndex++);

                                if (item.IsChiaCR == true)
                                {
                                    if (item.IsSaiChuyenDen == true)
                                    {
                                        cellKLGangChiaCR.Value = "Sai";
                                    }
                                    else
                                    {
                                        cellKLGangChiaCR.Value = item.KL_GangChiaCR_Display.HasValue
                                            ? item.KL_GangChiaCR_Display.Value
                                            : "";
                                    }

                                    cellKLGangChiaCR.Style.Font.FontColor = XLColor.FromHtml("#ef2337");
                                }
                                else
                                {
                                    cellKLGangChiaCR.Value = item.T_ID_TrangThai != (int)TinhTrang.DaNhan ? "" : item.G_KLGangLong.HasValue ? item.G_KLGangLong : "";
                                }

                                if (isFirst)
                                {
                                    mergedColumnCount = 0;

                                    var cellTongKLGang = worksheet.Cell(row, colIndex);
                                    cellTongKLGang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    cellTongKLGang.Style.Font.Bold = true;

                                    if (item.IsCopy == true)
                                    {
                                        cellTongKLGang.Value = "TTG Copy";
                                        cellTongKLGang.Style.Font.FontColor = XLColor.Red;
                                    }
                                    else if (item.Tong_KLGangNhan.HasValue)
                                    {
                                        cellTongKLGang.Value = item.Tong_KLGangNhan.Value;
                                        cellTongKLGang.Style.NumberFormat.Format = "0.00";
                                    }
                                    else
                                    {
                                        cellTongKLGang.Value = "";
                                    }

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge();
                                    colIndex++; mergedColumnCount++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.ID_LoThoi;
                                    colIndex++; mergedColumnCount++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.SoThungTG;
                                    colIndex++; mergedColumnCount++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.KLThungVaGang_Thoi;
                                    colIndex++; mergedColumnCount++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.KLThung_Thoi;
                                    colIndex++; mergedColumnCount++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.KL_phe;
                                    colIndex++; mergedColumnCount++;

                                    var cellKLGang_Thoi = worksheet.Cell(row, colIndex);
                                    if (item.KLGang_Thoi.HasValue)
                                    {
                                        cellKLGang_Thoi.Value = item.KLGang_Thoi.Value;
                                        cellKLGang_Thoi.Style.NumberFormat.Format = "0.00";
                                        cellKLGang_Thoi.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        cellKLGang_Thoi.Style.Font.Bold = true;
                                    }
                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge();
                                    colIndex++; mergedColumnCount++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.MaMeThoi;
                                    colIndex++; mergedColumnCount++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.GioChonMe;
                                    colIndex++; mergedColumnCount++;

                                    isFirst = false;
                                }
                                else
                                {
                                    // Bỏ qua cột merge cho các dòng tiếp theo
                                    colIndex += mergedColumnCount;
                                }

                                var tinhTrangT_cell = worksheet.Cell(row, colIndex++);
                                RenderTrangThaiCell(tinhTrangT_cell, item.TrangThaiLT, item.T_ID_TrangThai);

                                worksheet.Cell(row, colIndex++).Value = item.TenPhongBan;
                                worksheet.Cell(row, colIndex++).Value = item.HoVaTen;

                                var tinhTrang_cell = worksheet.Cell(row, colIndex++);
                                RenderTrangThaiCell(tinhTrang_cell, item.TrangThai, item.ID_TrangThai);

                                row++;
                                stt++;
                            }
                        }

                        // --- Dòng tổng ---
                        int sumRow = row;
                        var totalLabel = worksheet.Range($"A{sumRow}:N{sumRow}");
                        totalLabel.Merge();
                        totalLabel.Value = "Tổng:";
                        totalLabel.Style.Font.SetBold();
                        totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột O (15)
                        worksheet.Cell(sumRow, 15).FormulaA1 = $"=SUM(O8:O{row - 1})";
                        worksheet.Cell(sumRow, 15).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 15).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge P -> Y (16 -> 26)
                        worksheet.Range(sumRow, 16, sumRow, 27).Merge().Value = "";
                        worksheet.Range(sumRow, 16, sumRow, 27).Style.Fill.BackgroundColor = XLColor.White;

                        // Tổng cột AA (28)
                        worksheet.Cell(sumRow, 28).FormulaA1 = $"=SUM(AB8:AB{row - 1})";
                        worksheet.Cell(sumRow, 28).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 28).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 28).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge AA -> AE (29 -> 33)
                        worksheet.Range(sumRow, 29, sumRow, 33).Merge().Value = "";
                        worksheet.Range(sumRow, 29, sumRow, 33).Style.Fill.BackgroundColor = XLColor.White;

                        // Tổng cột AG (34)
                        worksheet.Cell(sumRow, 34).FormulaA1 = $"=SUM(AH8:AH{row - 1})";
                        worksheet.Cell(sumRow, 34).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 34).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 34).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge AH -> AM (34 -> 39)
                        worksheet.Range(sumRow, 34, sumRow, 39).Merge().Value = "";
                        worksheet.Range(sumRow, 34, sumRow, 39).Style.Fill.BackgroundColor = XLColor.White;

                        // --- Dòng tổng all ---
                        int sumAllRow = row + 1;
                        var totalAllLabel = worksheet.Range($"A{sumAllRow}:N{sumAllRow}");
                        totalAllLabel.Merge();
                        totalAllLabel.Value = "Tổng Tất Cả:";
                        totalAllLabel.Style.Font.SetBold();
                        totalAllLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        worksheet.Cell(sumAllRow, 15).Value = sumKLGang;
                        worksheet.Cell(sumAllRow, 15).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumAllRow, 15).Style.Font.SetBold();
                        worksheet.Cell(sumAllRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        worksheet.Range(sumAllRow, 16, sumAllRow, 39).Merge().Value = "";
                        worksheet.Range(sumAllRow, 16, sumAllRow, 39).Style.Fill.BackgroundColor = XLColor.White;

                        // Format toàn bảng
                        var usedRange = worksheet.Range($"A7:AN{sumAllRow}");
                        usedRange.Style.Font.SetFontName("Arial").Font.SetFontSize(11);
                        usedRange.Style.NumberFormat.SetFormat("General");
                        //usedRange.Style.Font.FontColor = XLColor.Black;
                        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        usedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        usedRange.Style.Alignment.WrapText = true;

                        // Chiều cao dòng
                        for (int i = 8; i <= sumRow; i++)
                        {
                            worksheet.Row(i).Height = 25;
                        }

                        workbook.SaveAs(ms);
                    }

                    ms.Position = 0;
                    string outputName = $"QTGN_Gang_Long_Gang_Thoi_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(ms.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                outputName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return RedirectToAction("Index", "BM_GangThoi_PKH");
            }
        }

        private void RenderTrangThaiCell(IXLCell cell, string trangThaiText, int? idTrangThai)
        {
            cell.Value = trangThaiText;
            cell.Style.Font.SetBold();
            switch (idTrangThai)
            {
                case 1:
                default:
                    // Xám nhạt
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#6c757d");
                    cell.Style.Font.FontColor = XLColor.FromHtml("#ffffff");
                    cell.Style.Font.Bold = true;
                    break;
                case 2:
                    // Cam
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#ffc107");
                    cell.Style.Font.FontColor = XLColor.FromHtml("#212529");
                    cell.Style.Font.Bold = true;
                    break;

                case 3:
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#61bd63");
                    cell.Style.Font.FontColor = XLColor.FromHtml("#000000");
                    cell.Style.Font.Bold = true;
                    break;
                case 4:
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#50cbde");
                    cell.Style.Font.FontColor = XLColor.FromHtml("#000000");
                    cell.Style.Font.Bold = true;
                    break;
                case 5:
                    // Xanh lá
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e7e34");
                    cell.Style.Font.FontColor = XLColor.FromHtml("#ffffff");
                    cell.Style.Font.Bold = true;
                    break;
            }
        }
    }
}
