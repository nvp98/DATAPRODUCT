using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class BM16_GangThoi_PKH : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public BM16_GangThoi_PKH(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public IActionResult Index()
        {
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
            if (dto.ID_Kip_LT.HasValue)
            {
                query = query.Where(x => x.T_ID_Kip == dto.ID_Kip_LT.Value);
            }
            if (dto.ID_Kip_LG.HasValue)
            {
                query = query.Where(x => x.G_ID_Kip == dto.ID_Kip_LG.Value);
            }
            if (!string.IsNullOrEmpty(dto.ChuyenDen))
            {
                query = query.Where(x => x.ChuyenDen.Contains(dto.ChuyenDen));
            }
            if (!string.IsNullOrEmpty(dto.ThungSo))
            {
                query = query.Where(x => x.BKMIS_ThungSo.Contains(dto.ChuyenDen));
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
                             join trangThai in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThai.ID
                             join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID
                             join kipG in _context.Tbl_Kip on a.G_ID_Kip equals kipG.ID_Kip into g1
                             from kipG in g1.DefaultIfEmpty()
                             join kipT in _context.Tbl_Kip on a.T_ID_Kip equals kipT.ID_Kip into g2
                             from kipT in g2.DefaultIfEmpty()
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = a.ID,
                                 MaThungGang = a.MaThungGang,
                                 BKMIS_ThungSo = a.BKMIS_ThungSo,
                                 NgayLuyenGang = a.NgayLuyenGang,
                                 ChuyenDen = a.ChuyenDen,
                                 Gio_NM = a.Gio_NM,
                                 KR = a.KR,
                                 ID_TrangThai = a.ID_TrangThai,
                                 T_ID_TrangThai = a.T_ID_TrangThai,
                                 TrangThaiLT = trangThai.TenTrangThai,
                                 ID_Locao = a.ID_Locao,
                                 TenLoCao = loCao.TenLoCao,
                                 G_TenKip = kipG != null ? kipG.TenKip : null,
                                 T_TenKip = kipT != null ? kipT.TenKip : null,
                             }).ToListAsync();
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> ChotThung([FromBody] List<ChotThungDto> selectedList)
        {
            if (selectedList == null || selectedList.Count == 0)
                return BadRequest("Danh sách ID trống.");

            var selectedIds = selectedList
                                .Select(x => x.id)
                                .ToList();

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                                       .Where(x => selectedIds.Contains(x.ID) && x.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                                       .ToListAsync();

            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");
            foreach (var t in thungs)
            {
                t.ID_TrangThai = (int)TinhTrang.DaChot;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
