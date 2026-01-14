using Data_Product.DTO;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Data_Product.Services
{
    public interface IHRC1LuyenThepService
    {
        Task<List<Tbl_KLGangVaoBOFBase>> LoadDanhSachThungThepHRC1ATAsync(LoadDanhSachThungThepHRC1ATDto dto);
        Task<bool> MocNoiAsync(List<int> idThungGangs, int idThungAT, int IdLoThoi, int Ca, DateTime NgayMocNoi);
        Task HuyMocNoi(List<int> idThungGangs);
    }
    public class HRC1LuyenThepService: IHRC1LuyenThepService
    {
        private readonly DataContext _context;
        public HRC1LuyenThepService(DataContext _context, ICompositeViewEngine viewEngine 
            )
        {
            this._context = _context;
            
        }
        private IQueryable<Tbl_KLGangVaoBOFBase> GetBOFQuery(int idLoThoi)
        {
            return idLoThoi switch
            {
                1 => _context.Tbl_KLGangVaoBOF1.Cast<Tbl_KLGangVaoBOFBase>(),
                2 => _context.Tbl_KLGangVaoBOF2.Cast<Tbl_KLGangVaoBOFBase>(),
                3 => _context.Tbl_KLGangVaoBOF3.Cast<Tbl_KLGangVaoBOFBase>(),
                4 => _context.Tbl_KLGangVaoBOF4.Cast<Tbl_KLGangVaoBOFBase>(),
                5 => _context.Tbl_KLGangVaoBOF5.Cast<Tbl_KLGangVaoBOFBase>(),
                _ => throw new Exception("Lò thổi không hợp lệ")
            };
        }

        public async Task<List<Tbl_KLGangVaoBOFBase>> LoadDanhSachThungThepHRC1ATAsync(LoadDanhSachThungThepHRC1ATDto dto)
        {
            var query = GetBOFQuery(dto.IdLoThoi);

            var startDate = dto.NgaySanXuat.Date;
            var endDate = startDate.AddDays(1);

            var result = await query
                .Where(x =>
                    x.Ca == dto.Ca &&
                    x.NgaySanXuat >= startDate &&
                    x.NgaySanXuat < endDate &&
                    x.MeThoi != null &&
                    x.IsUsed != true
                )
                .GroupBy(x => x.MeThoi)
                .Select(g => g
                    .OrderByDescending(x => x.NgayTao)
                    .FirstOrDefault()
                )
                .ToListAsync();

            return result;
        }

        public async Task<bool> MocNoiAsync(List<int> idThungGangs, int idThungAT, int IdLoThoi, int Ca, DateTime NgayMocNoi)
        {
            var query = GetBOFQuery(IdLoThoi);

            var thungGangAt = await query.Where(x => x.ID == idThungAT).FirstOrDefaultAsync();

            if (thungGangAt == null) return false;

            thungGangAt.IsUsed = true;

            var listThungGang = await _context.Tbl_BM_16_GangLong.Where(x => idThungGangs.Contains(x.ID)).ToListAsync();
            if (listThungGang == null) return false;

            foreach (var thungGang in listThungGang)
            {
                thungGang.T_KLThungVaGang = thungGangAt.KLThungVaGang;
                thungGang.T_KLThungChua = thungGangAt.KLThung;
                thungGang.T_KLGangLong = thungGangAt.KLThungVaGang - thungGangAt.KLThung;

                var thungTG = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.ID == thungGang.ID_TTG).FirstOrDefaultAsync();
                if (thungTG != null)
                {
                    thungTG.KLThungVaGang_Thoi = thungGangAt.KLThungVaGang;
                    thungTG.KLThung_Thoi = thungGangAt.KLThung;
                    thungTG.KLGang_Thoi = thungGangAt.KLThungVaGang - thungGangAt.KLThung;
                }
                var newMocNoi = new Tbl_MocNoiThungGangAT
                {
                    IdLoThoi = IdLoThoi,
                    Ca = Ca,
                    IdThungGangAT = thungGangAt.ID,
                    IdThungGang = thungGang.ID,
                    NgayMocNoi = NgayMocNoi
                };
                _context.Tbl_MocNoiThungGangAT.Add(newMocNoi);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task HuyMocNoi(List<int> idThungGangs)
        {
            var listMocNoi = await _context.Tbl_MocNoiThungGangAT.Where(x => idThungGangs.Contains(x.IdThungGang)).ToListAsync();

            var groupedByLo = listMocNoi
                .GroupBy(x => x.IdLoThoi)
                .Select(g => new
                {
                    IdLoThoi = g.Key,
                    IdThungGangATs = g.Select(x => x.IdThungGangAT).Distinct().ToList()
                })
                .ToList();

            foreach (var group in groupedByLo)
            {
                var query = GetBOFQuery(group.IdLoThoi);

                var thungGangATs = await query
                    .Where(x => group.IdThungGangATs.Contains(x.ID))
                    .ToListAsync();

                if (!thungGangATs.Any())
                    continue;

                // Lấy danh sách còn mốc nối một lần
                var thungATIds = thungGangATs.Select(x => x.ID).ToList();
                var conMocNoiDict = await _context.Tbl_MocNoiThungGangAT
                    .Where(x => thungATIds.Contains(x.IdThungGangAT) &&
                               !idThungGangs.Contains(x.IdThungGang))
                    .Select(x => x.IdThungGangAT)
                    .Distinct()
                    .ToDictionaryAsync(x => x, x => true);

                foreach (var thungAT in thungGangATs)
                {
                    bool conMocNoi = conMocNoiDict.ContainsKey(thungAT.ID);
                    if (!conMocNoi)
                        thungAT.IsUsed = null;
                }

            }
            _context.Tbl_MocNoiThungGangAT.RemoveRange(listMocNoi);
            await _context.SaveChangesAsync();
        }

    }
}
