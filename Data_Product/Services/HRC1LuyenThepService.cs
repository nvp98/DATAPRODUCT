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
        Task<bool> UpsertMeGangVaoLoAsync(TaoMoiMeGangVaoLoDto dto);
        Task<bool> ChuyenMeGangAsync(ChuyenMeGangDto dto);
        Task<bool> ThuHoiMeGangAsync(ThuHoiMeDto dto);

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
            //var query = GetBOFQuery(dto.IdLoThoi);

            //var startDate = dto.NgaySanXuat.Date;
            //var endDate = startDate.AddDays(1);

            //var result = await query
            //    .Where(x =>
            //        x.Ca == dto.Ca &&
            //        x.NgaySanXuat >= startDate &&
            //        x.NgaySanXuat < endDate &&
            //        x.MeThoi != null &&
            //        x.IsUsed != true
            //    )
            //    .OrderByDescending(x => x.NgayTao)
            //    .ToListAsync();

            //return result;

            var query = GetBOFQuery(dto.IdLoThoi);

            var startDate = dto.NgaySanXuat.Date;
            var endDate = startDate.AddDays(1);

            var data = await query
                .Where(x =>
                    x.Ca == dto.Ca &&
                    x.NgaySanXuat >= startDate &&
                    x.NgaySanXuat < endDate &&
                    x.MeThoi != null &&
                    x.IsUsed != true
                )
                .Select(x => new
                {
                    Item = x,

                    // 🔑 xác định mẻ cha cho mọi record
                    RootID = x.ParentID ?? x.ID
                })
                .OrderByDescending(x =>
                    query.Where(p => p.ID == x.RootID)
                         .Select(p => p.NgayTao)
                         .FirstOrDefault()
                )
                .ThenBy(x => x.Item.ParentID == null ? 0 : 1)
                .ThenBy(x => x.Item.NgayTao)
                .Select(x => x.Item)
                .ToListAsync();

            return data;

        }

        //public async Task<bool> MocNoiAsync(List<int> idThungGangs, int idThungAT, int IdLoThoi, int Ca, DateTime NgayMocNoi)
        //{
        //    var query = GetBOFQuery(IdLoThoi);

        //    var thungGangAt = await query.Where(x => x.ID == idThungAT).FirstOrDefaultAsync();

        //    if (thungGangAt == null) return false;

        //    thungGangAt.IsUsed = true;

        //    var listThungGang = await _context.Tbl_BM_16_GangLong.Where(x => idThungGangs.Contains(x.ID)).ToListAsync();
        //    if (listThungGang == null) return false;

        //    foreach (var thungGang in listThungGang)
        //    {
        //        thungGang.T_KLThungVaGang = thungGangAt.KLThungVaGang;
        //        thungGang.T_KLThungChua = thungGangAt.KLThung;
        //        thungGang.T_KLGangLong = thungGangAt.KLThungVaGang - thungGangAt.KLThung;

        //        var thungTG = await _context.Tbl_BM_16_ThungTrungGian.Where(x => x.ID == thungGang.ID_TTG).FirstOrDefaultAsync();
        //        if (thungTG != null)
        //        {
        //            var meThoiExist = await _context.Tbl_MeThoi.Where(x => x.MaMeThoi == thungGangAt.MeThoi).FirstOrDefaultAsync();
        //            if (meThoiExist == null)
        //            {
        //                throw new Exception("Không tìm thấy Mẻ thổi trong hệ thống, vui lòng tạo thêm.");
        //            }
        //            thungTG.KLThungVaGang_Thoi = thungGangAt.KLThungVaGang;
        //            thungTG.KLThung_Thoi = thungGangAt.KLThung;
        //            thungTG.KLGang_Thoi = thungGangAt.KLThungVaGang - thungGangAt.KLThung;
        //            thungTG.GioChonMe = thungGangAt.ThoiDiemRot;
        //            thungTG.ID_MeThoi = meThoiExist.ID;
        //        }
        //        var newMocNoi = new Tbl_MocNoiThungGangAT
        //        {
        //            IdLoThoi = IdLoThoi,
        //            Ca = Ca,
        //            IdThungGangAT = thungGangAt.ID,
        //            IdThungGang = thungGang.ID,
        //            NgayMocNoi = NgayMocNoi
        //        };
        //        _context.Tbl_MocNoiThungGangAT.Add(newMocNoi);
        //    }

        //    await _context.SaveChangesAsync();
        //    return true;
        //}
        public async Task<bool> MocNoiAsync(
                List<int> idThungGangs,
                int idThungAT,
                int idLoThoi,
                int ca,
                DateTime ngayMocNoi)
        {
            using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Lấy mẻ gang AT
                var query = GetBOFQuery(idLoThoi);

                var thungGangAt = await query
                    .FirstOrDefaultAsync(x => x.ID == idThungAT);

                if (thungGangAt == null)
                    throw new Exception("Không tìm thấy mẻ gang AT.");

                if (thungGangAt.IsUsed == true)
                    throw new Exception("Mẻ gang AT đã được sử dụng.");

                /* =====================================================
                 * 🔴 CHECK NGHIỆP VỤ TÁCH MẺ – MÓC ĐÚNG THÙNG
                 * ===================================================== */

                // 2️⃣ Xác định mẻ gốc của chuỗi
                var rootMeID = thungGangAt.ParentID ?? thungGangAt.ID;

                // 3️⃣ Lấy tất cả mẻ trong cùng chuỗi (cha + con)
                var meIdsInChain = await query
                    .Where(x => x.ID == rootMeID || x.ParentID == rootMeID)
                    .Select(x => x.ID)
                    .ToListAsync();

                // 4️⃣ Lấy các móc nối đã tồn tại của chuỗi
                var oldMocNoi = await _context.Tbl_MocNoiThungGangAT
                    .Where(x => meIdsInChain.Contains(x.IdThungGangAT))
                    .ToListAsync();

                if (oldMocNoi.Any())
                {
                    // 5️⃣ Lấy danh sách MaThungGang hợp lệ
                    var oldThungIds = oldMocNoi
                        .Select(x => x.IdThungGang)
                        .Distinct()
                        .ToList();

                    var validMaThungGangs = await _context.Tbl_BM_16_GangLong
                        .Where(x => oldThungIds.Contains(x.ID))
                        .Select(x => x.MaThungGang)
                        .Distinct()
                        .ToListAsync();

                    // 6️⃣ Lấy MaThungGang của request mới
                    var newMaThungGangs = await _context.Tbl_BM_16_GangLong
                        .Where(x => idThungGangs.Contains(x.ID))
                        .Select(x => x.MaThungGang)
                        .Distinct()
                        .ToListAsync();

                    // 7️⃣ Check sai thùng
                    var saiThung = newMaThungGangs
                        .Any(ma => !validMaThungGangs.Contains(ma));

                    if (saiThung)
                        throw new Exception(
                            "Mẻ này thuộc chuỗi đã móc nối, chỉ được móc nối với thùng gang đã sử dụng trước đó."
                        );
                }

                /* =====================================================
                 * 🔴 HẾT CHECK – BẮT ĐẦU MÓC NỐI
                 * ===================================================== */

                // 8️⃣ Đánh dấu mẻ AT đã sử dụng
                thungGangAt.IsUsed = true;

                // 9️⃣ Lấy danh sách thùng gang
                var listThungGang = await _context.Tbl_BM_16_GangLong
                    .Where(x => idThungGangs.Contains(x.ID))
                    .ToListAsync();

                if (!listThungGang.Any())
                    throw new Exception("Danh sách thùng gang không hợp lệ.");

                foreach (var thungGang in listThungGang)
                {
                    thungGang.T_KLThungVaGang = thungGangAt.KLThungVaGang;
                    thungGang.T_KLThungChua = thungGangAt.KLThung;
                    thungGang.T_KLGangLong =
                        (thungGangAt.KLThungVaGang ?? 0)
                        - (thungGangAt.KLThung ?? 0);

                    var thungTG = await _context.Tbl_BM_16_ThungTrungGian
                        .FirstOrDefaultAsync(x => x.ID == thungGang.ID_TTG);

                    if (thungTG != null)
                    {
                        var meThoiExist = await _context.Tbl_MeThoi
                            .FirstOrDefaultAsync(x => x.MaMeThoi == thungGangAt.MeThoi);

                        if (meThoiExist == null)
                            throw new Exception("Không tìm thấy Mẻ thổi trong hệ thống.");

                        thungTG.ID_MeThoi = meThoiExist.ID;
                        thungTG.GioChonMe = thungGangAt.ThoiDiemRot;
                        thungTG.KLThungVaGang_Thoi = thungGangAt.KLThungVaGang;
                        thungTG.KLThung_Thoi = thungGangAt.KLThung;
                        thungTG.KLGang_Thoi =
                            (thungGangAt.KLThungVaGang ?? 0)
                            - (thungGangAt.KLThung ?? 0);
                    }

                    _context.Tbl_MocNoiThungGangAT.Add(new Tbl_MocNoiThungGangAT
                    {
                        IdLoThoi = idLoThoi,
                        Ca = ca,
                        IdThungGangAT = thungGangAt.ID,
                        IdThungGang = thungGang.ID,
                        NgayMocNoi = ngayMocNoi
                    });
                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return true;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
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

                // Lấy danh sách còn móc nối một lần
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

        private async Task UpsertBoFAsync<T>(DbSet<T> dbSet, TaoMoiMeGangVaoLoDto dto) where T : Tbl_KLGangVaoBOFBase, new()
        {
            // 1️⃣ Lấy mã mẻ thổi
            var meThoi = await _context.Tbl_MeThoi
                .Where(x => x.ID == dto.ID_MeThoi)
                .Select(x => x.MaMeThoi)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(meThoi))
                throw new Exception("Không tìm thấy Mẻ thổi.");

            if (!dto.ID.HasValue || dto.ID == 0)
            {
                var isDuplicate = await dbSet.AnyAsync(x =>
                    x.MeThoi == meThoi &&
                    x.NgaySanXuat == dto.NgaySanXuat &&
                    x.Ca == dto.Ca &&
                    x.IsNM == false
                );

                if (isDuplicate)
                    throw new Exception(
                        $"Mẻ thổi [{meThoi}] đã tồn tại, không được tạo mới."
                    );

                var entity = new T();
                MapDtoToEntity(entity, dto, meThoi, isNew: true);
                await dbSet.AddAsync(entity);

                return;
            }

            var existing = await dbSet.FirstOrDefaultAsync(x => x.ID == dto.ID.Value);

            if (existing == null)
                throw new Exception("Không tìm thấy bản ghi cần sửa.");

            MapDtoToEntity(existing, dto, meThoi, isNew: false);
        }

        public async Task<bool> UpsertMeGangVaoLoAsync(TaoMoiMeGangVaoLoDto dto)
        {
            switch (dto.IdLoThoi)
            {
                case 1:
                    await UpsertBoFAsync(_context.Tbl_KLGangVaoBOF1, dto);
                    break;

                case 2:
                    await UpsertBoFAsync(_context.Tbl_KLGangVaoBOF2, dto);
                    break;

                case 3:
                    await UpsertBoFAsync(_context.Tbl_KLGangVaoBOF3, dto);
                    break;

                case 4:
                    await UpsertBoFAsync(_context.Tbl_KLGangVaoBOF4, dto);
                    break;

                case 5:
                    await UpsertBoFAsync(_context.Tbl_KLGangVaoBOF5, dto);
                    break;

                default:
                    throw new Exception("Lò thổi không hợp lệ");
            }

            await _context.SaveChangesAsync();
            return true;
        }
        private void MapDtoToEntity(
            Tbl_KLGangVaoBOFBase entity,
            TaoMoiMeGangVaoLoDto dto,
            string maMeThoi,
            bool isNew)
        {
            if (isNew)
            {
                entity.ID_NM = null;
                entity.NgayTao = DateTime.Now;
                entity.IsNM = false;

                entity.ParentID = dto.ParentID;
            }

            entity.ID_MeThoi = dto.ID_MeThoi;
            entity.Ca = dto.Ca;
            entity.NgaySanXuat = dto.NgaySanXuat;
            entity.MeThoi = maMeThoi;

            entity.KLThungVaGang = dto.KLThungVaGang;
            entity.KLThung = dto.KLThung;
            entity.KLGang = dto.KLGang;
            entity.SoThung = dto.SoThung;
            entity.ThoiDiemRot = dto.ThoiDiemRot;
            entity.LyDo = dto.LyDo;
        }

        private T CloneBoFEntity<T>(Tbl_KLGangVaoBOFBase src)
                where T : Tbl_KLGangVaoBOFBase, new()
        {
            return new T
            {
                ID_NM = src.ID_NM,
                NgayLaySoLieuAT = src.NgayLaySoLieuAT,
                CanCauTruc = src.CanCauTruc,

                ID_MeThoi = src.ID_MeThoi,
                MeThoi = src.MeThoi,

                ThoiDiemRot = src.ThoiDiemRot,

                KLThungVaGang = src.KLThungVaGang,
                KLThung = src.KLThung,
                KLGang = src.KLGang,

                SoThung = src.SoThung,

                LyDo = src.LyDo,

                IsUsed = src.IsUsed,
                IsNM = src.IsNM

            };
        }

        private async Task<Tbl_KLGangVaoBOFBase> InsertToTargetBoFAsync(
            int toLoID,
            Tbl_KLGangVaoBOFBase src)
        {
            Tbl_KLGangVaoBOFBase entity;

            switch (toLoID)
            {
                case 1:
                    entity = CloneBoFEntity<Tbl_KLGangVaoBOF1>(src);
                    await _context.Tbl_KLGangVaoBOF1.AddAsync((Tbl_KLGangVaoBOF1)entity);
                    break;

                case 2:
                    entity = CloneBoFEntity<Tbl_KLGangVaoBOF2>(src);
                    await _context.Tbl_KLGangVaoBOF2.AddAsync((Tbl_KLGangVaoBOF2)entity);
                    break;

                case 3:
                    entity = CloneBoFEntity<Tbl_KLGangVaoBOF3>(src);
                    await _context.Tbl_KLGangVaoBOF3.AddAsync((Tbl_KLGangVaoBOF3)entity);
                    break;

                case 4:
                    entity = CloneBoFEntity<Tbl_KLGangVaoBOF4>(src);
                    await _context.Tbl_KLGangVaoBOF4.AddAsync((Tbl_KLGangVaoBOF4)entity);
                    break;

                case 5:
                    entity = CloneBoFEntity<Tbl_KLGangVaoBOF5>(src);
                    await _context.Tbl_KLGangVaoBOF5.AddAsync((Tbl_KLGangVaoBOF5)entity);
                    break;

                default:
                    throw new Exception("Lò đích không hợp lệ");
            }

            return entity;
        }


        public async Task<bool> ChuyenMeGangAsync(ChuyenMeGangDto dto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Lấy mẻ nguồn
                var fromQuery = GetBOFQuery(dto.FromLo);

                var fromEntity = await fromQuery
                    .FirstOrDefaultAsync(x => x.ID == dto.FromID);

                if (fromEntity == null)
                    throw new Exception("Không tìm thấy mẻ nguồn.");

                if (fromEntity.IsChuyenMe == true)
                    throw new Exception("Mẻ này đã được chuyển.");

                // 2️⃣ Insert mẻ đích (clone + add đúng DbSet)
                var toEntity = await InsertToTargetBoFAsync(dto.ToLo, fromEntity);

                // 3️⃣ Set lại thông tin NGHIỆP VỤ cho mẻ đích
                toEntity.NgaySanXuat = dto.NgaySanXuatTo;
                toEntity.Ca = dto.CaTo;
                toEntity.NgayTao = DateTime.Now;

                toEntity.IsChuyenMe = false;
                toEntity.ParentID = null; // ❗ chuyển mẻ ≠ tách mẻ

                await _context.SaveChangesAsync(); // 🔑 để EF sinh ID

                // 4️⃣ Lưu mapping chuyển mẻ (Việt hoá)
                var map = new Tbl_BOF_ChuyenMe
                {
                    TuLoID = dto.FromLo,
                    TuMeID = fromEntity.ID,
                    DenLoID = dto.ToLo,
                    DenMeID = toEntity.ID
                };

                await _context.Tbl_BOF_ChuyenMe.AddAsync(map);

                // 5️⃣ Khoá mẻ nguồn
                fromEntity.IsChuyenMe = true;

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return true;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ThuHoiMeGangAsync(ThuHoiMeDto dto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Lấy mapping
                var map = await _context.Tbl_BOF_ChuyenMe
                    .FirstOrDefaultAsync(x =>
                        x.TuLoID == dto.tuLoID &&
                        x.TuMeID == dto.tuMeID);

                if (map == null)
                    throw new Exception("Không tìm thấy thông tin chuyển mẻ.");

                // 2️⃣ Lấy mẻ đích
                var toQuery = GetBOFQuery(map.DenLoID);

                var toEntity = await toQuery
                    .FirstOrDefaultAsync(x => x.ID == map.DenMeID);

                if (toEntity == null)
                    throw new Exception("Không tìm thấy mẻ đích để thu hồi.");

                // ❌ CHECK NGHIỆP VỤ QUAN TRỌNG
                if (toEntity.IsUsed == true)
                    throw new Exception("Mẻ đã được sử dụng, không thể thu hồi.");

                // 3️⃣ Xóa mẻ đích
                _context.Remove(toEntity);

                // 4️⃣ Mở khóa mẻ nguồn
                var fromQuery = GetBOFQuery(map.TuLoID);

                var fromEntity = await fromQuery
                    .FirstOrDefaultAsync(x => x.ID == map.TuMeID);

                if (fromEntity != null)
                    fromEntity.IsChuyenMe = false;

                // 5️⃣ Xóa mapping
                _context.Tbl_BOF_ChuyenMe.Remove(map);

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return true;
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }


    }
}
