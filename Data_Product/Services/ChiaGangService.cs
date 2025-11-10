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
                .Where(x => x.MaThungThep.Contains(maThungThep))
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


  
        public async Task<bool> TuDongTinhToanChiaGangCanRayAsync(string maThungGang)
        {
            if (string.IsNullOrWhiteSpace(maThungGang)) return true;

            try
            {
                static bool IsDuc(string? x)
                {
                    if (string.IsNullOrWhiteSpace(x)) return false;
                    var v = x.Trim();
                    return v.Equals("DUC1", StringComparison.OrdinalIgnoreCase) || v.Equals("DUC2", StringComparison.OrdinalIgnoreCase);
                }
                static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
                static decimal R6(decimal v) => Math.Round(v, 6, MidpointRounding.AwayFromZero);

                // 1) Tải record của MaThungGang gọi hàm
                var rowsOfGang = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.MaThungGang == maThungGang)
                    .ToListAsync();
                if (rowsOfGang.Count == 0) return true;

                // Chỉ chạy khi đã “nhận”
                var hasBeenReceived = await _context.Tbl_BM_16_TaiKhoan_Thung
                    .AnyAsync(x => x.MaThungGang == maThungGang);
                if (!hasBeenReceived) return true;

                var gangDest = rowsOfGang.Where(t => t.T_copy != true).Select(t => t.ChuyenDen).FirstOrDefault()
                              ?? rowsOfGang.Select(t => t.ChuyenDen).FirstOrDefault();
                bool gangIsDuc = IsDuc(gangDest);

                var steelsOfGang = rowsOfGang.Where(r => !string.IsNullOrWhiteSpace(r.MaThungThep))
                                             .Select(r => r.MaThungThep!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (steelsOfGang.Count == 0) return true;

                var steelHasKLGangChia = rowsOfGang.Where(r => !string.IsNullOrWhiteSpace(r.MaThungThep) && (r.KLGangChia ?? 0m) > 0m)
                                                   .Select(r => r.MaThungThep!).Distinct(StringComparer.OrdinalIgnoreCase)
                                                   .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // 2) Map MaThungThep -> MaChiaGang (dòng mới nhất)
                var latestSteelToGroup = await _context.Tbl_BM_16_ChiaGang
                    .Where(cg => steelsOfGang.Contains(cg.MaThungThep))
                    .GroupBy(cg => cg.MaThungThep)
                    .Select(g => new { MaThungThep = g.Key!, MaChiaGang = g.OrderByDescending(x => x.ID).Select(x => x.MaChiaGang).FirstOrDefault() })
                    .ToListAsync();

                var steelToGroup = latestSteelToGroup.Where(x => !string.IsNullOrWhiteSpace(x.MaChiaGang))
                                                     .ToDictionary(x => x.MaThungThep, x => x.MaChiaGang!, StringComparer.OrdinalIgnoreCase);

                // Lấy tất cả thép trong các nhóm đã tìm
                var groupCodes = steelToGroup.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                var allSteelsInGroups = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                if (groupCodes.Count > 0)
                {
                    var pairs = await _context.Tbl_BM_16_ChiaGang
                        .Where(cg => groupCodes.Contains(cg.MaChiaGang))
                        .Select(cg => new { cg.MaChiaGang, cg.MaThungThep })
                        .ToListAsync();

                    foreach (var p in pairs)
                    {
                        if (string.IsNullOrWhiteSpace(p.MaThungThep)) continue;
                        if (!allSteelsInGroups.TryGetValue(p.MaChiaGang, out var set))
                            allSteelsInGroups[p.MaChiaGang] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        set.Add(p.MaThungThep!);
                    }
                }

                // 3) MỞ RỘNG PHẠM VI: steels của gang gọi + steels cùng MaChiaGang + anh em cùng MaThungGang
                var affectedSteels = new HashSet<string>(steelsOfGang, StringComparer.OrdinalIgnoreCase);
                foreach (var set in allSteelsInGroups.Values) foreach (var s in set) affectedSteels.Add(s);

                var steelGangPairs = await _context.Tbl_BM_16_GangLong
                    .Where(r => r.MaThungThep != null && affectedSteels.Contains(r.MaThungThep))
                    .Select(r => new { r.MaThungThep, r.MaThungGang })
                    .Distinct()
                    .ToListAsync();

                var gangsToPull = steelGangPairs.Where(p => !string.IsNullOrWhiteSpace(p.MaThungGang))
                                                .Select(p => p.MaThungGang!)
                                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                                .ToList();

                var siblingSteels = await _context.Tbl_BM_16_GangLong
                    .Where(r => r.MaThungThep != null && r.MaThungGang != null && gangsToPull.Contains(r.MaThungGang))
                    .Select(r => r.MaThungThep!)
                    .Distinct()
                    .ToListAsync();
                foreach (var s in siblingSteels) affectedSteels.Add(s);

                // 4) Tải toàn bộ record của affectedSteels
                var allRows = await _context.Tbl_BM_16_GangLong
                    .Where(r => r.MaThungThep != null && affectedSteels.Contains(r.MaThungThep))
                    .ToListAsync();

                var rowsBySteel = allRows.GroupBy(r => r.MaThungThep!, StringComparer.OrdinalIgnoreCase)
                                         .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
                var rowsByGang = allRows.Where(r => !string.IsNullOrWhiteSpace(r.MaThungGang))
                                        .GroupBy(r => r.MaThungGang!, StringComparer.OrdinalIgnoreCase)
                                        .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                string? CanonDestForSteel(string steel)
                {
                    if (!rowsBySteel.TryGetValue(steel, out var list) || list.Count == 0) return null;
                    return list.Where(r => !string.IsNullOrWhiteSpace(r.ChuyenDen))
                               .GroupBy(r => r.ChuyenDen!.Trim(), StringComparer.OrdinalIgnoreCase)
                               .OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
                }

                var steelIsDuc = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in affectedSteels) steelIsDuc[s] = IsDuc(CanonDestForSteel(s));

                // ===== Chống gán lặp + idempotent =====
                var klCrByRow = new Dictionary<int, decimal>(allRows.Count);
                var saiRowIds = new HashSet<int>();
                var assignedRowIds = new HashSet<int>();
                var assignedSteels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var handledSteels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                void MarkAssigned(Tbl_BM_16_GangLong row)
                {
                    assignedRowIds.Add(row.ID);
                    if (!string.IsNullOrWhiteSpace(row.MaThungThep))
                        assignedSteels.Add(row.MaThungThep);
                }

                void SetCrIfNotAssigned(Tbl_BM_16_GangLong row, decimal cr)
                {
                    if (!klCrByRow.ContainsKey(row.ID))
                    {
                        klCrByRow[row.ID] = R2(cr);
                        MarkAssigned(row);
                    }
                }

                void DistributeByGangForSteels(string gangCode, HashSet<string>? filterSteels, bool markHandledSteels = false)
                {
                    if (!rowsByGang.TryGetValue(gangCode, out var list) || list.Count == 0) return;

                    var thungGoc = list.Where(t => t.T_copy != true).OrderBy(t => t.ID).FirstOrDefault();
                    if (thungGoc == null || (thungGoc.G_KLGangLong ?? 0m) <= 0m)
                    {
                        foreach (var t in list)
                        {
                            if (filterSteels != null && !filterSteels.Contains(t.MaThungThep!)) continue;
                            SetCrIfNotAssigned(t, 0m);
                        }
                        if (markHandledSteels && filterSteels != null)
                            foreach (var s in filterSteels) handledSteels.Add(s);
                        return;
                    }

                    var gKl = thungGoc.G_KLGangLong!.Value;

                    decimal? BaseOf(Tbl_BM_16_GangLong t)
                    {
                        var v = t.KLGangChia ?? t.T_KLGangLong;
                        return (v.HasValue && v.Value > 0m) ? v : null;
                    }
                    var baseMap = list.ToDictionary(t => t.ID, BaseOf);
                    var sumBase = baseMap.Values.Where(v => v.HasValue).Sum(v => v!.Value);

                    if (sumBase <= 0m)
                    {
                        var steelsToDistribute = filterSteels ?? list.Select(x => x.MaThungThep!)
                                                                     .Distinct(StringComparer.OrdinalIgnoreCase)
                                                                     .ToHashSet(StringComparer.OrdinalIgnoreCase);
                        var rowsToDistribute = list.Where(x => steelsToDistribute.Contains(x.MaThungThep!)).ToList();
                        if (rowsToDistribute.Count > 0)
                        {
                            var equal = R6(1m / rowsToDistribute.Count);
                            foreach (var t in rowsToDistribute) SetCrIfNotAssigned(t, gKl * equal);
                        }
                        if (markHandledSteels)
                            foreach (var s in steelsToDistribute) handledSteels.Add(s);
                        return;
                    }

                    foreach (var t in list)
                    {
                        if (filterSteels != null && !filterSteels.Contains(t.MaThungThep!)) continue;
                        if (klCrByRow.ContainsKey(t.ID)) continue; // không ghi đè
                        var b = baseMap[t.ID];
                        SetCrIfNotAssigned(t, b.HasValue ? gKl * (b.Value / sumBase) : 0m);
                    }

                    if (markHandledSteels && filterSteels != null)
                        foreach (var s in filterSteels) handledSteels.Add(s);
                }

                // ===== 5) XỬ LÝ NHÓM GỘP (MaChiaGang)
                foreach (var (grp, steels) in allSteelsInGroups)
                {
                    var steelsInAnyGang = steels.Where(s => rowsBySteel.ContainsKey(s)).ToList();
                    if (steelsInAnyGang.Count == 0) { foreach (var s in steels) handledSteels.Add(s); continue; }

                    var destSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    bool anyDuc = false;
                    foreach (var s in steels)
                    {
                        var d = CanonDestForSteel(s);
                        if (!string.IsNullOrWhiteSpace(d)) destSet.Add(d!);
                        if (IsDuc(d)) anyDuc = true;
                    }
                    bool sameDest = destSet.Count <= 1;
                    bool sameDestIsDuc = sameDest && anyDuc;
                    bool sameDestIsNonDuc = sameDest && !anyDuc;

                    if (sameDestIsDuc)
                    {
                        // All ĐÚC: KL = (KLGangChia>0?KLGangChia:T_KLGangLong)
                        foreach (var s in steelsInAnyGang)
                        {
                            foreach (var row in rowsBySteel[s])
                            {
                                var val = (row.KLGangChia.HasValue && row.KLGangChia.Value > 0m)
                                    ? row.KLGangChia.Value
                                    : (row.T_KLGangLong ?? 0m);
                                SetCrIfNotAssigned(row, val);
                            }
                            handledSteels.Add(s);
                        }
                    }
                    else if (sameDestIsNonDuc)
                    {
                        // All KHÔNG ĐÚC: chia theo từng gang liên quan (đánh dấu handled để không xét lại)
                        var gangsInGroup = steelsInAnyGang.Select(s => rowsBySteel[s].First().MaThungGang)
                                                          .Where(g => !string.IsNullOrWhiteSpace(g))
                                                          .Distinct(StringComparer.OrdinalIgnoreCase)
                                                          .ToList();
                        foreach (var gcode in gangsInGroup)
                        {
                            var gangSteels = rowsByGang[gcode!].Select(x => x.MaThungThep!)
                                                               .Distinct(StringComparer.OrdinalIgnoreCase)
                                                               .ToHashSet(StringComparer.OrdinalIgnoreCase);
                            DistributeByGangForSteels(gcode!, gangSteels, markHandledSteels: true);
                        }
                    }
                    else
                    {
                        // Mixed: ĐÚC -> base theo KLGangChia/T_KLGangLong; KHÔNG ĐÚC -> chia theo gang của chính nó + cờ sai cho chính nó
                        foreach (var s in steelsInAnyGang)
                        {
                            if (steelIsDuc[s])
                            {
                                foreach (var row in rowsBySteel[s])
                                {
                                    var val = (row.KLGangChia.HasValue && row.KLGangChia.Value > 0m)
                                        ? row.KLGangChia.Value
                                        : (row.T_KLGangLong ?? 0m);
                                    SetCrIfNotAssigned(row, val);
                                }
                                handledSteels.Add(s);
                            }
                        }

                        var nonDucSteels = steelsInAnyGang.Where(s => !steelIsDuc[s]).ToList();
                        foreach (var s in nonDucSteels)
                        {
                            var gcode = rowsBySteel[s].First().MaThungGang!;
                            var gangSteels = rowsByGang[gcode].Select(x => x.MaThungThep!)
                                                               .Distinct(StringComparer.OrdinalIgnoreCase)
                                                               .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            DistributeByGangForSteels(gcode, gangSteels, markHandledSteels: true);
                            foreach (var row in rowsBySteel[s]) saiRowIds.Add(row.ID);
                        }
                    }

                    foreach (var s in steels) handledSteels.Add(s);
                }

                // ===== 6) THÉP KHÔNG GỘP (tránh steels đã assigned/handled)
                var steelsNoGroup = affectedSteels
                    .Where(s => rowsBySteel.ContainsKey(s)
                             && !handledSteels.Contains(s)
                             && !assignedSteels.Contains(s))
                    .ToList();

                if (steelsNoGroup.Count > 0)
                {
                    // Gom theo gang để chia đúng logic từng gang
                    var byGang = steelsNoGroup
                        .GroupBy(s => rowsBySteel[s].First().MaThungGang!, StringComparer.OrdinalIgnoreCase);

                    foreach (var grp in byGang)
                    {
                        var gcode = grp.Key;
                        var steelsInGang = grp.Distinct(StringComparer.OrdinalIgnoreCase)
                                              .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        // 6.a) Thép ĐÚC: set theo KLGangChia/T_KLGangLong từng dòng
                        foreach (var s in steelsInGang.Where(s => steelIsDuc.TryGetValue(s, out var b) && b))
                        {
                            foreach (var row in rowsBySteel[s])
                            {
                                var val = (row.KLGangChia.HasValue && row.KLGangChia.Value > 0m)
                                    ? row.KLGangChia.Value
                                    : (row.T_KLGangLong ?? 0m);
                                SetCrIfNotAssigned(row, val);
                            }
                            handledSteels.Add(s);
                        }

                        // 6.b) Thép không ĐÚC: phân bổ theo gang (dựa trên tổng base toàn gang),
                        // chỉ ghi cho các steels còn lại trong gang
                        var nonDucSet = steelsInGang
                            .Where(s => !steelIsDuc.GetValueOrDefault(s))
                            .ToHashSet(StringComparer.OrdinalIgnoreCase);

                        if (nonDucSet.Count > 0)
                        {
                            DistributeByGangForSteels(gcode, nonDucSet, markHandledSteels: true);
                        }
                    }
                }

                // ===== 7) IDP: Reset các bản ghi phân bổ cũ cho TẬP DỮ LIỆU SẼ GHI (idempotent) =====
                var rowIdsAll = allRows.Select(x => x.ID).ToList();
                if (rowIdsAll.Count > 0)
                {
                    var oldAllocs = await _context.Tbl_BM_16_PhanBoGangCR
                        .Where(pb => rowIdsAll.Contains(pb.ID_GangLong))
                        .ToListAsync();

                    foreach (var r in oldAllocs)
                    {
                        r.KL_PhanBo_CR = 0m;
                        r.TyLeTrongMaTTG = 0m;
                        r.IsSaiChuyenDen = false;
                    }

                    await _context.SaveChangesAsync();
                }

                // ===== 8) Lấy TTG, chuẩn bị upsert
                var ttgIdsAll = allRows.Where(x => x.ID_TTG.HasValue).Select(x => x.ID_TTG!.Value).Distinct().ToList();
                if (ttgIdsAll.Count == 0) { await _context.SaveChangesAsync(); return true; }

                var ttgBasics = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(ttg => ttgIdsAll.Contains(ttg.ID))
                    .Select(ttg => new { ttg.ID, ttg.MaThungTG })
                    .ToListAsync();

                var maTTGs = ttgBasics.Select(x => x.MaThungTG).Distinct().ToList();

                var ttgFamily = await _context.Tbl_BM_16_ThungTrungGian
                    .Where(ttg => maTTGs.Contains(ttg.MaThungTG))
                    .Select(ttg => new { ttg.ID, ttg.MaThungTG, ttg.IsCopy, ttg.KLGang_Thoi })
                    .ToListAsync();

                var idTtgToMa = ttgBasics.ToDictionary(x => x.ID, x => x.MaThungTG);
                var maToTtgs = ttgFamily.GroupBy(x => x.MaThungTG).ToDictionary(g => g.Key, g => g.ToList());

                // Lấy (các dòng đã reset) để upsert điền giá trị mới
                var exAllocs = await _context.Tbl_BM_16_PhanBoGangCR
                    .Where(pb => rowIdsAll.Contains(pb.ID_GangLong))
                    .ToListAsync();
                var latestExAllocs = exAllocs
                        .GroupBy(pb => new { pb.ID_GangLong, pb.ID_TTG_Target })
                        .Select(g => g.OrderByDescending(x => x.ID).First())
                        .ToList();
                var exDict = latestExAllocs.ToDictionary(k => (k.ID_GangLong, k.ID_TTG_Target), v => v);
                var touched = new HashSet<(int, int)>();

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

                // 9) Ghi phân bổ TTG
                foreach (var g in allRows)
                {
                    if (!g.ID_TTG.HasValue) continue;

                    var cr = klCrByRow.TryGetValue(g.ID, out var v) ? v : 0m;
                    var isSai = saiRowIds.Contains(g.ID);

                    if (!idTtgToMa.TryGetValue(g.ID_TTG.Value, out var maTTG)) continue;
                    if (!maToTtgs.TryGetValue(maTTG, out var members) || members.Count == 0) continue;

                    if (cr <= 0m)
                    {
                        foreach (var m in members) UpsertAlloc(g, m.ID, maTTG, m.IsCopy, 0m, 0m, isSai);
                    }
                    else if (members.Count == 1)
                    {
                        var only = members[0];
                        UpsertAlloc(g, only.ID, maTTG, only.IsCopy, 1m, R2(cr), isSai);
                    }
                    else
                    {
                        var sumKLTG = members.Sum(m => m.KLGang_Thoi ?? 0m);
                        if (sumKLTG > 0m)
                        {
                            foreach (var m in members)
                            {
                                var part = m.KLGang_Thoi ?? 0m;
                                if (part <= 0m) continue;
                                var tyle = R6(part / sumKLTG);
                                var crOut = R2(cr * tyle);
                                UpsertAlloc(g, m.ID, maTTG, m.IsCopy, tyle, crOut, isSai);
                            }
                        }
                        else
                        {
                            var equal = R6(1m / members.Count);
                            var each = R2(cr * equal);
                            foreach (var m in members) UpsertAlloc(g, m.ID, maTTG, m.IsCopy, equal, each, isSai);
                        }
                    }
                }

                // (Không cần zero-out thêm vì đã reset trước đó; phần nào không "touched" đã = 0)

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] TuDongTinhToanChiaGangCanRayAsync({maThungGang}): {ex}");
                return false;
            }
        }

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
