using Data_Product.DTO;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class BM_16_LoSanXuatController : Controller
    {
        private readonly DataContext _context;

        public BM_16_LoSanXuatController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.PBList = new SelectList(pb, "ID_PhongBan", "TenPhongBan");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchLoSanXuatDto dto)
        {
            try
            {
                var query = _context.Tbl_BM_16_LoSanXuat.AsQueryable();

                if (!string.IsNullOrEmpty(dto.SearchText))
                {
                    query = query.Where(x => x.TenLo.Contains(dto.SearchText));
                }

                var danhsach = await (from lo in query
                                    join map in _context.Tbl_BM_16_LoSanXuat_TaiKhoan on lo.ID equals map.ID_LoSanXuat
                                    join tk in _context.Tbl_TaiKhoan on map.ID_TaiKhoan equals tk.ID_TaiKhoan
                                    join phongBan in _context.Tbl_PhongBan on lo.ID_BoPhan equals phongBan.ID_PhongBan
                                    select new
                                    {
                                        Lo = lo,
                                        TaiKhoan = new TaiKhoanViewModel
                                        {
                                            ID = tk.ID_TaiKhoan,
                                            TenTaiKhoan = tk.TenTaiKhoan,
                                            HoVaTen = tk.HoVaTen
                                        },
                                        ID_BoPhan = phongBan.ID_PhongBan,
                                        BoPhan = phongBan.TenPhongBan,
                                    }).ToListAsync();

                // Gom nhóm theo lò
                var grouped = danhsach
                    .GroupBy(x => x.Lo.ID)
                    .Select(g => new BM_16_LoSanXuatModelView
                    {
                        ID = g.Key,
                        TenLo = g.First().Lo.TenLo,
                        MaLo = g.First().Lo.MaLo,
                        ID_BoPhan = g.First().ID_BoPhan,
                        BoPhan = g.First().BoPhan,
                        IsActived = g.First().Lo.IsActived,
                        ListTaiKhoan = g.Select(x => x.TaiKhoan).ToList()
                    })
                    .OrderBy(x => x.ID)
                    .ToList();

                int totalPages = (int)Math.Ceiling((decimal)grouped.Count / dto.PageSize);

                var result = grouped
                    .Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToList();

                return Ok(new { TotalRecords = totalPages, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveOrUpdate([FromBody] LoSanXuatPayloadDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TenLo))
                return BadRequest("Tên lò không được để trống.");

            if (dto.MaLo <= 0)
                return BadRequest("Mã lò không hợp lệ.");

            if (dto.ID_BoPhan <= 0)
                return BadRequest("Bộ phận không hợp lệ.");

            try
            {
                Tbl_BM_16_LoSanXuat entity;

                if (dto.ID == null || dto.ID == 0)
                {
                    // Tạo mới
                    entity = new Tbl_BM_16_LoSanXuat
                    {
                        TenLo = dto.TenLo,
                        MaLo = dto.MaLo,
                        ID_BoPhan = dto.ID_BoPhan,
                        IsActived = dto.IsActived
                    };
                    _context.Tbl_BM_16_LoSanXuat.Add(entity);
                    await _context.SaveChangesAsync(); // cần để có ID
                }
                else
                {
                    // Cập nhật
                    entity = await _context.Tbl_BM_16_LoSanXuat.FindAsync(dto.ID);
                    if (entity == null) return NotFound();

                    entity.TenLo = dto.TenLo;
                    entity.MaLo = dto.MaLo;
                    entity.ID_BoPhan = dto.ID_BoPhan;
                    entity.IsActived = dto.IsActived;

                    _context.Update(entity);

                    // Xóa các mapping tài khoản cũ
                    var oldTks = _context.Tbl_BM_16_LoSanXuat_TaiKhoan.Where(x => x.ID_LoSanXuat == entity.ID);
                    _context.Tbl_BM_16_LoSanXuat_TaiKhoan.RemoveRange(oldTks);
                }

                // Thêm danh sách tài khoản mới (nếu có)
                if (dto.TaiKhoanIds != null && dto.TaiKhoanIds.Any())
                {
                    var mappings = dto.TaiKhoanIds.Select(id => new Tbl_BM_16_LoSanXuat_TaiKhoan
                    {
                        ID_LoSanXuat = entity.ID,
                        ID_TaiKhoan = id
                    }).ToList();
                    _context.Tbl_BM_16_LoSanXuat_TaiKhoan.AddRange(mappings);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetById([FromBody] int Id)
        {
            if(Id == null) return BadRequest(new { success = false});
            try
            {
                var item = await (from a in _context.Tbl_BM_16_LoSanXuat
                                  join phongBan in _context.Tbl_PhongBan on a.ID_BoPhan equals phongBan.ID_PhongBan
                                  where a.ID == Id
                                  select new BM_16_LoSanXuatModelView
                                  {
                                      ID = a.ID,
                                      MaLo = a.MaLo,
                                      TenLo = a.TenLo,
                                      IsActived = a.IsActived,
                                      ID_BoPhan = phongBan.ID_PhongBan,
                                      BoPhan = phongBan.TenPhongBan,
                                      ListTaiKhoan = new List<TaiKhoanViewModel>() 
                                  }).FirstOrDefaultAsync();

                if (item != null)
                {
                    item.ListTaiKhoan = await (from map in _context.Tbl_BM_16_LoSanXuat_TaiKhoan
                                               join tk in _context.Tbl_TaiKhoan on map.ID_TaiKhoan equals tk.ID_TaiKhoan
                                               where map.ID_LoSanXuat == Id
                                               select new TaiKhoanViewModel
                                               {
                                                   ID = tk.ID_TaiKhoan,
                                                   TenTaiKhoan = tk.TenTaiKhoan,
                                                   HoVaTen = tk.HoVaTen
                                               }).ToListAsync();
                }

                return Ok(new { success = true, item });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
