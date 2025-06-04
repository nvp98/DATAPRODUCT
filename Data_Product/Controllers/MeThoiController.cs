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
        public async Task<PartialViewResult> FilterMeThoi([FromBody] int id_LoThoi)
        {
            var currentYear = DateTime.Now.Year;
            var result = await _context.Tbl_MeThoi
                .Where(x => x.NgayTao.Year == currentYear && x.ID_LoThoi == id_LoThoi && x.ID_TrangThai == (int)TinhTrang.ChoXuLy)
                .ToListAsync();

            return PartialView("FilterMeThoi", result);
           
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

            var currentDate = DateTime.Now.Date;
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
                        NgayTao = DateTime.Now,
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
    }
}
