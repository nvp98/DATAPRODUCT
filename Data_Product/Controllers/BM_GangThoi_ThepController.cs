using ClosedXML.Excel;
using Data_Product.Common;
using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using Data_Product.Services;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Layout.Element;
using iText.Layout.Font;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BM_GangThoi_ThepController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IChiaGangService _chiaGangService;

        public BM_GangThoi_ThepController(DataContext _context, ICompositeViewEngine viewEngine, IChiaGangService chiaGangService)
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
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = await _context.Tbl_TaiKhoan
                         .FirstOrDefaultAsync(x => x.TenTaiKhoan == TenTaiKhoan);

            if (TaiKhoan == null)
            {
                return Unauthorized();
            }

            var quyenLo = await (from map in _context.Tbl_BM_16_LoSanXuat_TaiKhoan
                                 join lo in _context.Tbl_BM_16_LoSanXuat on map.ID_LoSanXuat equals lo.ID
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

            

            var idPhongBan = TaiKhoan.ID_PhongBan;

            List<Tbl_LoThoi> loThoiList = new List<Tbl_LoThoi>();
            if (quyenGroup.TryGetValue(idPhongBan, out var maLoList))
            {
                loThoiList = await _context.Tbl_LoThoi.Where(x => x.BoPhan == idPhongBan && quyenGroup[idPhongBan].Contains(x.ID)).ToListAsync();
            }
            else
            {
                loThoiList = new List<Tbl_LoThoi>(); // hoặc giữ rỗng nếu không có quyền
            }

            ViewBag.LoThoiList = loThoiList;

            var loThoiSearchList = await _context.Tbl_LoThoi.ToListAsync();
            ViewBag.LoThoiSearchList = loThoiSearchList;


            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SearchLT([FromBody] SearchLTDto payload)
        {
            try
            {
                var query = _context.Tbl_BM_16_GangLong
                        .Where(x => x.T_copy == false)
                        .AsQueryable();

                if (payload.G_Ca.HasValue)
                {
                    query = query.Where(x => x.G_Ca == payload.G_Ca.Value);
                }
                if (payload.ID_Locao.HasValue)
                {
                    query = query.Where(x => x.ID_Locao == payload.ID_Locao.Value);
                }

                if (payload.ID_TrangThai.HasValue)
                {
                    if (payload.ID_TrangThai.Value == (int)TinhTrang.DaChot)
                    {
                        query = query.Where(x => x.ID_TrangThai == payload.ID_TrangThai.Value);
                    }
                    else
                    {
                        query = query.Where(x => x.T_ID_TrangThai == payload.ID_TrangThai.Value);
                    }
                }

                if (payload.ChuyenDen != null && payload.ChuyenDen.Any())
                {
                    query = query.Where(x => payload.ChuyenDen.Contains(x.ChuyenDen));
                }

                if (!string.IsNullOrEmpty(payload.MaThungGang))
                {
                    query = query.Where(x => x.MaThungGang.Contains(payload.MaThungGang));
                }

                if (!string.IsNullOrEmpty(payload.BKMIS_ThungSo))
                {
                    query = query.Where(x => x.BKMIS_ThungSo.Contains(payload.BKMIS_ThungSo));  
                }

                if (payload.ID_LoThoi.HasValue)
                {
                    query = query.Where(x => x.ID_LoThoi == payload.ID_LoThoi.Value);
                }

                if (payload.TuNgay.HasValue && payload.DenNgay.HasValue)
                {
                    var tuNgay = payload.TuNgay.Value.Date;
                    var denNgay = payload.DenNgay.Value.Date.AddDays(1);

                    query = query.Where(x => x.NgayTao >= tuNgay && x.NgayTao < denNgay);
                }

                query = query.Take(350);
                

                var res = await (from a in query
                                 join trangThai in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThai.ID
                                 join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID
                                 where a.T_copy == false
                                 orderby a.BKMIS_Gio descending, a.MaThungGang descending
                                 select new Tbl_BM_16_GangLong
                                 {
                                     ID = a.ID,
                                     MaThungGang = a.MaThungGang,
                                     BKMIS_Gio = a.BKMIS_Gio,
                                     BKMIS_SoMe = a.BKMIS_SoMe,
                                     BKMIS_ThungSo = a.BKMIS_ThungSo,
                                     NgayLuyenGang = a.NgayLuyenGang,
                                     ChuyenDen = a.ChuyenDen,
                                     Gio_NM = a.Gio_NM,
                                     G_Ca = a.G_Ca,
                                     G_KLGangLong = a.G_KLGangLong,
                                     KR = a.KR,
                                     ID_TrangThai = a.ID_TrangThai,
                                     T_ID_TrangThai = a.T_ID_TrangThai,
                                     TrangThaiLT = trangThai.TenTrangThai,
                                     ID_Locao = a.ID_Locao,
                                     TenLoCao = loCao.TenLoCao,
                                     NgayTao = a.NgayTao
                                 }).ToListAsync();


                // get bang phu de lay ten va count so lan nhan thung
                var thungUserList = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .Join(_context.Tbl_TaiKhoan,
                          thung => thung.ID_taiKhoan,
                          user => user.ID_TaiKhoan,
                          (thung, user) => new
                          {
                              thung.MaThungGang,
                              user.HoVaTen,
                              user.ID_PhongBan
                          })
                    .Join(_context.Tbl_PhongBan,
                        temp => temp.ID_PhongBan,
                        phongban => phongban.ID_PhongBan,
                        (temp, phongban) => new
                        {
                            temp.MaThungGang,
                            HoVaTen = $"{temp.HoVaTen} - {phongban.TenNgan}"
                        })
                    .ToListAsync();

                // Gom theo MaThungGang để có danh sách tên + count
                var userStats = thungUserList
                    .GroupBy(x => x.MaThungGang)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(u => u.HoVaTen)
                              .Select(x => $"{x.Key} ({x.Count()})")
                              .ToList()
                    );

                
                var data = res.Select(x => {
                    var nguoiNhanList = userStats.ContainsKey(x.MaThungGang)
                        ? userStats[x.MaThungGang]
                        : new List<string>();

                    // Nếu trạng thái đang là ChoXuLy nhưng vẫn có người giữ thùng thì hiển thị là Đã nhận
                    var t_id_trang_thai = x.T_ID_TrangThai;
                    var trang_thai_lt = x.TrangThaiLT;

                    if (x.T_ID_TrangThai == (int)TinhTrang.ChoXuLy && nguoiNhanList.Count > 0)
                    {
                        t_id_trang_thai = (int)TinhTrang.DaNhan;
                        trang_thai_lt = "Đã nhận";
                    }

                    return new
                    {
                        x.ID,
                        x.MaThungGang,
                        x.BKMIS_ThungSo,
                        x.BKMIS_Gio,
                        x.BKMIS_SoMe,
                        x.NgayLuyenGang,
                        x.ChuyenDen,
                        x.G_KLGangLong,
                        x.Gio_NM,
                        x.KR,
                        x.ID_TrangThai,
                        T_ID_TrangThai = t_id_trang_thai,
                        TrangThaiLT = trang_thai_lt,
                        x.ID_Locao,
                        x.TenLoCao,
                        x.NgayTao,
                        x.G_Ca,
                        NguoiNhanList = nguoiNhanList
                    };
                }).ToList();
                return Ok(data);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

      
        [HttpPost]
        public async Task<IActionResult> TimKiemThungDaNhan([FromBody] SearchThungDaNhanDto payload)
        {
            try
            {
                var data = await GetDanhSachThungTrungGianDaNhan(payload);
                return Ok(data);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }


        public async Task<List<ThungTrungGianRenderViewModel>> GetDanhSachThungTrungGianDaNhan_Render([FromBody] SearchThungDaNhanDto payload)
        {
            // 1) Filter TTG
            var baseQuery = _context.Tbl_BM_16_ThungTrungGian.AsNoTracking().AsQueryable();

            if (payload.NgayLamViec.HasValue)
            {
                var ngay = payload.NgayLamViec.Value.Date;
                baseQuery = baseQuery.Where(x => x.NgayNhan >= ngay && x.NgayNhan < ngay.AddDays(1));
            }
            if (payload.T_Ca.HasValue)
                baseQuery = baseQuery.Where(x => x.CaNhan == payload.T_Ca);
            if (payload.ID_LoThoi.HasValue)
                baseQuery = baseQuery.Where(x => x.ID_LoThoi == payload.ID_LoThoi);

            // 2) Lấy TTG + MaMeThoi + order theo thùng gốc
            var ttgRows = await (
                from ttg in baseQuery
                join meThoiL in _context.Tbl_MeThoi.AsNoTracking()
                    on ttg.ID_MeThoi equals meThoiL.ID into meThoiJoin
                from meThoi in meThoiJoin.DefaultIfEmpty()
                join thungGocQ in baseQuery.Where(x => !x.IsCopy)
                    on ttg.MaThungTG equals thungGocQ.MaThungTG into thungGocJoin
                from thungGoc in thungGocJoin.DefaultIfEmpty()
                orderby thungGoc.NgayTaoTTG, ttg.MaThungTG, ttg.IsCopy
                select new
                {
                    TTG = new ThungTrungGianRenderViewModel
                    {
                        ID_TTG = ttg.ID,
                        MaThungTG = ttg.MaThungTG,
                        MaThungTG_Copy = ttg.MaThungTG_Copy,
                        SoThungTG = ttg.SoThungTG,
                        GhiChu = ttg.GhiChu,
                        IsCopy = ttg.IsCopy,
                        KLThung_Thoi = ttg.KLThung_Thoi,
                        KLThungVaGang_Thoi = ttg.KLThungVaGang_Thoi,
                        KL_phe = ttg.KL_phe,
                        KLGang_Thoi = ttg.KLGang_Thoi,
                        Tong_KLGangNhan = ttg.Tong_KLGangNhan,
                        ID_MeThoi = ttg.ID_MeThoi,
                        MaMeThoi = meThoi != null ? meThoi.MaMeThoi : null,
                        NgayTaoTTG = ttg.NgayTaoTTG,
                        GioChonMe = ttg.GioChonMe,
                        DanhSachThungGang = new List<ItemRowViewModel>(),
                        Groups = new List<GroupBlockViewModel>(),
                        TtgRowspan = 0
                    },
                    ttg.MaThungTG,
                    ttg.IsCopy
                }
            ).ToListAsync();

            // 3) Lấy toàn bộ thùng gang đã gán TTG
            var gangsRaw = await _context.Tbl_BM_16_GangLong.AsNoTracking()
                .Where(x => x.ID_TTG.HasValue)
                .Select(x => new
                {
                    x.ID_TTG,
                    Gang = new ItemRowViewModel
                    {
                        ID = x.ID,
                        MaThungGang = x.MaThungGang,
                        MaThungThep = x.MaThungThep,
                        BKMIS_ThungSo = x.BKMIS_ThungSo,
                        ID_LoCao = x.ID_Locao,
                        ID_TrangThai = x.ID_TrangThai,
                        T_KLGangLong = x.T_KLGangLong,
                        T_KLThungChua = x.T_KLThungChua,
                        T_KLThungVaGang = x.T_KLThungVaGang,
                        G_ID_NguoiChuyen = x.G_ID_NguoiChuyen,
                        G_ID_NguoiLuu = x.G_ID_NguoiLuu,
                        ChuyenDen = x.ChuyenDen,
                        BKMIS_SoMe = x.BKMIS_SoMe,
                        BKMIS_Gio = x.BKMIS_Gio,
                        NgayTaoG = x.NgayTao,
                        KLGangChia = x.KLGangChia,
                        T_ID_NguoiNhan = x.T_ID_NguoiNhan,
                        MaChiaGang = null
                    }
                })
                .ToListAsync();

            // 4) Rút MaChiaGang từ bảng ChiaGang theo ID_Thung / MaThungThep
            static string? Norm(string? s)
                => string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToUpperInvariant();

            var thungIds = gangsRaw.Select(g => g.Gang.ID).Distinct().ToList();
            var thepCodes = gangsRaw.Select(g => g.Gang.MaThungThep)
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToList();

            var chiaRows = await _context.Tbl_BM_16_ChiaGang.AsNoTracking()
                .Where(cg =>
                    (cg.ID_Thung.HasValue && thungIds.Contains(cg.ID_Thung.Value)) ||
                    (cg.MaThungThep != null && thepCodes.Contains(cg.MaThungThep)))
                .Select(cg => new { cg.MaChiaGang, cg.ID_Thung, cg.MaThungThep })
                .ToListAsync();

            var maByThungId = chiaRows.Where(r => r.ID_Thung.HasValue)
                .GroupBy(r => r.ID_Thung!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.MaChiaGang).First());

            var maByThep = chiaRows.Where(r => !r.ID_Thung.HasValue && r.MaThungThep != null)
                .GroupBy(r => r.MaThungThep!)
                .ToDictionary(g => g.Key, g => g.Select(x => x.MaChiaGang).First(), StringComparer.OrdinalIgnoreCase);

            var gangsEnriched = gangsRaw.Select(r =>
            {
                string? ma = null;
                if (!maByThungId.TryGetValue(r.Gang.ID, out ma) &&
                    !string.IsNullOrEmpty(r.Gang.MaThungThep))
                {
                    maByThep.TryGetValue(r.Gang.MaThungThep!, out ma);
                }
                return new GangEnriched
                {
                    ID_TTG = r.ID_TTG!.Value,
                    Item = r.Gang,
                    MaChiaGang = Norm(ma)
                };
            }).ToList();

            var itemsByTtg = gangsEnriched
                .GroupBy(x => x.ID_TTG)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 5) TTG copy dùng chung items với TTG gốc
            var originIdByMaThung = ttgRows
                .Where(x => !x.IsCopy)
                .GroupBy(x => x.MaThungTG)
                .ToDictionary(g => g.Key!, g => g.Select(x => x.TTG.ID_TTG).First());

            // ====== SẮP XẾP/ NHÓM TOÀN CỤC THEO MaChiaGang ======

            // 5.1) Nhóm toàn cục theo MaChiaGang (không rỗng)
            var groupsByKey = gangsEnriched
                .Where(z => !string.IsNullOrWhiteSpace(z.MaChiaGang))
                .GroupBy(z => z.MaChiaGang!, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // chỉ số nhóm
            var groupIndex = groupsByKey
                .Select((g, idx) => new { g.Key, idx })
                .ToDictionary(x => x.Key, x => x.idx, StringComparer.OrdinalIgnoreCase);

            // thứ tự toàn cục cho từng item (ưu tiên theo nhóm -> NgayTaoG -> ID)
            var itemGlobalOrder = new Dictionary<int, (int groupIdx, long local)>();
            long running = 0;
            foreach (var g in groupsByKey)
            {
                foreach (var it in g
                    .OrderBy(z => z.Item.NgayTaoG)
                    .ThenBy(z => z.Item.ID))
                {
                    itemGlobalOrder[it.Item.ID] = (groupIndex[g.Key], running++);
                }
            }

            // đặt thứ tự cho item KHÔNG có MaChiaGang (đẩy sau cùng, ổn định trong TTG)
            long tail = long.MaxValue / 2;
            foreach (var it in gangsEnriched
                         .Where(z => string.IsNullOrWhiteSpace(z.MaChiaGang))
                         .OrderBy(z => z.ID_TTG)
                         .ThenBy(z => z.Item.NgayTaoG)
                         .ThenBy(z => z.Item.ID))
            {
                itemGlobalOrder[it.Item.ID] = (int.MaxValue, tail++);
            }

            int EffectiveTtgId(string? maThungTG, bool isCopy, int ttgId)
                => (isCopy && !string.IsNullOrEmpty(maThungTG) && originIdByMaThung.TryGetValue(maThungTG, out var gid)) ? gid : ttgId;

            int DominantGroupIdxForTtg(int effectiveTtgId)
            {
                if (!itemsByTtg.TryGetValue(effectiveTtgId, out var list) || list.Count == 0)
                    return int.MaxValue;

                var best = list
                    .Where(z => !string.IsNullOrWhiteSpace(z.MaChiaGang))
                    .GroupBy(z => z.MaChiaGang!, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new { Key = g.Key, Count = g.Count(), Idx = groupIndex[g.Key] })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Idx)
                    .FirstOrDefault();

                return best?.Idx ?? int.MaxValue;
            }

            // 6) Build output theo thứ tự cụm mã
            var render = new List<ThungTrungGianRenderViewModel>();

            foreach (var x in ttgRows
                .OrderBy(o => DominantGroupIdxForTtg(EffectiveTtgId(o.MaThungTG, o.IsCopy, o.TTG.ID_TTG)))
                .ThenBy(o => o.TTG.NgayTaoTTG)
                .ThenBy(o => o.TTG.MaThungTG)
                .ThenBy(o => o.IsCopy))
            {
                var vm = x.TTG;

                // lấy list enriched cho TTG (copy -> dùng của gốc)
                List<GangEnriched> enriched;
                if (x.IsCopy && x.MaThungTG != null
                    && originIdByMaThung.TryGetValue(x.MaThungTG, out var originId)
                    && itemsByTtg.TryGetValue(originId, out var originItems))
                {
                    enriched = originItems;
                }
                else if (!itemsByTtg.TryGetValue(vm.ID_TTG, out enriched))
                {
                    enriched = new List<GangEnriched>();
                }

                // sắp xếp item trong TTG theo thứ tự toàn cục (đảm bảo cùng MaChiaGang đứng liền nhau)
                enriched = enriched
                    .OrderBy(e => itemGlobalOrder[e.Item.ID].groupIdx)
                    .ThenBy(e => itemGlobalOrder[e.Item.ID].local)
                    .ToList();

                // tách KHÔNG MÃ
                var noMa = enriched
                    .Where(i => string.IsNullOrWhiteSpace(i.MaChiaGang))
                    .Select(i => { i.Item.MaChiaGang = null; return i.Item; })
                    .OrderBy(i => i.MaThungThep)
                    .ThenBy(i => i.ID)
                    .ToList();

                // score để ưu tiên hàng giàu dữ liệu đứng đầu nhóm
                static int Score(ItemRowViewModel it)
                {
                    int s = 0;
                    if (it.T_KLThungVaGang.HasValue && it.T_KLThungVaGang.Value > 0) s++;
                    if (it.T_KLThungChua.HasValue && it.T_KLThungChua.Value > 0) s++;
                    if (it.T_KLGangLong.HasValue && it.T_KLGangLong.Value > 0) s++;
                    return s;
                }

                var coMaGroups = enriched
                    .Where(i => !string.IsNullOrWhiteSpace(i.MaChiaGang))
                    .GroupBy(i => i.MaChiaGang!, StringComparer.OrdinalIgnoreCase)
                    .OrderBy(g => groupIndex[g.Key])
                    .Select(g => new
                    {
                        Ma = g.Key,
                        Items = g.Select(z => { z.Item.MaChiaGang = g.Key; return z.Item; })
                                 .OrderByDescending(it => Score(it))
                                 .ThenBy(it => it.MaThungThep)
                                 .ThenBy(it => it.ID)
                                 .ToList()
                    })
                    .ToList();

                // build Groups (nếu cần)
                var groups = new List<GroupBlockViewModel>();
                foreach (var it in noMa)
                {
                    groups.Add(new GroupBlockViewModel
                    {
                        MaChiaGang = "",
                        GroupRowspan = 1,
                        Values = new GroupValuesViewModel
                        {
                            T_KLThungVaGang = it.T_KLThungVaGang,
                            T_KLThungChua = it.T_KLThungChua,
                            T_KLGangLong = it.T_KLGangLong
                        },
                        Items = new List<ItemRowViewModel> { it }
                    });
                }
                foreach (var g in coMaGroups)
                {
                    var first = g.Items[0];
                    groups.Add(new GroupBlockViewModel
                    {
                        MaChiaGang = g.Ma,
                        GroupRowspan = g.Items.Count,
                        Values = new GroupValuesViewModel
                        {
                            T_KLThungVaGang = first.T_KLThungVaGang,
                            T_KLThungChua = first.T_KLThungChua,
                            T_KLGangLong = first.T_KLGangLong
                        },
                        Items = g.Items
                    });
                }
                vm.Groups = groups;

                // phẳng: KHÔNG MÃ trước, rồi từng nhóm có MÃ theo groupIndex
                var flat = new List<ItemRowViewModel>();
                flat.AddRange(noMa);
                foreach (var g in coMaGroups) flat.AddRange(g.Items);

                vm.DanhSachThungGang = flat;
                vm.TtgRowspan = flat.Count;

                render.Add(vm);
            }

            return render;
        }

        public async Task<List<ThungTrungGianGroupViewModel>> GetDanhSachThungTrungGianDaNhan(SearchThungDaNhanDto payload)
        {
            // ===== 1) Filter TTG =====
            var ttgQuery = _context.Tbl_BM_16_ThungTrungGian.AsQueryable();
            if (payload.NgayLamViec.HasValue)
            {
                var ngay = payload.NgayLamViec.Value.Date;
                ttgQuery = ttgQuery.Where(x => x.NgayNhan >= ngay && x.NgayNhan < ngay.AddDays(1));
            }
            if (payload.T_Ca.HasValue) ttgQuery = ttgQuery.Where(x => x.CaNhan == payload.T_Ca);
            if (payload.ID_LoThoi.HasValue) ttgQuery = ttgQuery.Where(x => x.ID_LoThoi == payload.ID_LoThoi);

            // ===== 2) TTG + Mẻ thổi =====
            var thungList = await (
                from ttg in ttgQuery
                join meThoi in _context.Tbl_MeThoi on ttg.ID_MeThoi equals meThoi.ID into meThoiJoin
                from meThoi in meThoiJoin.DefaultIfEmpty()
                select new ThungTrungGianGroupViewModel
                {
                    ID_TTG = ttg.ID,
                    MaThungTG = ttg.MaThungTG,
                    MaThungTG_Copy = ttg.MaThungTG_Copy,
                    SoThungTG = ttg.SoThungTG,
                    GhiChu = ttg.GhiChu,
                    IsCopy = ttg.IsCopy,
                    KLThung_Thoi = ttg.KLThung_Thoi,
                    KLThungVaGang_Thoi = ttg.KLThungVaGang_Thoi,
                    KL_phe = ttg.KL_phe,
                    KLGang_Thoi = ttg.KLGang_Thoi,
                    Tong_KLGangNhan = ttg.Tong_KLGangNhan,
                    ID_MeThoi = ttg.ID_MeThoi,
                    MaMeThoi = meThoi != null ? meThoi.MaMeThoi : null,
                    NgayTaoTTG = ttg.NgayTaoTTG,
                    GioChonMe = ttg.GioChonMe
                }
            ).AsNoTracking().ToListAsync();

            if (thungList.Count == 0) return thungList;

            // ===== 3) MaChiaGang mới nhất của từng thùng =====
            var lastChiaGangPerThung = await _context.Tbl_BM_16_ChiaGang
                .GroupBy(c => c.ID_Thung)
                .Select(g => new { ID_Thung = g.Key, MaChiaGang = g.OrderByDescending(x => x.ID).Select(x => x.MaChiaGang).FirstOrDefault() })
                .ToListAsync();
            var chiaGangDict = lastChiaGangPerThung.ToDictionary(x => x.ID_Thung, x => x.MaChiaGang);

            // ===== 4) Load thùng gang (kèm T_ReceiveSeq) =====
            var gangList = await _context.Tbl_BM_16_GangLong
                .Where(x => x.ID_TTG.HasValue)
                .Select(x => new
                {
                    x.ID_TTG,
                    Gang = new GangLongItemViewModel
                    {
                        ID = x.ID,
                        MaThungGang = x.MaThungGang,
                        MaThungThep = x.MaThungThep,
                        BKMIS_ThungSo = x.BKMIS_ThungSo,
                        ID_LoCao = x.ID_Locao,
                        ID_TrangThai = x.ID_TrangThai,
                        T_KLGangLong = x.T_KLGangLong,
                        T_KLThungChua = x.T_KLThungChua,
                        T_KLThungVaGang = x.T_KLThungVaGang,
                        G_ID_NguoiChuyen = x.G_ID_NguoiChuyen,
                        G_ID_NguoiLuu = x.G_ID_NguoiLuu,
                        ChuyenDen = x.ChuyenDen,
                        BKMIS_SoMe = x.BKMIS_SoMe,
                        BKMIS_Gio = x.BKMIS_Gio,
                        NgayTaoG = x.NgayTao,
                        KLGangChia = x.KLGangChia,
                        T_ID_NguoiNhan = x.T_ID_NguoiNhan,
                        T_ReceiveSeq = x.T_ReceiveSeq, // có thể null
                        MaChiaGang = null
                    }
                })
                .AsNoTracking()
                .ToListAsync();

            foreach (var g in gangList)
            {
                if (chiaGangDict.TryGetValue(g.Gang.ID, out var m)) g.Gang.MaChiaGang = m;
                if (!g.Gang.T_ReceiveSeq.HasValue) g.Gang.T_ReceiveSeq = g.Gang.ID; // fallback ổn định
            }

            // ===== 5) Group theo TTG =====
            var gangByTtg = gangList
                .GroupBy(x => x.ID_TTG!.Value)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Gang).ToList());

            foreach (var ttg in thungList)
                if (!ttg.IsCopy && gangByTtg.TryGetValue(ttg.ID_TTG, out var list))
                    ttg.DanhSachThungGang = list;

            // TTG copy: clone hiển thị từ gốc
            foreach (var ttg in thungList.Where(x => x.IsCopy))
            {
                var goc = thungList.FirstOrDefault(x => !x.IsCopy && x.MaThungTG == ttg.MaThungTG);
                if (goc?.DanhSachThungGang != null)
                    ttg.DanhSachThungGang = goc.DanhSachThungGang
                        .Select(x => new GangLongItemViewModel
                        {
                            ID = x.ID,
                            MaThungGang = x.MaThungGang,
                            MaThungThep = x.MaThungThep,
                            BKMIS_ThungSo = x.BKMIS_ThungSo,
                            ID_LoCao = x.ID_LoCao,
                            ID_TrangThai = x.ID_TrangThai,
                            T_KLGangLong = x.T_KLGangLong,
                            T_KLThungChua = x.T_KLThungChua,
                            T_KLThungVaGang = x.T_KLThungVaGang,
                            G_ID_NguoiChuyen = x.G_ID_NguoiChuyen,
                            G_ID_NguoiLuu = x.G_ID_NguoiLuu,
                            ChuyenDen = x.ChuyenDen,
                            BKMIS_SoMe = x.BKMIS_SoMe,
                            BKMIS_Gio = x.BKMIS_Gio,
                            NgayTaoG = x.NgayTaoG,
                            KLGangChia = x.KLGangChia,
                            T_ID_NguoiNhan = x.T_ID_NguoiNhan,
                            T_ReceiveSeq = x.T_ReceiveSeq,
                            MaChiaGang = x.MaChiaGang
                        }).ToList();
            }

            // ===== Helpers (local) =====
            static string Norm(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToUpperInvariant();

            static List<GangLongItemViewModel> OrderInsideTtgAnchorFirst(IEnumerable<GangLongItemViewModel> items)
            {
                var list = items.OrderBy(x => x.T_ReceiveSeq ?? int.MaxValue).ThenBy(x => x.ID).ToList();

                var group = list.Where(x => !string.IsNullOrWhiteSpace(x.MaChiaGang))
                                .GroupBy(x => Norm(x.MaChiaGang))
                                .ToDictionary(g => g.Key, g => g.OrderBy(y => y.T_ReceiveSeq).ThenBy(y => y.ID).ToList());

                var used = new HashSet<int>();
                var seenKey = new HashSet<string>();
                var result = new List<GangLongItemViewModel>(list.Count);

                foreach (var row in list)
                {
                    if (used.Contains(row.ID)) continue;
                    var k = Norm(row.MaChiaGang);
                    if (k == null || !group.ContainsKey(k) || seenKey.Contains(k))
                    {
                        result.Add(row); used.Add(row.ID);
                    }
                    else
                    {
                        seenKey.Add(k);
                        foreach (var m in group[k])
                            if (used.Add(m.ID)) result.Add(m);
                    }
                }
                return result;
            }

            // ===== 6) Sắp trong phạm vi mỗi TTG (anchor-first) =====
            foreach (var ttg in thungList)
                if (ttg.DanhSachThungGang?.Count > 1)
                    ttg.DanhSachThungGang = OrderInsideTtgAnchorFirst(ttg.DanhSachThungGang);

            // ===== 7) Xếp theo "family TTG" để copy luôn liền kề gốc =====
            // Tạo danh sách family (ẩn danh)
            var families = thungList
                .GroupBy(ttg => ttg.MaThungTG ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    var origin = g.FirstOrDefault(x => !x.IsCopy) ?? g.First();
                    var minSeq = origin.DanhSachThungGang?.Min(x => x.T_ReceiveSeq ?? int.MaxValue) ?? int.MaxValue;

                    var allBlockKeys = g
                        .SelectMany(x => x.DanhSachThungGang ?? Enumerable.Empty<GangLongItemViewModel>())
                        .Select(x => Norm(x.MaChiaGang))
                        .Where(k => k != null)
                        .Distinct()
                        .ToList();

                    var items = g.OrderBy(x => x.IsCopy ? 1 : 0)
                                 .ThenBy(x => x.NgayTaoTTG)
                                 .ThenBy(x => x.ID_TTG)
                                 .ToList();

                    return new
                    {
                        Key = g.Key,
                        Origin = origin,
                        Items = items,
                        HasMe = string.IsNullOrEmpty(origin.MaMeThoi) ? 1 : 0,
                        origin.MaMeThoi,
                        MinSeq = minSeq,
                        Ngay = origin.NgayTaoTTG,
                        BlockKeys = allBlockKeys
                    };
                })
                .ToList();

            // Base order family: có mẻ -> MaMeThoi -> minSeq -> ngày
            var famBase = families
                .OrderBy(f => f.HasMe)
                .ThenBy(f => f.MaMeThoi)
                .ThenBy(f => f.MinSeq)
                .ThenBy(f => f.Ngay)
                .ToList();

            // ===== 8) Neo giữa các family theo MaChiaGang trùng =====
            var keyToFamilies = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            for (int idx = 0; idx < famBase.Count; idx++)
            {
                var f = famBase[idx];
                foreach (var k in f.BlockKeys)
                {
                    if (!keyToFamilies.TryGetValue(k, out var list)) keyToFamilies[k] = list = new List<int>();
                    if (list.Count == 0 || list[^1] != idx) list.Add(idx);
                }
            }
            var dupKeys = new HashSet<string>(keyToFamilies.Where(p => p.Value.Count > 1).Select(p => p.Key), StringComparer.OrdinalIgnoreCase);

            var emittedFamily = new HashSet<int>();
            var emittedKey = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var finalFamiliesIdx = new List<int>(famBase.Count);

            for (int i = 0; i < famBase.Count; i++)
            {
                if (emittedFamily.Contains(i)) continue;
                var f = famBase[i];

                string pivot = f.BlockKeys.FirstOrDefault(k => dupKeys.Contains(k) && !emittedKey.Contains(k));

                if (pivot != null)
                {
                    foreach (var fi in keyToFamilies[pivot])
                        if (emittedFamily.Add(fi)) finalFamiliesIdx.Add(fi);
                    emittedKey.Add(pivot);
                }
                else
                {
                    if (emittedFamily.Add(i)) finalFamiliesIdx.Add(i);
                }
            }
            // bảo hiểm nếu còn family rơi rớt
            for (int i = 0; i < famBase.Count; i++)
                if (emittedFamily.Add(i)) finalFamiliesIdx.Add(i);

            // ===== 9) Trả TTG theo thứ tự mới (copy luôn kề gốc) =====
            var finalBlocks = new List<ThungTrungGianGroupViewModel>(thungList.Count);
            foreach (var fi in finalFamiliesIdx)
                finalBlocks.AddRange(famBase[fi].Items);

            return finalBlocks;
        }

        private async Task<int> TaoThungTrungGian(DateTime ngayNhan, int idCa, int idLoThoi, string SoThungTG, int ID_NguoiNhan)
        {
            string maTTG = "T" + TaoMa.GenerateSafeCode(8);
            var thungTrungGian = new Tbl_BM_16_ThungTrungGian
            {
                MaThungTG = maTTG,
                NgayNhan = ngayNhan,
                CaNhan = idCa,
                SoThungTG = SoThungTG,
                ID_LoThoi = idLoThoi,
                IsCopy = false,
                ID_NguoiNhan = ID_NguoiNhan,
                NgayTaoTTG = DateTime.Now
            };
            _context.Tbl_BM_16_ThungTrungGian.Add(thungTrungGian);
            await _context.SaveChangesAsync();
            return thungTrungGian.ID;
        }

        [HttpPost]
        public async Task<IActionResult> CheckNhan([FromBody] List<string> selectedThungs)
        {
            try
            {
                var isInValid = await _context.Tbl_BM_16_GangLong
                                      .Where(x => selectedThungs.Contains(x.MaThungGang) && x.T_copy == false)
                                      .AnyAsync(x => x.ID_TrangThai == (int)TinhTrang.DaChot);
                if (isInValid)
                {
                    return Ok(new { isValid = false });
                }
                return Ok(new { isValid = true });
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Nhan([FromBody] NhanThungDto payload)
        {
            if (payload == null || payload.selectedThungs == null || payload.selectedThungs.Count == 0)
                return BadRequest("Danh sách thùng trống.");
            if (payload.idCa <= 0 || payload.idLoThoi <= 0 || payload.idNguoiNhan <= 0)
                return BadRequest("Thiếu thông tin ca, lò thổi hoặc người nhận.");

            // Chuẩn hóa + sắp thứ tự từ client
            var orderedSelected = payload.selectedThungs
                .Where(x => !string.IsNullOrWhiteSpace(x.maThungGang))
                .GroupBy(x => x.maThungGang.Trim())
                .Select(g => g.OrderBy(z => z.clientSeq).First())
                .OrderBy(x => x.clientSeq)
                .ThenBy(x => x.maThungGang)
                .ToList();

            if (orderedSelected.Count == 0)
                return BadRequest("Không có mã thùng hợp lệ.");

            try
            {
                await using var tran = await _context.Database.BeginTransactionAsync();

                // Khóa theo scope để tuần tự hóa cấp sequence
                var lockResource = $"NHAN:{payload.ngayNhan:yyyyMMdd}|{payload.idCa}|{payload.idLoThoi}";
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', @LockOwner='Transaction', @LockTimeout = 15000",
                    lockResource);

                // Kíp
                var kip = await (from a in _context.Tbl_Kip
                                 where a.NgayLamViec == payload.ngayNhan
                                    && a.TenCa == payload.idCa.ToString()
                                 select new Tbl_Kip { ID_Kip = a.ID_Kip, TenKip = a.TenKip })
                                .FirstOrDefaultAsync();
                if (kip == null) return BadRequest("Không xác định được kíp cho ngày/ca.");

                // Lấy max seq đang có trong scope
                var currentMaxSeq = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.NgayLuyenThep == payload.ngayNhan
                             && x.T_Ca == payload.idCa
                             && x.ID_LoThoi == payload.idLoThoi
                             && x.T_ReceiveSeq != null)
                    .MaxAsync(x => (int?)x.T_ReceiveSeq) ?? 0;

                // Common TTG nếu có
                int? idThungTG_Common = null;
                if (!string.IsNullOrWhiteSpace(payload.thungTrungGian))
                {
                    idThungTG_Common = await TaoThungTrungGian(
                        payload.ngayNhan, payload.idCa, payload.idLoThoi,
                        payload.thungTrungGian, payload.idNguoiNhan);
                }

                // Cache các bản gốc theo mã để giảm query
                var maSet = orderedSelected.Select(x => x.maThungGang.Trim()).ToHashSet();
                var baseThungs = await _context.Tbl_BM_16_GangLong
                    .Where(x => maSet.Contains(x.MaThungGang) && x.T_copy == false)
                    .ToListAsync();

                var baseByMa = baseThungs
                    .GroupBy(t => t.MaThungGang)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var sel in orderedSelected)
                {
                    var ma = sel.maThungGang.Trim();

                    if (!baseByMa.TryGetValue(ma, out var t))
                    {
                        t = await _context.Tbl_BM_16_GangLong
                                .Where(x => x.MaThungGang == ma && x.T_copy == false)
                                .FirstOrDefaultAsync();
                        if (t == null) continue; // hoặc throw nếu cần
                        baseByMa[ma] = t;
                    }

                    // Tính maThungThep kế tiếp
                    var allThungs = await _context.Tbl_BM_16_GangLong
                                        .Where(x => x.MaThungGang == t.MaThungGang)
                                        .ToListAsync();

                    string maThungThep;
                    var thungGoc = allThungs.FirstOrDefault(x => x.T_copy == false);

                    if (string.IsNullOrEmpty(thungGoc?.MaThungThep))
                    {
                        maThungThep = GenerateMaThungThep(
                            t.MaThungGang, payload.ngayNhan, payload.idLoThoi, payload.idCa, 0);
                    }
                    else
                    {
                        var usedIndexes = allThungs
                            .Where(x => !string.IsNullOrEmpty(x.MaThungThep))
                            .Select(x =>
                            {
                                var parts = x.MaThungThep.Split('.');
                                return int.TryParse(parts.LastOrDefault(), out int idx) ? idx : -1;
                            })
                            .Where(idx => idx >= 0)
                            .OrderBy(i => i)
                            .ToList();

                        int nextIndex = 0;
                        while (usedIndexes.Contains(nextIndex)) nextIndex++;

                        maThungThep = GenerateMaThungThep(
                            t.MaThungGang, payload.ngayNhan, payload.idLoThoi, payload.idCa, nextIndex);
                    }

                    int idThungTG = idThungTG_Common
                                    ?? await TaoThungTrungGian(payload.ngayNhan, payload.idCa, payload.idLoThoi,
                                                               t.BKMIS_ThungSo, payload.idNguoiNhan);

                    // Ghi nhận người nhận
                    _context.Tbl_BM_16_TaiKhoan_Thung.Add(new Tbl_BM_16_TaiKhoan_Thung
                    {
                        ID_taiKhoan = payload.idNguoiNhan,
                        MaThungGang = t.MaThungGang,
                        MaThungThep = maThungThep,
                        MaPhieu = t.MaPhieu
                    });

                    // Cấp sequence mới cho lần nhận này (không ghi đè nếu bản ghi đã có seq trong đúng scope)
                    int nextSeq = currentMaxSeq + 1;

                    if (t.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                    {
                        // Case đặc biệt: DUC1/DUC2 & chưa có MaThungThep -> chỉ update bản gốc rồi continue
                        if ((t.ChuyenDen == "DUC1" || t.ChuyenDen == "DUC2") && t.MaThungThep == null)
                        {
                            t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                            t.MaThungThep = maThungThep;
                            t.ID_LoThoi = payload.idLoThoi;
                            t.T_Ca = payload.idCa;
                            t.NgayLuyenThep = payload.ngayNhan;
                            t.T_ID_Kip = kip.ID_Kip;
                            t.ID_TTG = idThungTG;
                            t.T_ID_NguoiNhan = payload.idNguoiNhan;

                            if (t.T_ReceiveSeq == null &&
                                t.NgayLuyenThep == payload.ngayNhan &&
                                t.T_Ca == payload.idCa &&
                                t.ID_LoThoi == payload.idLoThoi)
                            {
                                t.T_ReceiveSeq = nextSeq;
                                currentMaxSeq = nextSeq;
                            }
                            continue;
                        }

                        // Các trường hợp “đã nhận” còn lại -> tạo clone
                        var clone = new Tbl_BM_16_GangLong
                        {
                            MaThungGang = t.MaThungGang,
                            BKMIS_SoMe = t.BKMIS_SoMe,
                            BKMIS_Gio = t.BKMIS_Gio,
                            BKMIS_PhanLoai = t.BKMIS_PhanLoai,
                            BKMIS_ThungSo = t.BKMIS_ThungSo,
                            NgayLuyenGang = t.NgayLuyenGang,
                            G_KLXeThungVaGang = t.G_KLXeThungVaGang,
                            G_KLXeVaThung = t.G_KLXeVaThung,
                            KL_XeGoong = t.KL_XeGoong,
                            G_KLThungChua = t.G_KLThungChua,
                            G_KLThungVaGang = t.G_KLThungVaGang,
                            G_KLGangLong = t.G_KLGangLong,
                            ChuyenDen = t.ChuyenDen,
                            Gio_NM = t.Gio_NM,
                            KR = t.KR,
                            G_GhiChu = t.G_GhiChu,
                            G_Ca = t.G_Ca,
                            G_ID_Kip = t.G_ID_Kip,
                            G_ID_NguoiChuyen = t.G_ID_NguoiChuyen,
                            G_ID_NguoiLuu = t.G_ID_NguoiLuu,
                            G_ID_NguoiThuHoi = t.G_ID_NguoiThuHoi,
                            G_ID_TrangThai = t.G_ID_TrangThai,
                            ID_TrangThai = t.ID_TrangThai,
                            T_ID_TrangThai = (int)TinhTrang.DaNhan,
                            ID_Locao = t.ID_Locao,
                            ID_Phieu = t.ID_Phieu,
                            MaPhieu = t.MaPhieu,
                            NgayTao = t.NgayTao,
                            T_copy = true,
                            MaThungThep = maThungThep,
                            ID_LoThoi = payload.idLoThoi,
                            T_Ca = payload.idCa,
                            NgayLuyenThep = payload.ngayNhan,
                            T_ID_NguoiNhan = payload.idNguoiNhan,
                            T_ID_Kip = kip.ID_Kip,
                            ID_TTG = idThungTG,
                            T_ReceiveSeq = nextSeq
                        };
                        _context.Tbl_BM_16_GangLong.Add(clone);
                        currentMaxSeq = nextSeq;
                    }
                    else
                    {
                        // Thùng chưa nhận -> cập nhật sang Đã nhận (và gán seq nếu cùng scope)
                        t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                        t.T_ID_NguoiNhan = payload.idNguoiNhan;
                        t.MaThungThep = maThungThep;
                        t.ID_LoThoi = payload.idLoThoi;
                        t.T_Ca = payload.idCa;
                        t.NgayLuyenThep = payload.ngayNhan;
                        t.T_ID_Kip = kip.ID_Kip;
                        t.ID_TTG = idThungTG;

                        if (t.T_ReceiveSeq == null &&
                            t.NgayLuyenThep == payload.ngayNhan &&
                            t.T_Ca == payload.idCa &&
                            t.ID_LoThoi == payload.idLoThoi)
                        {
                            t.T_ReceiveSeq = nextSeq;
                            currentMaxSeq = nextSeq;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return Ok(new { Message = "Đã xử lý nhận thùng.", Soluong = orderedSelected.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }



        [HttpPost]
        public async Task<IActionResult> CheckValidXoaThungTGCopy([FromBody] string maThungTGCopy)
        {
            try
            {
                var thungTGCopy = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.MaThungTG_Copy == maThungTGCopy && x.IsCopy == true).FirstOrDefaultAsync();

                if (thungTGCopy == null)
                    return Ok(new { isValid = true });

                var thungTGGoc = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.MaThungTG == thungTGCopy.MaThungTG && x.IsCopy == false).FirstOrDefaultAsync();

                var isInvalid = await _context.Tbl_BM_16_GangLong.Where(x => x.ID_TTG == thungTGGoc.ID).AnyAsync(x => x.ID_TrangThai == (int)TinhTrang.DaChot);

                if (isInvalid)
                {
                    return Ok(new { isValid = false });
                }

                return Ok(new { isValid = true });
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> XoaThungCopy([FromBody] string selectedMaThung)
        {
            try
            {
                if (selectedMaThung == null || string.IsNullOrEmpty(selectedMaThung))
                    return BadRequest("Mã thùng copy trống.");

                var thung = await _context.Tbl_BM_16_ThungTrungGian.FirstOrDefaultAsync(x => x.MaThungTG_Copy == selectedMaThung);
                if (thung != null)
                {
                    _context.Tbl_BM_16_ThungTrungGian.Remove(thung);
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, hasThung = true });
                }
                return Ok(new { success = true, hasThung = false });
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckValidCopyThungTG([FromBody] string maThungTG)
        {
            try 
            { 
                var thungTG = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.MaThungTG == maThungTG && x.IsCopy == false).FirstOrDefaultAsync();

                var isInvalid = await _context.Tbl_BM_16_GangLong.Where(x => x.ID_TTG == thungTG.ID).AnyAsync(x => x.ID_TrangThai == (int)TinhTrang.DaChot);

                if (isInvalid)
                {
                    return Ok(new { isValid = false });
                }

                return Ok(new { isValid  = true});
            }
            catch (Exception ex)
            {
                    TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                    return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> LuuKR([FromBody] List<LuuKRDto> selectedList)
        {
            if (selectedList == null || selectedList.Count == 0)
                return BadRequest("Danh sách ID trống.");
            try 
            { 
                var selectedMaThungs = selectedList
                                    .Select(x => x.maThungGang)
                                    .ToList();

                //var isInvalid = await _context.Tbl_BM_16_GangLong
                //                           .Where(x => selectedMaThungs.Contains(x.MaThungGang)).AnyAsync(x => x.ID_TrangThai == (int)TinhTrang.DaChot);
                //if (isInvalid)
                //{
                //    return Ok(new { isValid = false });
                //}

                // Lấy tất cả các thùng cần xử lý
                var thungs = await _context.Tbl_BM_16_GangLong
                                           .Where(x => selectedMaThungs.Contains(x.MaThungGang) && x.ID_TrangThai != (int)TinhTrang.DaChot)
                                           .ToListAsync();

                if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");

                foreach (var t in thungs)
                {
                    t.KR = selectedList.Find(x => x.maThungGang == t.MaThungGang).isChecked;
                }

                await _context.SaveChangesAsync();
                return Ok(new { isValid = true });
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> GetThungHuyNhan([FromBody] List<string> selectedMaThungHuy)
        {
            try
            {
                var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var taiKhoan = await _context.Tbl_TaiKhoan
                    .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);

                if (taiKhoan == null)
                    return BadRequest("Không tìm thấy tài khoản.");

                var isInvalid = await _context.Tbl_BM_16_GangLong.Where(x => selectedMaThungHuy.Contains(x.MaThungGang)).AnyAsync(x => x.ID_TrangThai == (int)TinhTrang.DaChot);

                if (isInvalid)
                {
                    return Ok(new { data = new List<object>(), truongHop = 1 });
                }

                var allThung = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .Where(x => selectedMaThungHuy.Contains(x.MaThungGang))
                    .ToListAsync();

                bool isValid = selectedMaThungHuy.All(maThung => allThung.Any(x => x.MaThungGang == maThung && x.ID_taiKhoan == taiKhoan.ID_TaiKhoan));

                if (isValid)
                {
                    var gangRecords = await _context.Tbl_BM_16_GangLong
                .Where(x => selectedMaThungHuy.Contains(x.MaThungGang)
                            && x.T_ID_NguoiNhan == taiKhoan.ID_TaiKhoan)
                .ToListAsync();

                    var result = selectedMaThungHuy.Select(ma =>
                    {
                        var subset = gangRecords.Where(g => g.MaThungGang == ma).ToList();

                        var detail = subset
                            .GroupBy(g => new { g.T_Ca, Ngay = g.NgayLuyenThep?.Date })
                            .Select(g => new
                            {
                                ca = g.Key.T_Ca,
                                ngay = g.Key.Ngay,
                                soLan = g.Count()
                            })
                            .OrderBy(x => x.ngay).ThenBy(x => x.ca)
                            .ToList();

                        return new
                        {
                            maThungGang = ma,
                            chiTietCaNgay = detail
                        };
                    }).ToList();

                    return Ok(new { data = result, truongHop = 2 });
                }
                else
                {
                    return Ok(new { data = new List<object>(), truongHop = 0 });
                }
                
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }


        // fix lần 2
        [HttpPost]
        public async Task<IActionResult> HuyNhan([FromBody] HuyNhanThungDto payload)
        {
            if (payload.selectedMaThungs == null || !payload.selectedMaThungs.Any() || payload.idNguoiHuyNhan == null)
                return BadRequest("Danh sách ID trống.");

            try
            {
                using var tran = await _context.Database.BeginTransactionAsync();

                var danhSachPhu = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .Where(x => payload.selectedMaThungs.Contains(x.MaThungGang) && x.ID_taiKhoan == payload.idNguoiHuyNhan)
                    .ToListAsync();

                var soDongXoa = await _chiaGangService.HuyChiaGangTheoNhieuThungGangAsync(payload.selectedMaThungs, payload.idNguoiHuyNhan);

                var dsMaThep = danhSachPhu.Select(x => x.MaThungThep).Distinct().ToList();

                _context.Tbl_BM_16_TaiKhoan_Thung.RemoveRange(danhSachPhu);

                var danhSachThungGang = await _context.Tbl_BM_16_GangLong
                    .Where(x => dsMaThep.Contains(x.MaThungThep) && x.T_ID_NguoiNhan == payload.idNguoiHuyNhan)
                    .ToListAsync();

                var thungCopyCanXoa = danhSachThungGang.Where(x => x.T_copy == true).ToList();
                var thungGocCanXet = danhSachThungGang.Where(x => x.T_copy == false).ToList();

                var ID_TTGList = danhSachThungGang
                    .Where(x => x.ID_TTG.HasValue)
                    .Select(x => x.ID_TTG.Value)
                    .Distinct()
                    .ToList();

                // Xử lý thùng trung gian
                foreach (var idTTG in ID_TTGList)
                {
                    var otherThungGang = await _context.Tbl_BM_16_GangLong
                        .Where(x => x.ID_TTG == idTTG && !payload.selectedMaThungs.Contains(x.MaThungGang))
                        .ToListAsync();

                    if (!otherThungGang.Any())
                    {
                        var thungTG = await _context.Tbl_BM_16_ThungTrungGian.FirstOrDefaultAsync(x => x.ID == idTTG);
                        if (thungTG != null)
                        {
                            var copies = await _context.Tbl_BM_16_ThungTrungGian
                                .Where(x => x.MaThungTG == thungTG.MaThungTG && x.IsCopy)
                                .ToListAsync();

                            _context.Tbl_BM_16_ThungTrungGian.RemoveRange(copies);
                            _context.Tbl_BM_16_ThungTrungGian.Remove(thungTG);
                        }
                    }
                    else
                    {
                        var thungTG = await _context.Tbl_BM_16_ThungTrungGian.FirstOrDefaultAsync(x => x.ID == idTTG);
                       
                        if (thungTG != null)
                        {
                            int count = 0;

                            foreach(var item in otherThungGang)
                            {
                                var KLSoSanh = item.T_KLThungVaGang - item.T_KLThungChua - thungTG.KL_phe;
                                if (item.T_KLGangLong == KLSoSanh)
                                {
                                    break;
                                }
                                else
                                {
                                    count++;
                                }
                            }
                            if(count == otherThungGang.Count())
                            {
                                var firstItem = otherThungGang.Where(x => x.T_KLGangLong >= thungTG.KL_phe).FirstOrDefault();
                                if (firstItem != null)
                                    firstItem.T_KLGangLong = firstItem.T_KLGangLong - thungTG.KL_phe;
                            }

                            thungTG.Tong_KLGangNhan = otherThungGang.Sum(x => x.T_KLGangLong ?? 0);
                        }

                    }
                }

                // Xử lý từng thùng gốc
                foreach (var thung in thungGocCanXet)
                {
                    var soNguoiConNhan = await _context.Tbl_BM_16_TaiKhoan_Thung
                        .Where(x => x.MaThungGang == thung.MaThungGang && x.ID_taiKhoan != payload.idNguoiHuyNhan)
                        .CountAsync();

                    if (soNguoiConNhan == 0)
                    {
                        // Không còn ai nhận -> reset thùng gốc
                        thung.MaThungThep = null;
                        thung.T_ID_Kip = null;
                        thung.NgayLuyenThep = null;
                        thung.T_KLThungVaGang = null;
                        thung.T_KLThungChua = null;
                        thung.T_KLGangLong = null;
                        thung.ThungTrungGian = null;
                        thung.T_KLThungVaGang_Thoi = null;
                        thung.T_KLThungChua_Thoi = null;
                        thung.T_KLGangLongThoi = null;
                        thung.T_GhiChu = null;
                        thung.T_ID_NguoiLuu = null;
                        thung.T_ID_Kip = null;
                        thung.ID_LoThoi = null;
                        thung.ID_MeThoi = null;
                        thung.T_Ca = null;
                        thung.T_ID_NguoiHuyNhan = payload.idNguoiHuyNhan;
                        thung.T_ID_TrangThai = (int)TinhTrang.ChoXuLy;
                        thung.ID_TTG = null;
                        thung.T_ID_NguoiNhan = null;
                        thung.KLGangChia = null;
                        thung.T_ReceiveSeq = null;
                    }
                    else
                    {
                        // Còn người khác đang giữ -> nâng 1 bản ghi copy lên thành gốc
                        var thungCopy = await _context.Tbl_BM_16_GangLong
                            .Where(x => x.MaThungGang == thung.MaThungGang && x.T_copy == true)
                            .OrderBy(x => x.NgayLuyenThep) // hoặc theo thứ tự nào bạn chọn
                            .FirstOrDefaultAsync();

                        if (thungCopy != null)
                        {
                            thungCopy.T_copy = false;
                            // Xoá thùng gốc hiện tại (nếu muốn)
                            _context.Tbl_BM_16_GangLong.Remove(thung);
                        }
                    }
                }

                
                _context.Tbl_BM_16_GangLong.RemoveRange(thungCopyCanXoa);

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Luu([FromBody] List<ThungTrungGianDto> dsThungTG)
        {
            if (dsThungTG == null || !dsThungTG.Any())
                return BadRequest("Không có dữ liệu.");
            try
            {
                var maThungThepCanTinhToan = new HashSet<string>();
                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
                if (TaiKhoan == null) return BadRequest("Không tìm thấy tài khoản.");

                var danhSachBiBoQua = new List<string>();

                foreach (var tgDto in dsThungTG)
                {
                    Tbl_BM_16_ThungTrungGian ttg = null;

                    // thung copy
                    if (tgDto.IsCopy && !string.IsNullOrEmpty(tgDto.MaThungTG_Copy))
                    {
                        // Là thùng copy => tìm theo MaThungTG_Copy
                        ttg = await _context.Tbl_BM_16_ThungTrungGian
                                    .FirstOrDefaultAsync(x => x.IsCopy && x.MaThungTG_Copy == tgDto.MaThungTG_Copy);

                        if (ttg == null)
                        {
                            // Tạo mới bản sao
                            ttg = new Tbl_BM_16_ThungTrungGian
                            {
                                MaThungTG = tgDto.MaThungTG,
                                SoThungTG = tgDto.SoThungTG,
                                MaThungTG_Copy = tgDto.MaThungTG_Copy,
                                NgayNhan = tgDto.NgayNhan,
                                CaNhan = tgDto.CaNhan,
                                ID_LoThoi = tgDto.ID_LoThoi,
                                GioChonMe = tgDto.GioChonMe,
                                ID_NguoiNhan = TaiKhoan.ID_TaiKhoan,
                                NgayTaoTTG = DateTime.Now,
                                Tong_KLGangNhan = null,
                                IsCopy = true
                            };
                            _context.Tbl_BM_16_ThungTrungGian.Add(ttg);
                        }
                    }
                    else // thung goc
                    {
                        ttg = await _context.Tbl_BM_16_ThungTrungGian
                                .FirstOrDefaultAsync(x => !x.IsCopy &&
                                                          x.MaThungTG == tgDto.MaThungTG);
                                                          //&& x.ID_NguoiNhan == TaiKhoan.ID_TaiKhoan);

                        if (ttg == null)
                            continue;
                    }

                    // ===== KIỂM TRA: Nếu có thùng gang đã chốt thì bỏ qua luôn =====
                    bool coGangDaChot = await _context.Tbl_BM_16_GangLong
                        .AnyAsync(x => x.ID_TTG == ttg.ID && x.ID_TrangThai == (int)TinhTrang.DaChot);
                    if (coGangDaChot)
                    {
                        danhSachBiBoQua.Add(ttg.IsCopy ? ttg.MaThungTG_Copy : ttg.MaThungTG);
                        continue;
                    }

                    // Cập nhật dữ liệu chung
                    if (ttg != null)
                    {
                        ttg.KLThungVaGang_Thoi = tgDto.KLThungVaGang_Thoi;
                        ttg.KLThung_Thoi = tgDto.KLThung_Thoi;
                        ttg.KL_phe = tgDto.KLPhe;
                        ttg.KLGang_Thoi = tgDto.KLGang_Thoi;
                        ttg.Tong_KLGangNhan = tgDto.Tong_KLGangNhan;
                        ttg.GhiChu = tgDto.GhiChu;
                        ttg.ID_MeThoi = tgDto.ID_MeThoi;
                        ttg.GioChonMe = tgDto.GioChonMe;
                    }

                    // Nếu là thùng gốc =>> cập nhật danh sách thùng gang
                    if (!tgDto.IsCopy && tgDto.DanhSachThungGang?.Any() == true)
                    {

                        var maThungList = tgDto.DanhSachThungGang.Select(x => x.MaThungThep).ToList();

                        var gangList = await _context.Tbl_BM_16_GangLong
                            .Where(x => maThungList.Contains(x.MaThungThep) && x.ID_TrangThai != (int)TinhTrang.DaChot)
                            .ToListAsync();

                        foreach (var thungGang in tgDto.DanhSachThungGang)
                        {
                            var entity = gangList.FirstOrDefault(x => x.MaThungThep == thungGang.MaThungThep);

                            if (entity != null)
                            {
                                bool isChanged = false;

                                if (entity.T_KLGangLong != thungGang.T_KLGangLong)
                                {
                                    isChanged = true;
                                    entity.T_KLGangLong = thungGang.T_KLGangLong;
                                   
                                }

                                entity.T_KLThungVaGang = thungGang.T_KLThungVaGang;
                                entity.T_KLThungChua = thungGang.T_KLThungChua;
                                maThungThepCanTinhToan.Add(thungGang.MaThungGang);
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();

                foreach (var ma in maThungThepCanTinhToan)
                {
                    try
                    {
                        await _chiaGangService.KiemTraVaTinhLaiTheoMaThungGangAsync(ma);
                    }
                    catch
                    {
                    }
                }
                return Ok(new { success = true, message = "Lưu thành công.", maThungThepCanTinhToan = maThungThepCanTinhToan });
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> TinhLaiChiaGang([FromBody] List<string> maThungThepCanTinhToan)
        //{
        //    try
        //    {
        //        foreach (var ma in maThungThepCanTinhToan)
        //        {
        //            try
        //            {
        //                await _chiaGangService.KiemTraVaTinhLaiTheoMaThungGangAsync(ma);
        //            }
        //            catch
        //            {
        //            }
        //        }
        //        return Ok();
        //    }catch(Exception ex)
        //    {
        //        return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> DanhSachTheoMaThungGang([FromBody] List<string> selectedThungs)
        //{
        //    try
        //    {
        //        var result = await _chiaGangService.TinhToanChiaGangAsync(selectedThungs);
        //        return Ok(result);
        //    }
        //    catch(Exception ex)
        //    {
        //        return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
        //    }
        //}

        

        //[HttpPost]
        //public async Task<IActionResult> DanhSachTheoMaThungGang([FromBody] List<int> IDs)
        //{
        //    try
        //    {
        //        var result = await _chiaGangService.TinhToanChiaGangAsync(IDs);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> XacNhanChiaGang([FromBody] List<ChiaGangDto> payload)
        //{
        //    if (payload == null || payload.Count == 0)
        //        return BadRequest("Dữ liệu đầu vào không hợp lệ.");
            
        //    try
        //    {
        //        var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
        //        var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
        //        string maChiaGang = "CG" + TaoMa.GenerateSafeCode(8);

        //        // Lấy danh sách ID_Thung duy nhất để query gang gốc
        //        var thungIds = payload.Select(p => p.ID).Distinct().ToList();

        //        var gangLongList = await _context.Tbl_BM_16_GangLong
        //            .Where(x => thungIds.Contains(x.ID))
        //            .ToDictionaryAsync(x => x.ID);

        //        var listChiaGang = new List<Tbl_BM_16_ChiaGang>();

        //        foreach (var item in payload)
        //        {
        //            // Tạo bản ghi chia gangTF
        //            listChiaGang.Add(new Tbl_BM_16_ChiaGang
        //            {
        //                ID_Thung = item.ID,
        //                MaChiaGang = maChiaGang,
        //                MaThungGang = item.MaThungGang,
        //                MaThungThep = item.MaThungThep,
        //                PhanTram = item.TyLeChia,
        //                KLGangChia = item.KLChia,
        //                ID_NguoiChia = TaiKhoan.ID_TaiKhoan
        //            });

        //            // Cập nhật gang gốc nếu tồn tại
        //            if (gangLongList.TryGetValue(item.ID, out var gang))
        //            {
        //                gang.KLGangChia = item.KLChia;
        //                //gang.T_KLGangLong = item.KLChia;
        //            }
        //        }

        //        // Thêm tất cả bản ghi chia gang 1 lần
        //        _context.Tbl_BM_16_ChiaGang.AddRange(listChiaGang);

        //        await _context.SaveChangesAsync();
        //        return Ok(new {success = true});
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> GopThungGang([FromBody] GopThungGang payload)
        {
            try
            {
                if (payload.IDs == null || payload.IDs.Count == 0)
                    throw new Exception("Danh sách ID trống.");

                var idSet = payload.IDs.ToHashSet();
                // 1) Validate input IDs
                var danhsachThung = await _context.Tbl_BM_16_GangLong
                    .AsNoTracking()
                    .Where(a => idSet.Contains(a.ID))
                    .Select(a => new
                    {
                        a.MaThungGang,
                        a.BKMIS_ThungSo,
                        a.ID,
                        a.T_KLGangLong,
                        a.ID_TTG
                    })
                    .ToListAsync();
                if (danhsachThung.Count != idSet.Count)
                    throw new Exception("Một số ID không tồn tại hoặc không hợp lệ.");

                if (danhsachThung.GroupBy(x => x.MaThungGang).Any(g => g.Count() > 1))
                    throw new Exception("Các mã thùng gang phải khác nhau.");

                if (danhsachThung.Select(x => x.BKMIS_ThungSo).Distinct().Count() != 1)
                    throw new Exception("Các thùng gang phải có cùng thùng số.");


                if (payload.PhongBan == "HRC2")
                {
                    // check các thùng được chọn để chia có cùng ID_TTG không (phải nằm cùng phạm vi 1 thùng trung gian thì mới được chia)
                    var listID_TTGs = danhsachThung.Select(a => a.ID_TTG).ToList();
                    var first = listID_TTGs.First();
                    if (!listID_TTGs.All(x => x == first))
                    {
                        throw new Exception("Các thùng phải nằm trong cùng 1 Thùng trung gian.");
                    }
                }

                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
                string maChiaGang = "CG" + TaoMa.GenerateSafeCode(8);

                // Lấy danh sách ID_Thung duy nhất để query gang gốc
                var gangLongList = await _context.Tbl_BM_16_GangLong
                    .Where(x => payload.IDs.Contains(x.ID)).ToListAsync();
                var listChiaGang = new List<Tbl_BM_16_ChiaGang>();

                foreach (var item in gangLongList)
                {
                    // Tạo bản ghi chia gangTF
                    listChiaGang.Add(new Tbl_BM_16_ChiaGang
                    {
                        ID_Thung = item.ID,
                        MaChiaGang = maChiaGang,
                        MaThungGang = item.MaThungGang,
                        MaThungThep = item.MaThungThep,
                        ID_NguoiChia = TaiKhoan.ID_TaiKhoan
                    });
                }
                // Thêm tất cả bản ghi chia gang 1 lần
                _context.Tbl_BM_16_ChiaGang.AddRange(listChiaGang);

                await _context.SaveChangesAsync();
                return Ok(new{success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
        }

        private string GenerateMaThungThep(string maThungGang, DateTime ngayNhan, int loThoiId, int? caValue, int index)
        {
            string ca = caValue == (int)CaLamViec.Ngay ? "N" : "Đ";
            string dayStr = ngayNhan.Day.ToString("00");
            string indexStr = index.ToString("D2"); // dạng 2 chữ số: 00, 01, 02, ...
            return $"{maThungGang}.{dayStr}{ca}T{loThoiId}.{indexStr}";
        }
        private decimal? RoundDecimal(decimal? value)
        {
            return value.HasValue ? Math.Round(value.Value, 2) : (decimal?)null;
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] SearchThungDaNhanDto payload)
        {
            try
            {
                var thungList = await GetDanhSachThungTrungGianDaNhan(payload);
                if (thungList == null || !thungList.Any())
                    return BadRequest("Danh sách trống.");

                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.FirstOrDefault(x => x.TenTaiKhoan == TenTaiKhoan);
                var pb = _context.Tbl_PhongBan.FirstOrDefault(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan);
                var PhongBan = (pb?.TenNgan ?? "").Split('.').Last(); // "HRC1" | "HRC2"
                bool isHRC1 = string.Equals(PhongBan, "HRC1", StringComparison.OrdinalIgnoreCase);
                bool isHRC2 = string.Equals(PhongBan, "HRC2", StringComparison.OrdinalIgnoreCase);

                // Cột dữ liệu
                const int COL_KL_VA_GANG = 6;
                const int COL_KL_THUNG = 7;
                const int COL_KL_GANGLONG = 8;
                const int COL_KLGANGCHIA = 9;   // Không merge

                // Cột cấp TTG (10..18)
                int[] TTG_COLS = { 10, 11, 12, 13, 14, 15, 16, 17, 18 };

                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "QTGN_Gang_Long_Thep.xlsx");
                using var ms = new MemoryStream();
                using (var workbook = new XLWorkbook(filePath))
                {
                    var ws = workbook.Worksheet("Sheet1");

                    // Header
                    string ca = payload.T_Ca == 1 ? "Ngày" : "Đêm";
                    var loThoiEntity = await _context.Tbl_LoThoi.FirstOrDefaultAsync(x => x.ID == payload.ID_LoThoi);
                    var loThoi = loThoiEntity?.TenLoThoi ?? "";
                    var headerCell = ws.Range("A5:P5").Merge();
                    headerCell.Value = $"Ngày: {payload.NgayLamViec:dd/MM/yyyy}     Ca: {ca}     Lò thổi: {loThoi}";
                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Font.FontSize = 11;

                    static string Norm(string s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim().ToUpperInvariant();

                    // Dùng để ghi nhận toàn bộ hàng đã ghi ra (phục vụ merge toàn cục cho HRC1)
                    var allRows = new List<(int Row, string Key, bool IsCopy)>();

                    // Các biến gom nhóm 3 cột 6–8 (dùng cho HRC2 trong phạm vi TTG)
                    string prevKey = "";
                    int groupStartRow = -1;
                    int groupCount = 0;
                    int firstValueRow = -1;

                    void ResetLocalGroup()
                    {
                        prevKey = "";
                        groupStartRow = -1;
                        groupCount = 0;
                        firstValueRow = -1;
                    }

                    void FinalizeLocalGroup_HRC2()
                    {
                        if (string.IsNullOrEmpty(prevKey) || groupCount <= 1) { ResetLocalGroup(); return; }
                        int start = groupStartRow;
                        int end = groupStartRow + groupCount - 1;

                        // Nếu cụm mở đầu bằng dòng copy (ô 6-7-8 rỗng), đưa giá trị từ dòng non-copy đầu tiên lên top
                        if (firstValueRow > 0 && firstValueRow != start)
                        {
                            ws.Cell(start, COL_KL_VA_GANG).Value = ws.Cell(firstValueRow, COL_KL_VA_GANG).Value;
                            ws.Cell(start, COL_KL_THUNG).Value = ws.Cell(firstValueRow, COL_KL_THUNG).Value;
                            ws.Cell(start, COL_KL_GANGLONG).Value = ws.Cell(firstValueRow, COL_KL_GANGLONG).Value;
                        }

                        // Style col 8 top
                        var topGangLong = ws.Cell(start, COL_KL_GANGLONG);
                        topGangLong.Style.NumberFormat.Format = "0.00";
                        topGangLong.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        topGangLong.Style.Font.Bold = true;

                        // Merge 6–8
                        ws.Range(start, COL_KL_VA_GANG, end, COL_KL_VA_GANG).Merge();
                        ws.Range(start, COL_KL_THUNG, end, COL_KL_THUNG).Merge();
                        ws.Range(start, COL_KL_GANGLONG, end, COL_KL_GANGLONG).Merge();

                        ResetLocalGroup();
                    }

                    int row = 8, stt = 1;

                    foreach (var ttg in thungList)
                    {
                        var danhSach = ttg.DanhSachThungGang ?? new List<GangLongItemViewModel>();
                        if (!danhSach.Any()) continue;

                        int startRow_TTG = row;

                        if (isHRC2) ResetLocalGroup();

                        foreach (var item in danhSach)
                        {
                            int c = 1;
                            string key = Norm(item.MaChiaGang);

                            // HRC2: gom theo key trong phạm vi TTG
                            if (isHRC2)
                            {
                                if (key != prevKey)
                                {
                                    FinalizeLocalGroup_HRC2();
                                    if (!string.IsNullOrEmpty(key))
                                    {
                                        prevKey = key;
                                        groupStartRow = row;
                                        groupCount = 0;
                                        firstValueRow = -1;
                                    }
                                }
                            }

                            // ==== Ghi hàng ====
                            ws.Cell(row, c++).Value = stt++;                  // STT
                            ws.Cell(row, c++).Value = item.MaThungThep;       // 2
                            ws.Cell(row, c++).Value = item.ID_LoCao;          // 3
                            ws.Cell(row, c++).Value = item.BKMIS_ThungSo;     // 4

                            // 5: Trạng thái
                            var statusCell = ws.Cell(row, c++);
                            statusCell.Style.Font.SetBold();
                            if (item.ID_TrangThai == 5) SetCellColor(statusCell, "Đã chốt", "#1e7e34", "#ffffff");
                            else if (item.ID_TrangThai == 2) SetCellColor(statusCell, "Chờ xử lý", "#ffc107", "#212529");
                            else SetCellColor(statusCell, "Chưa chuyển", "#6c757d", "#ffffff");

                            // 6–8–9
                            if (ttg.IsCopy == true)
                            {
                                ws.Cell(row, c++).Value = ""; // 6
                                ws.Cell(row, c++).Value = ""; // 7
                                ws.Cell(row, c++).Value = ""; // 8
                                ws.Cell(row, c++).Value = ""; // 9 (KLGangChia - không merge)
                            }
                            else
                            {
                                ws.Cell(row, c++).Value = item.T_KLThungVaGang; // 6
                                ws.Cell(row, c++).Value = item.T_KLThungChua;   // 7

                                var cellGangLong = ws.Cell(row, c++);           // 8
                                if (item.T_KLGangLong.HasValue)
                                {
                                    cellGangLong.Value = item.T_KLGangLong.Value;
                                    cellGangLong.Style.NumberFormat.Format = "0.00";
                                    cellGangLong.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    cellGangLong.Style.Font.Bold = true;
                                }

                                var cellChia = ws.Cell(row, c++);               // 9
                                if (item.KLGangChia.HasValue)
                                {
                                    cellChia.Value = item.KLGangChia.Value;
                                    cellChia.Style.NumberFormat.Format = "0.00";
                                    cellChia.Style.Font.FontColor = XLColor.Red;
                                }
                                else
                                {
                                    cellChia.Value = item.T_KLGangLong.HasValue ? item.T_KLGangLong.Value : "";
                                }
                                cellChia.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cellChia.Style.Font.Bold = true;

                                // Đánh dấu hàng non-copy đầu tiên trong cụm (HRC2)
                                if (isHRC2 && !string.IsNullOrEmpty(prevKey) && firstValueRow < 0)
                                    firstValueRow = row;
                            }

                            // Lưu meta để merge toàn cục cho HRC1
                            allRows.Add((row, key, ttg.IsCopy == true));

                            ws.Row(row).Height = 25;
                            if (isHRC2 && !string.IsNullOrEmpty(prevKey)) groupCount++;
                            row++;
                        }

                        // ===== Merge cột cấp TTG trong phạm vi TTG (cả HRC1/HRC2; với HRC1 1 hàng/TTG nên không ảnh hưởng) =====
                        int r1 = startRow_TTG, r2 = row - 1;
                        int col = 10;

                        var cellTongKLGang = ws.Cell(r1, col);
                        cellTongKLGang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        cellTongKLGang.Style.Font.Bold = true;

                        if (ttg.IsCopy == true)
                        {
                            cellTongKLGang.Value = "TTG Copy";
                            cellTongKLGang.Style.Font.FontColor = XLColor.Red;
                        }
                        else if (ttg.Tong_KLGangNhan.HasValue)
                        {
                            cellTongKLGang.Value = ttg.Tong_KLGangNhan.Value;
                            cellTongKLGang.Style.NumberFormat.Format = "0.00";
                        }
                        else cellTongKLGang.Value = "";

                        ws.Range(r1, col, r2, col).Merge(); col++;

                        ws.Cell(r1, col).Value = ttg.SoThungTG; ws.Range(r1, col, r2, col).Merge(); col++;
                        ws.Cell(r1, col).Value = ttg.KLThungVaGang_Thoi; ws.Range(r1, col, r2, col).Merge(); col++;
                        ws.Cell(r1, col).Value = ttg.KLThung_Thoi; ws.Range(r1, col, r2, col).Merge(); col++;
                        ws.Cell(r1, col).Value = ttg.KL_phe; ws.Range(r1, col, r2, col).Merge(); col++;

                        var cellKLGang = ws.Cell(r1, col);
                        if (ttg.KLGang_Thoi.HasValue)
                        {
                            cellKLGang.Value = ttg.KLGang_Thoi.Value;
                            cellKLGang.Style.NumberFormat.Format = "0.00";
                            cellKLGang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            cellKLGang.Style.Font.Bold = true;
                        }
                        ws.Range(r1, col, r2, col).Merge(); col++;

                        ws.Cell(r1, col).Value = ttg.MaMeThoi; ws.Range(r1, col, r2, col).Merge(); col++;
                        ws.Cell(r1, col).Value = ttg.GioChonMe; ws.Range(r1, col, r2, col).Merge(); col++;
                        ws.Cell(r1, col).Value = ttg.GhiChu ?? ""; ws.Range(r1, col, r2, col).Merge();

                        // Kết cụm 6–8 phạm vi TTG (HRC2)
                        if (isHRC2) FinalizeLocalGroup_HRC2();
                    }

                    // HRC1: merge theo MaChiaGang trên TOÀN BẢNG (6–8 và 10–18)
                    if (isHRC1)
                    {
                        // Quét theo thứ tự dòng đã ghi để tạo dải liên tiếp cùng key
                        int idx = 0;
                        while (idx < allRows.Count)
                        {
                            var key = allRows[idx].Key;
                            if (string.IsNullOrEmpty(key)) { idx++; continue; }

                            int start = idx, end = idx;
                            while (end + 1 < allRows.Count && allRows[end + 1].Key == key) end++;

                            int r1 = allRows[start].Row;
                            int r2 = allRows[end].Row;
                            if (r2 > r1)
                            {
                                // tìm dòng non-copy đầu tiên trong dải để “nhấc” 6–8 lên top
                                int firstNonCopyRow = -1;
                                for (int k = start; k <= end; k++)
                                    if (!allRows[k].IsCopy) { firstNonCopyRow = allRows[k].Row; break; }

                                if (firstNonCopyRow > 0 && firstNonCopyRow != r1)
                                {
                                    ws.Cell(r1, COL_KL_VA_GANG).Value = ws.Cell(firstNonCopyRow, COL_KL_VA_GANG).Value;
                                    ws.Cell(r1, COL_KL_THUNG).Value = ws.Cell(firstNonCopyRow, COL_KL_THUNG).Value;
                                    ws.Cell(r1, COL_KL_GANGLONG).Value = ws.Cell(firstNonCopyRow, COL_KL_GANGLONG).Value;
                                }

                                // Style col 8 top
                                var topGangLong = ws.Cell(r1, COL_KL_GANGLONG);
                                topGangLong.Style.NumberFormat.Format = "0.00";
                                topGangLong.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                topGangLong.Style.Font.Bold = true;

                                // Merge 6–8
                                ws.Range(r1, COL_KL_VA_GANG, r2, COL_KL_VA_GANG).Merge();
                                ws.Range(r1, COL_KL_THUNG, r2, COL_KL_THUNG).Merge();
                                ws.Range(r1, COL_KL_GANGLONG, r2, COL_KL_GANGLONG).Merge();

                                // Merge toàn bộ cột cấp TTG (10..18)
                                foreach (var col in TTG_COLS)
                                    ws.Range(r1, col, r2, col).Merge();
                            }
                            idx = end + 1;
                        }
                    }

                    // Dòng tổng
                    int sumRow = row;
                    var totalLabel = ws.Range($"A{sumRow}:I{sumRow}").Merge();
                    totalLabel.Value = "Tổng:";
                    totalLabel.Style.Font.SetBold();
                    totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell(sumRow, 10).FormulaA1 = $"=SUM(J8:J{row - 1})";
                    ws.Cell(sumRow, 10).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(sumRow, 10).Style.Font.SetBold();
                    ws.Cell(sumRow, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    var totalLabel2 = ws.Range($"K{sumRow}:N{sumRow}").Merge();
                    totalLabel2.Value = "";
                    totalLabel2.Style.Font.SetBold();
                    totalLabel2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    ws.Cell(sumRow, 15).FormulaA1 = $"=SUM(O8:O{row - 1})";
                    ws.Cell(sumRow, 15).Style.NumberFormat.Format = "#,##0.00";
                    ws.Cell(sumRow, 15).Style.Font.SetBold();
                    ws.Cell(sumRow, 15).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    var totalLabel3 = ws.Range($"P{sumRow}:R{sumRow}").Merge();
                    totalLabel3.Value = "";
                    totalLabel3.Style.Font.SetBold();
                    totalLabel3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                    // Format chung
                    var usedRange = ws.Range($"A7:R{sumRow}");
                    usedRange.Style.Font.SetFontName("Arial").Font.SetFontSize(11);
                    usedRange.Style.Font.FontColor = XLColor.Black;
                    usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    usedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    usedRange.Style.Alignment.WrapText = true;

                    workbook.SaveAs(ms);
                }

                ms.Position = 0;
                string outputName = $"QTGN_Gang_Long_Thep_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(ms.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    outputName);
            }
            catch
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";
                return RedirectToAction("Index", "BM_GangThoi_Thep");
            }
        }


        void SetCellColor(IXLCell cell, string text, string hexBackground, string hexFont)
        {
            cell.Value = text;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(hexBackground);
            cell.Style.Font.FontColor = XLColor.FromHtml(hexFont);
            cell.Style.Font.Bold = true;
        }

        public async Task<List<ThungTrungGianGroupViewModel>> GetDanhSachThungCoNguoiChuyen(SearchThungDaNhanDto payload)
        {
            var thungList = await GetDanhSachThungTrungGianDaNhan(payload);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault().TenNgan.Split('.').Last();


            if (string.IsNullOrEmpty(PhongBan))
                return new List<ThungTrungGianGroupViewModel>();


           
            var listThungChot = thungList
               .Where(t => t.DanhSachThungGang != null)
               .SelectMany(t => t.DanhSachThungGang
                   .Where(g => g.ID_TrangThai == 5 &&
                               _context.Tbl_TaiKhoan.Any(tk =>
                                   tk.ID_TaiKhoan == g.T_ID_NguoiNhan &&
                                   tk.ID_PhongBan == TaiKhoan.ID_PhongBan)))
               .ToList();

            if (!listThungChot.Any())
                        return new List<ThungTrungGianGroupViewModel>();


            // Lấy danh sách ID người chuyển (người lưu)
            var allNguoiChuyenIds = listThungChot
                .Where(g => g.G_ID_NguoiLuu.HasValue)
                .Select(g => g.G_ID_NguoiLuu.Value)
                .Distinct()
                .ToList();

            // Lấy thông tin người chuyển từ DB
            var thongTinNguoiChuyen = await (
                from user in _context.Tbl_TaiKhoan
                where allNguoiChuyenIds.Contains(user.ID_TaiKhoan)
                join phongBan in _context.Tbl_PhongBan on user.ID_PhongBan equals phongBan.ID_PhongBan into g_pb
                from phongBan in g_pb.DefaultIfEmpty()
                join vitri in _context.Tbl_ViTri on user.ID_ChucVu equals vitri.ID_ViTri into g_vt
                from vitri in g_vt.DefaultIfEmpty()
                select new
                {
                    user.ID_TaiKhoan,
                    user.HoVaTen,
                    user.TenTaiKhoan,
                    user.ChuKy,
                    TenPhongBan = phongBan != null ? phongBan.TenPhongBan : "",
                    TenViTri = vitri != null ? vitri.TenViTri : ""
                }
            ).ToListAsync();

            var thongTinDict = thongTinNguoiChuyen.ToDictionary(x => x.ID_TaiKhoan);

            // Gán thông tin người chuyển vào từng GangLongItem
            foreach (var ttg in thungList)
            {
                if (ttg.DanhSachThungGang != null)
                {
                    // Giữ lại chỉ các thùng gang đúng trạng thái và người nhận đúng phòng ban
                    ttg.DanhSachThungGang = ttg.DanhSachThungGang
                        .Where(g => g.ID_TrangThai == 5 &&
                                    _context.Tbl_TaiKhoan.Any(tk =>
                                        tk.ID_TaiKhoan == g.T_ID_NguoiNhan &&
                                        tk.ID_PhongBan == TaiKhoan.ID_PhongBan))
                        .ToList();

                    foreach (var gang in ttg.DanhSachThungGang)
                    {
                        if (gang.G_ID_NguoiLuu.HasValue &&
                            thongTinDict.TryGetValue(gang.G_ID_NguoiLuu.Value, out var info))
                        {
                            gang.HoVaTen = info.HoVaTen;
                            gang.TenTaiKhoan = info.TenTaiKhoan;
                            gang.TenPhongBan = info.TenPhongBan;
                            gang.ChuKy = info.ChuKy;
                            gang.TenViTri = info.TenViTri;
                        }
                    }
                }
            }

            return thungList.Where(x => !x.IsCopy && x.DanhSachThungGang != null && x.DanhSachThungGang.Any()).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPDF([FromBody] SearchThungDaNhanDto payload)
        {
            try {

                var thungThep = await GetDanhSachThungCoNguoiChuyen(payload);

                
                if (thungThep == null || !thungThep.Any())
                    return BadRequest("Danh sách trống.");

                var kip = await _context.Tbl_Kip.Where(x => x.TenCa == payload.T_Ca.ToString() && x.NgayLamViec == payload.NgayLamViec).FirstOrDefaultAsync();

                var thongTin = new ThongTinPDFViewModel
                {
                    T_Ca = kip.TenCa ?? "",
                    T_Kip = kip.TenKip ?? "",
                    NgayLuyenThep = kip.NgayLamViec ?? DateTime.Now
                };
                var data = new PDFLuyenThepViewModel
                {
                    Data = thungThep,
                    ThongTin = thongTin,
                };

                // 1. Render Razor View thành chuỗi HTML
                string html = await RenderViewToStringAsync("BBGN_thep_pdf", data);

                // 2. Chuyển đổi HTML sang PDF
                byte[] pdfBytes = ConvertHtmlToPdf(html);

                string filename = $"QTGN Gang long - Thep {DateTime.Now.ToString("yyyyMMddHHmm")}.pdf";

                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index", "BM_GangThoi_Thep");
            }
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
                return await Task.FromResult(writer.ToString());
            }
        }

        private byte[] ConvertHtmlToPdf(string htmlContent)
        {
            using (var memoryStream = new MemoryStream())
            {
                // 1. Cấu hình FontProvider để hỗ trợ Times New Roman
                var fontProvider = new FontProvider();
                fontProvider.AddFont("C:/Windows/Fonts/times.ttf");     // Regular
                fontProvider.AddFont("C:/Windows/Fonts/timesbd.ttf");   // Bold
                fontProvider.AddFont("C:/Windows/Fonts/timesi.ttf");    // Italic
                fontProvider.AddFont("C:/Windows/Fonts/timesbi.ttf");   // Bold Italic

                // 2. Tạo writer và document
                var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream);
                var pdfDocument = new iText.Kernel.Pdf.PdfDocument(writer);
                pdfDocument.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4.Rotate()); // Trang ngang

                // 3. Cấu hình Converter
                var converterProperties = new ConverterProperties();
                converterProperties.SetFontProvider(fontProvider);

                // 4. Cấu hình baseUri nếu HTML chứa ảnh
                string baseUri = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                converterProperties.SetBaseUri(baseUri);

                // 5. Chuyển đổi HTML sang PDF
                HtmlConverter.ConvertToPdf(htmlContent, pdfDocument, converterProperties);

                // 6. Trả về PDF dưới dạng byte[]
                return memoryStream.ToArray();
            }
        }

    }
}
