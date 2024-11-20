//using Data_Product.Models;
//using Data_Product.Repositorys;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;

//namespace Data_Product.Controllers
//{
//    public class YCauHieuChinhController : Controller
//    {
//        private readonly DataContext _context;

//        public YCauHieuChinhController(DataContext _context)
//        {
//            this._context = _context;
//        }
//        public async Task<IActionResult> Index(string search, int page = 1)
//        {
//            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
//            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
//            int ID_NhanVien_BN = TaiKhoan.ID_TaiKhoan;
//            var res = await (from a in _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BN == ID_NhanVien_BN && x.ID_TrangThai_BN == 0 && x.ID_TrangThai_BG == 1 && x.ID_QuyTrinh != 3 ||
//                                                                            x.ID_NhanVien_BN == ID_NhanVien_BN && x.ID_TrangThai_BN == 1 && x.ID_TrangThai_BG == 1 && x.ID_QuyTrinh != 3)
//                             select new Tbl_BienBanGiaoNhan
//                             {
//                                 ID_BBGN = a.ID_BBGN,
//                                 ID_NhanVien_BG = a.ID_NhanVien_BG,
//                                 ID_PhongBan_BG = a.ID_PhongBan_BG,
//                                 ID_Xuong_BG = a.ID_Xuong_BG,
//                                 ID_ViTri_BG = a.ID_ViTri_BG,
//                                 ThoiGianXuLyBG = (DateTime)a.ThoiGianXuLyBG,
//                                 ID_TrangThai_BG = a.ID_TrangThai_BG,
//                                 ID_NhanVien_BN = a.ID_NhanVien_BN,
//                                 ID_PhongBan_BN = a.ID_PhongBan_BN,
//                                 ID_Xuong_BN = a.ID_Xuong_BN,
//                                 ID_ViTri_BN = a.ID_ViTri_BN,
//                                 ThoiGianXuLyBN = (DateTime?)a.ThoiGianXuLyBN ?? default,
//                                 ID_TrangThai_BN = a.ID_TrangThai_BN,
//                                 SoPhieu = a.SoPhieu,
//                                 ID_TrangThai_BBGN = a.ID_TrangThai_BBGN,
//                                 ID_QuyTrinh = a.ID_QuyTrinh
//                             }).ToListAsync();
//            if (search != null)
//            {
//                res = res.Where(x => x.SoPhieu.Contains(search)).ToList();

//            }
//            const int pageSize = 20;
//            if (page < 1)
//            {
//                page = 1;
//            }
//            int resCount = res.Count;
//            var pager = new Pager(resCount, page, pageSize);
//            int recSkip = (page - 1) * pageSize;
//            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
//            this.ViewBag.Pager = pager;
//            return View(data);
//        }
//    }
//}
