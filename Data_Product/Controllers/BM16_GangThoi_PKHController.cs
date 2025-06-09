using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchDto dto)
        {
            var query = _context.Tbl_BM_16_GangLong.AsQueryable();

            if (dto.ID_LoCao.HasValue)
            {
                query = query.Where(x => x.ID_Locao == dto.ID_LoCao.Value);
            }

            if (dto.ID_LoThoi.HasValue)
            {
                query = query.Where(x => x.ID_LoThoi == dto.ID_LoThoi.Value);
            }

            if (dto.TuNgay_LG.HasValue && dto.DenNgay_LG.HasValue)
            {
                query = query.Where(x => x.NgayLuyenGang >= dto.TuNgay_LG.Value && x.NgayLuyenGang <= dto.DenNgay_LG.Value);

            }

            if (dto.TuNgay_LT.HasValue && dto.DenNgay_LT.HasValue)
            {
                query = query.Where(x => x.NgayLuyenThep >= dto.TuNgay_LT.Value && x.NgayLuyenThep <= dto.DenNgay_LT.Value);

            }
            if (dto.Ca_LT.HasValue)
            {
                query = query.Where(x => x.T_Ca == dto.Ca_LT.Value);
            }
            if (dto.Ca_LG.HasValue)
            {
                query = query.Where(x => x.G_Ca == dto.Ca_LG.Value);
            }

            // TODO
            if (dto.ID_Kip_LT.HasValue)
            {
                query = query.Where(x => x.T_ID_Kip == dto.ID_Kip_LT.Value);
            }
            
            if (dto.ID_Kip_LG.HasValue)
            {
                query = query.Where(x => x.G_ID_Kip == dto.ID_Kip_LG.Value);
            }
            // END TODO

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

            var res = await (from a in query
                             join trangThai in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThai.ID into g_tt
                             from trangThai in g_tt.DefaultIfEmpty()

                             join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID into g_lc
                             from loCao in g_lc.DefaultIfEmpty()

                             join methoi in _context.Tbl_MeThoi on a.ID_MeThoi equals methoi.ID into g_mt
                             from methoi in g_mt.DefaultIfEmpty()

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

                             orderby a.MaThungGang, a.MaThungThep
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = a.ID,
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
                                 ID_LoThoi = a.ID_LoThoi,
                                 KR = a.KR,
                                 ThungTrungGian = a.ThungTrungGian,
                                 T_KLThungVaGang_Thoi = a.T_KLThungVaGang_Thoi,
                                 T_KLThungChua_Thoi = a.T_KLThungChua_Thoi,
                                 T_KLGangLongThoi = a.T_KLGangLongThoi,
                                 MaMeThoi = methoi.MaMeThoi,
                                 T_ID_TrangThai = a.T_ID_TrangThai,
                                 ID_TrangThai = a.ID_TrangThai,
                                 TenLoCao = loCao.TenLoCao,

                                 
                                 HoVaTen = user.HoVaTen,
                                 TenPhongBan = phongban.TenNgan
                             }).ToListAsync();
            return Ok(res);
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
            foreach (var t in thungs)
            {
                t.ID_TrangThai = (int)TinhTrang.DaChot;
                t.ID_NguoiChot = TaiKhoan.ID_TaiKhoan;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
