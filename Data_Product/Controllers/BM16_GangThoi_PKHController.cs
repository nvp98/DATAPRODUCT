using ClosedXML.Excel;
using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BM16_GangThoi_PKHController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public BM16_GangThoi_PKHController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
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

            // Kiểm tra số liệu thùng gang thiếu
            var invalidThungsGang = thungs
                .Where(x =>
                    x.KL_XeGoong == null ||
                    x.G_KLThungVaGang == null ||
                    x.G_KLThungChua == null ||
                    x.G_KLGangLong == null ||
                    x.ChuyenDen == null ||
                    x.Gio_NM == null ||
                    x.T_KLThungVaGang == null ||
                    x.T_KLThungChua == null ||
                    x.T_KLGangLong == null ||
                    x.ID_TTG == null
                )
                .Select(x => new { x.ID, x.MaThungGang }) 
                .ToList();

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

            var invalidThungsTTG = relatedTTGs
                .Where(item =>
                    item.KLThungVaGang_Thoi == null ||
                    item.KLThung_Thoi == null ||
                    item.KL_phe == null ||
                    item.KLGang_Thoi == null ||
                    item.ID_MeThoi == null ||
                    item.GioChonMe == null
                )
                .Select(x => new { x.MaThungTG }) 
                .ToList();

            if (invalidThungsTTG.Any() || invalidThungsGang.Any())
            {
                return Ok(new
                {
                    isValid = false,
                    invalidThungsTTG,
                    invalidThungsGang
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
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        private async Task<PageResultViewModel<List<Tbl_BM_16_GangLong>>> SearchByPayload(SearchDto dto)
        {
            var query = _context.Tbl_BM_16_GangLong.OrderByDescending(x => x.NgayTao).ThenBy(x => x.ID_Locao).ThenByDescending(x => x.G_Ca).ThenByDescending(x => x.MaThungGang).ThenBy(x => x.MaThungThep).AsQueryable();
            decimal sumKLGang = 0;
            decimal sumKLGangLongThep = 0;
            decimal sumKLGangNhan = 0;
            decimal sumKLPhe = 0;
            decimal sumKLVaoLoThoi = 0;

            

            if (dto.ID_LoCao.HasValue)
            {
                query = query.Where(x => x.ID_Locao == dto.ID_LoCao.Value);
            }

            if (dto.ID_LoThoi.HasValue)
            {
                query = query.Where(x => x.ID_LoThoi == dto.ID_LoThoi.Value);
            }
            
            if (dto.Ca_LT.HasValue)
            {
                query = query.Where(x => x.T_Ca == dto.Ca_LT.Value);
            }

            if (dto.Ca_LG.HasValue)
            {
                query = query.Where(x => x.G_Ca == dto.Ca_LG.Value);
            }

            if (!string.IsNullOrEmpty(dto.ID_Kip_LT))
            {
                query = query.Where(thung =>
                                    thung.T_ID_Kip != null &&
                                    _context.Tbl_Kip
                                        .Where(k => k.TenKip == dto.ID_Kip_LT)
                                        .Select(k => k.ID_Kip)
                                        .Contains(thung.T_ID_Kip.Value)
                                );
            }

            if (!string.IsNullOrEmpty(dto.ID_Kip_LG))
            {
                query = query.Where(thung =>
                                    thung.G_ID_Kip != null &&
                                    _context.Tbl_Kip
                                        .Where(k => k.TenKip == dto.ID_Kip_LG)
                                        .Select(k => k.ID_Kip)
                                        .Contains(thung.G_ID_Kip.Value)
                                );
            }

            if (!string.IsNullOrEmpty(dto.ChuyenDen))
            {
                query = query.Where(x => x.ChuyenDen.Contains(dto.ChuyenDen));
            }

            if (!string.IsNullOrEmpty(dto.ThungSo))
            {
                query = query.Where(x => x.BKMIS_ThungSo.Contains(dto.ThungSo));
            }

            if (dto.ID_TinhTrang.HasValue)
            {
                query = query.Where(x => x.ID_TrangThai == dto.ID_TinhTrang.Value);
            }

            if (dto.ID_TinhTrang_LT.HasValue)
            {
                query = query.Where(x => x.T_ID_TrangThai == dto.ID_TinhTrang_LT.Value);
            }

            if (dto.ID_TinhTrang_LG.HasValue)
            {
                query = query.Where(x => x.G_ID_TrangThai == dto.ID_TinhTrang_LG.Value);
            }

            if (!string.IsNullOrEmpty(dto.MaThungGang))
            {
                query = query.Where(x => x.MaThungGang.Contains(dto.MaThungGang));
            }

            if (!string.IsNullOrEmpty(dto.MaThungThep))
            {
                query = query.Where(x => x.MaThungThep.Contains(dto.MaThungThep));
            }

            if (dto.TuNgay_LT.HasValue && dto.DenNgay_LT.HasValue)
            {
                var tuNgay = dto.TuNgay_LT.Value.Date;
                var denNgay = dto.DenNgay_LT.Value.Date.AddDays(1);
                query = query.Where(x => x.NgayLuyenThep >= tuNgay && x.NgayLuyenThep < denNgay);

            }
           
            if (dto.TuNgay_LG.HasValue && dto.DenNgay_LG.HasValue)
            {
                var tuNgay = dto.TuNgay_LG.Value.Date;
                var denNgay = dto.DenNgay_LG.Value.Date.AddDays(1);
                query = query.Where(x => x.NgayTao >= tuNgay && x.NgayTao < denNgay);
                sumKLGang = query.Where(x => x.T_copy != true).Sum(x => x.G_KLGangLong ?? 0);
                sumKLGangLongThep = query.Sum(x => x.T_KLGangLong ?? 0);

                var maThungTGList = (
                    from a in query
                    join ttgRoot in _context.Tbl_BM_16_ThungTrungGian
                        on a.ID_TTG equals ttgRoot.ID
                    select ttgRoot.MaThungTG
                ).Distinct().ToList();

                var relatedThung = _context.Tbl_BM_16_ThungTrungGian
                    .Where(x => maThungTGList.Contains(x.MaThungTG))
                    .ToList(); 

                 sumKLGangNhan = relatedThung.Sum(x => x.Tong_KLGangNhan ?? 0);
                 sumKLPhe = relatedThung.Sum(x => x.KL_phe ?? 0);
                 sumKLVaoLoThoi = relatedThung.Sum(x => x.KLGang_Thoi ?? 0);
            }

            //if (dto.TuNgay_LT.HasValue && dto.DenNgay_LT.HasValue)
            //{
            //    query = query.OrderByDescending(x => x.NgayLuyenThep)
            //                    .ThenBy(x => x.MaThungGang)
            //                    .ThenBy(x => x.MaThungThep);

            //}
            //else
            //{
            //    query = query.OrderByDescending(x => x.NgayTao)
            //                        .ThenBy(x => x.MaThungGang)
            //                        .ThenBy(x => x.MaThungThep);
            //}
            // Tổng số bản ghi
            var totalRecords = await query.CountAsync();


            // Check nếu không có pageNumber và pageSize sẽ lấy dữ liệu để xuất excel
            if (dto.PageNumber.HasValue && dto.PageSize.HasValue)
            {
                int page = dto.PageNumber.Value;
                int pageSize = dto.PageSize.Value;

                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }

            var gocData = await (from a in query
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

                            join ttg in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttg.ID into t_ttg
                            from ttg in t_ttg.DefaultIfEmpty()

                            join methoi in _context.Tbl_MeThoi on ttg.ID_MeThoi equals methoi.ID into g_mt
                            from methoi in g_mt.DefaultIfEmpty()

                            //orderby a.NgayTao descending, a.G_Ca, a.MaThungGang, a.MaThungThep
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
                                MaThungThep = a.MaThungThep != null ? a.MaThungThep : null,
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

                                HoVaTen = user.HoVaTen,
                                TenPhongBan = phongban.TenNgan,

                                ID_TTG = a.ID_TTG,
                                MaThungTG = ttg.MaThungTG,
                                ID_MeThoi = ttg.ID_MeThoi,
                                IsCopy = ttg.IsCopy,
                                MaMeThoi = methoi.MaMeThoi,
                                ID_LoThoi = ttg.ID_LoThoi,
                                SoThungTG = ttg.SoThungTG,
                                KLThungVaGang_Thoi = ttg.KLThungVaGang_Thoi,
                                KLThung_Thoi = ttg.KLThung_Thoi,
                                KLGang_Thoi = ttg.KLGang_Thoi,
                                KL_phe = ttg.KL_phe,
                                Tong_KLGangNhan = ttg.Tong_KLGangNhan,
                                GioChonMe = ttg.GioChonMe
                            }).ToListAsync();

            var maTTGs = gocData.Where(x => !string.IsNullOrEmpty(x.MaThungTG)).Select(x => x.MaThungTG).Distinct();
            var thungTG_Copies = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.IsCopy == true && maTTGs.Contains(x.MaThungTG)).ToListAsync();

            for (int i = 0; i < gocData.Count; i++)
            {
                var item = gocData[i];

                if (!item.ID_TTG.HasValue || item.IsCopy == true) continue;

                // Tìm các thùng copy có cùng MaThungTG
                var copies = thungTG_Copies
                    .Where(x => x.MaThungTG == item.MaThungTG)
                    .ToList();

                int insertIndex = i + 1;

                foreach (var copy in copies)
                {
                    var clone = CloneGangLong(item);

                    var methoi = await _context.Tbl_MeThoi
                        .Where(x => x.ID == copy.ID_MeThoi)
                        .FirstOrDefaultAsync();

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
                    gocData.Insert(insertIndex, clone);
                    insertIndex++; 
                    i++; 
                }
            }
            var test = gocData;
            var groupedData = gocData
                                .GroupBy(x => x.ID_TTG.HasValue ? x.ID_TTG.Value.ToString() : $"null_{x.ID}")
                                //.OrderByDescending(g => g.Max(x => x.NgayTao))
                                .Select(g => g.ToList())
                                .ToList();

            return new PageResultViewModel<List<Tbl_BM_16_GangLong>>
            {
                    TotalRecords = totalRecords,
                    SumKLGang = sumKLGang,
                    SumKLGangLongThep = sumKLGangLongThep,
                    SumKLGangNhan = sumKLGangNhan,
                    SumKLPhe = sumKLPhe,
                    SumKLVaoLoThoi = sumKLVaoLoThoi,
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

                //var groupByTTG = thungList.GroupBy(x => x.ID_TTG).ToList();
                //var groupByTTG = thungList
                //                .GroupBy(x => x.ID_TTG.HasValue ? x.ID_TTG.Value.ToString() : $"null_{x.ID}")
                //                .OrderByDescending(g => g.Max(x => x.NgayTao))
                //                .ToList();
                var groupByTTG = thungList;
                // Đường dẫn đến template
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "QTGN_Gang_Long_PKH.xlsx");
                using (var ms = new MemoryStream())
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet("Sheet1");

                        // Xóa dữ liệu cũ (nếu có)
                        var lastRow = Math.Max(worksheet.LastRowUsed()?.RowNumber() ?? 8, 8);
                        if (lastRow >= 8)
                        {
                            var rangeClear = worksheet.Range($"A8:AE{lastRow}");
                            rangeClear.Clear(XLClearOptions.Contents | XLClearOptions.NormalFormats);
                        }

                        int row = 8, stt = 1;
                        foreach (var group in groupByTTG)
                        {
                            int rowspan = group.Count();
                            bool isFirst = true;

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

                                if(item.T_copy == true || item.IsCopy == true)
                                {
                                    cellSoMe.Style.Font.FontColor = XLColor.Red;
                                    cellThungSo.Style.Font.FontColor = XLColor.Red;

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
                                if (isFirst)
                                {
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
                                    colIndex++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.ID_LoThoi;
                                    colIndex++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.SoThungTG;
                                    colIndex++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.KLThungVaGang_Thoi;
                                    colIndex++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.KLThung_Thoi;
                                    colIndex++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.KL_phe;
                                    colIndex++;

                                    var cellKLGang_Thoi = worksheet.Cell(row, colIndex);
                                    if (item.KLGang_Thoi.HasValue)
                                    {
                                        cellKLGang_Thoi.Value = item.KLGang_Thoi.Value;
                                        cellKLGang_Thoi.Style.NumberFormat.Format = "0.00";
                                        cellKLGang_Thoi.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        cellKLGang_Thoi.Style.Font.Bold = true;
                                    }
                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge();
                                    colIndex++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.MaMeThoi;
                                    colIndex++;

                                    worksheet.Range(row, colIndex, row + rowspan - 1, colIndex).Merge().Value = item.GioChonMe;
                                    colIndex++;

                                    isFirst = false;
                                }
                                else
                                {
                                    colIndex += 9; // Skip các cột merge
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
                       
                        // Dòng tổng
                        int sumRow = row;
                        var totalLabel = worksheet.Range($"A{sumRow}:M{sumRow}");
                        totalLabel.Merge();
                        totalLabel.Value = "Tổng:";
                        totalLabel.Style.Font.SetBold();
                        totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột 14
                        worksheet.Cell(sumRow, 14).FormulaA1 = $"=SUM(N8:N{row - 1})";
                        worksheet.Cell(sumRow, 14).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 14).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge các cột O -> X (15 -> 24)
                        var mergeRange1 = worksheet.Range(sumRow, 15, sumRow, 24);
                        mergeRange1.Merge();
                        mergeRange1.Value = ""; 
                        mergeRange1.Style.Fill.BackgroundColor = XLColor.White; 

                        // Tổng cột 25
                        worksheet.Cell(sumRow, 25).FormulaA1 = $"=SUM(Y8:Y{row - 1})";
                        worksheet.Cell(sumRow, 25).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 25).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 25).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge các cột Z -> AD (26 -> 30)
                        var mergeRange2 = worksheet.Range(sumRow, 26, sumRow, 30);
                        mergeRange2.Merge();
                        mergeRange2.Value = "";
                        mergeRange2.Style.Fill.BackgroundColor = XLColor.White;

                        // Tổng cột 31
                        worksheet.Cell(sumRow, 31).FormulaA1 = $"=SUM(AE8:AE{row - 1})";
                        worksheet.Cell(sumRow, 31).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 31).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 31).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge các cột AF -> AJ (32 -> 37)
                        var mergeRange3 = worksheet.Range(sumRow, 32, sumRow, 37);
                        mergeRange3.Merge();
                        mergeRange3.Value = "";
                        mergeRange3.Style.Fill.BackgroundColor = XLColor.White;

                        // Dòng tổng All
                        int sumAllRow = row+1;
                        var totalAllLabel = worksheet.Range($"A{sumAllRow}:M{sumAllRow}");
                        totalAllLabel.Merge();
                        totalAllLabel.Value = "Tổng Tất Cả:";
                        totalAllLabel.Style.Font.SetBold();
                        totalAllLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột 14
                        worksheet.Cell(sumAllRow, 14).Value = sumKLGang;
                        worksheet.Cell(sumAllRow, 14).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumAllRow, 14).Style.Font.SetBold();
                        worksheet.Cell(sumAllRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Merge các cột O -> U (15 -> 21)
                        var mergeRangeAll = worksheet.Range(sumAllRow, 15, sumAllRow, 37);
                        mergeRangeAll.Merge();
                        mergeRangeAll.Value = "";
                        mergeRangeAll.Style.Fill.BackgroundColor = XLColor.White;

                        // Format toàn bảng
                        var usedRange = worksheet.Range($"A7:AK{sumAllRow}");
                        usedRange.Style.Font.SetFontName("Arial").Font.SetFontSize(11);
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

                        // Ghi workbook vào MemoryStream (không lưu ra ổ đĩa)
                        workbook.SaveAs(ms);
                    }
                    // Trả file ra MemoryStream
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
