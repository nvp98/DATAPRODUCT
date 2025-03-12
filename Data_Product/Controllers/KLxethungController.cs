using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace Data_Product.Controllers
{
    public class KLxethungController : Controller
    {
        private readonly DataContext _context;
        public KLxethungController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(string search, int page = 1)
        {
            // Join Tbl_KL_THUNGXE with Tbl_LoCao to get TenLoCao
            var res = await (from a in _context.Tbl_KL_THUNGXE
                             join l in _context.Tbl_LoCao // Assuming Tbl_LoCao is in your DbContext
                             on a.ID_LOCAO equals l.ID // Join condition: ID_LOCAO matches ID in Tbl_LoCao
                             select new Tbl_KL_THUNGXE // You can use a ViewModel instead if preferred
                             {
                                 ID = a.ID,
                                 ID_LOCAO = a.ID_LOCAO,
                                 KL_THUNGXE = a.KL_THUNGXE,
                                 TenLoCao = l.TenLoCao // Add TenLoCao from Tbl_LoCao
                             }).ToListAsync();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                res = res.Where(x =>
                    x.ID_LOCAO.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.KL_THUNGXE.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    x.TenLoCao.Contains(search, StringComparison.OrdinalIgnoreCase) // Add search by TenLoCao
                ).ToList();
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
        //public async Task<IActionResult> Create()
        //{

        //    return PartialView();
        //}
        public IActionResult Create()
        {
            // Lấy danh sách lò cao từ bảng Tbl_LoCao
            var loCaoList = _context.Tbl_LoCao
                .Select(lc => new SelectListItem
                {
                    Value = lc.ID.ToString(), // Giả sử ID là int, nếu không thì điều chỉnh kiểu dữ liệu
                    Text = lc.TenLoCao
                }).ToList();

            ViewBag.LoCaoList = new SelectList(loCaoList, "Value", "Text");
            return View();
        }

        // POST: Xử lý thêm mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tbl_KL_THUNGXE _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                // Kiểm tra xem ID đã tồn tại hay chưa (nếu bạn có trường ID riêng trong Tbl_KL_THUNGXE)

                if (!ModelState.IsValid)
                {
                    TempData["msgError"] = "<script>alert('Nhập KL xe thùng không hợp lệ');</script>";
                    return RedirectToAction("Index", "KLxethung");
                }


                // Join dữ liệu từ bảng Tbl_LoCao để lấy tên lò cao
                var locaoInfo = await (from lc in _context.Tbl_LoCao
                                       where lc.ID == _DO.ID_LOCAO
                                       select new { lc.TenLoCao }).FirstOrDefaultAsync();

                if (locaoInfo == null)
                {
                    TempData["msgError"] = "<script>alert('Không tìm thấy thông tin lò cao');</script>";
                    return RedirectToAction("Index", "KLxethung");
                }
                // Kiểm tra xem KL_THUNGXE có phải là số hay không
                if (!double.TryParse(_DO.KL_THUNGXE.ToString(), out double klThungXe))
                {
                    TempData["msgError"] = "<script>alert('Khối lượng thùng xe phải là số');</script>";
                    return RedirectToAction("Index", "KLxethung");
                }
                // Gán tên lò cao vào đối tượng _DO
                _DO.TenLoCao = locaoInfo.TenLoCao;

                // Thực hiện thêm mới
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_KL_THUNGXE_Insert {0}, {1}",
                    _DO.ID_LOCAO, _DO.KL_THUNGXE);

                TempData["msgSuccess"] = "<script>alert('Thêm mới thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Thêm mới thất bại: " + e.Message + "');</script>";
            }

            return RedirectToAction("Index", "KLxethung");
        }
        public async Task<IActionResult> CapNhat_xethung()
        {

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat_xethung(Tbl_KL_THUNGXE _DO)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                // Lấy tên lò cao từ database
                var loCao = await _context.Tbl_LoCao
                    .Where(l => l.ID == _DO.ID_LOCAO)
                    .Select(l => l.TenLoCao)
                    .FirstOrDefaultAsync();

                if (loCao == null)
                {
                    TempData["msgError"] = "<script>alert('Không tìm thấy lò cao tương ứng');</script>";
                    return RedirectToAction("Index", "KLxethung");
                }

                // Truyền TenLoCao qua ViewBag
                ViewBag.TenLoCao = loCao;

                // Thực hiện cập nhật dữ liệu vào bảng Tbl_KL_THUNGXE
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_KL_THUNGXE_update {0},{1},{2}", _DO.ID, _DO.ID_LOCAO, _DO.KL_THUNGXE);

                // Trả về view và gửi dữ liệu Tbl_KL_THUNGXE
                ViewBag.Message = "Cập nhật thành công";
                return View("CapNhat_xethung", _DO); // Trả về lại view "CapNhat_xethung" và dữ liệu của _DO
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
                return RedirectToAction("Index", "KLxethung");
            }
        }
        public async Task<IActionResult> Delete(int id)
        {

            try
            {
                var exists = _context.Tbl_ChiTiet_BBGNGangLong.Any(ct => ct.ID_LoCao == id);

                if (exists)
                {
                    TempData["msgError"] = "<script>alert('Không thể xóa vì ID đã tồn tại');</script>";


                    return RedirectToAction("Index"); // trả về view tương ứng
                }

                // Nếu id không tồn tại trong bảng Tbl_ChiTiet_BBGNGangLong, thực hiện xóa
                var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_KL_THUNGXE_delete {0}", id);

                TempData["msgSuccess"] = "<script>alert('Xóa dữ liệu thành công');</script>";

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xóa dữ liệu thất bại');</script>";
            }


            return RedirectToAction("Index", "KLxethung");
        }
        public async Task<IActionResult> Edit(int id)
        {
            // Lấy thông tin thùng xe cần chỉnh sửa
            var thungXe = await _context.Tbl_KL_THUNGXE.FindAsync(id);

            if (thungXe == null)
            {
                return NotFound();
            }

            // Lấy danh sách lò cao từ bảng Tbl_LoCao
            var loCaoList = await _context.Tbl_LoCao
                .Select(l => new SelectListItem
                {
                    Value = l.ID.ToString(), // Giá trị là ID_LOCAO
                    Text = l.TenLoCao // Hiển thị là TEN_LOCAO
                })
                .ToListAsync();

            // Truyền danh sách lò cao vào ViewBag
            ViewBag.LoCaoList = new SelectList(loCaoList, "Value", "Text", thungXe.ID_LOCAO);

            return View(thungXe);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Tbl_KL_THUNGXE _DO)
        {
            try
            {


                // Kiểm tra xem KL_THUNGXE có phải là một số hợp lệ không


                if (!ModelState.IsValid)
                {
                    TempData["msgError"] = "<script>alert('Nhập KL xe thùng không hợp lệ');</script>";
                    return RedirectToAction("Index", "KLxethung");
                }

                // Thực hiện cập nhật thông tin
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC Tbl_KL_THUNGXE_update {0}, {1}, {2}",
                    _DO.ID, _DO.ID_LOCAO, _DO.KL_THUNGXE);

                if (result > 0)
                {
                    TempData["msgSuccess"] = "<script>alert('Cập nhật thành công');</script>";
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Không có dữ liệu nào được cập nhật');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Cập nhật thất bại: " + e.Message + "');</script>";
            }

            return RedirectToAction("Index", "KLxethung");
        }
    }
}
