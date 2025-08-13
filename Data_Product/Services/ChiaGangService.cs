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

                var selectedThungs = listThepDaChia.Select(x => x.ID_Thung).ToList();
                var result = await TinhToanChiaGangAsync(selectedThungs);
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

            var test = await _context.Tbl_BM_16_GangLong
                .Where(x => IDs.Contains(x.ID))
                .Select(x => new
                {
                    x.MaThungGang,
                    x.BKMIS_ThungSo
                })
                .ToListAsync();
            //foreach(var item in maThungTheps)
            //{
            //    var thung = await _context.Tbl_BM_16_GangLong
            //        .Where(x => x.MaThungThep.Trim() == item.Trim())
            //        .Select(x => new
            //        {
            //            x.MaThungGang,
            //            x.BKMIS_ThungSo
            //        }).FirstOrDefaultAsync();
            //}

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
                .Where(x => x.T_KLGangLong.HasValue && !IDs.Contains(x.ID))
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

                if (coThungGocNullGKL || nguon == null || tongConLai <= 0)
                {
                    item.TyLeChia = 0;
                    item.KLChia = 0;
                }
                else
                {
                    item.TyLeChia = Math.Round((nguon.KLConLai / tongConLai) * 100, 2);
                    item.KLChia = Math.Round((item.TyLeChia ?? 0) * tongT_KLGangLongChon / 100, 2);
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
            var listDaCoKL = listAll.Where(x => x.T_KLGangLong.HasValue && !selectedIds.Contains(x.ID)).ToList();

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
            }
            _context.Tbl_BM_16_ChiaGang.RemoveRange(listCanXoa);
            return await _context.SaveChangesAsync(); // Trả về số bản ghi đã xóa
        }

    }
}
