using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Data_Product.Services
{
    public interface IChiaGangService
    {
        Task<ChiaGangResultModel> TinhToanChiaGangAsync(List<int> IDs);
        Task<int> HuyChiaGangTheoNhieuThungGangAsync(List<string> maThungGangList, int idNguoiChia);
        Task KiemTraVaTinhLaiTheoMaThungGangAsync(string maThungGang);
        Task<ChiaGangResultModel> GetDetailChiaGangAsync(int idThung);
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
                return;

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
                        gangGoc.KLGangChia = thung.KLChia;
                        //gangGoc.T_KLGangLong = thung.KLChia;
                    }
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<ChiaGangResultModel> TinhToanChiaGangAsync(List<int> IDs)
        {
            var danhsachThung = await (from a in _context.Tbl_BM_16_GangLong
                                       where IDs.Contains(a.ID)
                                       select new
                                       {
                                           a.MaThungGang,
                                           a.BKMIS_ThungSo,
                                           a.ID,
                                           a.T_KLGangLong
                                       }).ToListAsync();

            bool hasDuplicateMaThungGang = danhsachThung.GroupBy(x => x.MaThungGang)
                                                        .Any(g => g.Count() > 1);

            if (hasDuplicateMaThungGang)
            {
                throw new Exception("Các mã thùng gang phải khác nhau.");
            }

            bool isAllThungSoSame = danhsachThung.Select(x => x.BKMIS_ThungSo)
                                                .Distinct()
                                                .Count() == 1;

            if (!isAllThungSoSame)
            {
                throw new Exception("Các thùng gang phải có cùng thùng số.");
            }

            if (danhsachThung.All(x => !x.T_KLGangLong.HasValue || x.T_KLGangLong == 0))
            {
                throw new Exception("Không có thùng gang nào có KL Gang lỏng cân bên HRC.");
            }

            var danhSachMaThung = danhsachThung.Select(x => x.MaThungGang).Distinct().ToList();

            var listAll = await (from a in _context.Tbl_BM_16_GangLong
                                 join ttg in _context.Tbl_BM_16_ThungTrungGian on a.ID_TTG equals ttg.ID
                                 where danhSachMaThung.Contains(a.MaThungGang)
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
                                     KL_Phe = ttg.KL_phe,
                                     T_copy = a.T_copy,

                                     TyLeChia = null,
                                     KLChia = null
                                 }).ToListAsync();

            var listGoc = listAll.Where(x => x.T_copy == false).ToList();

            // Nếu có bất kỳ thùng gốc nào thiếu G_KLGangLong thì không chia
            bool coThungGocNullGKL = listGoc.Any(x => !x.G_KLGangLong.HasValue || x.G_KLGangLong == 0);
            if (coThungGocNullGKL == true)
            {
                throw new Exception("KL Gang Lỏng bên Luyện Gang chưa được nhập đầy đủ. Vui lòng kiểm tra lại");
            }

            foreach (var thungGoc in listGoc)
            {
                var tongHRCDaRot = listAll
                    .Where(x => x.MaThungGang == thungGoc.MaThungGang && !IDs.Contains(x.ID))
                    .Where(x => x.T_KLGangLong.HasValue)
                    .Sum(x => x.T_KLGangLong.Value);

                var klLuyenGang = thungGoc.G_KLGangLong ?? 0;

                if (klLuyenGang > 0 && klLuyenGang < tongHRCDaRot)
                {
                    throw new Exception($"Mã thùng {thungGoc.MaThungGang} có KL Gang Lỏng ({klLuyenGang}) nhỏ hơn tổng HRC đã rót ({tongHRCDaRot}). Vui lòng check lại số liệu để chia lại KL Gang Chia");
                }
            }

            var daRongTheoMa = listAll
                .Where(x => x.T_KLGangLong.HasValue && !IDs.Contains(x.ID) && x.KLChia == null)
                .GroupBy(x => x.MaThungGang)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.T_KLGangLong ?? 0)
                );

            var danhSachConLai = listGoc
                .Select(x =>
                {
                    var daRong = daRongTheoMa.TryGetValue(x.MaThungGang, out var val) ? val : 0;
                    var conLai = (x.G_KLGangLong ?? 0) - daRong;
                    return new
                    {
                        MaThungGang = x.MaThungGang,
                        KLConLai = conLai > 0 ? conLai : 0
                    };
                })
                .Where(x => x.KLConLai > 0)
                .ToList();

            var tongConLai = danhSachConLai.Sum(x => x.KLConLai);

            if (tongConLai < 0)
                throw new Exception("Không có đủ khối lượng gang bên Luyện Gang để chia. Vui Lòng kiểm tra lại.");

            var tongT_KLGangLongChon = listAll
                .Where(x => IDs.Contains(x.ID) && x.T_KLGangLong.HasValue && x.T_KLThungVaGang.HasValue && x.T_KLThungChua.HasValue)
                .Sum(x => x.T_KLGangLong.Value);

            foreach (var item in listAll.Where(x => IDs.Contains(x.ID)))
            {
                var nguon = danhSachConLai.FirstOrDefault(x => x.MaThungGang == item.MaThungGang);
                if (nguon != null)
                {
                    item.TyLeChia = Math.Round((nguon.KLConLai / tongConLai) * 100, 2);
                    item.KLChia = Math.Round((item.TyLeChia ?? 0) * tongT_KLGangLongChon / 100, 2);
                }
                else
                {
                    item.TyLeChia = 0;
                    item.KLChia = 0;
                }
            }

            var listSelected = listAll.Where(x => IDs.Contains(x.ID)).ToList();

            return new ChiaGangResultModel
            {
                ThungGoc = listGoc,
                ThungDaCoKL = listAll.Where(x => x.T_KLGangLong.HasValue && !IDs.Contains(x.ID)).ToList(),
                ThungAll = listSelected,
                ListAll = listAll
            };
        }
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

        public async Task<ChiaGangResultModel> GetDetailChiaGangAsync(int idThung)
        {
            var chiaGang = await _context.Tbl_BM_16_ChiaGang
                .Where(x => x.ID_Thung == idThung)
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

    }
}
