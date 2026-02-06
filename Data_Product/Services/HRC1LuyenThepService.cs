using Data_Product.DTO;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Models.ModelView;
using Data_Product.Repositorys;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Threading.Tasks;

namespace Data_Product.Services
{
    public interface IHRC1LuyenThepService
    {
        Task<List<ThongTinMeThoiATModel>> LoadDanhSachThungThepHRC1ATAsync(LoadDanhSachThungThepHRC1ATDto dto);
        Task<bool> MocNoiAsync(List<int> idThungGangs, int idThungAT, int IdLoThoi, int Ca, DateTime NgayMocNoi);
        Task HuyMocNoi(List<int> idThungGangs);
        Task<bool> UpsertMeGangVaoLoAsync(TaoMoiMeGangVaoLoDto dto);
        Task<bool> ChuyenMeGangAsync(ChuyenMeGangDto dto);
        Task<bool> ThuHoiMeGangAsync(ThuHoiMeDto dto);

        Task<Tbl_KLGangVaoBOFBase> ThongTinMeThoiAT(ThongTinMeThoiATDto dto);
        Task<bool> XoaMeTaoTayAsync(XoaMeTaoTayDto dto);

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

        public async Task<Tbl_KLGangVaoBOFBase> ThongTinMeThoiAT(ThongTinMeThoiATDto dto)
        {
            var baseQuery = GetBOFQuery(dto.IdLoThoi);
            var thongTinMe = await baseQuery.Where(x => x.MeThoi == dto.MeThoi).OrderByDescending(x => x.NgayTao).FirstOrDefaultAsync();
            if (thongTinMe != null) return thongTinMe;
            return new Tbl_KLGangVaoBOFBase();
        }

