using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Mysqlx;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class MeThoiController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public MeThoiController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }


        [HttpPost]
        public async Task<IActionResult> FilterMeThoi([FromBody] MeThoiSearchDto dto)
        {
            var query = _context.Tbl_MeThoi
                    .Where(x => x.ID_LoThoi == dto.id_LoThoi &&
                    //x.NgayTao.Year == dto.ngayLamViec.Year && 
                    x.ID_TrangThai == (int)TinhTrang.ChoXuLy &&
                    x.Is_Delete == false);

            if (!string.IsNullOrWhiteSpace(dto.searchText))
            {
                query = query.Where(x => x.MaMeThoi.Contains(dto.searchText));
            }


            var result = await query
                .OrderBy(x => x.MaMeThoi)
                .Take(100)
                .Select(x => new { id = x.ID, maMeThoi = x.MaMeThoi })
                .ToListAsync();

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> TaoMeThoi()
        {
            //list lo thoi
            var loThoiList = await _context.Tbl_LoThoi.ToListAsync();
            ViewBag.LoThoiList = loThoiList;

            return PartialView("TaoMeThoi"); 
        }

        [HttpPost]
        public async Task<IActionResult> TaoMeThoi([FromBody] TaoMeThoiDto payload)
        {
            if (payload.soLuong <= 0 || string.IsNullOrEmpty(payload.maLoThoi))
                return BadRequest("Dữ liệu không hợp lệ.");

            // get User
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

            var currentDate = payload.ngayTao;
            string yearPart = (currentDate.Year % 100).ToString("D2"); 
            string loaiMe = payload.maLoThoi.Trim(); // A,B,C
            string prefix = yearPart + loaiMe;       // Ví dụ: 25A

            using var transaction = await _context.Database.BeginTransactionAsync();
            try {
                // Kiểm tra hoặc tạo mới dòng counter
                var counter = await _context.Tbl_Counter_MeThoi
                    .FirstOrDefaultAsync(x => x.Prefix == prefix);

                if (counter == null)
                {
                    counter = new Tbl_Counter_MeThoi
                    {
                        Prefix = prefix,
                        STT_HienTai = 0
                    };
                    _context.Tbl_Counter_MeThoi.Add(counter);
                    await _context.SaveChangesAsync(); // Lưu để lấy ID
                }

                // Tạo danh sách mẻ mới
                var newItems = new List<Tbl_MeThoi>();
                for (int i = 1; i <= payload.soLuong; i++)
                {
                    counter.STT_HienTai += 1;
                    string maMeThoi = $"{prefix}{counter.STT_HienTai.ToString("D6")}"; // VD: 25A000123

                    var meThoi = new Tbl_MeThoi
                    {
                        MaMeThoi = maMeThoi,
                        NgayTao = payload.ngayTao,
                        ID_LoThoi = payload.id_LoThoi,
                        ID_TrangThai = (int)TinhTrang.ChoXuLy,
                        ID_NguoiTao = TaiKhoan.ID_TaiKhoan
                    };

                    newItems.Add(meThoi);
                    _context.Tbl_MeThoi.Add(meThoi);
                }

                // Cập nhật counter sau cùng
                _context.Tbl_Counter_MeThoi.Update(counter);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { success = true, Message = "Đã xử tạo mới.", meThoiMoi = newItems });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi khi tạo mẻ thổi: " + ex.Message);
            }


        }


        [HttpPost]
        public async Task<IActionResult> DsMeThoi([FromBody] int id)
        {
            var currentYear = DateTime.Now.Year;
            var result = await _context.Tbl_MeThoi
                .Where(x => x.NgayTao.Year == currentYear && x.ID_LoThoi == id && x.Is_Delete == false)
                .ToListAsync();

            return Ok(result);

        }
        [HttpPost]
        public async Task<IActionResult> XoaMeThoi([FromBody] int id)
        {
            var meThoi = await _context.Tbl_MeThoi.FirstOrDefaultAsync(x => x.ID == id && !x.Is_Delete);
            if (meThoi == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy mẻ thời cần xóa." });
            }

            meThoi.Is_Delete = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đã xóa mềm mẻ thời thành công." });
        }
        [HttpPost]
        public async Task<IActionResult>KhoiPhucMeThoi([FromBody] int id)
        {
            var methoi = await _context.Tbl_MeThoi.FirstOrDefaultAsync(x => x.ID == id && x.Is_Delete);
             if (methoi == null)
              return NotFound(new { success = false, message = "Không tìm thấy dữ liệu đã xóa để khôi phục." });

            methoi.Is_Delete = false;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Khôi phục mẻ thời thành công." });
        }
    }
}
