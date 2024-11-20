using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class PhongBanController : Controller
    {
        private readonly DataContext _context;

        public PhongBanController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_PhongBan
                             select new Tbl_PhongBan
                             {
                                 ID_PhongBan = a.ID_PhongBan,
                                 TenPhongBan = a.TenPhongBan,
                                 TenNgan = a.TenNgan,
                                 ID_TrangThai = (int?)a.ID_TrangThai?? default
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.TenPhongBan.Contains(search) || x.TenNgan.Contains(search)).ToList();

            }
            const int pageSize = 20;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }
        public async Task<IActionResult> Create()
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_PhongBan _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_PhongBan_insert {0},{1},{2}", _DO.TenPhongBan, _DO.TenNgan, 1);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "PhongBan");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "PhongBan");
            }

            var res = await (from a in _context.Tbl_PhongBan.Where(x=>x.ID_PhongBan == id)
                             select new Tbl_PhongBan
                             {
                                 ID_PhongBan = a.ID_PhongBan,
                                 TenPhongBan = a.TenPhongBan,
                                 TenNgan = a.TenNgan,
                                 ID_TrangThai = (int)a.ID_TrangThai
                             }).ToListAsync();

            Tbl_PhongBan DO = new Tbl_PhongBan();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_PhongBan = a.ID_PhongBan;
                    DO.TenPhongBan = a.TenPhongBan;
                    DO.TenNgan = a.TenNgan;
                    DO.ID_TrangThai = (int)a.ID_TrangThai;
                }
            }
            else
            {
                return NotFound();
            }



            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_PhongBan _DO)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_PhongBan_update {0},{1},{2}", id, _DO.TenPhongBan, _DO.TenNgan);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }



            return RedirectToAction("Index", "PhongBan");
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_PhongBan_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "PhongBan");
        }
        public async Task<IActionResult> Lock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_PhongBan_lock {0},{1}", id, 0);

                TempData["msgSuccess"] = "<script>alert('Khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "PhongBan");
        }
        public async Task<IActionResult> Unlock(int id)
        {
            try
            {

                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_PhongBan_lock {0},{1}", id, 1);

                TempData["msgSuccess"] = "<script>alert('Mở khóa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Mở khóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "PhongBan");
        }
    }
}