        public async Task<List<ThongTinMeThoiATModel>> LoadDanhSachThungThepHRC1ATAsync(
    LoadDanhSachThungThepHRC1ATDto dto)
        {
            IQueryable<Tbl_KLGangVaoBOFBase> query = GetBOFQuery(dto.IdLoThoi);

            // ===============================
            // 1️⃣ FILTER THEO THỜI GIAN
            // ===============================
            if (dto.TuNgay.HasValue && dto.DenNgay.HasValue)
            {
                var tuNgay = dto.TuNgay.Value;
                var denNgay = dto.DenNgay.Value;

                query = query.Where(x =>
                    x.NgayTao.HasValue &&
                    x.NgayTao.Value >= tuNgay &&
                    x.NgayTao.Value <= denNgay
                );
            }
            else
            {
                if (!dto.NgaySanXuat.HasValue)
                    return new List<ThongTinMeThoiATModel>();

                var start = dto.NgaySanXuat.Value.Date;
                var end = start.AddDays(1);

                query = query.Where(x =>
                    x.NgaySanXuat >= start &&
                    x.NgaySanXuat < end
                );

                if (dto.Ca.HasValue)
                {
                    query = query.Where(x => x.Ca == dto.Ca.Value);
                }
            }

            // ===============================
            // 2️⃣ FILTER NGHIỆP VỤ
            // ===============================
            if (!string.IsNullOrWhiteSpace(dto.MeThoi))
            {
                var meThoi = dto.MeThoi.Trim();
                query = query.Where(x => x.MeThoi.Contains(meThoi));
            }

            if (dto.TrangThaiAT.HasValue)
            {
                if (dto.TrangThaiAT.Value)
                {
                    // Đã sử dụng
                    query = query.Where(x => x.IsUsed == true);
                }
                else
                {
                    // Chưa sử dụng
                    query = query.Where(x => x.IsUsed == null);
                }
            }

            // ===============================
            // 3️⃣ LOAD DANH SÁCH MẺ (SORT)
            // ===============================
            var mes = await query
                .OrderByDescending(x => x.NgayTao) // 🔑 CHA trước – CON sau (do +1s)
                .ToListAsync();

            if (mes.Count == 0)
                return new List<ThongTinMeThoiATModel>();

            var parentHasChildSet = mes
                .Where(x => x.ParentID.HasValue)
                .Select(x => x.ParentID!.Value)
                .ToHashSet();

            var meByIdLookup = mes.ToDictionary(x => x.ID, x => x);

            // ===============================
            // 4️⃣ LOAD CHUYỂN MẺ LIÊN QUAN (1 QUERY)
            // ===============================
            var ids = mes.Select(x => x.ID).ToList();

            var chuyenMes = await _context.Tbl_BOF_ChuyenMe
                .Where(cm =>
                    (cm.TuLoID == dto.IdLoThoi && ids.Contains(cm.TuMeID)) ||
                    (cm.DenLoID == dto.IdLoThoi && ids.Contains(cm.DenMeID))
                )
                .ToListAsync();

            var chuyenDenGroups = chuyenMes
            .Where(x => x.DenLoID == dto.IdLoThoi)   // mẻ CHUYỂN ĐẾN
            .GroupBy(x => x.TuLoID);

            var meChaLookup = new Dictionary<int, Tbl_KLGangVaoBOFBase>();

            foreach (var group in chuyenDenGroups)
            {
                int tuLoID = group.Key;

                var tuMeIds = group
                    .Select(x => x.TuMeID)
                    .Distinct()
                    .ToList();

                // lấy (ID con → ParentID)
                var conToParent = await GetBOFQuery(tuLoID)
                    .Where(x => tuMeIds.Contains(x.ID) && x.ParentID.HasValue)
                    .Select(x => new { ConID = x.ID, ChaID = x.ParentID!.Value })
                    .ToListAsync();

                if (!conToParent.Any()) continue;

                var chaIds = conToParent
                    .Select(x => x.ChaID)
                    .Distinct()
                    .ToList();

                // lấy mẻ CHA thật
                var chaEntities = await GetBOFQuery(tuLoID)
                    .Where(x => chaIds.Contains(x.ID))
                    .ToListAsync();

                var chaById = chaEntities.ToDictionary(x => x.ID, x => x);

                // map CON → CHA
                foreach (var map in conToParent)
                {
                    if (chaById.TryGetValue(map.ChaID, out var cha))
                    {
                        meChaLookup[map.ConID] = cha;
                    }
                }
            }
            // ===============================
            // 5️⃣ BUILD LOOKUP (O(1))
            // ===============================
            var chuyenDiLookup = chuyenMes
                .Where(x => x.TuLoID == dto.IdLoThoi)
                .ToDictionary(x => x.TuMeID, x => x);

            var chuyenDenLookup = chuyenMes
                .Where(x => x.DenLoID == dto.IdLoThoi)
                .ToDictionary(x => x.DenMeID, x => x);

            // ===============================
            // 6️⃣ MAP + RENDER
            // ===============================
            var result = new List<ThongTinMeThoiATModel>(mes.Count);

            foreach (var me in mes)
            {
                result.Add(
                    MapMeBestPractice(
                        me,
                        dto.IdLoThoi,
                        chuyenDiLookup,
                        chuyenDenLookup,
                        parentHasChildSet,
                        meChaLookup,
                        meByIdLookup
                    )
                );
            }

            return result;
        }
        private ThongTinMeThoiATModel MapMeBestPractice(
        Tbl_KLGangVaoBOFBase me,
        int idLoThoi,
        Dictionary<int, Tbl_BOF_ChuyenMe> chuyenDiLookup,
        Dictionary<int, Tbl_BOF_ChuyenMe> chuyenDenLookup, HashSet<int> parentHasChildSet, Dictionary<int, Tbl_KLGangVaoBOFBase> meChaLookup, Dictionary<int, Tbl_KLGangVaoBOFBase> meByIdLookup
)
        {
            Tbl_BOF_ChuyenMe? cm = null;
            bool isChuyenDen = false;

            // 🔹 Ưu tiên mẻ ĐI (từ lò hiện tại)
            if (chuyenDiLookup.TryGetValue(me.ID, out var chuyenDi))
            {
                cm = chuyenDi;
                isChuyenDen = false;
            }
            // 🔹 Nếu không có → kiểm tra mẻ ĐẾN
            else if (chuyenDenLookup.TryGetValue(me.ID, out var chuyenDen))
            {
                cm = chuyenDen;
                isChuyenDen = true;
            }

            Tbl_KLGangVaoBOFBase? meCha = null;

            if (cm != null && isChuyenDen)
            {
                // key = TuMeID (mẻ con ở lò nguồn)
                meChaLookup.TryGetValue(cm.TuMeID, out meCha);
            }

            bool chaDaChuyen = false;
            if (me.ParentID.HasValue)
            {
                if (meByIdLookup.TryGetValue(me.ParentID.Value, out var meChaLocal))
                {
                    chaDaChuyen = meChaLocal.IsChuyenMe == true;
                }
            }
            return new ThongTinMeThoiATModel
            {
                ThungGoc = me,
                DaTachMe = parentHasChildSet.Contains(me.ID),
                ChaDaChuyen = chaDaChuyen,
                ThungChuyen = cm == null
                    ? null
                    : new ThongTinMeChuyenModel
                    {
                        TuMeID = isChuyenDen ? cm.TuMeID : cm.DenMeID,
                        TuLoID = isChuyenDen ? cm.TuLoID : cm.DenLoID,
                        MeThoi = me.MeThoi,
                        Ca = me.Ca,
                        NgayTao = cm.NgayTao,
                        IsUsed = me.IsUsed,
                        IsChuyenDen = isChuyenDen,
                        MeCha = meCha == null
                            ? null
                            : new ThongTinMeGocModel
                            {
                                ID = meCha.ID,
                                MeThoi = meCha.MeThoi,
                                Ca = meCha.Ca,
                                NgayTao = meCha.NgayTao
                            }

                    }
            };
        }


