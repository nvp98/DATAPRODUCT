using Data_Product.Common;
using Data_Product.Common.Enums;
using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Security.Claims;

namespace Data_Product.Services
{
    public interface INhanGangService
    {
        Task ExecuteAsync(MocNoiThung payload, string tenTaiKhoan);
        Task ExecuteInternalAsync(MocNoiThung payload, string tenTaiKhoan);
    }
    public class NhanGangService: INhanGangService
    {
        private readonly DataContext _context;
        private readonly IHRC1LuyenThepService _hrc1LuyenThepService;
        private readonly IChiaGangService _chiaGangService;

        public NhanGangService(
            DataContext context,
            IHRC1LuyenThepService hrc1LuyenThepService,
            IChiaGangService chiaGangService)
        {
            _context = context;
            _hrc1LuyenThepService = hrc1LuyenThepService;
            _chiaGangService = chiaGangService;
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
        public async Task ExecuteAsync(MocNoiThung payload, string tenTaiKhoan)
        {
            await using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                await ExecuteInternalAsync(payload, tenTaiKhoan);
                await tran.CommitAsync();
            }
            catch
            {
                await tran.RollbackAsync();
                throw;
            }
        }

        public async Task ExecuteInternalAsync(MocNoiThung payload, string tenTaiKhoan)
        {

                /* =====================================================
                 *  STEP 1 → STEP 3
                 *  (toàn bộ code hiện đang nằm trong Controller của anh)
                 * ===================================================== */

                if (payload == null || payload.selectedThungs == null || payload.selectedThungs.Count == 0)
                    return;
                if (payload.idCa <= 0 || payload.idLoThoi <= 0 || payload.idNguoiNhan <= 0)
                    return; 

                //var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);

                // Chuẩn hóa + sắp thứ tự từ client
                var orderedSelected = payload.selectedThungs
                    .Where(x => !string.IsNullOrWhiteSpace(x.maThungGang))
                    .GroupBy(x => x.maThungGang.Trim())
                    .Select(g => g.OrderBy(z => z.clientSeq).First())
                    .OrderBy(x => x.clientSeq)
                    .ThenBy(x => x.maThungGang)
                    .ToList();

                if (orderedSelected.Count == 0)
                    return;


                // Khóa theo scope để tuần tự hóa cấp sequence
                var lockResource = $"NHAN:{payload.ngayNhan:yyyyMMdd}|{payload.idCa}|{payload.idLoThoi}";
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_getapplock @Resource = {0}, @LockMode = 'Exclusive', @LockOwner='Transaction', @LockTimeout = 15000",
                    lockResource);

                // Kíp
                var kip = await (from a in _context.Tbl_Kip
                                    where a.NgayLamViec == payload.ngayNhan
                                    && a.TenCa == payload.idCa.ToString()
                                    select new Tbl_Kip { ID_Kip = a.ID_Kip, TenKip = a.TenKip })
                                .FirstOrDefaultAsync();
                if (kip == null) return ;

                // Lấy max seq đang có trong scope
                var currentMaxSeq = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.NgayLuyenThep == payload.ngayNhan
                                && x.T_Ca == payload.idCa
                                && x.ID_LoThoi == payload.idLoThoi
                                && x.T_ReceiveSeq != null)
                    .MaxAsync(x => (int?)x.T_ReceiveSeq) ?? 0;

                // Common TTG nếu có
                int? idThungTG_Common = null;
                if (!string.IsNullOrWhiteSpace(payload.thungTrungGian))
                {
                    idThungTG_Common = await TaoThungTrungGian(
                        payload.ngayNhan, payload.idCa, payload.idLoThoi,
                        payload.thungTrungGian, payload.idNguoiNhan, payload.NoiNhan);
                }

                // Cache các bản gốc theo mã để giảm query
                var maSet = orderedSelected.Select(x => x.maThungGang.Trim()).ToHashSet();
                var baseThungs = await _context.Tbl_BM_16_GangLong
                    .Where(x => maSet.Contains(x.MaThungGang) && x.T_copy == false)
                    .ToListAsync();

                var baseByMa = baseThungs
                    .GroupBy(t => t.MaThungGang)
                    .ToDictionary(g => g.Key, g => g.First());
                var ttgEntitiesCanTinh = new HashSet<int>();

                var clones = new List<Tbl_BM_16_GangLong>();
                var thungDuocNhanIds = new List<int>();

