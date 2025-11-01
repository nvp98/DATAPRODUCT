using Data_Product.DTO;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

namespace Data_Product.Services
{
    public interface IChiaGangService
    {
        //Task<ChiaGangResultModel> TinhToanChiaGangAsync(List<int> IDs);
        Task<int> HuyChiaGangTheoNhieuThungGangAsync(List<string> maThungGangList, int idNguoiChia);
        Task KiemTraVaTinhLaiTheoMaThungGangAsync(string maThungGang);
        Task<ChiaGangResultModel> GetDetailChiaGangAsync(string maThungThep);
        Task<ChiTietChiaGangResponse> GetDetailChiaGangCRAsync(DetailChiaGangCRDto dto);
    }
    public class ChiaGangService: IChiaGangService
    {
        private readonly DataContext _context;
        public ChiaGangService(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
        }

        public async Task KiemTraVaTinhLaiTheoMaThungGangAsync(string maThungGang)
        {
            // Lấy tất cả các lần chia có liên quan đến cùng MaThungGang
            var chiaGangList = await _context.Tbl_BM_16_ChiaGang
                .Where(x => x.MaThungGang == maThungGang)
                .ToListAsync();

            if (!chiaGangList.Any())
            {
                await TuDongTinhToanChiaGangCanRayAsync(maThungGang);
                return;
            }

            // Lấy tất cả mã chia gang duy nhất (đại diện cho các lần chia riêng biệt)
            var maChiaGangList = chiaGangList
                .Select(x => x.MaChiaGang)
                .Distinct()
                .ToList();

            // Duyệt từng lần chia để tính toán lại
            foreach (var maChiaGang in maChiaGangList)
            {
                var listThepDaChia = await _context.Tbl_BM_16_ChiaGang
                    .Where(x => x.MaChiaGang == maChiaGang)
                    .ToListAsync();

                //var selectedThungs = listThepDaChia.Select(x => x.ID_Thung).ToList();
                var selectedThungs = listThepDaChia
                                        .Where(x => x.ID_Thung.HasValue)   // lọc null
                                        .Select(x => x.ID_Thung!.Value)    // lấy int
                                        .Distinct()                        // (tuỳ) bỏ trùng
                                        .ToList();
                var result = await TinhToanTuDongChiaGangAsync(selectedThungs);
                var thungAll = result.ThungAll;

                // Cache gang gốc cần cập nhật để xử lý một lần
                var gangGocList = await _context.Tbl_BM_16_GangLong
                    .Where(x => selectedThungs.Contains(x.ID))
                    .ToListAsync();

                foreach (var thung in thungAll)
                {
                    var chiaGangItem = listThepDaChia
                        .FirstOrDefault(x => x.ID_Thung == thung.ID && x.MaThungThep == thung.MaThungThep);

                    if (chiaGangItem != null)
                    {
                        chiaGangItem.PhanTram = thung.TyLeChia ?? 0;
                        chiaGangItem.KLGangChia = thung.KLChia ?? 0;
                    }

                    var gangGoc = gangGocList.FirstOrDefault(x => x.ID == thung.ID);
                    if (gangGoc != null)
                    {
                        gangGoc.KLGangChia = (thung.KLChia == null || thung.KLChia == 0) ? null : thung.KLChia;
                        //gangGoc.T_KLGangLong = thung.KLChia;
                    }
                }
            }
            await _context.SaveChangesAsync();

            await TuDongTinhToanChiaGangCanRayAsync(maThungGang);
        }

        //public async Task<ChiaGangResultModel> TinhToanChiaGangAsync(List<int> IDs)
        //{
        //    var danhsachThung = await (from a in _context.Tbl_BM_16_GangLong
        //                               where IDs.Contains(a.ID)
        //                               select new
        //                               {
        //                                   a.MaThungGang,
        //                                   a.BKMIS_ThungSo,
        //                                   a.ID,
        //                                   a.T_KLGangLong
        //                               }).ToListAsync();

        //    bool hasDuplicateMaThungGang = danhsachThung.GroupBy(x => x.MaThungGang)
        //                                                .Any(g => g.Count() > 1);

        //    if (hasDuplicateMaThungGang)
        //    {
        //        throw new Exception("Các mã thùng gang phải khác nhau.");
        //    }

        //    bool isAllThungSoSame = danhsachThung.Select(x => x.BKMIS_ThungSo)
        //                                        .Distinct()
        //                                        .Count() == 1;

        //    if (!isAllThungSoSame)
        //    {
        //        throw new Exception("Các thùng gang phải có cùng thùng số.");
        //    }

        //    if (danhsachThung.All(x => !x.T_KLGangLong.HasValue || x.T_KLGangLong == 0))
        //    {
        //        throw new Exception("Không có thùng gang nào có KL Gang lỏng cân bên HRC.");
        //    }

        //    var danhSachMaThung = danhsachThung.Select(x => x.MaThungGang).Distinct().ToList();

        //    var listAll = await (from a in _context.Tbl_BM_16_GangLong
        //                         join ttg in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttg.ID
        //                         where danhSachMaThung.Contains(a.MaThungGang)
        //                         select new ThungGangChiaModel
        //                         {
        //                             ID = a.ID,
        //                             MaThungGang = a.MaThungGang,
        //                             ID_Locao = a.ID_Locao,
        //                             BKMIS_ThungSo = a.BKMIS_ThungSo,
        //                             MaThungThep = a.MaThungThep,
        //                             G_KLGangLong = a.G_KLGangLong,
        //                             T_KLGangLong = a.T_KLGangLong,
        //                             T_KLThungChua = a.T_KLThungChua,
        //                             T_KLThungVaGang = a.T_KLThungVaGang,
        //                             KLGangChia = a.KLGangChia,
        //                             KL_Phe = ttg.KL_phe,
        //                             T_copy = a.T_copy,

        //                             TyLeChia = null,
        //                             KLChia = null
        //                         }).ToListAsync();

        //    var listGoc = listAll.Where(x => x.T_copy == false).ToList();

        //    // Nếu có bất kỳ thùng gốc nào thiếu G_KLGangLong thì không chia
        //    bool coThungGocNullGKL = listGoc.Any(x => !x.G_KLGangLong.HasValue || x.G_KLGangLong == 0);
        //    if (coThungGocNullGKL == true)
        //    {
        //        throw new Exception("KL Gang Lỏng bên Luyện Gang chưa được nhập đầy đủ. Vui lòng kiểm tra lại");
        //    }

        //    foreach (var thungGoc in listGoc)
        //    {
        //        var tongHRCDaRot = listAll
        //            .Where(x => x.MaThungGang == thungGoc.MaThungGang && !IDs.Contains(x.ID))
        //            .Where(x => x.T_KLGangLong.HasValue)
        //            .Sum(x => x.T_KLGangLong.Value);

        //        var klLuyenGang = thungGoc.G_KLGangLong ?? 0;

        //        if (klLuyenGang > 0 && klLuyenGang < tongHRCDaRot)
        //        {
        //            throw new Exception($"Mã thùng {thungGoc.MaThungGang} có KL Gang Lỏng ({klLuyenGang}) nhỏ hơn tổng HRC đã rót ({tongHRCDaRot}). Vui lòng check lại số liệu để chia lại KL Gang Chia");
        //        }
        //    }

        //    var daRongTheoMa = listAll
        //        .Where(x => x.T_KLGangLong.HasValue && !IDs.Contains(x.ID) && x.KLChia == null)
        //        .GroupBy(x => x.MaThungGang)
        //        .ToDictionary(
        //            g => g.Key,
        //            g => g.Sum(x => x.T_KLGangLong ?? 0)
        //        );

        //    var danhSachConLai = listGoc
        //        .Select(x =>
        //        {
        //            var daRong = daRongTheoMa.TryGetValue(x.MaThungGang, out var val) ? val : 0;
        //            var conLai = (x.G_KLGangLong ?? 0) - daRong;
        //            return new
        //            {
        //                MaThungGang = x.MaThungGang,
        //                KLConLai = conLai > 0 ? conLai : 0
        //            };
        //        })
        //        .Where(x => x.KLConLai > 0)
        //        .ToList();

        //    var tongConLai = danhSachConLai.Sum(x => x.KLConLai);

        //    if (tongConLai < 0)
        //        throw new Exception("Không có đủ khối lượng gang bên Luyện Gang để chia. Vui Lòng kiểm tra lại.");

        //    var tongT_KLGangLongChon = listAll
        //        .Where(x => IDs.Contains(x.ID) && x.T_KLGangLong.HasValue && x.T_KLThungVaGang.HasValue && x.T_KLThungChua.HasValue)
        //        .Sum(x => x.T_KLGangLong.Value);

        //    foreach (var item in listAll.Where(x => IDs.Contains(x.ID)))
        //    {
        //        var nguon = danhSachConLai.FirstOrDefault(x => x.MaThungGang == item.MaThungGang);
        //        if (nguon != null)
        //        {
        //            item.TyLeChia = Math.Round((nguon.KLConLai / tongConLai) * 100, 2);
        //            item.KLChia = Math.Round((item.TyLeChia ?? 0) * tongT_KLGangLongChon / 100, 2);
        //        }
        //        else
        //        {
        //            item.TyLeChia = 0;
        //            item.KLChia = 0;
        //        }
        //    }

        //    var listSelected = listAll.Where(x => IDs.Contains(x.ID)).ToList();

