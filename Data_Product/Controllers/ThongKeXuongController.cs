using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class ThongKeXuongController : Controller
    {
        private readonly DataContext _context;

        public ThongKeXuongController(DataContext _context)
        {
            this._context = _context;
        }
        // GET: ThongKeXuongController
        public async Task<IActionResult> Index(string search, int page = 1)
        {

            var res = await (from a in _context.Tbl_ThongKeXuong
                             join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                             join c in _context.Tbl_Xuong on a.ID_Xuong equals c.ID_Xuong
                             select new Tbl_ThongKeXuong
                             {
                                
                             }).ToListAsync();
            if (search != null)
            {
               

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

        // GET: ThongKeXuongController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ThongKeXuongController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ThongKeXuongController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ThongKeXuongController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ThongKeXuongController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ThongKeXuongController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ThongKeXuongController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