                foreach (var sel in orderedSelected)
                {
                    var ma = sel.maThungGang.Trim();

                    if (!baseByMa.TryGetValue(ma, out var t))
                    {
                        t = await _context.Tbl_BM_16_GangLong
                                .Where(x => x.MaThungGang == ma && x.T_copy == false)
                                .FirstOrDefaultAsync();
                        if (t == null) continue; // hoặc throw nếu cần
                        baseByMa[ma] = t;
                    }

                    // Tính maThungThep kế tiếp
                    var allThungs = await _context.Tbl_BM_16_GangLong
                                        .Where(x => x.MaThungGang == t.MaThungGang)
                                        .ToListAsync();

                    string maThungThep;
                    var thungGoc = allThungs.FirstOrDefault(x => x.T_copy == false);

                    if (string.IsNullOrEmpty(thungGoc?.MaThungThep))
                    {
                        maThungThep = GenerateMaThungThep(
                            t.MaThungGang, payload.ngayNhan, payload.idLoThoi, payload.idCa, 0);
                    }
                    else
                    {
                        var usedIndexes = allThungs
                            .Where(x => !string.IsNullOrEmpty(x.MaThungThep))
                            .Select(x =>
                            {
                                var parts = x.MaThungThep.Split('.');
                                return int.TryParse(parts.LastOrDefault(), out int idx) ? idx : -1;
                            })
                            .Where(idx => idx >= 0)
                            .OrderBy(i => i)
                            .ToList();

                        int nextIndex = 0;
                        while (usedIndexes.Contains(nextIndex)) nextIndex++;

                        maThungThep = GenerateMaThungThep(
                            t.MaThungGang, payload.ngayNhan, payload.idLoThoi, payload.idCa, nextIndex);
                    }

                    int idThungTG = idThungTG_Common
                                    ?? await TaoThungTrungGian(payload.ngayNhan, payload.idCa, payload.idLoThoi,
                                                                t.BKMIS_ThungSo, payload.idNguoiNhan, payload.NoiNhan);
                    ttgEntitiesCanTinh.Add(idThungTG);
                    // Ghi nhận người nhận
                    _context.Tbl_BM_16_TaiKhoan_Thung.Add(new Tbl_BM_16_TaiKhoan_Thung
                    {
                        ID_taiKhoan = payload.idNguoiNhan,
                        MaThungGang = t.MaThungGang,
                        MaThungThep = maThungThep,
                        MaPhieu = t.MaPhieu
                    });

                    // Cấp sequence mới cho lần nhận này (không ghi đè nếu bản ghi đã có seq trong đúng scope)
                    int nextSeq = currentMaxSeq + 1;

                    if (t.T_ID_TrangThai == (int)TinhTrang.DaNhan)
                    {
                        // Case đặc biệt: DUC1/DUC2 & chưa có MaThungThep -> chỉ update bản gốc rồi continue
                        if ((t.ChuyenDen == "DUC1" || t.ChuyenDen == "DUC2") && t.MaThungThep == null)
                        {
                            t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                            t.MaThungThep = maThungThep;
                            t.ID_LoThoi = payload.idLoThoi;
                            t.T_Ca = payload.idCa;
                            t.NgayLuyenThep = payload.ngayNhan;
                            t.T_ID_Kip = kip.ID_Kip;
                            t.ID_TTG = idThungTG;
                            t.T_ID_NguoiNhan = payload.idNguoiNhan;

                            if (t.T_ReceiveSeq == null &&
                                t.NgayLuyenThep == payload.ngayNhan &&
                                t.T_Ca == payload.idCa &&
                                t.ID_LoThoi == payload.idLoThoi)
                            {
                                t.T_ReceiveSeq = nextSeq;
                                currentMaxSeq = nextSeq;
                            }
                            continue;
                        }

                        // Các trường hợp “đã nhận” còn lại -> tạo clone
                        var clone = new Tbl_BM_16_GangLong
                        {
                            MaThungGang = t.MaThungGang,
                            BKMIS_SoMe = t.BKMIS_SoMe,
                            BKMIS_Gio = t.BKMIS_Gio,
                            BKMIS_PhanLoai = t.BKMIS_PhanLoai,
                            BKMIS_ThungSo = t.BKMIS_ThungSo,
                            NgayLuyenGang = t.NgayLuyenGang,
                            G_KLXeThungVaGang = t.G_KLXeThungVaGang,
                            G_KLXeVaThung = t.G_KLXeVaThung,
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
                            NgayTao = t.NgayTao,
                            T_copy = true,
                            MaThungThep = maThungThep,
                            ID_LoThoi = payload.idLoThoi,
                            T_Ca = payload.idCa,
                            NgayLuyenThep = payload.ngayNhan,
                            T_ID_NguoiNhan = payload.idNguoiNhan,
                            T_ID_Kip = kip.ID_Kip,
                            ID_TTG = idThungTG,
                            T_ReceiveSeq = nextSeq,
                            XacNhan = t.XacNhan,
                            ID_NguoiXacNhan = t.ID_NguoiXacNhan,
                            G_SanRaGang = t.G_SanRaGang
                        };
                        //_context.Tbl_BM_16_GangLong.Add(clone);
                        _context.Tbl_BM_16_GangLong.Add(clone);
                        //await _context.SaveChangesAsync(); // hoặc SaveChanges sau loop
                        //thungDuocNhanIds.Add(clone.ID);
                        clones.Add(clone);
                        currentMaxSeq = nextSeq;
                    }
                    else
                    {
                        // Thùng chưa nhận -> cập nhật sang Đã nhận (và gán seq nếu cùng scope)
                        t.T_ID_TrangThai = (int)TinhTrang.DaNhan;
                        t.T_ID_NguoiNhan = payload.idNguoiNhan;
                        t.MaThungThep = maThungThep;
                        t.ID_LoThoi = payload.idLoThoi;
                        t.T_Ca = payload.idCa;
                        t.NgayLuyenThep = payload.ngayNhan;
                        t.T_ID_Kip = kip.ID_Kip;
                        t.ID_TTG = idThungTG;

                        if (t.T_ReceiveSeq == null &&
                            t.NgayLuyenThep == payload.ngayNhan &&
                            t.T_Ca == payload.idCa &&
                            t.ID_LoThoi == payload.idLoThoi)
                        {
                            t.T_ReceiveSeq = nextSeq;
                            currentMaxSeq = nextSeq;
                        }

                        thungDuocNhanIds.Add(t.ID);
                    }
                }