        public async Task<bool> MocNoiAsync(
                List<int> idThungGangs,
                int idThungAT,
                int idLoThoi,
                int ca,
                DateTime ngayMocNoi)
        {

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

                //// 2️⃣ Xác định mẻ gốc của chuỗi
                //var rootMeID = thungGangAt.ParentID ?? thungGangAt.ID;

                //// 3️⃣ Lấy tất cả mẻ trong cùng chuỗi (cha + con)
                //var meIdsInChain = await query
                //    .Where(x => x.ID == rootMeID || x.ParentID == rootMeID)
                //    .Select(x => x.ID)
                //    .ToListAsync();
                // 2️⃣ Xác định mẻ gốc THẬT
                var (rootLoID, rootMe) = await GetRootMeAsync(idLoThoi, thungGangAt);
                var rootMeID = rootMe.ID;

                // 3️⃣ Lấy toàn bộ mẻ trong chuỗi (mọi mẻ có root này)
                var meIdsInChain = await query
                    .Where(x => x.ID == rootMeID || x.ParentID == rootMeID)
                    .Select(x => x.ID)
                    .ToListAsync();

                // 4️⃣ Lấy các móc nối đã tồn tại của chuỗi
                var oldMocNoi = await _context.Tbl_MocNoiThungGangAT
                    .Where(x => x.IdLoThoi == rootLoID && meIdsInChain.Contains(x.IdThungGangAT))
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
                            "1 thùng gang đổ vào 2 mẻ khác lò thì khi móc nối phải chọn cùng 1 thùng đó cho cả 2 mẻ"
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

                var klThungVaGang_R = Math.Round(
                    thungGangAt.KLThungVaGang ?? 0m,
                    2,
                    MidpointRounding.AwayFromZero
                );

                var klThung_R = Math.Round(
                    thungGangAt.KLThung ?? 0m,
                    2,
                    MidpointRounding.AwayFromZero
                );

                var klGang_R = klThungVaGang_R - klThung_R;
                foreach (var thungGang in listThungGang)
                {
                    thungGang.T_KLThungVaGang = klThungVaGang_R;
                    thungGang.T_KLThungChua = klThung_R;
                    thungGang.T_KLGangLong = klGang_R;

                    var thungTG = await _context.Tbl_BM_16_ThungTrungGian
                        .FirstOrDefaultAsync(x => x.ID == thungGang.ID_TTG);

                    if (thungTG != null)
                    {
                        var meThoiExist = await _context.Tbl_MeThoi
                            .FirstOrDefaultAsync(x => x.MaMeThoi == thungGangAt.MeThoi);

                        if (meThoiExist == null)
                            throw new Exception("Số mẻ lớn hơn số mẻ hiện tại của hệ thống hoặc Có mẻ tách chưa nhập số mẻ");

                        thungTG.ID_MeThoi = meThoiExist.ID;
                        thungTG.GioChonMe = thungGangAt.ThoiDiemRot;

                        thungTG.KLThungVaGang_Thoi = klThungVaGang_R;
                        thungTG.KLThung_Thoi = klThung_R;
                        thungTG.KLGang_Thoi = klGang_R;
                        thungTG.Tong_KLGangNhan = klGang_R;
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

                return true;
            }
            catch
            {
                throw;
            }
        }

        private async Task<(int RootLoID, Tbl_KLGangVaoBOFBase RootMe)> GetRootMeAsync(
            int idLoThoi,
            Tbl_KLGangVaoBOFBase me
        )
        {
            // 1️⃣ Check xem mẻ hiện tại có phải mẻ CHUYỂN ĐẾN không
            var chuyenDen = await _context.Tbl_BOF_ChuyenMe
                .FirstOrDefaultAsync(x =>
                    x.DenLoID == idLoThoi &&
                    x.DenMeID == me.ID
                );

            if (chuyenDen != null)
            {
                // 2️⃣ Mẻ gốc nằm ở lò nguồn (TuLoID)
                var tuLoID = chuyenDen.TuLoID;

                // 3️⃣ Lấy mẻ CON ở lò nguồn
                var meConNguon = await GetBOFQuery(tuLoID)
                    .FirstOrDefaultAsync(x => x.ID == chuyenDen.TuMeID);

                if (meConNguon != null && meConNguon.ParentID.HasValue)
                {
                    // 4️⃣ Lấy mẻ CHA THẬT (root)
                    var meCha = await GetBOFQuery(tuLoID)
                        .FirstOrDefaultAsync(x => x.ID == meConNguon.ParentID.Value);

                    if (meCha != null)
                        return (tuLoID, meCha);
                }
            }

            // ===============================
            // FALLBACK: mẻ không chuyển hoặc không tìm được mapping
            // ===============================

            // Nếu bản thân nó là mẻ cha
            if (!me.ParentID.HasValue)
                return (idLoThoi, me);

            // Nếu là mẻ con cùng lò
            var parent = await GetBOFQuery(idLoThoi)
                .FirstOrDefaultAsync(x => x.ID == me.ParentID.Value);

            if (parent != null)
                return (idLoThoi, parent);

            throw new Exception("Không xác định được mẻ gốc.");
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
        private async Task UpsertBoFAsync<T>(
            DbSet<T> dbSet,
            TaoMoiMeGangVaoLoDto dto
        ) where T : Tbl_KLGangVaoBOFBase, new()
        {
            // =========================
            // 🔒 Validate bắt buộc
            // =========================
            if (dto.IdLoThoi <= 0)
                throw new Exception("IdLoThoi là bắt buộc.");

            if (dto.Ca <= 0)
                throw new Exception("Ca là bắt buộc.");

            if (dto.NgaySanXuat == default)
                throw new Exception("NgaySanXuat là bắt buộc.");

            // =========================
            // 🔑 Lấy mã mẻ thổi (OPTIONAL)
            // =========================
            string? meThoi = null;

            if (dto.ID_MeThoi.HasValue)
            {
                meThoi = await _context.Tbl_MeThoi
                    .Where(x => x.ID == dto.ID_MeThoi.Value)
                    .Select(x => x.MaMeThoi)
                    .FirstOrDefaultAsync();
            }

            // =========================
            // ➕ CREATE
            // =========================
            if (!dto.ID.HasValue || dto.ID == 0)
            {
                //if(meThoi != null)
                //{
                //    var isDuplicate = await dbSet.AnyAsync(x =>
                //            x.MeThoi == meThoi &&
                //            x.NgaySanXuat == dto.NgaySanXuat &&
                //            x.Ca == dto.Ca &&
                //            x.IsNM == false
                //        );

                //    if (isDuplicate)
                //        throw new Exception(
                //            $"Mẻ thổi [{meThoi}] đã tồn tại, không được tạo mới."
                //        );
                //}
                

                var entity = new T();
                if (dto.ParentID.HasValue)
                {
                    var parrent = await dbSet.Where(x => x.ID == dto.ParentID).FirstOrDefaultAsync();
                    dto.NgayTao = parrent.NgayTao ?? DateTime.Now;
                }
                await MapDtoToEntity(entity, dto, meThoi, isNew: true);

                await dbSet.AddAsync(entity);
                return;
            }

            // =========================
            // ✏ UPDATE
            // =========================
            var existing = await dbSet.FirstOrDefaultAsync(x => x.ID == dto.ID.Value);

            if (existing == null)
                throw new Exception("Không tìm thấy bản ghi cần sửa.");

            await MapDtoToEntity(existing, dto, meThoi, isNew: false);
            var existChuyenMe = await _context.Tbl_BOF_ChuyenMe
                .Where(x =>  x.DenMeID == existing.ID).FirstOrDefaultAsync();

            if (existChuyenMe != null)
            {
                var meGoc = await GetBOFQuery(existChuyenMe.TuLoID)
                    .Where(x => x.ID == existChuyenMe.TuMeID)
                    .FirstOrDefaultAsync();
                if (meGoc != null)
                {
                    await MapDtoToEntity(meGoc, dto, meThoi, isNew: false);
                }
            }

        }

        private async Task MapDtoToEntity(
            Tbl_KLGangVaoBOFBase entity,
            TaoMoiMeGangVaoLoDto dto,
            string? maMeThoi,
            bool isNew
        )
        {
            /* =========================
             *  KHỞI TẠO
             * ========================= */
            TimeSpan timeOfDay;

            if (!string.IsNullOrWhiteSpace(dto.ThoiDiemRot)
                && TimeSpan.TryParse(dto.ThoiDiemRot, out var parsedTime) && entity.IsChuyenMe != true)
            {
                timeOfDay = parsedTime;
            }
            else
            {
                timeOfDay = DateTime.Now.TimeOfDay;
            }

            var ngayTaoTheoThoiDiem = dto.NgaySanXuat.Date.Add(timeOfDay);


            if (isNew)
            {
                entity.ID_NM = null;
                entity.IsNM = false;
                entity.ParentID = dto.ParentID;
                //var now = DateTime.Now;
                // Ngày = NgaySanXuat, giờ = Now
                if (dto.LoaiMe == LoaiMe.Con)
                {
                    var baseQuery = GetBOFQuery(dto.IdLoThoi);
                    var ngayTaoMeCham = await baseQuery
                        .Where(x => x.ID == dto.ParentID)
                        .Select(x => x.NgayTao)
                        .FirstOrDefaultAsync();
                    if (ngayTaoMeCham.HasValue) {
                        entity.NgayTao = ngayTaoMeCham.Value.AddSeconds(-1);
                    }
                    else
                    {
                        entity.NgayTao = ngayTaoTheoThoiDiem;
                    }
                }
                else
                {
                    entity.NgayTao = ngayTaoTheoThoiDiem;
                }
            }

            /* =========================
             *  BẮT BUỘC
             * ========================= */
            entity.Ca = dto.Ca;
            entity.NgaySanXuat = dto.NgaySanXuat;

            /* =========================
             *  XỬ LÝ THEO LOẠI MẺ
             * ========================= */
            switch (dto.LoaiMe)
            {
                case LoaiMe.Cha:
                    if (!string.IsNullOrWhiteSpace(maMeThoi))
                    {
                        entity.MeThoi = maMeThoi;
                    }
                    entity.KLThungVaGang = dto.KLThungVaGang;
                    entity.KLThung = dto.KLThung;
                    entity.KLGang = dto.KLGang;
                    return;

                case LoaiMe.Con:
                    entity.ID_MeThoi = dto.ID_MeThoi;
                    entity.MeThoi = maMeThoi;

                    entity.KLThungVaGang = dto.KLThungVaGang;
                    entity.KLThung = dto.KLThung;
                    entity.KLGang = dto.KLGang;

                    entity.SoThung = dto.SoThung;
                    entity.ThoiDiemRot = dto.ThoiDiemRot;
                    entity.LyDo = dto.LyDo;

                    return;
                default:
                    if (!isNew
                         && entity.IsChuyenMe != true
                         && !string.IsNullOrWhiteSpace(dto.ThoiDiemRot))
                    {
                        entity.NgayTao = ngayTaoTheoThoiDiem;
                    }
                    entity.ID_MeThoi = dto.ID_MeThoi; 
                    entity.MeThoi = maMeThoi;

                    entity.KLThungVaGang = dto.KLThungVaGang;
                    entity.KLThung = dto.KLThung;
                    entity.KLGang = dto.KLGang;

                    entity.SoThung = dto.SoThung;
                    entity.ThoiDiemRot = dto.ThoiDiemRot;
                    entity.LyDo = dto.LyDo;

                    return;                
            }
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
        

        private T CloneBoFEntity<T>(Tbl_KLGangVaoBOFBase src)
                where T : Tbl_KLGangVaoBOFBase, new()
        {
            return new T
            {
                NgaySanXuat = src.NgaySanXuat,
                Ca = src.Ca,
                NgayLaySoLieuAT = src.NgayLaySoLieuAT,
                CanCauTruc = src.CanCauTruc,
                KLThungVaGang = src.KLThungVaGang,
                KLThung = src.KLThung,
                KLGang = src.KLGang,

                SoThung = src.SoThung,

                LyDo = src.LyDo,

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

                //toEntity.NgaySanXuat = dto.NgaySanXuatTo;
                //toEntity.Ca = dto.CaTo;
                toEntity.NgayTao = fromEntity.NgayTao;
                toEntity.NgaySanXuat = fromEntity.NgaySanXuat;

                toEntity.IsChuyenMe = false;
                toEntity.ParentID = fromEntity.ID; // ❗ chuyển mẻ ≠ tách mẻ

                await _context.SaveChangesAsync(); // 🔑 để EF sinh ID

                // 4️⃣ Lưu mapping chuyển mẻ (Việt hoá)
                var map = new Tbl_BOF_ChuyenMe
                {
                    TuLoID = dto.FromLo,
                    TuMeID = fromEntity.ID,
                    DenLoID = dto.ToLo,
                    DenMeID = toEntity.ID,
                    NgayTao = DateTime.Now
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
                // ===============================
                // 1️⃣ LẤY MAPPING CHUYỂN MẺ
                // ===============================
                var map = await _context.Tbl_BOF_ChuyenMe
                    .FirstOrDefaultAsync(x =>
                        x.TuLoID == dto.tuLoID &&
                        x.TuMeID == dto.tuMeID);

                if (map == null)
                    throw new Exception("Không tìm thấy thông tin chuyển mẻ.");

                // ===============================
                // 2️⃣ KIỂM TRA MẺ ĐÍCH
                // ===============================
                var toQuery = GetBOFQuery(map.DenLoID);

                var toEntity = await toQuery
                    .FirstOrDefaultAsync(x => x.ID == map.DenMeID);

                if (toEntity == null)
                    throw new Exception("Không tìm thấy mẻ đích để thu hồi.");

                // ❌ NGHIỆP VỤ CỨNG: mẻ đã dùng thì KHÔNG thu hồi
                if (toEntity.IsUsed == true)
                    throw new Exception("Mẻ đã được sử dụng, không thể thu hồi.");

                // ===============================
                // 3️⃣ XÓA MẺ ĐÍCH (ĐÚNG LÒ)
                // ===============================
                await DeleteBOFAsync(map.DenLoID, map.DenMeID);

                // ===============================
                // 4️⃣ MỞ KHÓA MẺ NGUỒN
                // ===============================
                var fromQuery = GetBOFQuery(map.TuLoID);

                var fromEntity = await fromQuery
                    .FirstOrDefaultAsync(x => x.ID == map.TuMeID);

                var meCha = await fromQuery.Where(x => x.ID == fromEntity.ParentID).FirstOrDefaultAsync();
                if (meCha == null)
                    throw new Exception("Không tồn tại mẻ cha của mẻ đang thu hồi");

                if(meCha.IsUsed == true)
                    throw new Exception("Mẻ cha đã được sử dụng, vui lòng hủy nhận mẻ cha trước khi thu hồi mẻ con");

                if (fromEntity != null)
                {
                    fromEntity.IsChuyenMe = false;
                    fromEntity.MeThoi = null;
                    fromEntity.ID_MeThoi = null;
                    fromEntity.ThoiDiemRot = null;
                }

                // ===============================
                // 5️⃣ XÓA MAPPING CHUYỂN MẺ
                // ===============================
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

        private async Task DeleteBOFAsync(int idLoThoi, int id)
        {
            switch (idLoThoi)
            {
                case 1:
                    var e1 = await _context.Tbl_KLGangVaoBOF1.FindAsync(id);
                    if (e1 != null) _context.Tbl_KLGangVaoBOF1.Remove(e1);
                    break;

                case 2:
                    var e2 = await _context.Tbl_KLGangVaoBOF2.FindAsync(id);
                    if (e2 != null) _context.Tbl_KLGangVaoBOF2.Remove(e2);
                    break;

                case 3:
                    var e3 = await _context.Tbl_KLGangVaoBOF3.FindAsync(id);
                    if (e3 != null) _context.Tbl_KLGangVaoBOF3.Remove(e3);
                    break;

                case 4:
                    var e4 = await _context.Tbl_KLGangVaoBOF4.FindAsync(id);
                    if (e4 != null) _context.Tbl_KLGangVaoBOF4.Remove(e4);
                    break;

                case 5:
                    var e5 = await _context.Tbl_KLGangVaoBOF5.FindAsync(id);
                    if (e5 != null) _context.Tbl_KLGangVaoBOF5.Remove(e5);
                    break;

                default:
                    throw new Exception("Lò thổi không hợp lệ");
            }
        }

        public async Task<bool> XoaMeTaoTayAsync(XoaMeTaoTayDto dto)
        {
            var baseQuery = GetBOFQuery(dto.IdLoThoi);
            var meCanXoa = await baseQuery.Where(x => x.ID == dto.Id).FirstOrDefaultAsync();
            if (meCanXoa == null)
                throw new Exception("Không tồn tại mẻ này");

            if(meCanXoa.IsUsed == true)
                throw new Exception("Mẻ này đã được sử dụng");

            if( meCanXoa.ParentID.HasValue)
            {
                var meCha = await baseQuery.Where(x => x.ID == meCanXoa.ParentID).FirstOrDefaultAsync();
                if(meCha != null)
                {
                    meCha.KLThung = meCanXoa.KLThung;
                    meCha.KLGang = meCha.KLThungVaGang - meCanXoa.KLThung;
                }
            }

            await DeleteBOFAsync(dto.IdLoThoi, dto.Id);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
