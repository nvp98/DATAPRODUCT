using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Data_Product.Controllers
{

    public enum TinhTrang
    {
        [Display(Name = "Chưa chuyển")]
        ChuaChuyen,
        
        [Display(Name = "Chờ xử lý")]
        ChoXuLy,
        
        [Display(Name = "Đã chuyển")]
        DaChuyen,
        
        [Display(Name = "Đã nhận")]
        DaNhan,

        [Display(Name = "Đã chốt")]
        DaChot
    }

    public enum CaLamViec
    {
        Ngay, // 1 = "N"
        Dem, // 2 = "Đ"
    }

    public class NhanThungDto
    {
        public List<int> selectedIds { get; set; }
        public DateTime ngayNhan { get; set; }
        public int idCa { get; set; }
        public int idLoThoi { get; set; }
        public int idNguoiNhan { get; set; }


    }
    public class BM_GangThoi_ThepController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;


        private static List<Tbl_BM_16_GangLong> allItems = new();
        // Danh sách được nhận
        private static List<Tbl_BM_16_GangLong> selectedItems = new();

        public BM_GangThoi_ThepController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }


        public async Task<IActionResult> Index(DateTime? ngayLamViec, int? idKip, string? chuyenDen, int? idTrangThai, int? idLocao)
        {
            // Lò cao
            var loCaoList = await _context.Tbl_LoCao.ToListAsync();
            ViewBag.LoCaoList = loCaoList;

            // Lò Thổi
            var loThoiList = await _context.Tbl_LoThoi.ToListAsync();
            ViewBag.LoThoiList = loThoiList;

            // Tình trạng
            var enumValues = System.Enum.GetValues(typeof(TinhTrang))
                                   .Cast<TinhTrang>()
                                   .Select(e => new SelectListItem
                                   {
                                       Value = ((int)e).ToString(),
                                       Text = e.GetType()
                                                .GetMember(e.ToString())
                                                .First()
                                                .GetCustomAttribute<DisplayAttribute>()?
                                                .GetName() ?? e.ToString()
                                   }).ToList();

            ViewBag.TinhTrangList = enumValues;

            var query = _context.Tbl_BM_16_GangLong.AsQueryable();

            if (ngayLamViec.HasValue)
            {
                query = query.Where(x => x.NgayLuyenGang == ngayLamViec.Value);
            }

            if (idKip.HasValue)
            {
                query = query.Where(x => x.G_ID_Kip == idKip.Value);
            }

            if (idLocao.HasValue)
            {
                query = query.Where(x => x.ID_Locao == idLocao.Value);
            }

            if (idTrangThai.HasValue)
            {
                query = query.Where(x => x.ID_TrangThai == idTrangThai.Value);
            }

            if (!string.IsNullOrEmpty(chuyenDen))
            {
                query = query.Where(x => x.ChuyenDen.Contains(chuyenDen));
            }

            var res = await (from a in _context.Tbl_BM_16_GangLong
                             join trangThai in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThai.ID
                             join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = a.ID,
                                 MaThungGang = a.MaThungGang,
                                 BKMIS_ThungSo = a.BKMIS_ThungSo,
                                 NgayLuyenGang = a.NgayLuyenGang,
                                 ChuyenDen = a.ChuyenDen,
                                 Gio_NM = a.Gio_NM,
                                 KR = a.KR,
                                 T_ID_TrangThai = a.T_ID_TrangThai,
                                 TrangThaiLT = trangThai.TenTrangThai,
                                 ID_Locao = a.ID_Locao,
                                 TenLoCao = loCao.TenLoCao,
                                 T_NguoiNhanList = a.T_NguoiNhanList
                             }).ToListAsync();

            var data = res;
           
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemThungDaNhan(DateTime? ngayLamViec, int? idKip, int? idLoThoi)
        {
            var query = _context.Tbl_BM_16_GangLong.AsQueryable();

            query = query.Where(x => x.T_ID_TrangThai == (int)TinhTrang.DaNhan);

            if (ngayLamViec.HasValue)
            {
                query = query.Where(x => x.NgayLuyenThep == ngayLamViec.Value);
            }

            if (idKip.HasValue)
            {
                query = query.Where(x => x.T_ID_Kip == idKip.Value);
            }

            if (idLoThoi.HasValue)
            {
                query = query.Where(x => x.ID_LoThoi == idLoThoi.Value);
            }

            var res = await (from a in _context.Tbl_BM_16_GangLong
                             join trangThai in _context.Tbl_BM_16_TrangThai on a.ID_TrangThai equals trangThai.ID
                             join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID
                             join meThoi in _context.Tbl_MeThoi on a.ID_MeThoi equals meThoi.ID
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = a.ID,
                                 MaThungThep = a.MaThungThep,
                                 BKMIS_ThungSo = a.BKMIS_ThungSo,
                                 NgayLuyenThep = a.NgayLuyenThep,
                                 ChuyenDen = a.ChuyenDen,
                                 KR = a.KR,
                                 T_KLThungVaGang = a.T_KLThungVaGang,
                                 T_KLThungChua = a.T_KLThungChua,
                                 T_KLGangLong = a.T_KLGangLong,
                                 ThungTrungGian = a.ThungTrungGian,
                                 T_KLThungVaGang_Thoi = a.T_KLThungVaGang_Thoi,
                                 T_KLThungChua_Thoi = a.T_KLThungChua_Thoi,
                                 T_KLGangLongThoi = a.T_KLGangLongThoi,
                                 MaMeThoi = meThoi.MaMeThoi,
                                 TrangThai = trangThai.TenTrangThai,
                                 TenLoCao = loCao.TenLoCao,
                             }).ToListAsync();

            var data = res.ToList();
            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Nhan([FromBody] NhanThungDto payload)
        {
            if (payload.selectedIds == null || payload.selectedIds.Count == 0 || payload.ngayNhan == null || payload.idCa == null || payload.idLoThoi == null || payload.idNguoiNhan == null)
                return BadRequest("Danh sách ID trống.");

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                                       .Where(x => payload.selectedIds.Contains(x.ID))
                                       .ToListAsync();

            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");

            // Bọc trong transaction để bảo toàn dữ liệu
            using var tran = await _context.Database.BeginTransactionAsync();

            foreach (var t in thungs)
            {
                // tao ma thung Thep
                int countItem = thungs.Count(s => s.MaThungGang == t.MaThungGang);
                string countSelected = countItem == 0 ? "00" : countItem < 10 ? "0" + countItem : countItem.ToString();

                string ca = t.T_Ca == (int)CaLamViec.Ngay ? "N" : "Đ";
                string dayStr = DateTime.Now.Day.ToString("00");
                string MaThungThep = $"{t.MaThungGang}.{dayStr}{ca}T{payload.idLoThoi}.{countSelected}";


                // tao danh sach nguoi nhan
                List<NguoiNhanModel> listNguoiNhan = t.T_NguoiNhanList;
                var item = listNguoiNhan.FirstOrDefault(x => x.ID_NguoiNhan == payload.idNguoiNhan);
                if (item != null)
                {
                    item.TongSoLanNhan += 1;
                }
                else
                {
                    listNguoiNhan.Add(new NguoiNhanModel
                    {
                        ID_NguoiNhan = payload.idNguoiNhan,
                        TongSoLanNhan = 1
                    });
                }

                if (t.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                {
                    // 2. Tạo bản sao
                    var clone = new Tbl_BM_16_GangLong
                    {
                        MaThungGang = t.MaThungGang,
                        BKMIS_SoMe = t.BKMIS_SoMe,
                        BKMIS_Gio = t.BKMIS_Gio,
                        BKMIS_PhanLoai = t.BKMIS_PhanLoai,
                        BKMIS_ThungSo = t.BKMIS_ThungSo,
                        NgayLuyenGang = t.NgayLuyenGang,
                        KL_XeGoong = t.KL_XeGoong,
                        G_KLThungChua = t.G_KLThungChua,
                        G_KLThungVaGang = t.G_KLThungVaGang,
                        G_KLGangLong = t.G_KLGangLong,
                        ChuyenDen = t.ChuyenDen,
                        Gio_NM = t.Gio_NM,
                        G_GhiChu = t.G_GhiChu,
                        G_Ca = t.G_Ca,
                        G_ID_Kip = t.G_ID_Kip,
                        G_ID_NguoiChuyen = t.G_ID_NguoiChuyen,
                        G_ID_NguoiLuu = t.G_ID_NguoiLuu,
                        G_ID_NguoiThuHoi = t.G_ID_NguoiThuHoi,
                        G_ID_TrangThai = t.G_ID_TrangThai,
                        MaThungThep = MaThungThep,
                        ID_TrangThai = t.ID_TrangThai,
                        T_ID_TrangThai = (int)TinhTrang.DaNhan,
                        ID_Locao = t.ID_Locao,
                        ID_Phieu = t.ID_Phieu,
                        T_NguoiNhanList = listNguoiNhan

                    };
                    _context.Tbl_BM_16_GangLong.Add(clone);
                }
                else
                {
                    // Thùng chưa nhận → cập nhật sang Đã nhận
                    t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                    t.MaThungThep = MaThungThep;
                    t.T_NguoiNhanList = listNguoiNhan;
                }
            }

            await _context.SaveChangesAsync();
            await tran.CommitAsync();

            return Ok(new { Message = "Đã xử lý nhận thùng.", Count = payload.selectedIds.Count });
        }

        //[HttpPost]
        //public JsonResult HuyNhan([FromBody] List<int> selectedIds)
        //{
        //    var selected = allItems
        //        .Where(x => selectedIds.Contains(x.Id))
        //        .ToList();

        //    foreach (var item in selected)
        //    {
        //        item.TinhTrang = TinhTrang.ChoXuLy;

        //        selectedItems.RemoveAll(x => x.Id == item.Id);
        //    }

        //    return Json(new { success = true, message = "Hủy nhận thành công!" });
        //}
    }
}
