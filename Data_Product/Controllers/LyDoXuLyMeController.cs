using Data_Product.Common.Enums;
using Data_Product.DTO;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Wordprocessing;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace Data_Product.Controllers
{
    public class LyDoXuLyMeController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public LyDoXuLyMeController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchLyDoDto dto)
        {
            try
            {
                var query = _context.Tbl_LyDoXuLyMe.AsQueryable();

                if (!string.IsNullOrEmpty(dto.SearchText))
                {
                    query = query.Where(x => x.LyDo.Contains(dto.SearchText));
                }

                if (dto.TrangThai.HasValue)
                {
                    query = query.Where(x => x.TrangThai == dto.TrangThai.Value);
                }

                if (dto.TuNgay.HasValue && dto.DenNgay.HasValue)
                {
                    var tuNgay = dto.TuNgay.Value.Date;
                    var denNgay = dto.DenNgay.Value.Date.AddDays(1);

                    query = query.Where(x => x.NgayTao >= tuNgay && x.NgayTao < denNgay);
                }
                int totalPages = await query.CountAsync();

                query = query.Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize);

                var result = await (from lo in query
                                    orderby lo.ID
                                    select new Tbl_LyDoXuLyMe
                                    {
                                        ID = lo.ID,
                                        LyDo = lo.LyDo,
                                        TrangThai = lo.TrangThai,
                                        NgayTao = lo.NgayTao,

                                    }).ToListAsync();

                return Ok(new { TotalRecords = totalPages, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchAutoComplete([FromBody] SearchLyDoDto dto)
        {
            try
            {
                var query = _context.Tbl_LyDoXuLyMe.AsQueryable().Where(x => x.TrangThai == true);


                if (!string.IsNullOrEmpty(dto.SearchText))
                {
                    query = query.Where(x => x.LyDo.Contains(dto.SearchText));
                }
                
                int totalPages = await query.CountAsync();

                query = query.Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize);

                var result = await (from lo in query
                                    orderby lo.ID
                                    select new Tbl_LyDoXuLyMe
                                    {
                                        ID = lo.ID,
                                        LyDo = lo.LyDo,
                                        TrangThai = lo.TrangThai,
                                        NgayTao = lo.NgayTao,

                                    }).ToListAsync();

                return Ok(new { TotalRecords = totalPages, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpsertLyDoXuLyMe([FromBody] LyDoXuLyMeDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.LyDo))
                    return BadRequest(new { success = false, message = "Lý do không được để trống." });

                var lyDo = dto.LyDo.Trim();

                if (!dto.ID.HasValue || dto.ID == 0)
                {
                    var isDuplicate = await _context.Tbl_LyDoXuLyMe
                        .AnyAsync(x => x.LyDo == lyDo);

                    if (isDuplicate)
                        return BadRequest(new { success = false, message = "Lý do đã tồn tại." });

                    var entity = new Tbl_LyDoXuLyMe
                    {
                        LyDo = lyDo,
                        TrangThai = dto.TrangThai,
                        NgayTao = DateTime.UtcNow
                    };

                    _context.Tbl_LyDoXuLyMe.Add(entity);
                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, data = entity });
                }

                var existing = await _context.Tbl_LyDoXuLyMe
                    .FirstOrDefaultAsync(x => x.ID == dto.ID.Value);

                if (existing == null)
                    return BadRequest(new { success = false, message = "Không tìm thấy lý do cần sửa." });

                var isDuplicateUpdate = await _context.Tbl_LyDoXuLyMe
                    .AnyAsync(x => x.LyDo == lyDo && x.ID != dto.ID.Value);

                if (isDuplicateUpdate)
                    return BadRequest(new { success = false, message = "Lý do đã tồn tại." });

                existing.LyDo = lyDo;
                existing.TrangThai = dto.TrangThai;

                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTrangThaiLyDoXuLyMe(int id)
        {
            try
            {
                var entity = await _context.Tbl_LyDoXuLyMe
                    .FirstOrDefaultAsync(x => x.ID == id);

                if (entity == null)
                    return NotFound(new { success = false, message = "Không tìm thấy lý do." });

                entity.TrangThai = !entity.TrangThai;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    trangThai = entity.TrangThai,
                    message = entity.TrangThai
                        ? "Đã kích hoạt trạng thái."
                        : "Đã ngừng kích hoạt trạng thái."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteLyDoXuLyMe(int id)
        {
            try
            {
                var entity = await _context.Tbl_LyDoXuLyMe.FirstOrDefaultAsync(x => x.ID == id);
                if (entity == null)
                    return NotFound(new { success = false, message = "Không tìm thấy lý do." });

                _context.Tbl_LyDoXuLyMe.Remove(entity);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã xóa lý do xử lý mẻ." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

    }
}
