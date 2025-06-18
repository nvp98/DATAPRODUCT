using ClosedXML.Excel;
using Data_Product.Common;
using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using iText.Html2pdf;
using iText.Kernel.Events;
using iText.Layout.Font;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Security.Claims;
using static Data_Product.Controllers.BM_11Controller;

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

            if (payload.ChuyenDen != null && payload.ChuyenDen.Any())
            {
                query = query.Where(x => payload.ChuyenDen.Contains(x.ChuyenDen));
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
                                 G_KLGangLong = a.G_KLGangLong,
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
                x.G_KLGangLong,
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
            var data = await GetDanhSachThungTrungGianDaNhan(payload);
            return Ok(data);
        }

        private async Task<List<Tbl_BM_16_GangLong>> GetThungDaNhan(SearchThungDaNhanDto payload)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            if (TaiKhoan == null) return null;

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
                             join trangThai in _context.Tbl_BM_16_TrangThai on x.gangLong.ID_TrangThai equals trangThai.ID
                             join kipT in _context.Tbl_Kip on x.gangLong.T_ID_Kip equals kipT.ID_Kip
                             select new Tbl_BM_16_GangLong
                             {
                                 ID = x.gangLong.ID,
                                 BKMIS_SoMe = x.gangLong.BKMIS_SoMe,
                                 BKMIS_Gio = x.gangLong.BKMIS_Gio,
                                 BKMIS_PhanLoai = x.gangLong.BKMIS_PhanLoai,
                                 MaThungThep = x.gangLong.MaThungThep,
                                 BKMIS_ThungSo = x.gangLong.BKMIS_ThungSo,
                                 NgayLuyenThep = x.gangLong.NgayLuyenThep,
                                 ChuyenDen = x.gangLong.ChuyenDen,
                                 KL_XeGoong = x.gangLong.KL_XeGoong,
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
                                 TrangThai = trangThai.TenTrangThai,
                                 ID_MeThoi = x.gangLong.ID_MeThoi,
                                 MaMeThoi = meThoi != null ? meThoi.MaMeThoi : null,
                                 T_KL_phe = x.gangLong.T_KL_phe,
                                 Gio_NM = x.gangLong.Gio_NM,
                                 T_Ca = x.gangLong.T_Ca,
                                 T_TenKip = kipT != null ? kipT.TenKip : null,
                                 G_ID_NguoiChuyen = x.gangLong.G_ID_NguoiChuyen
                             }).ToListAsync();

            return res;
        }

        public async Task<List<ThungTrungGianGroupViewModel>> GetDanhSachThungTrungGianDaNhan(SearchThungDaNhanDto payload)
        {
            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var taiKhoan = await _context.Tbl_TaiKhoan
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);
            if (taiKhoan == null) return new List<ThungTrungGianGroupViewModel>();

            // Truy vấn thùng trung gian theo ngày, ca, lò thổi, người nhận
            var query = _context.Tbl_BM_16_ThungTrungGian
                                .Where(x => x.ID_NguoiNhan == taiKhoan.ID_TaiKhoan);

            if (payload.NgayLamViec.HasValue)
            {
                var ngay = payload.NgayLamViec.Value.Date;
                query = query.Where(x => x.NgayNhan >= ngay && x.NgayNhan < ngay.AddDays(1));
            }

            if (payload.T_Ca.HasValue)
                query = query.Where(x => x.CaNhan == payload.T_Ca);

            if (payload.ID_LoThoi.HasValue)
                query = query.Where(x => x.ID_LoThoi == payload.ID_LoThoi);

            var thungList = await (from ttg in query
                                   join meThoi in _context.Tbl_MeThoi
                                       on ttg.ID_MeThoi equals meThoi.ID into meThoiJoin
                                   from meThoi in meThoiJoin.DefaultIfEmpty()
                                   orderby ttg.NgayTao, ttg.MaThungTG
                                   select new ThungTrungGianGroupViewModel
                                   {
                                       ID_TTG = ttg.ID,
                                       MaThungTG = ttg.MaThungTG,
                                       MaThungTG_Copy = ttg.MaThungTG_Copy,
                                       SoThungTG = ttg.SoThungTG,
                                       GhiChu = ttg.GhiChu,
                                       IsCopy = ttg.IsCopy,
                                       KLThung_Thoi = ttg.KLThung_Thoi,
                                       KLThungVaGang_Thoi = ttg.KLThungVaGang_Thoi,
                                       KL_phe = ttg.KL_phe,
                                       KLGang_Thoi = ttg.KLGang_Thoi,
                                       Tong_KLGangNhan = ttg.Tong_KLGangNhan,
                                       ID_MeThoi = ttg.ID_MeThoi,
                                       MaMeThoi = meThoi != null ? meThoi.MaMeThoi : null
                                   }).ToListAsync();

            // Lấy danh sách gang lỏng theo ID_TTG
            var gangList = await _context.Tbl_BM_16_GangLong
                .Where(x => x.ID_TTG.HasValue)
                .Select(x => new
                {
                    x.ID_TTG,
                    Gang = new GangLongItemViewModel
                    {
                        ID = x.ID,
                        MaThungGang = x.MaThungGang,
                        MaThungThep = x.MaThungThep,
                        BKMIS_ThungSo = x.BKMIS_ThungSo,
                        ID_LoCao = x.ID_Locao,
                        ID_TrangThai = x.ID_TrangThai,
                        T_KLGangLong = x.T_KLGangLong,
                        T_KLThungChua = x.T_KLThungChua,
                        T_KLThungVaGang = x.T_KLThungVaGang
                    }
                }).ToListAsync();

            var groupGang = gangList
                .GroupBy(x => x.ID_TTG)
                .ToDictionary(g => g.Key.Value, g => g.Select(x => x.Gang).ToList());

            // Gán danh sách gang vào thùng gốc
            foreach (var ttg in thungList)
            {
                if (!ttg.IsCopy && groupGang.ContainsKey(ttg.ID_TTG))
                {
                    ttg.DanhSachThungGang = groupGang[ttg.ID_TTG];
                }
            }

            // Gán danh sách gang cho thùng copy (dựa vào MaThungTG giống thùng gốc)
            foreach (var ttg in thungList.Where(x => x.IsCopy))
            {
                var thungGoc = thungList.FirstOrDefault(x => !x.IsCopy && x.MaThungTG == ttg.MaThungTG);
                if (thungGoc != null && thungGoc.DanhSachThungGang != null)
                {
                    ttg.DanhSachThungGang = thungGoc.DanhSachThungGang
                        .Select(x => new GangLongItemViewModel
                        {
                            ID = x.ID,
                            MaThungGang = x.MaThungGang,
                            MaThungThep = x.MaThungThep,
                            BKMIS_ThungSo = x.BKMIS_ThungSo,
                            ID_LoCao = x.ID_LoCao,
                            ID_TrangThai = x.ID_TrangThai,
                            T_KLGangLong = x.T_KLGangLong,
                            T_KLThungChua = x.T_KLThungChua,
                            T_KLThungVaGang = x.T_KLThungVaGang
                        }).ToList();
                }
            }

            return thungList;
        }

        private async Task<int> TaoThungTrungGian(DateTime ngayNhan, int idCa, int idLoThoi, string SoThungTG, int ID_NguoiNhan)
        {
            string maTTG = "T" + TaoMa.GenerateSafeCode(8);
            var thungTrungGian = new Tbl_BM_16_ThungTrungGian
            {
                MaThungTG = maTTG,
                NgayNhan = ngayNhan,
                CaNhan = idCa,
                SoThungTG = SoThungTG,
                ID_LoThoi = idLoThoi,
                IsCopy = false,
                ID_NguoiNhan = ID_NguoiNhan,
                NgayTao = DateTime.Now
            };
            _context.Tbl_BM_16_ThungTrungGian.Add(thungTrungGian);
            await _context.SaveChangesAsync();
            return thungTrungGian.ID;
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
            // Nếu là HRC2 thì tạo 1 thùng trung gian chung
            int? idThungTG_Common = null;
            if (!string.IsNullOrEmpty(payload.thungTrungGian))
            {
                idThungTG_Common = await TaoThungTrungGian(payload.ngayNhan, payload.idCa, payload.idLoThoi, payload.thungTrungGian, payload.idNguoiNhan);
            }

            foreach (var t in thungs)
            {
                int countItem = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .CountAsync(s => s.MaThungGang == t.MaThungGang);

                string maThungThep = GenerateMaThungThep(t.MaThungGang, payload.idLoThoi, t.T_Ca, countItem);

                int idThungTG = idThungTG_Common ?? await TaoThungTrungGian(payload.ngayNhan, payload.idCa, payload.idLoThoi, t.BKMIS_ThungSo, payload.idNguoiNhan);

                // Ghi nhận người nhận
                _context.Tbl_BM_16_TaiKhoan_Thung.Add(new Tbl_BM_16_TaiKhoan_Thung
                {
                    ID_taiKhoan = payload.idNguoiNhan,
                    MaThungGang = t.MaThungGang,
                    MaThungThep = maThungThep,
                    MaPhieu = t.MaPhieu
                });

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
                        MaThungThep = maThungThep,
                        ID_LoThoi = payload.idLoThoi,
                        T_Ca = payload.idCa,
                        NgayLuyenThep = payload.ngayNhan,
                        T_ID_NguoiNhan = payload.idNguoiNhan,
                        T_ID_Kip = kip.ID_Kip,
                        ID_TTG = idThungTG
                    };
                    _context.Tbl_BM_16_GangLong.Add(clone);
                }
                else
                {
                    // Thùng chưa nhận ->  cập nhật sang Đã nhận
                    t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                    t.MaThungThep = maThungThep;
                    t.ID_LoThoi = payload.idLoThoi;
                    t.T_Ca = payload.idCa;
                    t.NgayLuyenThep = payload.ngayNhan;
                    t.T_ID_Kip = kip.ID_Kip;
                    t.ID_TTG = idThungTG;
                }
            }

            await _context.SaveChangesAsync();
            await tran.CommitAsync();

            return Ok(new { Message = "Đã xử lý nhận thùng.", Soluong = payload.selectedIds.Count});
        }

        
        [HttpPost]
        public async Task<IActionResult> XoaThungCopy([FromBody] string selectedMaThung)
        {
            if(selectedMaThung == null || string.IsNullOrEmpty(selectedMaThung))
                return BadRequest("Mã thùng copy trống.");

            var thung = await _context.Tbl_BM_16_ThungTrungGian.FirstOrDefaultAsync(x => x.MaThungTG_Copy == selectedMaThung);
            if (thung != null)
            {
                _context.Tbl_BM_16_ThungTrungGian.Remove(thung);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, hasThung = true });
            }
            return Ok(new {success = true, hasThung = false });
        }

        [HttpPost]
        public async Task<IActionResult> LuuKR([FromBody] List<LuuKRDto> selectedList)
        {
            if (selectedList == null || selectedList.Count == 0)
                return BadRequest("Danh sách ID trống.");

            var selectedMaThungs = selectedList
                                .Select(x => x.maThungGang)
                                .ToList();

            // Lấy tất cả các thùng cần xử lý
            var thungs = await _context.Tbl_BM_16_GangLong
                                       .Where(x => selectedMaThungs.Contains(x.MaThungGang))
                                       .ToListAsync();

            if (thungs.Count == 0) return NotFound("Không tìm thấy thùng nào.");
            foreach (var t in thungs)
            {
                t.KR = selectedList.Find(x => x.maThungGang == t.MaThungGang).isChecked;
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
                        thung.ID_TTG = null;
                    }
                }

                // Lấy tất cả ID_TTG của các thùng gang mà người dùng đang hủy
                var ID_TTGList = await _context.Tbl_BM_16_GangLong
                    .Where(x => payload.selectedMaThungs.Contains(x.MaThungGang) && x.ID_TTG != null)
                    .Select(x => x.ID_TTG.Value)
                    .Distinct()
                    .ToListAsync();

                // Duyệt qua từng ID_TTG để kiểm tra và xử lý thùng trung gian
                foreach (var idTTG in ID_TTGList)
                {
                    // Lấy tất cả thùng gang có cùng ID_TTG này và kiểm tra xem có thùng gang nào khác ngoài những thùng đang hủy không
                    var remainingGangLongRecords = await _context.Tbl_BM_16_GangLong
                        .Where(x => x.ID_TTG == idTTG && !payload.selectedMaThungs.Contains(x.MaThungGang))
                        .ToListAsync();

                    // Nếu không còn thùng gang nào khác sử dụng thùng trung gian này, thì xóa thùng trung gian
                    if (!remainingGangLongRecords.Any())
                    {
                        var thungTrungGianToDelete = await _context.Tbl_BM_16_ThungTrungGian
                            .Where(x => x.ID == idTTG)
                            .FirstOrDefaultAsync();

                        if (thungTrungGianToDelete != null)
                        {
                            _context.Tbl_BM_16_ThungTrungGian.Remove(thungTrungGianToDelete);
                        }
                    }
                    else
                    {
                        //  Nếu còn thùng gang khác sử dụng thùng trung gian, cập nhật tổng KL Gang nhận
                        var thungTrungGianToUpdate = await _context.Tbl_BM_16_ThungTrungGian
                            .Where(x => x.ID == idTTG)
                            .FirstOrDefaultAsync();

                        if (thungTrungGianToUpdate != null)
                        {
                            // Cập nhật lại Tong_KLGangNhan bằng tổng T_KLGangLong
                            thungTrungGianToUpdate.Tong_KLGangNhan = remainingGangLongRecords
                                .Sum(x => x.T_KLGangLong ?? 0); 
                        }
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
        public async Task<IActionResult> Luu([FromBody] List<ThungTrungGianDto> dsThungTG)
        {
            if (dsThungTG == null || !dsThungTG.Any())
                return BadRequest("Không có dữ liệu.");
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            if (TaiKhoan == null) return BadRequest("Không tìm thấy tài khoản.");

            foreach (var tgDto in dsThungTG)
            {
                Tbl_BM_16_ThungTrungGian ttg = null;

                // thung copy
                if (tgDto.IsCopy && !string.IsNullOrEmpty(tgDto.MaThungTG_Copy))
                {
                    // Là thùng copy => tìm theo MaThungTG_Copy
                    ttg = await _context.Tbl_BM_16_ThungTrungGian
                                .FirstOrDefaultAsync(x => x.IsCopy && x.MaThungTG_Copy == tgDto.MaThungTG_Copy);

                    if (ttg == null)
                    {
                        // Tạo mới bản sao
                        ttg = new Tbl_BM_16_ThungTrungGian
                        {
                            MaThungTG = tgDto.MaThungTG,
                            SoThungTG = tgDto.SoThungTG,
                            MaThungTG_Copy = tgDto.MaThungTG_Copy,
                            NgayNhan = tgDto.NgayNhan,
                            CaNhan = tgDto.CaNhan,
                            ID_LoThoi = tgDto.ID_LoThoi,
                            ID_NguoiNhan = TaiKhoan.ID_TaiKhoan,
                            NgayTao = DateTime.Now,
                            IsCopy = true
                        };
                        _context.Tbl_BM_16_ThungTrungGian.Add(ttg);
                    }
                }
                else // thung goc
                {
                    ttg = await _context.Tbl_BM_16_ThungTrungGian
                            .FirstOrDefaultAsync(x => !x.IsCopy &&
                                                      x.MaThungTG == tgDto.MaThungTG &&
                                                      x.ID_NguoiNhan == TaiKhoan.ID_TaiKhoan);

                    if (ttg == null)
                        continue; 
                }

                // Cập nhật dữ liệu chung
                if (ttg != null)
                {
                    ttg.KLThungVaGang_Thoi = tgDto.KLThungVaGang_Thoi;
                    ttg.KLThung_Thoi = tgDto.KLThung_Thoi;
                    ttg.KL_phe = tgDto.KLPhe;
                    ttg.KLGang_Thoi = tgDto.KLGang_Thoi;
                    ttg.Tong_KLGangNhan = tgDto.Tong_KLGangNhan;
                    ttg.GhiChu = tgDto.GhiChu;
                    ttg.ID_MeThoi = tgDto.ID_MeThoi;
                }

                // Nếu là thùng gốc =>> cập nhật danh sách thùng gang
                if (!tgDto.IsCopy && tgDto.DanhSachThungGang?.Any() == true)
                {
                    foreach (var thungGang in tgDto.DanhSachThungGang)
                    {
                        var entity = await _context.Tbl_BM_16_GangLong
                            .FirstOrDefaultAsync(x => x.MaThungThep == thungGang.MaThungThep);

                        if (entity != null)
                        {
                            entity.T_KLThungVaGang = thungGang.T_KLThungVaGang;
                            entity.T_KLThungChua = thungGang.T_KLThungChua;
                            entity.T_KLGangLong = thungGang.T_KLGangLong;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Lưu thành công." });
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

        [HttpPost]
        public async Task<IActionResult> ExportToExcel([FromBody] SearchThungDaNhanDto payload)
        {
            try
            {
                var thungList = await GetThungDaNhan(payload);
                if (thungList == null || !thungList.Any())
                    return BadRequest("Danh sách trống.");

                // Đường dẫn đến template
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "QTGN_Gang_Long_Thep.xlsx");
                using (var ms = new MemoryStream())
                {
                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet("Sheet1");

                        // Gộp ô dòng 5 và hiển thị thông tin từ payload
                        string ca = payload.T_Ca == 1 ? "Ngày" : "Đêm";
                        var loThoiEntity = await _context.Tbl_LoThoi.FirstOrDefaultAsync(x => x.ID == payload.ID_LoThoi);
                        var loThoi = loThoiEntity?.TenLoThoi ?? "";

                        var headerCell = worksheet.Range("A5:O5").Merge();
                        headerCell.Value = $"Ngày: {payload.NgayLamViec:dd/MM/yyyy}     Ca: {ca}     Lò thổi: {loThoi}";
                        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerCell.Style.Font.Bold = true;
                        headerCell.Style.Font.FontSize = 11;

                        // Xóa dữ liệu cũ (nếu có)
                        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 8;
                        var rangeClear = worksheet.Range($"A8:O{lastRow}");
                        rangeClear.Clear(XLClearOptions.Contents | XLClearOptions.NormalFormats);
                       
                        int row = 8, stt = 1;

                        foreach (var item in thungList)
                        {
                            int icol = 1;

                            worksheet.Cell(row, icol++).Value = stt++;
                            worksheet.Cell(row, icol++).Value = item.MaThungThep;
                            worksheet.Cell(row, icol++).Value = item.ID_Locao;
                            worksheet.Cell(row, icol++).Value = item.BKMIS_ThungSo;
                            worksheet.Cell(row, icol++).Value = item.T_KLThungVaGang;
                            worksheet.Cell(row, icol++).Value = item.T_KLThungChua;

                            // Cột 7: T_KLGangLong
                            var cell7 = worksheet.Cell(row, icol++);
                            if (item.T_KLGangLong.HasValue)
                            {
                                cell7.Value = item.T_KLGangLong.Value;
                                cell7.Style.NumberFormat.Format = "0.00";
                                cell7.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell7.Style.Font.Bold = true;
                            }

                            worksheet.Cell(row, icol++).Value = item.ThungTrungGian;
                            worksheet.Cell(row, icol++).Value = item.T_KLThungVaGang_Thoi;
                            worksheet.Cell(row, icol++).Value = item.T_KLThungChua_Thoi;
                            worksheet.Cell(row, icol++).Value = item.T_KL_phe;

                            // Cột 12: T_KLGangLongThoi
                            var cell12 = worksheet.Cell(row, icol++);
                            if (item.T_KLGangLongThoi.HasValue)
                            {
                                cell12.Value = item.T_KLGangLongThoi.Value;
                                cell12.Style.NumberFormat.Format = "0.00";
                                cell12.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cell12.Style.Font.Bold = true;
                            }

                            worksheet.Cell(row, icol++).Value = item.MaMeThoi;
                            worksheet.Cell(row, icol++).Value = item.T_GhiChu;

                            var statusCell = worksheet.Cell(row, icol++);
                            statusCell.Value = item.TrangThai;
                            statusCell.Style.Font.SetBold();

                            // Màu theo trạng thái
                            if (item.ID_TrangThai == 5)
                            {
                                statusCell.Style.Fill.BackgroundColor = XLColor.FromArgb(112, 173, 71);
                                statusCell.Style.Font.FontColor = XLColor.White;
                            }
                            else if (item.ID_TrangThai == 2)
                            {
                                statusCell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 153, 0);
                                statusCell.Style.Font.FontColor = XLColor.White;
                            }
                            else
                            {
                                statusCell.Style.Fill.BackgroundColor = XLColor.FromArgb(215, 215, 215);
                                statusCell.Style.Font.FontColor = XLColor.Black;
                            }
                            worksheet.Row(row).Height = 25;
                            row++;
                        }

                        // Dòng tổng
                        int sumRow = row;
                        var totalLabel = worksheet.Range($"A{sumRow}:F{sumRow}");
                        totalLabel.Merge();
                        totalLabel.Value = "Tổng:";
                        totalLabel.Style.Font.SetBold();
                        totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột 7
                        worksheet.Cell(sumRow, 7).FormulaA1 = $"=SUM(G8:G{row - 1})";
                        worksheet.Cell(sumRow, 7).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 7).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Tổng cột 12
                        worksheet.Cell(sumRow, 12).FormulaA1 = $"=SUM(L8:L{row - 1})";
                        worksheet.Cell(sumRow, 12).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 12).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 12).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Format toàn bảng
                        var usedRange = worksheet.Range($"A7:O{sumRow}");
                        usedRange.Style.Font.SetFontName("Arial").Font.SetFontSize(11);
                        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        usedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        usedRange.Style.Alignment.WrapText = true;


                        // Chiều cao dòng
                        for (int i = 8; i <= sumRow; i++)
                        {
                            worksheet.Row(i).Height = 25;
                        }

                        // Ghi workbook vào MemoryStream (không lưu ra ổ đĩa)
                        workbook.SaveAs(ms);
                    }
                    // Trả file ra MemoryStream
                    ms.Position = 0;

                    string outputName = $"QTGN_Gang_Long_Thep_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(ms.ToArray(),
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                outputName);
                }
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index", "BM_GangThoi_Thep");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPDF([FromBody] SearchThungDaNhanDto payload)
        {
            try { 
                

                var listThungs = await GetThungDaNhan(payload);
                if (listThungs == null || !listThungs.Any())
                    return BadRequest("Danh sách trống.");

                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
                if (TaiKhoan == null) return BadRequest("Tài khoản không tồn tại.");

                var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault().TenNgan.Split('.').Last();

                if (PhongBan == null) return BadRequest("Không có phòng ban.");

                //var data = listThungs.Where(x => x.ChuyenDen == PhongBan && x.ID_TrangThai == (int)TinhTrang.DaChot).ToList();
                var dataListThung = listThungs.Where(x => x.ChuyenDen == "HRC2" && x.ID_TrangThai == (int)TinhTrang.DaChot).ToList();


                var data = (from thung in dataListThung
                            join user in _context.Tbl_TaiKhoan
                                on thung.G_ID_NguoiChuyen equals user.ID_TaiKhoan into g_user
                            from user in g_user.DefaultIfEmpty()
                            join phongBan in _context.Tbl_PhongBan
                                on user.ID_PhongBan equals phongBan.ID_PhongBan into g_phongBan
                            from phongBan in g_phongBan.DefaultIfEmpty()
                            join vitri in _context.Tbl_ViTri
                                on user.ID_ChucVu equals vitri.ID_ViTri into g_vitri
                            from vitri in g_vitri.DefaultIfEmpty()
                            select new Tbl_BM_16_GangLong
                              {
                                  HoVaTen = user != null ? user.HoVaTen : "",
                                TenTaiKhoan = user != null ? user.TenTaiKhoan : "",
                                TenPhongBan = phongBan != null ? phongBan.TenNgan : "",
                                  ChuKy = user != null ? user.ChuKy : "",
                                  TenViTri = vitri != null ? vitri.TenViTri : "",
                                  ID = thung.ID,
                                  BKMIS_SoMe = thung.BKMIS_SoMe,
                                  BKMIS_Gio = thung.BKMIS_Gio,
                                  BKMIS_PhanLoai = thung.BKMIS_PhanLoai,
                                  MaThungThep = thung.MaThungThep,
                                  BKMIS_ThungSo = thung.BKMIS_ThungSo,
                                  NgayLuyenThep = thung.NgayLuyenThep,
                                  ChuyenDen = thung.ChuyenDen,
                                  KL_XeGoong = thung.KL_XeGoong,
                                  T_ID_TrangThai = thung.T_ID_TrangThai,
                                  T_KLThungVaGang = thung.T_KLThungVaGang,
                                  T_KLThungChua = thung.T_KLThungChua,
                                  T_KLGangLong = thung.T_KLGangLong,
                                  ThungTrungGian = thung.ThungTrungGian,
                                  T_KLThungVaGang_Thoi = thung.T_KLThungVaGang_Thoi,
                                  T_KLThungChua_Thoi = thung.T_KLThungChua_Thoi,
                                  T_KLGangLongThoi = thung.T_KLGangLongThoi,
                                  T_GhiChu = thung.T_GhiChu,
                                  ID_Locao = thung.ID_Locao,
                                  ID_TrangThai = thung.ID_TrangThai,
                                  TrangThai = thung.TrangThai != null ? thung.TrangThai : null,
                                  MaMeThoi = thung.MaMeThoi != null ? thung.MaMeThoi : null,
                                  T_KL_phe = thung.T_KL_phe,
                                  Gio_NM = thung.Gio_NM,
                                  T_Ca = thung.T_Ca,
                                  T_TenKip = thung.T_TenKip != null ? thung.T_TenKip : null 
                              }).ToList();

                if (data == null) return BadRequest("Danh sách trống với phòng ban.");

                // 1. Render Razor View thành chuỗi HTML
                string html = await RenderViewToStringAsync("BBGN_thep_pdf", data);

                // 2. Chuyển đổi HTML sang PDF
                byte[] pdfBytes = ConvertHtmlToPdf(html);

                string filename = $"QTGN Gang long - Thep {DateTime.Now.ToString("yyyyMMddHHmm")}.pdf";

                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                TempData["msgSuccess"] = "<script>alert('Có lỗi khi truy xuất dữ liệu.');</script>";

                return RedirectToAction("Index", "BM_GangThoi_Thep");
            }
        }

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
                if (!viewResult.Success)
                {
                    throw new FileNotFoundException($"View '{viewName}' không tìm thấy.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new Microsoft.AspNetCore.Mvc.ViewFeatures.HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return await Task.FromResult(writer.ToString());
            }
        }

        private byte[] ConvertHtmlToPdf(string htmlContent)
        {
            using (var memoryStream = new MemoryStream())
            {
                // 1. Cấu hình FontProvider để hỗ trợ Times New Roman
                var fontProvider = new FontProvider();
                fontProvider.AddFont("C:/Windows/Fonts/times.ttf");     // Regular
                fontProvider.AddFont("C:/Windows/Fonts/timesbd.ttf");   // Bold
                fontProvider.AddFont("C:/Windows/Fonts/timesi.ttf");    // Italic
                fontProvider.AddFont("C:/Windows/Fonts/timesbi.ttf");   // Bold Italic

                // 2. Tạo writer và document
                var writer = new iText.Kernel.Pdf.PdfWriter(memoryStream);
                var pdfDocument = new iText.Kernel.Pdf.PdfDocument(writer);
                pdfDocument.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4.Rotate()); // Trang ngang

                // 3. Cấu hình Converter
                var converterProperties = new ConverterProperties();
                converterProperties.SetFontProvider(fontProvider);

                // 4. Cấu hình baseUri nếu HTML chứa ảnh
                string baseUri = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                converterProperties.SetBaseUri(baseUri);

                // 5. Chuyển đổi HTML sang PDF
                HtmlConverter.ConvertToPdf(htmlContent, pdfDocument, converterProperties);

                // 6. Trả về PDF dưới dạng byte[]
                return memoryStream.ToArray();
            }
        }
    }
}
