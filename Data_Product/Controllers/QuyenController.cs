using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class QuyenController : Controller
    {
        private readonly DataContext _context;

        public QuyenController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_Quyen
                             select new Tbl_Quyen
                             {
                                 ID_Quyen = a.ID_Quyen,
                                 TenQuyen = a.TenQuyen
                             }).ToListAsync();
            if (search != null)
            {
                res = res.Where(x => x.TenQuyen.Contains(search)).ToList();

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
        public async Task<IActionResult> Create(Tbl_Quyen _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Quyen_insert {0}", _DO.TenQuyen);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "Quyen");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "Quyen");
            }

            var res = await (from a in _context.Tbl_Quyen.Where(x=>x.ID_Quyen == id)
                             select new Tbl_Quyen
                             {
                                 ID_Quyen = a.ID_Quyen,
                                 TenQuyen = a.TenQuyen
                             }).ToListAsync();

            Tbl_Quyen DO = new Tbl_Quyen();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID_Quyen = a.ID_Quyen;
                    DO.TenQuyen = a.TenQuyen;
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
        public async Task<IActionResult> Edit(int id, Tbl_Quyen _DO)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Quyen_update {0},{1}", id, _DO.TenQuyen);

                TempData["msgSuccess"] = "<script>alert('Chỉnh sửa thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
            }
            return RedirectToAction("Index", "Quyen");
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_Quyen_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }
            return RedirectToAction("Index", "Quyen");
        }
    }
}
