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
using iText.Layout.Element;
using iText.Layout.Font;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
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
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

            var loThoiList = await _context.Tbl_LoThoi.Where(x => x.BoPhan == TaiKhoan.ID_PhongBan).ToListAsync();
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

            if (payload.TuNgay.HasValue && payload.DenNgay.HasValue)
            {
                var tuNgay = payload.TuNgay.Value.Date;
                var denNgay = payload.DenNgay.Value.Date.AddDays(1); 

                query = query.Where(x => x.NgayTao >= tuNgay && x.NgayTao < denNgay);
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
                if(payload.ID_TrangThai.Value == (int)TinhTrang.DaChot)
                {
                    query = query.Where(x => x.ID_TrangThai == payload.ID_TrangThai.Value);
                } else
                {
                    query = query.Where(x => x.T_ID_TrangThai == payload.ID_TrangThai.Value);
                }
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
                                 TenLoCao = loCao.TenLoCao,
                                 NgayTao = a.NgayTao
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
                x.NgayTao,
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

                                        // Tính NgayTaoGoc là ngày tạo của thùng gốc theo MaThungTG
                                    join thungGoc in query.Where(x => !x.IsCopy)
                                        on ttg.MaThungTG equals thungGoc.MaThungTG into thungGocJoin
                                    from thungGoc in thungGocJoin.DefaultIfEmpty()

                                    orderby thungGoc.NgayTao, ttg.MaThungTG, ttg.IsCopy

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
                                        MaMeThoi = meThoi != null ? meThoi.MaMeThoi : null,
                                        NgayTao = ttg.NgayTao
                                    }
                                ).ToListAsync();

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
                        T_KLThungVaGang = x.T_KLThungVaGang,
                        G_ID_NguoiChuyen = x.G_ID_NguoiChuyen,
                        ChuyenDen = x.ChuyenDen,
                        BKMIS_SoMe = x.BKMIS_SoMe,
                        BKMIS_Gio = x.BKMIS_Gio,
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
                            T_KLThungVaGang = x.T_KLThungVaGang,
                            G_ID_NguoiChuyen = x.G_ID_NguoiChuyen,
                            ChuyenDen = x.ChuyenDen,
                            BKMIS_SoMe = x.BKMIS_SoMe,
                            BKMIS_Gio = x.BKMIS_Gio
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

                string maThungThep = GenerateMaThungThep(t.MaThungGang,payload.ngayNhan, payload.idLoThoi, t.T_Ca, countItem);

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
        public async Task<IActionResult> GetThungHuyNhan([FromBody] List<string> selectedMaThungHuy)
        {
            var tenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var taiKhoan = await _context.Tbl_TaiKhoan
                .FirstOrDefaultAsync(x => x.TenTaiKhoan == tenTaiKhoan);

            if (taiKhoan == null)
                return BadRequest("Không tìm thấy tài khoản.");

            // Bước 1: Lấy tất cả bản ghi theo danh sách mã thùng
            var allThung = await _context.Tbl_BM_16_TaiKhoan_Thung
                .Where(x => selectedMaThungHuy.Contains(x.MaThungGang))
                .ToListAsync();

            bool isValid = selectedMaThungHuy.All(maThung => allThung.Any(x => x.MaThungGang == maThung && x.ID_taiKhoan == taiKhoan.ID_TaiKhoan));

            if (isValid)
            {
                // Bước 3: Trả về các thùng đúng tài khoản
                var danhSachPhu = allThung
                    .Where(x => x.ID_taiKhoan == taiKhoan.ID_TaiKhoan)
                    .ToList();

                // Lấy tất cả thùng gang liên quan
                var gangLongList = await _context.Tbl_BM_16_GangLong
                    .Where(x => selectedMaThungHuy.Contains(x.MaThungGang))
                    .ToListAsync();

                // Ánh xạ MaThungGang => danh sách ID_TTG
                var maThungToListIDTTG = gangLongList
                    .Where(x => x.ID_TTG.HasValue)
                    .GroupBy(x => x.MaThungGang)
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .Select(x => x.ID_TTG.Value)
                            .Distinct()
                            .ToList()
                    );

                var allIDTTG = maThungToListIDTTG.Values.SelectMany(x => x).Distinct().ToList();

                // Lấy thông tin TTG: Ca, Ngày
                var dictTTG = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(x => allIDTTG.Contains(x.ID))
                    .ToListAsync();

                var ttgLookup = dictTTG
                    .GroupBy(x => x.ID)
                    .ToDictionary(x => x.Key, x => x.First()); // Unique theo ID

                // Kết quả gộp
                var ketQua = danhSachPhu
                    .GroupBy(x => x.MaThungGang)
                    .Select(g =>
                    {
                        var maThung = g.Key;
                        var lanNhanList = new List<(int Ca, DateTime Ngay)>();

                        if (maThungToListIDTTG.TryGetValue(maThung, out var idTTGs))
                        {
                            foreach (var id in idTTGs)
                            {
                                if (ttgLookup.TryGetValue(id, out var ttg))
                                {
                                    lanNhanList.Add((ttg.CaNhan, ttg.NgayNhan.Date));
                                }
                            }
                        }

                        // Gộp theo Ca + Ngày
                        var soLanDaNhan = lanNhanList
                            .GroupBy(x => new { x.Ca, x.Ngay })
                            .Select(g2 => new
                            {
                                Ca = g2.Key.Ca,
                                Ngay = g2.Key.Ngay,
                                SoLan = g2.Count()
                            })
                            .OrderBy(x => x.Ngay)
                            .ThenBy(x => x.Ca)
                            .ToList();

                        return new
                        {
                            maThungGang = maThung,
                            soLanDaNhan
                        };
                    })
                    .ToList();

                return Ok(new { data = ketQua, isExist = true });
            } else
            {
                return Ok(new { data = new List<object>(), isExist = false });
            }
            
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
                            var thungTrungGianCopy = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.MaThungTG == thungTrungGianToDelete.MaThungTG && x.IsCopy == true).ToListAsync();
                            if (thungTrungGianCopy.Any())
                            {
                                _context.Tbl_BM_16_ThungTrungGian.RemoveRange(thungTrungGianCopy);
                            }
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


        private string GenerateMaThungThep(string maThungGang, DateTime ngayNhan, int loThoiId, int? caValue, int count)
        {
            string ca = caValue == (int)CaLamViec.Ngay ? "N" : "Đ";
            string dayStr = ngayNhan.Day.ToString("00");
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
                var thungList = await GetDanhSachThungTrungGianDaNhan(payload);
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

                        var headerCell = worksheet.Range("A5:P5").Merge();
                        headerCell.Value = $"Ngày: {payload.NgayLamViec:dd/MM/yyyy}     Ca: {ca}     Lò thổi: {loThoi}";
                        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerCell.Style.Font.Bold = true;
                        headerCell.Style.Font.FontSize = 11;

                        // Xóa dữ liệu cũ (nếu có)
                        //var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 8;
                        //var rangeClear = worksheet.Range($"A8:O{lastRow}");
                        //rangeClear.Clear(XLClearOptions.Contents | XLClearOptions.NormalFormats);

                        int row = 8, stt = 1;

                        foreach (var thungTG in thungList)
                        {
                            var danhSach = thungTG.DanhSachThungGang;
                            int rowspan = danhSach.Any() ? danhSach.Count : 1;
                            int startRow = row;

                            if (danhSach.Any())
                            {
                                foreach (var item in danhSach)
                                {
                                    int icol = 1; // Bắt đầu cột từ 1 cho thùng gang

                                    worksheet.Cell(row, icol++).Value = stt++;
                                    worksheet.Cell(row, icol++).Value = item.MaThungThep;
                                    worksheet.Cell(row, icol++).Value = item.ID_LoCao;
                                    worksheet.Cell(row, icol++).Value = item.BKMIS_ThungSo;

                                    var statusCell = worksheet.Cell(row, icol++);
                                    statusCell.Style.Font.SetBold();

                                    if (item.ID_TrangThai == 5)
                                    {
                                        SetCellColor(statusCell, "Đã chốt", "#1e7e34", "#ffffff");
                                    }
                                    else if (item.ID_TrangThai == 2)
                                    {
                                        SetCellColor(statusCell, "Chờ xử lý", "#ffc107", "#212529");
                                    }
                                    else
                                    {
                                        SetCellColor(statusCell, "Chưa chuyển", "#6c757d", "#ffffff");
                                    }

                                    if (thungTG.IsCopy)
                                    {
                                        worksheet.Cell(row, icol++).Value = "";
                                        worksheet.Cell(row, icol++).Value = "";
                                        worksheet.Cell(row, icol++).Value = "";

                                    }
                                    else
                                    {
                                        worksheet.Cell(row, icol++).Value = item.T_KLThungVaGang;
                                        worksheet.Cell(row, icol++).Value = item.T_KLThungChua;

                                        var cell7 = worksheet.Cell(row, icol++);
                                        if (item.T_KLGangLong.HasValue)
                                        {
                                            cell7.Value =  item.T_KLGangLong.Value;
                                            cell7.Style.NumberFormat.Format = "0.00";
                                            cell7.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            cell7.Style.Font.Bold = true;
                                        }
                                    }
                                    

                                    worksheet.Row(row).Height = 25;
                                    row++;
                                }
                            }
                            else
                            {
                                // Nếu không có thùng gang, render dòng trống
                                int icol = 1;
                                worksheet.Cell(row, icol++).Value = stt++;
                                worksheet.Cell(row, icol++).Value = "-";
                                worksheet.Cell(row, icol++).Value = "-";
                                worksheet.Cell(row, icol++).Value = "-";
                                worksheet.Cell(row, icol++).Value = "-";
                                worksheet.Cell(row, icol++).Value = "-";
                                worksheet.Cell(row, icol++).Value = "-";
                                row++;
                            }

                            // Render và merge các ô Thùng Trung Gian từ cột 8 trở đi
                            int colTg = 9;
                            var cellTongKLGang = worksheet.Cell(startRow, colTg);
                            cellTongKLGang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            cellTongKLGang.Style.Font.Bold = true;

                            if (thungTG.IsCopy == true)
                            {
                                cellTongKLGang.Value = "TTG Copy";
                                cellTongKLGang.Style.Font.FontColor = XLColor.Red;
                            }
                            else if (thungTG.Tong_KLGangNhan.HasValue)
                            {
                                cellTongKLGang.Value = thungTG.Tong_KLGangNhan.Value;
                                cellTongKLGang.Style.NumberFormat.Format = "0.00";
                            }
                            else
                            {
                                cellTongKLGang.Value = "";
                            }
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                            colTg++;

                            worksheet.Cell(startRow, colTg).Value = thungTG.SoThungTG;
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                            colTg++;

                            worksheet.Cell(startRow, colTg).Value = thungTG.KLThungVaGang_Thoi;
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                            colTg++;

                            worksheet.Cell(startRow, colTg).Value = thungTG.KLThung_Thoi;
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                            colTg++;

                            worksheet.Cell(startRow, colTg).Value = thungTG.KL_phe;
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                            colTg++;

                            var cellKLGang = worksheet.Cell(startRow, colTg);
                            if (thungTG.KLGang_Thoi.HasValue)
                            {
                                cellKLGang.Value = thungTG.KLGang_Thoi.Value;
                                cellKLGang.Style.NumberFormat.Format = "0.00";
                                cellKLGang.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                cellKLGang.Style.Font.Bold = true;
                            }
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                            colTg++;

                            worksheet.Cell(startRow, colTg).Value = thungTG.MaMeThoi;
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                            colTg++;

                            worksheet.Cell(startRow, colTg).Value = thungTG.GhiChu ?? "";
                            worksheet.Range(startRow, colTg, row - 1, colTg).Merge();
                        }


                        // Dòng tổng
                        int sumRow = row;
                        var totalLabel = worksheet.Range($"A{sumRow}:H{sumRow}");
                        totalLabel.Merge();
                        totalLabel.Value = "Tổng:";
                        totalLabel.Style.Font.SetBold();
                        totalLabel.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột 9
                        worksheet.Cell(sumRow, 9).FormulaA1 = $"=SUM(I8:I{row - 1})";
                        worksheet.Cell(sumRow, 9).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 9).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        var totalLabel2 = worksheet.Range($"J{sumRow}:M{sumRow}");
                        totalLabel2.Merge();
                        totalLabel2.Value = "";
                        totalLabel2.Style.Font.SetBold();
                        totalLabel2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Tổng cột 14
                        worksheet.Cell(sumRow, 14).FormulaA1 = $"=SUM(N8:N{row - 1})";
                        worksheet.Cell(sumRow, 14).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(sumRow, 14).Style.Font.SetBold();
                        worksheet.Cell(sumRow, 14).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        var totalLabel3 = worksheet.Range($"O{sumRow}:P{sumRow}");
                        totalLabel3.Merge();
                        totalLabel3.Value = "";
                        totalLabel3.Style.Font.SetBold();
                        totalLabel3.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        // Format toàn bảng
                        var usedRange = worksheet.Range($"A7:P{sumRow}");
                        usedRange.Style.Font.SetFontName("Arial").Font.SetFontSize(11);
                        usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        usedRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        usedRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        usedRange.Style.Alignment.WrapText = true;


                        // Chiều cao dòng

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

        void SetCellColor(IXLCell cell, string text, string hexBackground, string hexFont)
        {
            cell.Value = text;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(hexBackground);
            cell.Style.Font.FontColor = XLColor.FromHtml(hexFont);
            cell.Style.Font.Bold = true;
        }

        public async Task<List<ThungTrungGianGroupViewModel>> GetDanhSachThungCoNguoiChuyen(SearchThungDaNhanDto payload)
        {
            var thungList = await GetDanhSachThungTrungGianDaNhan(payload);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault().TenNgan.Split('.').Last();


            if (string.IsNullOrEmpty(PhongBan))
                return new List<ThungTrungGianGroupViewModel>();

            // Lọc danh sách người chuyển dựa theo điều kiện
            var allNguoiChuyenIds = thungList
                .Where(t => t.DanhSachThungGang != null)
                .SelectMany(t => t.DanhSachThungGang
                    .Where(g => g.ID_TrangThai == 5 && g.ChuyenDen == PhongBan)
                )
                .Where(g => g.G_ID_NguoiChuyen.HasValue)
                .Select(g => g.G_ID_NguoiChuyen.Value)
                .Distinct()
                .ToList();

            // Lấy thông tin người chuyển, phòng ban, vị trí
            var thongTinNguoiChuyen = await (
                from user in _context.Tbl_TaiKhoan
                where allNguoiChuyenIds.Contains(user.ID_TaiKhoan)
                join phongBan in _context.Tbl_PhongBan on user.ID_PhongBan equals phongBan.ID_PhongBan into g_pb
                from phongBan in g_pb.DefaultIfEmpty()
                join vitri in _context.Tbl_ViTri on user.ID_ChucVu equals vitri.ID_ViTri into g_vt
                from vitri in g_vt.DefaultIfEmpty()
                select new
                {
                    user.ID_TaiKhoan,
                    user.HoVaTen,
                    user.TenTaiKhoan,
                    user.ChuKy,
                    TenPhongBan = phongBan != null ? phongBan.TenNgan : "",
                    TenViTri = vitri != null ? vitri.TenViTri : ""
                }
            ).ToListAsync();

            var thongTinDict = thongTinNguoiChuyen.ToDictionary(x => x.ID_TaiKhoan);

            // Gán thông tin người chuyển vào từng GangLongItem
            foreach (var ttg in thungList)
            {
                if (ttg.DanhSachThungGang != null)
                {
                    // Giữ lại chỉ những thùng gang đúng trạng thái và chuyển đến
                    ttg.DanhSachThungGang = ttg.DanhSachThungGang
                        .Where(g => g.ID_TrangThai == 5 && g.ChuyenDen == PhongBan)
                        .ToList();

                    foreach (var gang in ttg.DanhSachThungGang)
                    {
                        if (gang.G_ID_NguoiChuyen.HasValue &&
                            thongTinDict.TryGetValue(gang.G_ID_NguoiChuyen.Value, out var info))
                        {
                            gang.HoVaTen = info.HoVaTen;
                            gang.TenTaiKhoan = info.TenTaiKhoan;
                            gang.TenPhongBan = info.TenPhongBan;
                            gang.ChuKy = info.ChuKy;
                            gang.TenViTri = info.TenViTri;
                        }
                    }
                }
            }

            return thungList.Where(x => !x.IsCopy && x.DanhSachThungGang != null && x.DanhSachThungGang.Any()).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> ExportToPDF([FromBody] SearchThungDaNhanDto payload)
        {
            try {

                //GetDanhSachThungTrungGianDaNhan
                var data = await GetDanhSachThungCoNguoiChuyen(payload);
                if (data == null || !data.Any())
                    return BadRequest("Danh sách trống.");

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