                await _context.SaveChangesAsync();
                thungDuocNhanIds.AddRange(clones.Select(x => x.ID));

                bool isSuccess = false;

                if (payload.selectedThungs.Count > 1 && thungDuocNhanIds.Count > 1)
                {
                    var dtoGopThung = new GopThungGangDto
                    {
                        IDs = thungDuocNhanIds,
                        PhongBan = "HRC1"
                    };

                    await this._chiaGangService.GopThungGangAsync(dtoGopThung, tenTaiKhoan);
                    isSuccess = true;
                }

                //await this._hrc1LuyenThepService.MocNoiAsync(
                //    thungDuocNhanIds,
                //    payload.idThungAT,
                //    payload.idLoThoi,
                //    payload.idCa,
                //    payload.ngayNhan
                //);

                var table = new DataTable();
                table.Columns.Add("ID_TTG", typeof(int));

                foreach (var ID in ttgEntitiesCanTinh.Where(x => x > 0))
                {
                    table.Rows.Add(ID);
                }

                var param = new SqlParameter("@ListID_TTG", table)
                {
                    TypeName = "dbo.TVP_ID_TTG",
                    SqlDbType = SqlDbType.Structured
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.SP_BM16_Calc_KLXiKR @ListID_TTG",
                    param
                );

                await _hrc1LuyenThepService.MocNoiAsync(
                    thungDuocNhanIds,
                    payload.idThungAT,
                    payload.idLoThoi,
                    payload.idCa,
                    payload.ngayNhan
                );

                /* =====================================================
                 *  🔴 STEP 4 – CHẠY LẠI PIPELINE CHO MẺ CON
                 * ===================================================== */
                if (!payload.IsChildRun)
                {
                    var query = GetBOFQuery(payload.idLoThoi);
                    var meCons = await query
                        .Where(x => x.ParentID == payload.idThungAT && x.IsChuyenMe != true)
                        .ToListAsync();

                    foreach (var meCon in meCons)
                    {
                        var clonedPayload = ClonePayloadForChild(payload, meCon.ID);

                        // 🔥 GỌI LẠI CHÍNH PIPELINE NÀY
                        await ExecuteInternalAsync(clonedPayload, tenTaiKhoan);
                    }
                }
            
        }

        private MocNoiThung ClonePayloadForChild(MocNoiThung src, int childMeId)
        {
            return new MocNoiThung
            {
                idThungAT = childMeId,
                idCa = src.idCa,
                idLoThoi = src.idLoThoi,
                ngayNhan = src.ngayNhan,
                selectedThungs = src.selectedThungs,
                idNguoiNhan = src.idNguoiNhan,
                NoiNhan = src.NoiNhan,
                thungTrungGian = src.thungTrungGian,
                IsChildRun = true
            };
        }

        private async Task<int> TaoThungTrungGian(DateTime ngayNhan, int idCa, int idLoThoi, string SoThungTG, int ID_NguoiNhan, string NoiNhan)
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
                NgayTaoTTG = DateTime.Now,
                NoiNhan = NoiNhan
            };
            _context.Tbl_BM_16_ThungTrungGian.Add(thungTrungGian);
            await _context.SaveChangesAsync();
            return thungTrungGian.ID;
        }
        private string GenerateMaThungThep(string maThungGang, DateTime ngayNhan, int loThoiId, int? caValue, int index)
        {
            string ca = caValue == (int)CaLamViec.Ngay ? "N" : "D";
            string dayStr = ngayNhan.Day.ToString("00");
            string indexStr = index.ToString("D2"); // dạng 2 chữ số: 00, 01, 02, ...
            return $"{maThungGang}.{dayStr}{ca}T{loThoiId}.{indexStr}";
        }
    }
}