        //    return new ChiaGangResultModel
        //    {
        //        ThungGoc = listGoc,
        //        ThungDaCoKL = listAll.Where(x => x.T_KLGangLong.HasValue && !IDs.Contains(x.ID)).ToList(),
        //        ThungAll = listSelected,
        //        ListAll = listAll
        //    };
        //}
        public async Task<ChiaGangResultModel> TinhToanTuDongChiaGangAsync(List<int> IDs)
        {
            if (IDs == null || IDs.Count == 0)
                throw new Exception("Danh sách ID trống.");

            var idSet = IDs.ToHashSet();

            // 1) Validate input IDs
            var danhsachThung = await _context.Tbl_BM_16_GangLong
                .AsNoTracking()
                .Where(a => idSet.Contains(a.ID))
                .Select(a => new
                {
                    a.MaThungGang,
                    a.BKMIS_ThungSo,
                    a.ID,
                    a.T_KLGangLong
                })
                .ToListAsync();

            if (danhsachThung.Count != idSet.Count)
                throw new Exception("Một số ID không tồn tại hoặc không hợp lệ.");

            if (danhsachThung.GroupBy(x => x.MaThungGang).Any(g => g.Count() > 1))
                throw new Exception("Các mã thùng gang phải khác nhau.");

            if (danhsachThung.Select(x => x.BKMIS_ThungSo).Distinct().Count() != 1)
                throw new Exception("Các thùng gang phải có cùng thùng số.");

            bool khongCoT_KLGangLong = danhsachThung.All(x => !x.T_KLGangLong.HasValue || x.T_KLGangLong == 0);

            var thungSo = danhsachThung.First().BKMIS_ThungSo;
            var danhSachMaThung = danhsachThung.Select(x => x.MaThungGang).Distinct().ToList();

            // 2) Kéo toàn bộ thùng (gốc + copy) theo MaThungGang + ThungSo
            var listAll = await (from a in _context.Tbl_BM_16_GangLong.AsNoTracking()
                                 join ttg in _context.Tbl_BM_16_ThungTrungGian.AsNoTracking()
                                    on a.ID_TTG equals ttg.ID
                                 where danhSachMaThung.Contains(a.MaThungGang) && a.BKMIS_ThungSo == thungSo
                                 select new ThungGangChiaModel
                                 {
                                     ID = a.ID,
                                     MaThungGang = a.MaThungGang,
                                     ID_Locao = a.ID_Locao,
                                     BKMIS_ThungSo = a.BKMIS_ThungSo,
                                     MaThungThep = a.MaThungThep,
                                     G_KLGangLong = a.G_KLGangLong,
                                     T_KLGangLong = a.T_KLGangLong,
                                     T_KLThungChua = a.T_KLThungChua,
                                     T_KLThungVaGang = a.T_KLThungVaGang,
                                     KLGangChia = a.KLGangChia,   // cờ "đã chia" ở bảng GangLong
                                     KL_Phe = ttg.KL_phe,
                                     T_copy = a.T_copy,

                                     TyLeChia = null,
                                     KLChia = null
                                 }).ToListAsync();

            var listGoc = listAll.Where(x => x.T_copy == false).ToList();

            // 3) Xác định các thùng "đÃ CHIA" từ bảng Tbl_BM_16_ChiaGang (ID_Thung / MaThungThep)
            var allIds = listAll.Select(x => x.ID).Distinct().ToList();
            var allTheps = listAll.Select(x => x.MaThungThep).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

            var chiaRows = await _context.Tbl_BM_16_ChiaGang
                .AsNoTracking()
                .Where(cg => (cg.ID_Thung.HasValue && allIds.Contains(cg.ID_Thung.Value)) ||
                             (cg.MaThungThep != null && allTheps.Contains(cg.MaThungThep)))
                .Select(cg => new { cg.ID_Thung, cg.MaThungThep })
                .ToListAsync();

            var daChiaById = chiaRows.Where(r => r.ID_Thung.HasValue).Select(r => r.ID_Thung!.Value).ToHashSet();
            var daChiaByThep = chiaRows.Where(r => r.MaThungThep != null).Select(r => r.MaThungThep!).ToHashSet(StringComparer.OrdinalIgnoreCase);

            bool DaChia(ThungGangChiaModel x) =>
                x.KLGangChia.HasValue ||                                   // đã có KLGangChia
                daChiaById.Contains(x.ID) ||                               // có dòng trong bảng chia theo ID_Thung
                (!string.IsNullOrEmpty(x.MaThungThep) && daChiaByThep.Contains(x.MaThungThep!)); // hoặc theo Mã Thùng Thép

            // 4) Kiểm tra dữ liệu gốc
            bool coThungGocNullGKL = listGoc.Any(x => !x.G_KLGangLong.HasValue || x.G_KLGangLong == 0);

            // 5) Đánh dấu các MaThungGang "không đủ KL" (G_KLGangLong < tổng HRC đã rót CHƯA CHIA)
            var maThungKhongDuKL = new HashSet<string>();
            foreach (var thungGoc in listGoc)
            {
                var tongHRCDaRotChuaChia = listAll
                    .Where(x => x.MaThungGang == thungGoc.MaThungGang && !idSet.Contains(x.ID))
                    .Where(x => x.T_KLGangLong.HasValue && !DaChia(x))   // KHÔNG trừ các thùng đã chia
                    .Sum(x => x.T_KLGangLong!.Value);

                var klLuyenGang = thungGoc.G_KLGangLong ?? 0;
                if (klLuyenGang > 0 && klLuyenGang < tongHRCDaRotChuaChia)
                    maThungKhongDuKL.Add(thungGoc.MaThungGang);
            }

            // 6) Tổng đã rót CHƯA CHIA theo MaThungGang (không gồm IDs)
            var daRongTheoMa = listAll
                .Where(x => x.T_KLGangLong.HasValue && !idSet.Contains(x.ID) && !DaChia(x))
                .GroupBy(x => x.MaThungGang)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.T_KLGangLong ?? 0m));

            // 7) KL còn lại cho từng thùng gốc
            var danhSachConLai = listGoc
                .Select(x =>
                {
                    var daRong = daRongTheoMa.TryGetValue(x.MaThungGang, out var v) ? v : 0m;
                    var conLai = (x.G_KLGangLong ?? 0m) - daRong;
                    return new { x.MaThungGang, KLConLai = conLai > 0m ? conLai : 0m };
                })
                .Where(x => x.KLConLai > 0m)
                .ToList();

            var tongConLai = danhSachConLai.Sum(x => x.KLConLai);

            // 8) Tổng KL cần chia của các thùng đang chọn
            var tongT_KLGangLongChon = listAll
                .Where(x => idSet.Contains(x.ID))
                .Sum(x => x.T_KLGangLong ?? 0);

            // 9) Phân tỷ lệ & KL chia cho từng thùng được chọn
            foreach (var item in listAll.Where(x => idSet.Contains(x.ID)))
            {
                var nguon = danhSachConLai.FirstOrDefault(x => x.MaThungGang == item.MaThungGang);

                if (coThungGocNullGKL ||
                    khongCoT_KLGangLong ||
                    tongConLai <= 0 ||
                    nguon == null ||
                    maThungKhongDuKL.Count() > 0 ||
                    tongT_KLGangLongChon <= 0)
                {
                    item.TyLeChia = 0;
                    item.KLChia = 0;
                }
                else
                {
                    var ratio = nguon.KLConLai / tongConLai;
                    item.TyLeChia = Math.Round(ratio * 100m, 2);
                    item.KLChia = Math.Round(ratio * tongT_KLGangLongChon, 2);
                }
            }

            var listSelected = listAll.Where(x => idSet.Contains(x.ID)).ToList();

