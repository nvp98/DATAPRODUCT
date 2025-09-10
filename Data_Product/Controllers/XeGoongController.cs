using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Controllers
{
    public class XeGoongController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        public XeGoongController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public async Task<IActionResult> Index()
        {
            var ds = await _context.Tbl_XeGoong.ToListAsync();
            var loCaos = await _context.Tbl_LoCao.ToListAsync();
            ViewBag.LoCaoList = new SelectList(loCaos, "ID", "TenLoCao");
            return View(ds);
        }

        [HttpGet]
        public async Task<IActionResult> KLXeLoCao(int idlocao)
        {
            var xe =  await _context.Tbl_XeGoong.FirstOrDefaultAsync(x=>x.ID_LoCao== idlocao);
            if (xe != null)           
                return Json(new { success = true, klXe = xe.KL_Xe });
            else
                return Json(new { success = false });
        }
        [HttpPost]
        public async Task<IActionResult> Create(Tbl_XeGoong xe)
        {
            if (ModelState.IsValid)
            {
                bool locao = await _context.Tbl_XeGoong.AnyAsync(x => x.ID_LoCao == xe.ID_LoCao);
                if (locao)
                {
                    TempData["msgError"] = "<script>alert('Lò này đã có xe. Vui lòng chọn lò khác.');</script>";
                    return RedirectToAction("Index");
                }
                try
                {
                    _context.Tbl_XeGoong.Add(xe);
                    await _context.SaveChangesAsync();
                    TempData["msgSuccess"] = "<script>alert('Thêm mới thành công!');</script>";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi lưu dữ liệu. Vui lòng thử lại.");
                }
            }
            return View(xe);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var xe = await _context.Tbl_XeGoong.FindAsync(id);
            if (xe == null)
                return NotFound();

            var loCaos = await _context.Tbl_LoCao.ToListAsync();
            ViewBag.LoCaoList = new SelectList(loCaos, "ID", "TenLoCao", xe.ID_LoCao);

            return PartialView("_ModalEditXeGoong", xe);
        }

        [HttpPost]
        public async Task<IActionResult> Edit (Tbl_XeGoong xe)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_ModalEditXeGoong", xe);
            }

            bool loCaoDaTonTai = await _context.Tbl_XeGoong
                .AnyAsync(x => x.ID_LoCao == xe.ID_LoCao && x.Id != xe.Id);

            if (loCaoDaTonTai)
            {
                ModelState.AddModelError("ID_LoCao", "Lò này đã có xe khác.");
                return PartialView("_ModalEditXeGoong", xe);
            }

            _context.Update(xe);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var xe = await _context.Tbl_XeGoong.FindAsync(id);
            if (xe == null)
            {
                return NotFound();
            }
            return PartialView("_ModalDeleteXeGoong", xe);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var xe = await _context.Tbl_XeGoong.FindAsync(id);
            if (xe == null)
            {
                return Json(new { success = false, message = "Không tìm thấy xe để xóa." });
            }

            _context.Tbl_XeGoong.Remove(xe);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
