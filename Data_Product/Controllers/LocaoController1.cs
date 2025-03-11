using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class LocaoController : Controller
    {
        private readonly DataContext _context;

        public LocaoController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            var res = await (from a in _context.Tbl_LoCao
                             select new Tbl_LoCao
                             {
                                 ID = a.ID,
                                 TenLoCao = a.TenLoCao
                             }).ToListAsync();
            if (search == null)
            {
                TempData["msgSuccess"] = "<script>alert('Vui lòng nhập ID hoặc tên Lò cao để tìm kiếm ');</script>";
            }
            else
            {
                res = res.Where(x => x.TenLoCao.Contains(search) || x.ID.ToString().Contains(search)).ToList();
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
        public async Task<IActionResult> Create(Tbl_LoCao _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_LoCao_insert {0},{1}", _DO.ID, _DO.TenLoCao);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại');</script>";
            }

            return RedirectToAction("Index", "Locao");
        }
        public async Task<IActionResult> CapNhat_Locao()
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat_Locao(Tbl_LoCao _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {

                try
                {
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_LoCao_update {0},{1}", _DO.ID, _DO.TenLoCao);

                    TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
                }
                catch (Exception e)
                {
                    TempData["msgError"] = "<script>alert('cập nhật thất bại');</script>";
                }


                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('cập nhật thất bại');</script>";
            }

            return RedirectToAction("Index", "Locao");
        }
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_LoCao_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "Locao");
        }
        public async Task<IActionResult> Edit(int? id, int? page)
        {
            if (id == null)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";

                return RedirectToAction("Index", "Locao");
            }

            var res = await (from a in _context.Tbl_LoCao.Where(x => x.ID == id)
                             select new Tbl_LoCao
                             {
                                 ID = a.ID,
                                 TenLoCao = a.TenLoCao,

                             }).ToListAsync();

            Tbl_LoCao DO = new Tbl_LoCao();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.TenLoCao = a.TenLoCao;

                }

                // var ID = _context.Tbl_LoCao.Where(x => x.ID == id).ToList();
                ViewBag.ID = DO.ID;
            }
            else
            {
                return NotFound();
            }

            return PartialView(DO);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tbl_LoCao _DO)
        {
            try
            {
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_LoCao_update {0},{1}", id, _DO.TenLoCao);

                TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('cập nhật thất bại');</script>";
            }


            TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
            return RedirectToAction("Index", "Locao");
        }


    }
}
