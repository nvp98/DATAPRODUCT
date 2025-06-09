using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BM_GangThoi_ThepController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

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

            var currentYear = DateTime.Now.Year;
            var result = await _context.Tbl_MeThoi
                            .Where(x => x.NgayTao.Year == currentYear
                                     && x.ID_TrangThai == (int)TinhTrang.ChoXuLy
                                     )
                            .Select(x => new { x.ID, x.MaMeThoi })
                            .ToListAsync();
            ViewBag.MeThoiList = result;
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
            var thungUserList = await _context.Tbl_BM_16_TaiKhoan_Thung
                .Join(_context.Tbl_TaiKhoan,
                      thung => thung.ID_taiKhoan,
                      user => user.ID_TaiKhoan,
                      (thung, user) => new
                      {
                          thung.MaThungGang,
                          user.HoVaTen,
                          user.ID_PhongBan
                      })
                .Join(_context.Tbl_PhongBan,
                    temp => temp.ID_PhongBan,
                    phongban => phongban.ID_PhongBan,
                    (temp, phongban) => new
                    {
                        temp.MaThungGang,
                        HoVaTen = $"{temp.HoVaTen} - {phongban.TenNgan}"
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

      
        [HttpPost]
        public async Task<IActionResult> TimKiemThungDaNhan([FromBody] SearchThungDaNhanDto payload)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            if (TaiKhoan == null) return Unauthorized();

            var query = from gangLong in _context.Tbl_BM_16_GangLong
                        join phu in _context.Tbl_BM_16_TaiKhoan_Thung
                            on gangLong.MaThungGang equals phu.MaThungGang
                        where phu.ID_taiKhoan == TaiKhoan.ID_TaiKhoan
                              && gangLong.T_ID_TrangThai == (int)TinhTrang.DaNhan && gangLong.MaThungThep == phu.MaThungThep
                        select new { gangLong, phu };

            if (payload.NgayLamViec.HasValue)
            {
                query = query.Where(x => x.gangLong.NgayLuyenThep >= payload.NgayLamViec.Value && x.gangLong.NgayLuyenThep < payload.NgayLamViec.Value.AddDays(1));
            }

            if (payload.T_Ca.HasValue)
            {
                query = query.Where(x => x.gangLong.T_Ca == (int)payload.T_Ca.Value);
            }

            if (payload.ID_LoThoi.HasValue)
            {
                query = query.Where(x => x.gangLong.ID_LoThoi == (int)payload.ID_LoThoi.Value);
            }

            var res = await (from x in query
                             join meThoi in _context.Tbl_MeThoi on x.gangLong.ID_MeThoi equals meThoi.ID into meThoiJoin
                             from meThoi in meThoiJoin.DefaultIfEmpty() 
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = x.gangLong.ID,
                                 MaThungThep = x.gangLong.MaThungThep,
                                 BKMIS_ThungSo = x.gangLong.BKMIS_ThungSo,
                                 NgayLuyenThep = x.gangLong.NgayLuyenThep,
                                 ChuyenDen = x.gangLong.ChuyenDen,
                                 KR = x.gangLong.KR,
                                 T_ID_TrangThai = x.gangLong.T_ID_TrangThai,
                                 T_KLThungVaGang = x.gangLong.T_KLThungVaGang,
                                 T_KLThungChua = x.gangLong.T_KLThungChua,
                                 T_KLGangLong = x.gangLong.T_KLGangLong,
                                 ThungTrungGian = x.gangLong.ThungTrungGian,
                                 T_KLThungVaGang_Thoi = x.gangLong.T_KLThungVaGang_Thoi,
                                 T_KLThungChua_Thoi = x.gangLong.T_KLThungChua_Thoi,
                                 T_KLGangLongThoi = x.gangLong.T_KLGangLongThoi,
                                 T_GhiChu = x.gangLong.T_GhiChu,
                                 ID_Locao = x.gangLong.ID_Locao,
                                 ID_TrangThai = x.gangLong.ID_TrangThai,
                                 ID_MeThoi = x.gangLong.ID_MeThoi,
                                 MaMeThoi = meThoi != null ? meThoi.MaMeThoi : null
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
            var kip = await (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == payload.ngayNhan && x.TenCa == payload.idCa.ToString())
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenKip = a.TenKip
                               }).FirstOrDefaultAsync();

            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");

            using var tran = await _context.Database.BeginTransactionAsync();

            foreach (var t in thungs)
            {
                // tao ma thung Thep
                int countItem = await _context.Tbl_BM_16_TaiKhoan_Thung.CountAsync(s => s.MaThungGang == t.MaThungGang);
                string MaThungThep = GenerateMaThungThep(t.MaThungGang, payload.idLoThoi, t.T_Ca, countItem);

                // xu ly nguoi nhan thung
                var user_thung = new Tbl_BM_16_TaiKhoan_Thung
                {
                    ID_taiKhoan = payload.idNguoiNhan,
                    MaThungGang = t.MaThungGang,
                    MaThungThep = MaThungThep
                };
                _context.Tbl_BM_16_TaiKhoan_Thung.Add(user_thung);

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
                        KR = t.KR,
                        G_GhiChu = t.G_GhiChu,
                        G_Ca = t.G_Ca,
                        G_ID_Kip = t.G_ID_Kip,
                        G_ID_NguoiChuyen = t.G_ID_NguoiChuyen,
                        G_ID_NguoiLuu = t.G_ID_NguoiLuu,
                        G_ID_NguoiThuHoi = t.G_ID_NguoiThuHoi,
                        G_ID_TrangThai = t.G_ID_TrangThai,
                        ID_TrangThai = t.ID_TrangThai,
                        T_ID_TrangThai = (int)TinhTrang.DaNhan,
                        ID_Locao = t.ID_Locao,
                        ID_Phieu = t.ID_Phieu,
                        MaPhieu = t.MaPhieu,
                        NgayTao = DateTime.Today,
                        T_copy = true,
                        MaThungThep = MaThungThep,
                        ID_LoThoi = payload.idLoThoi,
                        T_Ca = payload.idCa,
                        NgayLuyenThep = payload.ngayNhan,
                        T_ID_NguoiNhan = payload.idNguoiNhan,
                        T_ID_Kip = kip.ID_Kip
                    };
                    _context.Tbl_BM_16_GangLong.Add(clone);
                }
                else
                {
                    // Thùng chưa nhận ->  cập nhật sang Đã nhận
                    t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                    t.MaThungThep = MaThungThep;
                    t.ID_LoThoi = payload.idLoThoi;
                    t.T_Ca = payload.idCa;
                    t.NgayLuyenThep = payload.ngayNhan;
                    t.T_ID_Kip = kip.ID_Kip;
                }
            }

            await _context.SaveChangesAsync();
            await tran.CommitAsync();

            return Ok(new { Message = "Đã xử lý nhận thùng.", Count = payload.selectedIds.Count});
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


        [HttpPost]
        public async Task<IActionResult> HuyNhan([FromBody] HuyNhanThungDto payload)
        {
            if (payload.selectedMaThungs == null || payload.selectedMaThungs.Count == 0 || payload.idNguoiHuyNhan == null)
                return BadRequest("Danh sách ID trống.");
            try
            {
                using var tran = await _context.Database.BeginTransactionAsync();
                //  Lấy các bản ghi bảng phụ của user hiện tại theo MaThung
                var phuToDelete = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .Where(x => payload.selectedMaThungs.Contains(x.MaThungGang) && x.ID_taiKhoan == payload.idNguoiHuyNhan)
                    .ToListAsync();

                // Lấy danh sách MaThep để xoá trong bảng thùng copy
                var dsMaThepCanXoa = phuToDelete.Select(x => x.MaThungThep).Distinct().ToList();

                //  Kiểm tra MaThung còn người dùng khác sử dụng không
                var maThungConNguoiKhacDung = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .Where(x => payload.selectedMaThungs.Contains(x.MaThungGang) && x.ID_taiKhoan != payload.idNguoiHuyNhan)
                    .Select(x => x.MaThungGang)
                    .Distinct()
                    .ToListAsync();

                var maThungKhongConAiDung = payload.selectedMaThungs.Except(maThungConNguoiKhacDung).ToList();

                //  Xoá bản ghi bảng phụ của user hiện tại
                _context.Tbl_BM_16_TaiKhoan_Thung.RemoveRange(phuToDelete);


                // Xoá bản sao (copy) trong bảng chính theo MaThep và user hiện tại
                var thungCopyToDelete = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.T_copy == true && dsMaThepCanXoa.Contains(x.MaThungThep) && x.T_ID_NguoiNhan == payload.idNguoiHuyNhan)
                    .ToListAsync();

                _context.Tbl_BM_16_GangLong.RemoveRange(thungCopyToDelete);

                //  Cập nhật trạng thái thùng gốc nếu không còn ai dùng
                if (maThungKhongConAiDung.Any())
                {
                    var thungGocCanCapNhat = await _context.Tbl_BM_16_GangLong
                        .Where(x => x.T_copy == false && maThungKhongConAiDung.Contains(x.MaThungGang))
                        .ToListAsync();

                    foreach (var thung in thungGocCanCapNhat)
                    {
                        thung.MaThungThep = null;
                        thung.T_ID_Kip = null;
                        thung.NgayLuyenThep = null;
                        thung.T_KLThungVaGang = null;
                        thung.T_KLThungChua = null;
                        thung.T_KLGangLong = null;
                        thung.ThungTrungGian = null;
                        thung.T_KLThungVaGang_Thoi = null;
                        thung.T_KLThungChua_Thoi = null;
                        thung.T_KLGangLongThoi = null;
                        thung.T_GhiChu = null;
                        thung.T_ID_NguoiLuu = null;
                        thung.T_ID_Kip = null;
                        thung.ID_LoThoi = null;
                        thung.ID_MeThoi = null;
                        thung.T_Ca = null;
                        thung.T_ID_NguoiHuyNhan = payload.idNguoiHuyNhan;
                        thung.T_ID_TrangThai = (int)TinhTrang.ChoXuLy;
                    }
                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi xử lý trên server: " + ex.Message);
            }
            
        }

        
        [HttpPost]
        public async Task<IActionResult> Luu([FromBody] List<LuuThepDto> payload)
        {
            if (payload == null || !payload.Any())
                return BadRequest("Không có dữ liệu.");

            var idsGangLong = payload.Select(x => x.ID).ToList();
            var idsMeThoiMoi = payload.Where(x => x.ID_MeThoi.HasValue).Select(x => x.ID_MeThoi.Value).Distinct().ToList();

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

            // Lấy danh sách ganglong và methoi liên quan
            var gangLongDict = await _context.Tbl_BM_16_GangLong
                .Where(x => idsGangLong.Contains(x.ID))
                .ToDictionaryAsync(x => x.ID);

            // Lấy cả mẻ thổi cũ và mới để xử lý trạng thái
            var idMeThoiCu = gangLongDict.Values
                .Where(x => x.ID_MeThoi.HasValue)
                .Select(x => x.ID_MeThoi.Value);

            var allMeThoiIds = idsMeThoiMoi.Union(idMeThoiCu).Distinct().ToList();

            var meThoiDict = await _context.Tbl_MeThoi
                .Where(x => allMeThoiIds.Contains(x.ID))
                .ToDictionaryAsync(x => x.ID);

            foreach (var item in payload)
            {
                if (gangLongDict.TryGetValue(item.ID, out var entity))
                {
                    var oldMeThoiId = entity.ID_MeThoi;

                    // Cập nhật dữ liệu
                    entity.T_KLThungVaGang = RoundDecimal(item.T_KLThungVaGang);
                    entity.T_KLThungChua = RoundDecimal(item.T_KLThungChua);
                    entity.T_KLGangLong = RoundDecimal(item.T_KLGangLong);
                    entity.ThungTrungGian = item.ThungTrungGian;
                    entity.T_KLThungVaGang_Thoi = RoundDecimal(item.T_KLThungVaGang_Thoi);
                    entity.T_KLThungChua_Thoi = RoundDecimal(item.T_KLThungChua_Thoi);
                    entity.T_KLGangLongThoi = RoundDecimal(item.T_KLGangLongThoi);
                    entity.ID_MeThoi = item.ID_MeThoi;
                    entity.T_GhiChu = item.GhiChu;
                    entity.T_ID_NguoiLuu = TaiKhoan.ID_TaiKhoan;

                    // Nếu đổi mẻ thổi, thì cập nhật lại trạng thái mẻ thổi cũ
                    if (oldMeThoiId.HasValue && oldMeThoiId != item.ID_MeThoi)
                    {
                        if (meThoiDict.TryGetValue(oldMeThoiId.Value, out var methoiCu))
                        {
                            methoiCu.ID_TrangThai = (int)TinhTrang.ChoXuLy;
                        }
                    }

                    // Gán trạng thái cho mẻ thổi mới
                    if (item.ID_MeThoi.HasValue && meThoiDict.TryGetValue(item.ID_MeThoi.Value, out var methoiMoi))
                    {
                        methoiMoi.ID_TrangThai = (int)TinhTrang.DaChot;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        private string GenerateMaThungThep(string maThungGang, int loThoiId, int? caValue, int count)
        {
            string ca = caValue == (int)CaLamViec.Ngay ? "N" : "Đ";
            string dayStr = DateTime.Now.Day.ToString("00");
            string countStr = count == 0 ? "00" : count < 10 ? "0" + count : count.ToString();
            return $"{maThungGang}.{dayStr}{ca}T{loThoiId}.{countStr}";
        }
        private decimal? RoundDecimal(decimal? value)
        {
            return value.HasValue ? Math.Round(value.Value, 2) : (decimal?)null;
        }
    }
}