            return new ChiaGangResultModel
            {
                ThungGoc = listGoc,
                ThungDaCoKL = listAll.Where(x => x.T_KLGangLong.HasValue && !idSet.Contains(x.ID)).ToList(),
                ThungAll = listSelected,
                ListAll = listAll
            };
        }

        public async Task<ChiaGangResultModel> GetDetailChiaGangAsync(string maThungThep)
        {
            var chiaGang = await _context.Tbl_BM_16_ChiaGang
                .Where(x => x.MaThungThep == maThungThep)
                .FirstOrDefaultAsync();


            if (chiaGang == null)
                return null;

            var chiaGangList = await _context.Tbl_BM_16_ChiaGang
                .Where(x => x.MaChiaGang == chiaGang.MaChiaGang)
                .ToListAsync();
            // Lấy các mã thùng gang và thép liên quan
            var maThungGangList = chiaGangList.Select(x => x.MaThungGang).Distinct().ToList();

            // Lấy danh sách từ bảng GangLong
            var listAll = await (
                                from a in _context.Tbl_BM_16_GangLong
                                where maThungGangList.Contains(a.MaThungGang)

                                // LEFT JOIN với bảng Tbl_BM_16_ChiaGang
                                join cg in _context.Tbl_BM_16_ChiaGang
                                    on new { a.MaThungThep, MaChiaGang = chiaGang.MaChiaGang }
                                    equals new { cg.MaThungThep, cg.MaChiaGang } into cgGroup
                                from cg in cgGroup.DefaultIfEmpty()

                                    // LEFT JOIN với Thùng trung gian
                                join ttg in _context.Tbl_BM_16_ThungTrungGian
                                    on a.ID_TTG equals ttg.ID into ttgGroup
                                from ttg in ttgGroup.DefaultIfEmpty()

                                select new ThungGangChiaModel
                                {
                                    ID = a.ID,
                                    MaThungGang = a.MaThungGang,
                                    ID_Locao = a.ID_Locao,
                                    BKMIS_ThungSo = a.BKMIS_ThungSo,
                                    MaThungThep = a.MaThungThep,
                                    G_KLGangLong = a.G_KLGangLong,
                                    T_KLGangLong = a.T_KLGangLong,
                                    T_KLThungChua = a.T_KLThungChua,
                                    T_KLThungVaGang = a.T_KLThungVaGang,
                                    KLGangChia = a.KLGangChia,
                                    T_copy = a.T_copy,
                                    KL_Phe = ttg != null ? ttg.KL_phe : null,
                                    TyLeChia = cg != null ? cg.PhanTram : null,
                                    KLChia = cg != null ? cg.KLGangChia : null,
                                    MaChiaGang = cg != null ? cg.MaChiaGang : null
                                }).ToListAsync();


            // Lọc danh sách
            var selectedIds = chiaGangList.Select(x => x.ID_Thung).ToHashSet();

            var listSelected = listAll.Where(x => selectedIds.Contains(x.ID)).ToList();

            var listGoc = listAll.Where(x => x.T_copy == false).ToList();

            // === NEW: chỉ lấy các thùng CÙNG MaThungGang với thùng gốc & CHƯA được chia (KLGangChia null/0) & không thuộc selectedIds ===
            var maThungGocSet = new HashSet<string>(
                listGoc.Where(x => !string.IsNullOrEmpty(x.MaThungGang))
                       .Select(x => x.MaThungGang!),
                StringComparer.OrdinalIgnoreCase
            );

            var listDaCoKL = listAll
                .Where(x => x.T_KLGangLong.HasValue                       // đã có KL thực nhập
                            && !selectedIds.Contains(x.ID)                // không nằm trong nhóm đang chia
                            && !string.IsNullOrEmpty(x.MaThungGang)       // có mã thùng
                            && maThungGocSet.Contains(x.MaThungGang!)     // cùng MaThungGang với thùng gốc
                            && (!x.KLGangChia.HasValue)) // CHƯA chia
                .ToList();
            // === END NEW ===
            return new ChiaGangResultModel
            {
                ThungGoc = listGoc,
                ThungDaCoKL = listDaCoKL,
                ThungAll = listSelected,
                ListAll = listAll
            };
        }

        public async Task<int> HuyChiaGangTheoNhieuThungGangAsync(List<string> maThungGangList, int idNguoiChia)
        {
            if (maThungGangList == null || !maThungGangList.Any() || idNguoiChia <= 0)
                throw new ArgumentException("Danh sách mã thùng gang hoặc người dùng không hợp lệ.");

            // Lấy tất cả MaChiaGang mà người này đã tạo từ các thùng cụ thể
            var maChiaGangList = await _context.Tbl_BM_16_ChiaGang
                .Where(x => maThungGangList.Contains(x.MaThungGang) && x.ID_NguoiChia == idNguoiChia)
                .Select(x => x.MaChiaGang)
                .Distinct()
                .ToListAsync();

            if (!maChiaGangList.Any())
                return 0;

            // Lấy tất cả bản ghi chia gang có cùng MaChiaGang đó
            var listCanXoa = await _context.Tbl_BM_16_ChiaGang
                .Where(x => maChiaGangList.Contains(x.MaChiaGang))
                .ToListAsync();

            // 3. Lấy danh sách MaThungThep để cập nhật KLGangChia
            var maThungThepList = listCanXoa
                .Select(x => x.MaThungThep)
                .Distinct()
                .ToList();

            // 4. Cập nhật KLGangChia = null cho các thùng thép liên quan
            var thungThepCanUpdate = await _context.Tbl_BM_16_GangLong
                .Where(x => maThungThepList.Contains(x.MaThungThep))
                .ToListAsync();

            foreach (var thung in thungThepCanUpdate)
            {
                thung.KLGangChia = null;
                thung.T_KLThungVaGang = null;
                thung.T_KLThungChua = null;
                thung.T_KLGangLong = null;
                // update với các thùng trong thùng trung gian

                var thungttg = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.ID == thung.ID_TTG).FirstOrDefaultAsync();
                thungttg.KL_phe = null;
                thungttg.KLGang_Thoi = null;
                thungttg.KLThung_Thoi = null;
                thungttg.KLThungVaGang_Thoi = null;
                thungttg.ID_MeThoi = null;
                thungttg.GioChonMe = null;
                thungttg.Tong_KLGangNhan = null;
                thungttg.GhiChu = null;
            }
            _context.Tbl_BM_16_ChiaGang.RemoveRange(listCanXoa);
            return await _context.SaveChangesAsync(); // Trả về số bản ghi đã xóa
        }


        //public async Task<bool> TuDongTinhToanChiaGangCanRayAsync(string maThungGang)
        //{
        //    if (string.IsNullOrWhiteSpace(maThungGang)) return true;

        //    try
        //    {
        //        // ===== TẦNG 1: TÍNH CR CHO TỪNG THÙNG GANG (RAM) =====
        //        var thungs = await _context.Tbl_BM_16_GangLong
        //            .Where(x => x.MaThungGang == maThungGang)
        //            .ToListAsync();
        //        if (thungs.Count == 0) return true;

        //        // Chỉ tính khi đã nhận
        //        var hasBeenReceived = await _context.Tbl_BM_16_TaiKhoan_Thung
        //            .AnyAsync(x => x.MaThungGang == maThungGang);
        //        if (!hasBeenReceived) return true;

        //        // Chọn thùng gốc: T_copy != true (null/false => gốc)
        //        var thungGoc = thungs.Where(t => t.T_copy != true).OrderBy(t => t.ID).FirstOrDefault();
        //        if (thungGoc == null || !thungGoc.G_KLGangLong.HasValue || thungGoc.G_KLGangLong.Value <= 0m) return true;

        //        decimal gKl = thungGoc.G_KLGangLong.Value;

        //        static decimal? BaseOf(Tbl_BM_16_GangLong t)
        //        {
        //            var v = t.KLGangChia ?? t.T_KLGangLong;
        //            return (v.HasValue && v.Value > 0m) ? v : null;
        //        }

        //        var baseMap = thungs.ToDictionary(t => t.ID, BaseOf);
        //        decimal sumBase = baseMap.Values.Where(v => v.HasValue).Sum(v => v!.Value);
        //        if (sumBase <= 0m) return true;

        //        // KL CR (tầng 1) cho từng thùng
        //        var klCrDict = new Dictionary<int, decimal>(thungs.Count); // <GangID, KL_CR>
        //        foreach (var t in thungs)
        //        {
        //            var b = baseMap[t.ID];
        //            if (b.HasValue)
        //            {
        //                var tyLe = b.Value / sumBase;
        //                var cr = Math.Round(tyLe * gKl, 2, MidpointRounding.AwayFromZero); // 2 chữ số
        //                klCrDict[t.ID] = cr;
        //            }
        //            else klCrDict[t.ID] = 0m;
        //        }

        //        // ===== TẦNG 2: PHÂN BỔ VÀO TTG (chỉ khi có copy mới chia tỷ lệ) =====
        //        var ttgIdsOfGangs = thungs.Where(x => x.ID_TTG.HasValue).Select(x => x.ID_TTG!.Value).Distinct().ToList();
        //        if (ttgIdsOfGangs.Count == 0) return true;

        //        // ID_TTG -> MaThungTG
        //        var ttgBasics = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(ttg => ttgIdsOfGangs.Contains(ttg.ID))
        //            .Select(ttg => new { ttg.ID, ttg.MaThungTG })
        //            .ToListAsync();

        //        var maTTGs = ttgBasics.Select(x => x.MaThungTG).Distinct().ToList();

        //        // Toàn bộ TTG (gốc + copy) theo từng MaThungTG
        //        var ttgFamily = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(ttg => maTTGs.Contains(ttg.MaThungTG))
        //            .Select(ttg => new { ttg.ID, ttg.MaThungTG, ttg.IsCopy, ttg.KLGang_Thoi })
        //            .ToListAsync();

        //        var idTtgToMa = ttgBasics.ToDictionary(x => x.ID, x => x.MaThungTG);
        //        var maToTtgs = ttgFamily.GroupBy(x => x.MaThungTG).ToDictionary(g => g.Key, g => g.ToList());

        //        // Nạp sẵn các phân bổ hiện có để update thay vì add trùng
        //        var gangIdsAll = thungs.Select(x => x.ID).ToList();
        //        var ttgTargetAll = ttgFamily.Select(x => x.ID).ToList();
        //        var exAllocs = await _context.Tbl_BM_16_PhanBoGangCR
        //            .Where(pb => gangIdsAll.Contains(pb.ID_GangLong) && ttgTargetAll.Contains(pb.ID_TTG_Target))
        //            .ToListAsync();
        //        var exDict = exAllocs.ToDictionary(k => (k.ID_GangLong, k.ID_TTG_Target), v => v);
        //        var touched = new HashSet<(int, int)>();

        //        foreach (var g in thungs)
        //        {
        //            if (!g.ID_TTG.HasValue) continue;

        //            if (!klCrDict.TryGetValue(g.ID, out var crOfGang) || crOfGang <= 0m)
        //                continue;

        //            // Family theo MaThungTG của TTG gắn với thùng gang này
        //            if (!idTtgToMa.TryGetValue(g.ID_TTG.Value, out var maTTG)) continue;
        //            if (!maToTtgs.TryGetValue(maTTG, out var members) || members.Count == 0) continue;

        //            if (members.Count == 1)
        //            {
        //                // ✅ Không có copy → KHÔNG tính tỷ lệ TTG, dồn 100% vào TTG gốc
        //                var only = members[0];
        //                var key = (g.ID, only.ID);
        //                touched.Add(key);

        //                if (exDict.TryGetValue(key, out var row))
        //                {
        //                    row.MaThungGang = g.MaThungGang ?? string.Empty;
        //                    row.MaThungTG = maTTG;
        //                    row.IsTTGCopy = only.IsCopy;      // sẽ là false
        //                    row.TyLeTrongMaTTG = 1m;               // 100%
        //                    row.KL_PhanBo_CR = crOfGang;         // CR tầng 1
        //                }
        //                else
        //                {
        //                    _context.Tbl_BM_16_PhanBoGangCR.Add(new Tbl_BM_16_PhanBoGangCR
        //                    {
        //                        ID_GangLong = g.ID,
        //                        ID_TTG_Target = only.ID,
        //                        MaThungGang = g.MaThungGang ?? string.Empty,
        //                        MaThungTG = maTTG,
        //                        IsTTGCopy = only.IsCopy,
        //                        TyLeTrongMaTTG = 1m,
        //                        KL_PhanBo_CR = crOfGang
        //                    });
        //                }
        //            }
        //            else
        //            {
        //                // ✅ Có copy → CHIA TỶ LỆ theo KLGang_Thoi
        //                decimal sumKLTG = members.Sum(m => m.KLGang_Thoi ?? 0m);

        //                if (sumKLTG > 0m)
        //                {
        //                    foreach (var m in members)
        //                    {
        //                        var part = m.KLGang_Thoi ?? 0m;
        //                        if (part <= 0m) continue;

        //                        var tyle = Math.Round(part / sumKLTG, 6, MidpointRounding.AwayFromZero);
        //                        var crOut = Math.Round(crOfGang * tyle, 2, MidpointRounding.AwayFromZero);

        //                        var key = (g.ID, m.ID);
        //                        touched.Add(key);

        //                        if (exDict.TryGetValue(key, out var row))
        //                        {
        //                            row.MaThungGang = g.MaThungGang ?? string.Empty;
        //                            row.MaThungTG = maTTG;
        //                            row.IsTTGCopy = m.IsCopy;
        //                            row.TyLeTrongMaTTG = tyle;
        //                            row.KL_PhanBo_CR = crOut;
        //                        }
        //                        else
        //                        {
        //                            _context.Tbl_BM_16_PhanBoGangCR.Add(new Tbl_BM_16_PhanBoGangCR
        //                            {
        //                                ID_GangLong = g.ID,
        //                                ID_TTG_Target = m.ID,
        //                                MaThungGang = g.MaThungGang ?? string.Empty,
        //                                MaThungTG = maTTG,
        //                                IsTTGCopy = m.IsCopy,
        //                                TyLeTrongMaTTG = tyle,
        //                                KL_PhanBo_CR = crOut
        //                            });
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    // Có copy nhưng KLGang_Thoi không có số liệu → CHIA ĐỀU
        //                    var equal = Math.Round(1m / members.Count, 6, MidpointRounding.AwayFromZero);
        //                    var each = Math.Round(crOfGang * equal, 2, MidpointRounding.AwayFromZero);

        //                    foreach (var m in members)
        //                    {
        //                        var key = (g.ID, m.ID);
        //                        touched.Add(key);

        //                        if (exDict.TryGetValue(key, out var row))
        //                        {
        //                            row.MaThungGang = g.MaThungGang ?? string.Empty;
        //                            row.MaThungTG = maTTG;
        //                            row.IsTTGCopy = m.IsCopy;
        //                            row.TyLeTrongMaTTG = equal;
        //                            row.KL_PhanBo_CR = each;
        //                        }
        //                        else
        //                        {
        //                            _context.Tbl_BM_16_PhanBoGangCR.Add(new Tbl_BM_16_PhanBoGangCR
        //                            {
        //                                ID_GangLong = g.ID,
        //                                ID_TTG_Target = m.ID,
        //                                MaThungGang = g.MaThungGang ?? string.Empty,
        //                                MaThungTG = maTTG,
        //                                IsTTGCopy = m.IsCopy,
        //                                TyLeTrongMaTTG = equal,
        //                                KL_PhanBo_CR = each
        //                            });
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        // (Tuỳ chọn) zero-out các dòng cũ trong phạm vi nhưng không còn hợp lệ ở lần chạy này
        //        foreach (var kv in exDict)
        //        {
        //            var key = (kv.Key.Item1, kv.Key.Item2);
        //            if (!touched.Contains(key) &&
        //                gangIdsAll.Contains(key.Item1) &&
        //                ttgTargetAll.Contains(key.Item2) &&
        //                string.Equals(kv.Value.MaThungGang, maThungGang, StringComparison.OrdinalIgnoreCase))
        //            {
        //                kv.Value.TyLeTrongMaTTG = 0m; // hoặc null
        //                kv.Value.KL_PhanBo_CR = 0m; // hoặc null
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[ERROR] TuDongTinhToanChiaGangCanRayAsync({maThungGang}): {ex}");
        //        return false;
        //    }
        //}
        public async Task<bool> TuDongTinhToanChiaGangCanRayAsync(string maThungGang)
        {
            if (string.IsNullOrWhiteSpace(maThungGang)) return true;

            try
            {
                // ===== TẦNG 1: TÍNH CR CHO TỪNG THÙNG GANG (RAM) =====
                var thungs = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.MaThungGang == maThungGang)
                    .ToListAsync();
                if (thungs.Count == 0) return true;

                // Chỉ tính khi đã nhận
                var hasBeenReceived = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .AnyAsync(x => x.MaThungGang == maThungGang);
                if (!hasBeenReceived) return true;

                // Thùng gốc: T_copy != true
                var thungGoc = thungs.Where(t => t.T_copy != true).OrderBy(t => t.ID).FirstOrDefault();
                if (thungGoc == null || !thungGoc.G_KLGangLong.HasValue || thungGoc.G_KLGangLong.Value <= 0m) return true;

                decimal gKl = thungGoc.G_KLGangLong.Value;

                static decimal? BaseOf(Tbl_BM_16_GangLong t)
                {
                    var v = t.KLGangChia ?? t.T_KLGangLong;
                    return (v.HasValue && v.Value > 0m) ? v : null;
                }

                var baseMap = thungs.ToDictionary(t => t.ID, BaseOf);
                decimal sumBase = baseMap.Values.Where(v => v.HasValue).Sum(v => v!.Value);
                if (sumBase <= 0m) return true;

                // CR tầng 1
                var klCrDict = new Dictionary<int, decimal>(thungs.Count);
                foreach (var t in thungs)
                {
                    var b = baseMap[t.ID];
                    if (b.HasValue)
                    {
                        var tyLe = b.Value / sumBase;
                        var cr = Math.Round(tyLe * gKl, 2, MidpointRounding.AwayFromZero);
                        klCrDict[t.ID] = cr;
                    }
                    else klCrDict[t.ID] = 0m;
                }

                // ===== RULE MaChiaGang + DUC1/DUC2 (cấy giữa Tầng 1 và 2) =====
                static bool IsDuc(string? chuyenDen)
                {
                    if (string.IsNullOrWhiteSpace(chuyenDen)) return false;
                    var val = chuyenDen.Trim();
                    return val.Equals("DUC1", StringComparison.OrdinalIgnoreCase)
                        || val.Equals("DUC2", StringComparison.OrdinalIgnoreCase);
                }

                var maThungCoKL = thungs
                    .Where(t => t.KLGangChia.HasValue && t.KLGangChia.Value > 0m && !string.IsNullOrWhiteSpace(t.MaThungGang))
                    .Select(t => t.MaThungGang!.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var invalidChuyenDenGangIds = new HashSet<int>();    // thùng bị coi sai tuyến
                var overrideCrDict = new Dictionary<int, decimal>(); // ép CR = KLGangChia (Case A)

                if (maThungCoKL.Count > 0)
                {
                    var chiaRows = await _context.Tbl_BM_16_ChiaGang
                        .Where(x => maThungCoKL.Contains(x.MaThungGang))
                        .Select(x => new { x.MaChiaGang, x.MaThungGang })
                        .ToListAsync();

                    var maChiaToThung = chiaRows
                        .GroupBy(x => x.MaChiaGang)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.MaThungGang).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

                    var allMaThungInChia = maChiaToThung.Values.SelectMany(v => v).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                    var inMem = thungs
                        .Where(t => !string.IsNullOrWhiteSpace(t.MaThungGang))
                        .ToDictionary(t => t.MaThungGang!, t => t, StringComparer.OrdinalIgnoreCase);

                    var missing = allMaThungInChia.Where(ma => !inMem.ContainsKey(ma)).ToList();
                    if (missing.Count > 0)
                    {
                        var extra = await _context.Tbl_BM_16_GangLong
                            .Where(t => missing.Contains(t.MaThungGang))
                            .Select(t => new { t.ID, t.MaThungGang, t.ChuyenDen, t.KLGangChia })
                            .ToListAsync();

                        foreach (var e in extra)
                        {
                            inMem[e.MaThungGang!] = new Tbl_BM_16_GangLong
                            {
                                ID = e.ID,
                                MaThungGang = e.MaThungGang,
                                ChuyenDen = e.ChuyenDen,
                                KLGangChia = e.KLGangChia
                            };
                        }
                    }

                    foreach (var kv in maChiaToThung)
                    {
                        var listMa = kv.Value; // kỳ vọng 2
                        if (listMa.Count != 2) continue;

                        var a = inMem.TryGetValue(listMa[0], out var A) ? A : null;
                        var b = inMem.TryGetValue(listMa[1], out var B) ? B : null;
                        if (a == null || b == null) continue;

                        var aIsDuc = IsDuc(a.ChuyenDen);
                        var bIsDuc = IsDuc(b.ChuyenDen);

                        if (aIsDuc && bIsDuc)
                        {
                            if (a.KLGangChia.HasValue) overrideCrDict[a.ID] = Math.Round(a.KLGangChia.Value, 2, MidpointRounding.AwayFromZero);
                            if (b.KLGangChia.HasValue) overrideCrDict[b.ID] = Math.Round(b.KLGangChia.Value, 2, MidpointRounding.AwayFromZero);
                        }
                        else if (!aIsDuc && !bIsDuc)
                        {
                            // giữ nguyên
                        }
                        else
                        {
                            // một đúc, một không → thằng không đúc bị đánh dấu sai
                            var wrong = aIsDuc ? b : a;
                            if (wrong != null)
                            {
                                invalidChuyenDenGangIds.Add(wrong.ID);
                                klCrDict[wrong.ID] = 0m; // chặn phân bổ
                            }
                        }
                    }

                    foreach (var ov in overrideCrDict)
                        klCrDict[ov.Key] = ov.Value;
                }

                // ===== TẦNG 2: PHÂN BỔ VÀO TTG =====
                var ttgIdsOfGangs = thungs.Where(x => x.ID_TTG.HasValue).Select(x => x.ID_TTG!.Value).Distinct().ToList();
                if (ttgIdsOfGangs.Count == 0) { await _context.SaveChangesAsync(); return true; }

                var ttgBasics = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(ttg => ttgIdsOfGangs.Contains(ttg.ID))
                    .Select(ttg => new { ttg.ID, ttg.MaThungTG })
                    .ToListAsync();

                var maTTGs = ttgBasics.Select(x => x.MaThungTG).Distinct().ToList();

                var ttgFamily = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(ttg => maTTGs.Contains(ttg.MaThungTG))
                    .Select(ttg => new { ttg.ID, ttg.MaThungTG, ttg.IsCopy, ttg.KLGang_Thoi })
                    .ToListAsync();

                var idTtgToMa = ttgBasics.ToDictionary(x => x.ID, x => x.MaThungTG);
                var maToTtgs = ttgFamily.GroupBy(x => x.MaThungTG).ToDictionary(g => g.Key, g => g.ToList());

                var gangIdsAll = thungs.Select(x => x.ID).ToList();
                var ttgTargetAll = ttgFamily.Select(x => x.ID).ToList();
                var exAllocs = await _context.Tbl_BM_16_PhanBoGangCR
                    .Where(pb => gangIdsAll.Contains(pb.ID_GangLong) && ttgTargetAll.Contains(pb.ID_TTG_Target))
                    .ToListAsync();
                var exDict = exAllocs.ToDictionary(k => (k.ID_GangLong, k.ID_TTG_Target), v => v);
                var touched = new HashSet<(int, int)>();

                // helper ghi/ cập nhật 1 dòng phân bổ
                void UpsertAlloc(Tbl_BM_16_GangLong g, int targetId, string maTtg, bool isCopy, decimal tyLe, decimal kl, bool isSai)
                {
                    var key = (g.ID, targetId);
                    touched.Add(key);

                    if (exDict.TryGetValue(key, out var row))
                    {
                        row.MaThungGang = g.MaThungGang ?? string.Empty;
                        row.MaThungTG = maTtg;
                        row.IsTTGCopy = isCopy;
                        row.TyLeTrongMaTTG = tyLe;
                        row.KL_PhanBo_CR = kl;
                        row.IsSaiChuyenDen = isSai;
                    }
                    else
                    {
                        _context.Tbl_BM_16_PhanBoGangCR.Add(new Tbl_BM_16_PhanBoGangCR
                        {
                            ID_GangLong = g.ID,
                            ID_TTG_Target = targetId,
                            MaThungGang = g.MaThungGang ?? string.Empty,
                            MaThungTG = maTtg,
                            IsTTGCopy = isCopy,
                            TyLeTrongMaTTG = tyLe,
                            KL_PhanBo_CR = kl,
                            IsSaiChuyenDen = isSai
                        });
                    }
                }

                foreach (var g in thungs)
                {
                    if (!g.ID_TTG.HasValue) continue;

                    // Nếu sai ChuyenDen → ghi dòng 0 kèm cờ cho tất cả TTG cùng nhóm để UI biết
                    if (invalidChuyenDenGangIds.Contains(g.ID))
                    {
                        if (!idTtgToMa.TryGetValue(g.ID_TTG.Value, out var maTTG0)) continue;
                        if (!maToTtgs.TryGetValue(maTTG0, out var members0) || members0.Count == 0) continue;

                        foreach (var m in members0)
                            UpsertAlloc(g, m.ID, maTTG0, m.IsCopy, 0m, 0m, true);

                        continue; // không phân bổ thực tế
                    }

                    if (!klCrDict.TryGetValue(g.ID, out var crOfGang) || crOfGang <= 0m) continue;

                    if (!idTtgToMa.TryGetValue(g.ID_TTG.Value, out var maTTG)) continue;
                    if (!maToTtgs.TryGetValue(maTTG, out var members) || members.Count == 0) continue;

                    if (members.Count == 1)
                    {
                        var only = members[0];
                        UpsertAlloc(g, only.ID, maTTG, only.IsCopy, 1m, crOfGang, false);
                    }
                    else
                    {
                        decimal sumKLTG = members.Sum(m => m.KLGang_Thoi ?? 0m);

                        if (sumKLTG > 0m)
                        {
                            foreach (var m in members)
                            {
                                var part = m.KLGang_Thoi ?? 0m;
                                if (part <= 0m) continue;

                                var tyle = Math.Round(part / sumKLTG, 6, MidpointRounding.AwayFromZero);
                                var crOut = Math.Round(crOfGang * tyle, 2, MidpointRounding.AwayFromZero);

                                UpsertAlloc(g, m.ID, maTTG, m.IsCopy, tyle, crOut, false);
                            }
                        }
                        else
                        {
                            var equal = Math.Round(1m / members.Count, 6, MidpointRounding.AwayFromZero);
                            var each = Math.Round(crOfGang * equal, 2, MidpointRounding.AwayFromZero);

                            foreach (var m in members)
                                UpsertAlloc(g, m.ID, maTTG, m.IsCopy, equal, each, false);
                        }
                    }
                }

                // Zero-out dòng cũ (giữ nguyên những dòng đã cờ sai để UI thấy)
                foreach (var kv in exDict)
                {
                    var key = (kv.Key.Item1, kv.Key.Item2);
                    if (!touched.Contains(key) &&
                        gangIdsAll.Contains(key.Item1) &&
                        ttgTargetAll.Contains(key.Item2) &&
                        string.Equals(kv.Value.MaThungGang, maThungGang, StringComparison.OrdinalIgnoreCase) &&
                        (kv.Value.IsSaiChuyenDen == null || kv.Value.IsSaiChuyenDen == false))
                    {
                        kv.Value.TyLeTrongMaTTG = 0m;
                        kv.Value.KL_PhanBo_CR = 0m;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TuDongTinhToanChiaGangCanRayAsync({maThungGang}): {ex}");
                return false;
            }
        }
        //public async Task<bool> TuDongTinhToanChiaGangCanRayAsync(string maThungGang)
        //{
        //    if (string.IsNullOrWhiteSpace(maThungGang)) return true;

        //    try
        //    {
        //        // ===== Helper =====
        //        static bool IsDuc(string? chuyenDen)
        //        {
        //            if (string.IsNullOrWhiteSpace(chuyenDen)) return false;
        //            var v = chuyenDen.Trim();
        //            return v.Equals("DUC1", StringComparison.OrdinalIgnoreCase) ||
        //                   v.Equals("DUC2", StringComparison.OrdinalIgnoreCase);
        //        }

        //        static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
        //        static decimal R6(decimal v) => Math.Round(v, 6, MidpointRounding.AwayFromZero);

        //        // ===== 1) Lấy toàn bộ record của MaThungGang gọi hàm =====
        //        var rowsOfGang = await _context.Tbl_BM_16_GangLong
        //            .Where(x => x.MaThungGang == maThungGang)
        //            .ToListAsync();
        //        if (rowsOfGang.Count == 0) return true;

        //        // Đã được nhận chưa? (theo gang gốc như trước)
        //        var hasBeenReceived = await _context.Tbl_BM_16_TaiKhoan_Thung
        //            .AnyAsync(x => x.MaThungGang == maThungGang);
        //        if (!hasBeenReceived) return true;

        //        // Tập MaThungThep xuất phát từ thùng gang này (có thể nhiều steel ladles)
        //        var steelListOfThisGang = rowsOfGang
        //            .Where(x => !string.IsNullOrWhiteSpace(x.MaThungThep))
        //            .Select(x => x.MaThungThep!)
        //            .Distinct(StringComparer.OrdinalIgnoreCase)
        //            .ToList();

        //        if (steelListOfThisGang.Count == 0) return true;

        //        // ===== 2) Tìm nhóm gộp theo MaThungThep trong Tbl_BM_16_ChiaGang =====
        //        // Mỗi steel có thể thuộc một MaChiaGang (gộp theo thép)
        //        var chiaBySteel = await _context.Tbl_BM_16_ChiaGang
        //            .Where(cg => steelListOfThisGang.Contains(cg.MaThungThep))
        //            .Select(cg => new { cg.MaThungThep, cg.MaChiaGang })
        //            .ToListAsync();

        //        // Tập MaChiaGang liên quan
        //        var maChiaSet = chiaBySteel
        //            .Where(x => !string.IsNullOrWhiteSpace(x.MaChiaGang))
        //            .Select(x => x.MaChiaGang!)
        //            .Distinct(StringComparer.OrdinalIgnoreCase)
        //            .ToList();

        //        // Tập MaThungThep bị ảnh hưởng = steelListOfThisGang ∪ tất cả steel thuộc các MaChiaGang liên quan
        //        var affectedSteels = new HashSet<string>(steelListOfThisGang, StringComparer.OrdinalIgnoreCase);

        //        if (maChiaSet.Count > 0)
        //        {
        //            var steelsFromGroups = await _context.Tbl_BM_16_ChiaGang
        //                .Where(cg => maChiaSet.Contains(cg.MaChiaGang))
        //                .Select(cg => cg.MaThungThep)
        //                .Distinct()
        //                .ToListAsync();

        //            foreach (var s in steelsFromGroups.Where(s => !string.IsNullOrWhiteSpace(s)))
        //                affectedSteels.Add(s!);
        //        }

        //        // ===== 3) Lấy toàn bộ record GangLong cho các MaThungThep bị ảnh hưởng (kể cả steel kéo từ nhóm khác) =====
        //        var allRows = await _context.Tbl_BM_16_GangLong
        //            .Where(g => g.MaThungThep != null && affectedSteels.Contains(g.MaThungThep))
        //            .ToListAsync();
        //        if (allRows.Count == 0) return true;

        //        // Gom theo MaThungThep
        //        var rowsBySteel = allRows
        //            .GroupBy(g => g.MaThungThep!, StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        //        // ===== 4) Xác định nhóm gộp theo steel và đích đúc cho từng steel =====
        //        // Map: steel -> MaChiaGang (nếu có)
        //        var steelToGroup = await _context.Tbl_BM_16_ChiaGang
        //            .Where(cg => affectedSteels.Contains(cg.MaThungThep))
        //            .GroupBy(cg => cg.MaThungThep)
        //            .Select(g => new { MaThungThep = g.Key!, MaChiaGang = g.Max(x => x.MaChiaGang) })
        //            .ToListAsync();

        //        var steelGroupMap = steelToGroup
        //            .Where(x => !string.IsNullOrWhiteSpace(x.MaChiaGang))
        //            .ToDictionary(x => x.MaThungThep, x => x.MaChiaGang!, StringComparer.OrdinalIgnoreCase);

        //        // Map: GroupCode -> danh sách steels trong nhóm
        //        var groupToSteels = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        //        foreach (var kv in steelGroupMap)
        //        {
        //            var steel = kv.Key;
        //            var grp = kv.Value;
        //            if (!groupToSteels.TryGetValue(grp, out var set))
        //            {
        //                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        //                groupToSteels[grp] = set;
        //            }
        //            set.Add(steel);
        //        }

        //        // Xác định steel nào "đi đúc"
        //        var steelIsDuc = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        //        foreach (var steel in affectedSteels)
        //        {
        //            // Lấy ChuyenDen đại diện của steel (ổn định theo steel)
        //            var cd = allRows.Where(r => string.Equals(r.MaThungThep, steel, StringComparison.OrdinalIgnoreCase))
        //                            .Select(r => r.ChuyenDen)
        //                            .FirstOrDefault();
        //            steelIsDuc[steel] = IsDuc(cd);
        //        }

        //        // ===== 5) Tính KL_CR (tầng 1) cho TỪNG RECORD theo luật mới =====
        //        var klCrOfRow = new Dictionary<int, decimal>(allRows.Count);
        //        var invalidRowIds = new HashSet<int>();

        //        // Helper: chia bình thường cho 1 steel (không gộp hoặc all-non-đúc)
        //        void DistributeNormalForSteel(string steel)
        //        {
        //            var list = rowsBySteel[steel];
        //            var thungGoc = list.Where(t => t.T_copy != true).OrderBy(t => t.ID).FirstOrDefault();
        //            if (thungGoc == null || (thungGoc.G_KLGangLong ?? 0m) <= 0m)
        //            {
        //                foreach (var t in list) klCrOfRow[t.ID] = 0m;
        //                return;
        //            }

        //            decimal gKl = thungGoc.G_KLGangLong!.Value;

        //            decimal? BaseOf(Tbl_BM_16_GangLong t)
        //            {
        //                var v = t.KLGangChia ?? t.T_KLGangLong;
        //                return (v.HasValue && v.Value > 0m) ? v : null;
        //            }

        //            var baseMap = list.ToDictionary(t => t.ID, BaseOf);
        //            var sumBase = baseMap.Values.Where(v => v.HasValue).Sum(v => v!.Value);
        //            if (sumBase <= 0m)
        //            {
        //                foreach (var t in list) klCrOfRow[t.ID] = 0m;
        //                return;
        //            }

        //            foreach (var t in list)
        //            {
        //                var b = baseMap[t.ID];
        //                klCrOfRow[t.ID] = b.HasValue ? R2(gKl * (b.Value / sumBase)) : 0m;
        //            }
        //        }

        //        // Xử lý theo từng nhóm gộp steel; các steel không thuộc nhóm thì tự xử lý đơn lẻ
        //        var steelsHandled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        //        foreach (var kv in groupToSteels)
        //        {
        //            var grp = kv.Key;
        //            var steels = kv.Value.ToList();

        //            // Phân loại duc/non-duc trong nhóm
        //            var ducSteels = steels.Where(s => steelIsDuc.TryGetValue(s, out var d) && d).ToList();
        //            var nonDucSteels = steels.Where(s => !steelIsDuc.TryGetValue(s, out var d) || !d).ToList();

        //            bool mixed = ducSteels.Count > 0 && nonDucSteels.Count > 0;
        //            bool allDuc = ducSteels.Count > 0 && nonDucSteels.Count == 0;
        //            bool allNonDuc = nonDucSteels.Count > 0 && ducSteels.Count == 0;

        //            if (mixed)
        //            {
        //                // Duc: CR = KLGangChia; Non-duc: CR = 0 & IsSai
        //                foreach (var s in ducSteels)
        //                {
        //                    foreach (var t in rowsBySteel[s])
        //                        klCrOfRow[t.ID] = R2(t.KLGangChia ?? 0m);
        //                }
        //                foreach (var s in nonDucSteels)
        //                {
        //                    foreach (var t in rowsBySteel[s])
        //                    {
        //                        klCrOfRow[t.ID] = 0m;
        //                        invalidRowIds.Add(t.ID);
        //                    }
        //                }
        //            }
        //            else if (allDuc)
        //            {
        //                // All duc: CR = KLGangChia
        //                foreach (var s in steels)
        //                    foreach (var t in rowsBySteel[s])
        //                        klCrOfRow[t.ID] = R2(t.KLGangChia ?? 0m);
        //            }
        //            else // allNonDuc
        //            {
        //                // All non-duc: chia bình thường
        //                foreach (var s in steels)
        //                    DistributeNormalForSteel(s);
        //            }

        //            foreach (var s in steels) steelsHandled.Add(s);
        //        }

        //        // Các steel không thuộc nhóm (không gộp)
        //        var steelsNoGroup = affectedSteels.Where(s => !steelsHandled.Contains(s)).ToList();
        //        foreach (var s in steelsNoGroup)
        //        {
        //            if (steelIsDuc[s])
        //            {
        //                // **Yêu cầu mới**: ĐI ĐÚC nhưng KHÔNG GỘP → CR = T_KLGangLong của chính record
        //                foreach (var t in rowsBySteel[s])
        //                    klCrOfRow[t.ID] = R2(t.T_KLGangLong ?? 0m);
        //            }
        //            else
        //            {
        //                // Không đúc, không gộp → chia bình thường
        //                DistributeNormalForSteel(s);
        //            }
        //        }

        //        // ===== 6) Phân bổ vào TTG (tầng 2) cho toàn bộ record liên quan =====
        //        var ttgIdsAll = allRows.Where(x => x.ID_TTG.HasValue).Select(x => x.ID_TTG!.Value).Distinct().ToList();
        //        if (ttgIdsAll.Count == 0) { await _context.SaveChangesAsync(); return true; }

        //        var ttgBasics = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(ttg => ttgIdsAll.Contains(ttg.ID))
        //            .Select(ttg => new { ttg.ID, ttg.MaThungTG })
        //            .ToListAsync();

        //        var maTTGs = ttgBasics.Select(x => x.MaThungTG).Distinct().ToList();

        //        var ttgFamily = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(ttg => maTTGs.Contains(ttg.MaThungTG))
        //            .Select(ttg => new { ttg.ID, ttg.MaThungTG, ttg.IsCopy, ttg.KLGang_Thoi })
        //            .ToListAsync();

        //        var idTtgToMa = ttgBasics.ToDictionary(x => x.ID, x => x.MaThungTG);
        //        var maToTtgs = ttgFamily.GroupBy(x => x.MaThungTG).ToDictionary(g => g.Key, g => g.ToList());

        //        var rowIdsAll = allRows.Select(x => x.ID).ToList();
        //        var ttgTargetAll = ttgFamily.Select(x => x.ID).ToList();

        //        var exAllocs = await _context.Tbl_BM_16_PhanBoGangCR
        //            .Where(pb => rowIdsAll.Contains(pb.ID_GangLong) && ttgTargetAll.Contains(pb.ID_TTG_Target))
        //            .ToListAsync();
        //        var exDict = exAllocs.ToDictionary(k => (k.ID_GangLong, k.ID_TTG_Target), v => v);
        //        var touched = new HashSet<(int, int)>();

        //        void UpsertAlloc(Tbl_BM_16_GangLong g, int targetId, string maTtg, bool isCopy, decimal tyLe, decimal kl, bool isSai)
        //        {
        //            var key = (g.ID, targetId);
        //            touched.Add(key);

        //            if (exDict.TryGetValue(key, out var row))
        //            {
        //                row.MaThungGang = g.MaThungGang ?? string.Empty;
        //                row.MaThungTG = maTtg;
        //                row.IsTTGCopy = isCopy;
        //                row.TyLeTrongMaTTG = tyLe;
        //                row.KL_PhanBo_CR = kl;
        //                row.IsSaiChuyenDen = isSai;
        //            }
        //            else
        //            {
        //                _context.Tbl_BM_16_PhanBoGangCR.Add(new Tbl_BM_16_PhanBoGangCR
        //                {
        //                    ID_GangLong = g.ID,
        //                    ID_TTG_Target = targetId,
        //                    MaThungGang = g.MaThungGang ?? string.Empty,
        //                    MaThungTG = maTtg,
        //                    IsTTGCopy = isCopy,
        //                    TyLeTrongMaTTG = tyLe,
        //                    KL_PhanBo_CR = kl,
        //                    IsSaiChuyenDen = isSai
        //                });
        //            }
        //        }

        //        foreach (var g in allRows)
        //        {
        //            if (!g.ID_TTG.HasValue) continue;

        //            var isSai = invalidRowIds.Contains(g.ID);
        //            var crOfRow = klCrOfRow.TryGetValue(g.ID, out var v) ? v : 0m;

        //            if (!idTtgToMa.TryGetValue(g.ID_TTG.Value, out var maTTG)) continue;
        //            if (!maToTtgs.TryGetValue(maTTG, out var members) || members.Count == 0) continue;

        //            if (isSai)
        //            {
        //                foreach (var m in members)
        //                    UpsertAlloc(g, m.ID, maTTG, m.IsCopy, 0m, 0m, true);
        //                continue;
        //            }

        //            if (crOfRow <= 0m)
        //            {
        //                // vẫn ghi 0 vào tất cả để hiển thị rõ
        //                foreach (var m in members)
        //                    UpsertAlloc(g, m.ID, maTTG, m.IsCopy, 0m, 0m, false);
        //                continue;
        //            }

        //            if (members.Count == 1)
        //            {
        //                var only = members[0];
        //                UpsertAlloc(g, only.ID, maTTG, only.IsCopy, 1m, R2(crOfRow), false);
        //            }
        //            else
        //            {
        //                var sumKLTG = members.Sum(m => m.KLGang_Thoi ?? 0m);
        //                if (sumKLTG > 0m)
        //                {
        //                    foreach (var m in members)
        //                    {
        //                        var part = m.KLGang_Thoi ?? 0m;
        //                        if (part <= 0m) continue;

        //                        var tyle = R6(part / sumKLTG);
        //                        var crOut = R2(crOfRow * tyle);
        //                        UpsertAlloc(g, m.ID, maTTG, m.IsCopy, tyle, crOut, false);
        //                    }
        //                }
        //                else
        //                {
        //                    var equal = R6(1m / members.Count);
        //                    var each = R2(crOfRow * equal);
        //                    foreach (var m in members)
        //                        UpsertAlloc(g, m.ID, maTTG, m.IsCopy, equal, each, false);
        //                }
        //            }
        //        }

        //        // ===== 7) Zero-out các dòng cũ trong phạm vi các steel bị ảnh hưởng, trừ dòng có cờ sai =====
        //        foreach (var kv in exDict)
        //        {
        //            var key = (kv.Key.Item1, kv.Key.Item2);
        //            if (!touched.Contains(key) &&
        //                rowIdsAll.Contains(key.Item1) &&
        //                ttgTargetAll.Contains(key.Item2) &&
        //                (kv.Value.IsSaiChuyenDen == null || kv.Value.IsSaiChuyenDen == false))
        //            {
        //                kv.Value.TyLeTrongMaTTG = 0m;
        //                kv.Value.KL_PhanBo_CR = 0m;
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[ERROR] TuDongTinhToanChiaGangCanRayAsync({maThungGang}): {ex}");
        //        return false;
        //    }
        //}
        //public async Task<bool> TuDongTinhToanChiaGangCanRayAsync(string maThungGang)
        //{
        //    if (string.IsNullOrWhiteSpace(maThungGang)) return true;

        //    try
        //    {
        //        // ===== Helper =====
        //        static bool IsDuc(string? chuyenDen)
        //        {
        //            if (string.IsNullOrWhiteSpace(chuyenDen)) return false;
        //            var v = chuyenDen.Trim();
        //            return v.Equals("DUC1", StringComparison.OrdinalIgnoreCase) ||
        //                   v.Equals("DUC2", StringComparison.OrdinalIgnoreCase);
        //        }

        //        static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
        //        static decimal R6(decimal v) => Math.Round(v, 6, MidpointRounding.AwayFromZero);

        //        // ===== 1) Lấy toàn bộ record của MaThungGang gọi hàm =====
        //        var rowsOfGang = await _context.Tbl_BM_16_GangLong
        //            .Where(x => x.MaThungGang == maThungGang)
        //            .ToListAsync();
        //        if (rowsOfGang.Count == 0) return true;

        //        // Đã được nhận chưa? (theo gang gốc như trước)
        //        var hasBeenReceived = await _context.Tbl_BM_16_TaiKhoan_Thung
        //            .AnyAsync(x => x.MaThungGang == maThungGang);
        //        if (!hasBeenReceived) return true;

        //        // Tập MaThungThep xuất phát từ thùng gang này (có thể nhiều steel ladles)
        //        var steelListOfThisGang = rowsOfGang
        //            .Where(x => !string.IsNullOrWhiteSpace(x.MaThungThep))
        //            .Select(x => x.MaThungThep!)
        //            .Distinct(StringComparer.OrdinalIgnoreCase)
        //            .ToList();

        //        if (steelListOfThisGang.Count == 0) return true;

        //        // ===== 2) Tìm nhóm gộp theo MaThungThep trong Tbl_BM_16_ChiaGang =====
        //        // Mỗi steel có thể thuộc một MaChiaGang (gộp theo thép)
        //        var chiaBySteel = await _context.Tbl_BM_16_ChiaGang
        //            .Where(cg => steelListOfThisGang.Contains(cg.MaThungThep))
        //            .Select(cg => new { cg.MaThungThep, cg.MaChiaGang })
        //            .ToListAsync();

        //        // Tập MaChiaGang liên quan
        //        var maChiaSet = chiaBySteel
        //            .Where(x => !string.IsNullOrWhiteSpace(x.MaChiaGang))
        //            .Select(x => x.MaChiaGang!)
        //            .Distinct(StringComparer.OrdinalIgnoreCase)
        //            .ToList();

        //        // Tập MaThungThep bị ảnh hưởng = steelListOfThisGang ∪ tất cả steel thuộc các MaChiaGang liên quan
        //        var affectedSteels = new HashSet<string>(steelListOfThisGang, StringComparer.OrdinalIgnoreCase);

        //        if (maChiaSet.Count > 0)
        //        {
        //            var steelsFromGroups = await _context.Tbl_BM_16_ChiaGang
        //                .Where(cg => maChiaSet.Contains(cg.MaChiaGang))
        //                .Select(cg => cg.MaThungThep)
        //                .Distinct()
        //                .ToListAsync();

        //            foreach (var s in steelsFromGroups.Where(s => !string.IsNullOrWhiteSpace(s)))
        //                affectedSteels.Add(s!);
        //        }

        //        // ===== 3) Lấy toàn bộ record GangLong cho các MaThungThep bị ảnh hưởng (kể cả steel kéo từ nhóm khác) =====
        //        var allRows = await _context.Tbl_BM_16_GangLong
        //            .Where(g => g.MaThungThep != null && affectedSteels.Contains(g.MaThungThep))
        //            .ToListAsync();
        //        if (allRows.Count == 0) return true;

        //        // Gom theo MaThungThep
        //        var rowsBySteel = allRows
        //            .GroupBy(g => g.MaThungThep!, StringComparer.OrdinalIgnoreCase)
        //            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        //        // ===== 4) Xác định nhóm gộp theo steel và đích đúc cho từng steel =====
        //        // Map: steel -> MaChiaGang (nếu có)
        //        var steelToGroup = await _context.Tbl_BM_16_ChiaGang
        //            .Where(cg => affectedSteels.Contains(cg.MaThungThep))
        //            .GroupBy(cg => cg.MaThungThep)
        //            .Select(g => new { MaThungThep = g.Key!, MaChiaGang = g.Max(x => x.MaChiaGang) })
        //            .ToListAsync();

        //        var steelGroupMap = steelToGroup
        //            .Where(x => !string.IsNullOrWhiteSpace(x.MaChiaGang))
        //            .ToDictionary(x => x.MaThungThep, x => x.MaChiaGang!, StringComparer.OrdinalIgnoreCase);

        //        // Map: GroupCode -> danh sách steels trong nhóm
        //        var groupToSteels = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        //        foreach (var kv in steelGroupMap)
        //        {
        //            var steel = kv.Key;
        //            var grp = kv.Value;
        //            if (!groupToSteels.TryGetValue(grp, out var set))
        //            {
        //                set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        //                groupToSteels[grp] = set;
        //            }
        //            set.Add(steel);
        //        }

        //        // Xác định steel nào "đi đúc"
        //        var steelIsDuc = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        //        foreach (var steel in affectedSteels)
        //        {
        //            // Lấy ChuyenDen đại diện của steel (ổn định theo steel)
        //            var cd = allRows.Where(r => string.Equals(r.MaThungThep, steel, StringComparison.OrdinalIgnoreCase))
        //                            .Select(r => r.ChuyenDen)
        //                            .FirstOrDefault();
        //            steelIsDuc[steel] = IsDuc(cd);
        //        }

        //        // ===== 5) Tính KL_CR (tầng 1) cho TỪNG RECORD theo luật mới =====
        //        var klCrOfRow = new Dictionary<int, decimal>(allRows.Count);
        //        var invalidRowIds = new HashSet<int>();

        //        // Helper: chia bình thường cho 1 steel (không gộp hoặc all-non-đúc)
        //        void DistributeNormalForSteel(string steel)
        //        {
        //            var list = rowsBySteel[steel];
        //            var thungGoc = list.Where(t => t.T_copy != true).OrderBy(t => t.ID).FirstOrDefault();
        //            if (thungGoc == null || (thungGoc.G_KLGangLong ?? 0m) <= 0m)
        //            {
        //                foreach (var t in list) klCrOfRow[t.ID] = 0m;
        //                return;
        //            }

        //            decimal gKl = thungGoc.G_KLGangLong!.Value;

        //            decimal? BaseOf(Tbl_BM_16_GangLong t)
        //            {
        //                var v = t.KLGangChia ?? t.T_KLGangLong;
        //                return (v.HasValue && v.Value > 0m) ? v : null;
        //            }

        //            var baseMap = list.ToDictionary(t => t.ID, BaseOf);
        //            var sumBase = baseMap.Values.Where(v => v.HasValue).Sum(v => v!.Value);
        //            if (sumBase <= 0m)
        //            {
        //                foreach (var t in list) klCrOfRow[t.ID] = 0m;
        //                return;
        //            }

        //            foreach (var t in list)
        //            {
        //                var b = baseMap[t.ID];
        //                klCrOfRow[t.ID] = b.HasValue ? R2(gKl * (b.Value / sumBase)) : 0m;
        //            }
        //        }

        //        // Xử lý theo từng nhóm gộp steel; các steel không thuộc nhóm thì tự xử lý đơn lẻ
        //        var steelsHandled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        //        foreach (var kv in groupToSteels)
        //        {
        //            var grp = kv.Key;
        //            var steels = kv.Value.ToList();

        //            // Phân loại duc/non-duc trong nhóm
        //            var ducSteels = steels.Where(s => steelIsDuc.TryGetValue(s, out var d) && d).ToList();
        //            var nonDucSteels = steels.Where(s => !steelIsDuc.TryGetValue(s, out var d) || !d).ToList();

        //            bool mixed = ducSteels.Count > 0 && nonDucSteels.Count > 0;
        //            bool allDuc = ducSteels.Count > 0 && nonDucSteels.Count == 0;
        //            bool allNonDuc = nonDucSteels.Count > 0 && ducSteels.Count == 0;

        //            if (mixed)
        //            {
        //                // Duc: CR = KLGangChia; Non-duc: CR = 0 & IsSai
        //                foreach (var s in ducSteels)
        //                {
        //                    foreach (var t in rowsBySteel[s])
        //                        klCrOfRow[t.ID] = R2(t.KLGangChia ?? 0m);
        //                }
        //                foreach (var s in nonDucSteels)
        //                {
        //                    foreach (var t in rowsBySteel[s])
        //                    {
        //                        klCrOfRow[t.ID] = 0m;
        //                        invalidRowIds.Add(t.ID);
        //                    }
        //                }
        //            }
        //            else if (allDuc)
        //            {
        //                // All duc: CR = KLGangChia
        //                foreach (var s in steels)
        //                    foreach (var t in rowsBySteel[s])
        //                        klCrOfRow[t.ID] = R2(t.KLGangChia ?? 0m);
        //            }
        //            else // allNonDuc
        //            {
        //                // All non-duc: chia bình thường
        //                foreach (var s in steels)
        //                    DistributeNormalForSteel(s);
        //            }

        //            foreach (var s in steels) steelsHandled.Add(s);
        //        }

        //        // Các steel không thuộc nhóm (không gộp)
        //        var steelsNoGroup = affectedSteels.Where(s => !steelsHandled.Contains(s)).ToList();
        //        foreach (var s in steelsNoGroup)
        //        {
        //            if (steelIsDuc[s])
        //            {
        //                // **Yêu cầu mới**: ĐI ĐÚC nhưng KHÔNG GỘP → CR = T_KLGangLong của chính record
        //                foreach (var t in rowsBySteel[s])
        //                    klCrOfRow[t.ID] = R2(t.T_KLGangLong ?? 0m);
        //            }
        //            else
        //            {
        //                // Không đúc, không gộp → chia bình thường
        //                DistributeNormalForSteel(s);
        //            }
        //        }

        //        // ===== 6) Phân bổ vào TTG (tầng 2) cho toàn bộ record liên quan =====
        //        var ttgIdsAll = allRows.Where(x => x.ID_TTG.HasValue).Select(x => x.ID_TTG!.Value).Distinct().ToList();
        //        if (ttgIdsAll.Count == 0) { await _context.SaveChangesAsync(); return true; }

        //        var ttgBasics = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(ttg => ttgIdsAll.Contains(ttg.ID))
        //            .Select(ttg => new { ttg.ID, ttg.MaThungTG })
        //            .ToListAsync();

        //        var maTTGs = ttgBasics.Select(x => x.MaThungTG).Distinct().ToList();

        //        var ttgFamily = await _context.Tbl_BM_16_ThungTrungGian
        //            .Where(ttg => maTTGs.Contains(ttg.MaThungTG))
        //            .Select(ttg => new { ttg.ID, ttg.MaThungTG, ttg.IsCopy, ttg.KLGang_Thoi })
        //            .ToListAsync();

        //        var idTtgToMa = ttgBasics.ToDictionary(x => x.ID, x => x.MaThungTG);
        //        var maToTtgs = ttgFamily.GroupBy(x => x.MaThungTG).ToDictionary(g => g.Key, g => g.ToList());

        //        var rowIdsAll = allRows.Select(x => x.ID).ToList();
        //        var ttgTargetAll = ttgFamily.Select(x => x.ID).ToList();

        //        var exAllocs = await _context.Tbl_BM_16_PhanBoGangCR
        //            .Where(pb => rowIdsAll.Contains(pb.ID_GangLong) && ttgTargetAll.Contains(pb.ID_TTG_Target))
        //            .ToListAsync();
        //        var exDict = exAllocs.ToDictionary(k => (k.ID_GangLong, k.ID_TTG_Target), v => v);
        //        var touched = new HashSet<(int, int)>();

        //        void UpsertAlloc(Tbl_BM_16_GangLong g, int targetId, string maTtg, bool isCopy, decimal tyLe, decimal kl, bool isSai)
        //        {
        //            var key = (g.ID, targetId);
        //            touched.Add(key);

        //            if (exDict.TryGetValue(key, out var row))
        //            {
        //                row.MaThungGang = g.MaThungGang ?? string.Empty;
        //                row.MaThungTG = maTtg;
        //                row.IsTTGCopy = isCopy;
        //                row.TyLeTrongMaTTG = tyLe;
        //                row.KL_PhanBo_CR = kl;
        //                row.IsSaiChuyenDen = isSai;
        //            }
        //            else
        //            {
        //                _context.Tbl_BM_16_PhanBoGangCR.Add(new Tbl_BM_16_PhanBoGangCR
        //                {
        //                    ID_GangLong = g.ID,
        //                    ID_TTG_Target = targetId,
        //                    MaThungGang = g.MaThungGang ?? string.Empty,
        //                    MaThungTG = maTtg,
        //                    IsTTGCopy = isCopy,
        //                    TyLeTrongMaTTG = tyLe,
        //                    KL_PhanBo_CR = kl,
        //                    IsSaiChuyenDen = isSai
        //                });
        //            }
        //        }

        //        foreach (var g in allRows)
        //        {
        //            if (!g.ID_TTG.HasValue) continue;

        //            var isSai = invalidRowIds.Contains(g.ID);
        //            var crOfRow = klCrOfRow.TryGetValue(g.ID, out var v) ? v : 0m;

        //            if (!idTtgToMa.TryGetValue(g.ID_TTG.Value, out var maTTG)) continue;
        //            if (!maToTtgs.TryGetValue(maTTG, out var members) || members.Count == 0) continue;

        //            if (isSai)
        //            {
        //                foreach (var m in members)
        //                    UpsertAlloc(g, m.ID, maTTG, m.IsCopy, 0m, 0m, true);
        //                continue;
        //            }

        //            if (crOfRow <= 0m)
        //            {
        //                // vẫn ghi 0 vào tất cả để hiển thị rõ
        //                foreach (var m in members)
        //                    UpsertAlloc(g, m.ID, maTTG, m.IsCopy, 0m, 0m, false);
        //                continue;
        //            }

        //            if (members.Count == 1)
        //            {
        //                var only = members[0];
        //                UpsertAlloc(g, only.ID, maTTG, only.IsCopy, 1m, R2(crOfRow), false);
        //            }
        //            else
        //            {
        //                var sumKLTG = members.Sum(m => m.KLGang_Thoi ?? 0m);
        //                if (sumKLTG > 0m)
        //                {
        //                    foreach (var m in members)
        //                    {
        //                        var part = m.KLGang_Thoi ?? 0m;
        //                        if (part <= 0m) continue;

        //                        var tyle = R6(part / sumKLTG);
        //                        var crOut = R2(crOfRow * tyle);
        //                        UpsertAlloc(g, m.ID, maTTG, m.IsCopy, tyle, crOut, false);
        //                    }
        //                }
        //                else
        //                {
        //                    var equal = R6(1m / members.Count);
        //                    var each = R2(crOfRow * equal);
        //                    foreach (var m in members)
        //                        UpsertAlloc(g, m.ID, maTTG, m.IsCopy, equal, each, false);
        //                }
        //            }
        //        }

        //        // ===== 7) Zero-out các dòng cũ trong phạm vi các steel bị ảnh hưởng, trừ dòng có cờ sai =====
        //        foreach (var kv in exDict)
        //        {
        //            var key = (kv.Key.Item1, kv.Key.Item2);
        //            if (!touched.Contains(key) &&
        //                rowIdsAll.Contains(key.Item1) &&
        //                ttgTargetAll.Contains(key.Item2) &&
        //                (kv.Value.IsSaiChuyenDen == null || kv.Value.IsSaiChuyenDen == false))
        //            {
        //                kv.Value.TyLeTrongMaTTG = 0m;
        //                kv.Value.KL_PhanBo_CR = 0m;
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[ERROR] TuDongTinhToanChiaGangCanRayAsync({maThungGang}): {ex}");
        //        return false;
        //    }
        //}



        public async Task<ChiTietChiaGangResponse?> GetDetailChiaGangCRAsync(DetailChiaGangCRDto input)
        {
            if (input == null || input.idThung <= 0) return null;

            try
            {
                var current = await _context.Tbl_BM_16_GangLong
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ID == input.idThung);
                if (current == null || string.IsNullOrWhiteSpace(current.MaThungGang)) return null;

                var maThungGang = current.MaThungGang;

                // 1) Lấy toàn bộ thùng gang cùng mã
                var gangList = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.MaThungGang == maThungGang)
                    .Select(g => new ThungGangDetailDto
                    {
                        Id = g.ID,
                        MaThungGang = g.MaThungGang ?? string.Empty,
                        MaThungThep = g.MaThungThep ?? string.Empty,
                        IsCopy = g.T_copy == true,
                        GKLGangLong = g.G_KLGangLong,
                        TKLGangLong = g.T_KLGangLong,
                        KLGangChia = g.KLGangChia,
                        IdTTG = g.ID_TTG
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!gangList.Any()) return null;

                var gangDict = gangList.ToDictionary(x => x.Id, x => x);
                var gangIds = gangList.Select(x => x.Id).ToList();

                // 2) Phân bổ CR
                var allocs = await _context.Tbl_BM_16_PhanBoGangCR
                    .Where(pb => gangIds.Contains(pb.ID_GangLong))
                    .Select(pb => new
                    {
                        pb.ID_GangLong,
                        pb.ID_TTG_Target,
                        pb.MaThungTG,
                        pb.IsTTGCopy,
                        pb.TyLeTrongMaTTG,
                        pb.KL_PhanBo_CR
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!allocs.Any())
                {
                    return new ChiTietChiaGangResponse
                    {
                        MaThungGang = maThungGang,
                        listThung = new()
                    };
                }

                // 3) TTG gốc + copy
                var ttgIds = allocs.Select(a => a.ID_TTG_Target).ToList();
                var ttgs = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(t => ttgIds.Contains(t.ID))
                    .Select(t => new
                    {
                        t.ID,
                        t.MaThungTG,
                        t.IsCopy,
                        t.KLGang_Thoi
                    })
                    .AsNoTracking()
                    .ToListAsync();
                var ttgDict = ttgs.ToDictionary(x => x.ID, x => x);

                // 4) Map kết quả
                var result = new List<TtgBasicDto>();
                foreach (var a in allocs)
                {
                    if (!gangDict.TryGetValue(a.ID_GangLong, out var gang)) continue;
                    if (!ttgDict.TryGetValue(a.ID_TTG_Target, out var ttg)) continue;

                    // ✅ Tính lại tỷ lệ dựa theo G_KLGangLong
                    decimal? newTyLe = null;
                    if (gang.GKLGangLong.HasValue && gang.GKLGangLong.Value > 0 && a.KL_PhanBo_CR.HasValue)
                        newTyLe = a.KL_PhanBo_CR.Value / gang.GKLGangLong.Value ;

                    result.Add(new TtgBasicDto
                    {
                        Id = ttg.ID,
                        MaThungTG = ttg.MaThungTG,
                        IsCopy = ttg.IsCopy,
                        KLGangThoi = ttg.KLGang_Thoi,
                        thungGang = gang,
                        PhanBo = new PhanBoRowDto
                        {
                            IdTtg = a.ID_TTG_Target,
                            MaThungTG = a.MaThungTG,
                            IsTTGCopy = a.IsTTGCopy,
                            TyLeTrongMaTTG = newTyLe,
                            KLPhanBoCR = a.KL_PhanBo_CR
                        }
                    });
                }

                return new ChiTietChiaGangResponse
                {
                    MaThungGang = maThungGang,
                    listThung = result
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetChiTietChiaGangAsync: {ex}");
                return null;
            }
        }



    }
}
