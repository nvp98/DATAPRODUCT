using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Mysqlx;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Data_Product.Controllers
{
    public class BM_GangThoi_ThepController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public static List<ThungNhanResultDto> resultList = new List<ThungNhanResultDto>();

        public BM_GangThoi_ThepController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }


        public async Task<IActionResult> Index()
        {
            // Lò cao
            var loCaoList = await _context.Tbl_LoCao.ToListAsync();
            ViewBag.LoCaoList = loCaoList;

            // Lò Thổi
            var loThoiList = await _context.Tbl_LoThoi.ToListAsync();
            ViewBag.LoThoiList = loThoiList;

            // Tình trạng
            //var enumValues = System.Enum.GetValues(typeof(TinhTrang))
            //                       .Cast<TinhTrang>()
            //                       .Select(e => new SelectListItem
            //                       {
            //                           Value = ((int)e).ToString(),
            //                           Text = e.GetType()
            //                                    .GetMember(e.ToString())
            //                                    .First()
            //                                    .GetCustomAttribute<DisplayAttribute>()?
            //                                    .GetName() ?? e.ToString()
            //                       }).ToList();

            //ViewBag.TinhTrangList = enumValues;
           
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> SearchLT([FromBody] SearchLTDto payload)
        {
            var query = _context.Tbl_BM_16_GangLong
                        .Where(x => x.T_copy == false)
                        .AsQueryable();

            if (payload.NgayLuyenGang.HasValue)
            {
                query = query.Where(x => x.NgayLuyenGang >= payload.NgayLuyenGang.Value && x.NgayLuyenGang < payload.NgayLuyenGang.Value.AddDays(1));
                
            }

            if (payload.G_Ca.HasValue)
            {
                query = query.Where(x => x.G_Ca == payload.G_Ca.Value);
            }
            if (payload.ID_Locao.HasValue)
            {
                query = query.Where(x => x.ID_Locao == payload.ID_Locao.Value);
            }

            if (payload.ID_TrangThai.HasValue)
            {
                query = query.Where(x => x.T_ID_TrangThai == payload.ID_TrangThai.Value);
            }

            if (!string.IsNullOrEmpty(payload.ChuyenDen))
            {
                query = query.Where(x => x.ChuyenDen.Contains(payload.ChuyenDen));
            }
           
            var res = await (from a in query
                             join trangThai in _context.Tbl_BM_16_TrangThai on a.T_ID_TrangThai equals trangThai.ID
                             join loCao in _context.Tbl_LoCao on a.ID_Locao equals loCao.ID
                             where a.T_copy == false && a.G_ID_TrangThai == (int)TinhTrang.DaChuyen
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = a.ID,
                                 MaThungGang = a.MaThungGang,
                                 BKMIS_ThungSo = a.BKMIS_ThungSo,
                                 NgayLuyenGang = a.NgayLuyenGang,
                                 ChuyenDen = a.ChuyenDen,
                                 Gio_NM = a.Gio_NM,
                                 KR = a.KR,
                                 ID_TrangThai = a.ID_TrangThai,
                                 T_ID_TrangThai = a.T_ID_TrangThai,
                                 TrangThaiLT = trangThai.TenTrangThai,
                                 ID_Locao = a.ID_Locao,
                                 TenLoCao = loCao.TenLoCao
                             }).ToListAsync();


            // get bang phu de lay ten va count so lan nhan thung
            var thungUserList = await _context.Tbl_BM_16_TaiKhoan_Thung.Join(_context.Tbl_TaiKhoan,
                      thung => thung.ID_taiKhoan,
                      user => user.ID_TaiKhoan,
                      (thung, user) => new
                      {
                          thung.MaThungGang,
                          user.HoVaTen,
                      })
                .ToListAsync();

            // Gom theo MaThungGang để có danh sách tên + count
            var userStats = thungUserList
                .GroupBy(x => x.MaThungGang)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(u => u.HoVaTen)
                          .Select(x => $"{x.Key} ({x.Count()})")
                          .ToList()
                );

            var data = res.Select(x => new
            {
                x.ID,
                x.MaThungGang,
                x.BKMIS_ThungSo,
                x.NgayLuyenGang,
                x.ChuyenDen,
                x.Gio_NM,
                x.KR,
                x.ID_TrangThai,
                x.T_ID_TrangThai,
                x.TrangThaiLT,
                x.ID_Locao,
                x.TenLoCao,
                NguoiNhanList = userStats.ContainsKey(x.MaThungGang)
                            ? userStats[x.MaThungGang]
                            : new List<string>()
                }).ToList();
            return Ok(data);
        }

        // TODO
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
                int countItem = await _context.Tbl_BM_16_GangLong.CountAsync(s => s.MaThungGang == t.MaThungGang);
                

                // xu ly nguoi nhan thung
                var user_thung = new Tbl_BM_16_TaiKhoan_Thung
                {
                    ID_taiKhoan = payload.idNguoiNhan,
                    MaThungGang = t.MaThungGang
                };
                _context.Tbl_BM_16_TaiKhoan_Thung.Add(user_thung);

                //// get bang phu de lay ten va count so lan nhan thung
                //var thungUserList = await _context.Tbl_BM_16_TaiKhoan_Thung.Join(_context.Tbl_TaiKhoan,
                //          thung => thung.ID_taiKhoan,
                //          user => user.ID_TaiKhoan,
                //          (thung, user) => new
                //          {
                //              thung.MaThungGang,
                //              user.HoVaTen,
                //          })
                //    .ToListAsync();

                //// Gom theo MaThungGang để có danh sách tên + count
                //var userStats = thungUserList
                //    .GroupBy(x => x.MaThungGang)
                //    .ToDictionary(
                //        g => g.Key,
                //        g => g.GroupBy(u => u.HoVaTen)
                //              .Select(x => $"{x.Key} ({x.Count()})")
                //              .ToList()
                //    );
                //var NguoiNhanList = userStats.ContainsKey(t.MaThungGang)
                //            ? userStats[t.MaThungGang]
                //            : new List<string>();

                if (t.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                {
                    string MaThungThep = GenerateMaThungThep(t.MaThungGang, payload.idLoThoi, t.T_Ca, countItem);
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
                        KR = t.KR,
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
                        MaPhieu = t.MaPhieu,
                        NgayTao = DateTime.Today,
                        T_copy = true
                    };
                    _context.Tbl_BM_16_GangLong.Add(clone);

                    // Thêm vào danh sách trả về
                    resultList.Add(new ThungNhanResultDto
                    {
                        ID = clone.ID,
                        MaThungGang = clone.MaThungGang,
                        MaThungThep = clone.MaThungThep,
                        BKMIS_ThungSo = clone.BKMIS_ThungSo,
                        ID_TrangThai = clone.ID_TrangThai,
                        T_ID_TrangThai = clone.T_ID_TrangThai,
                        ID_LoCao = clone.ID_Locao
                    });
                }
                else
                {
                    // Thùng chưa nhận → cập nhật sang Đã nhận
                    string MaThungThep = GenerateMaThungThep(t.MaThungGang, payload.idLoThoi, t.T_Ca, countItem - 1);
                    t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                    t.MaThungThep = MaThungThep;

                    resultList.Add(new ThungNhanResultDto
                    {
                        ID = t.ID,
                        MaThungGang = t.MaThungGang,
                        MaThungThep = t.MaThungThep,
                        BKMIS_ThungSo = t.BKMIS_ThungSo,
                        ID_TrangThai = t.ID_TrangThai,
                        T_ID_TrangThai = t.T_ID_TrangThai,
                        ID_LoCao = t.ID_Locao
                    });
                }
            }

            await _context.SaveChangesAsync();
            await tran.CommitAsync();

            return Ok(new { Message = "Đã xử lý nhận thùng.", Count = payload.selectedIds.Count,  Data = resultList });
        }

        [HttpPost]
        public async Task<IActionResult> LuuKR([FromBody] List<LuuKRDto> selectedList)
        {
            if (selectedList == null || selectedList.Count == 0)
                return BadRequest("Danh sách ID trống.");

            var selectedIds = selectedList
                                .Select(x => x.id)
                                .ToList();

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                                       .Where(x => selectedIds.Contains(x.ID))
                                       .ToListAsync();

            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");
            foreach (var t in thungs)
            {
                t.KR = selectedList.Find(x => x.id == t.ID).isChecked;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // TODO
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

        private string GenerateMaThungThep(string maThungGang, int loThoiId, int? caValue, int count)
        {
            string ca = caValue == (int)CaLamViec.Ngay ? "N" : "Đ";
            string dayStr = DateTime.Now.Day.ToString("00");
            string countStr = count == 0 ? "00" : count < 10 ? "0" + count : count.ToString();
            return $"{maThungGang}.{dayStr}{ca}T{loThoiId}.{countStr}";
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
