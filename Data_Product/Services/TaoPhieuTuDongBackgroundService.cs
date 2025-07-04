using Data_Product.Models;
using Data_Product.Repositorys;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore;

namespace Data_Product.Services
{
    public class TaoPhieuTuDongBackgroundService : BackgroundService
    {
        private readonly DataContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TaoPhieuTuDongBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public TaoPhieuTuDongBackgroundService(IServiceProvider serviceProvider, ILogger<TaoPhieuTuDongBackgroundService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int demLanChay = 1;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("========== [Lần chạy #{demLanChay}] ==========", demLanChay);
                    GhiLogFile($"========== [Lần chạy #{demLanChay}] ==========");

                    _logger.LogInformation("===> [TaoPhieuTuDong] Bắt đầu lúc: {time}", DateTime.Now);
                    GhiLogFile($"========== [TaoPhieuTuDong] Bắt đầu lúc: {DateTime.Now}");

                    await TaoPhieuVaNapThungAsync();

                    _logger.LogInformation("===> [TaoPhieuTuDong] Hoàn thành lúc: {time}", DateTime.Now);
                    GhiLogFile($"========== [TaoPhieuTuDong] Hoàn thành lúc: {DateTime.Now}");
                    _logger.LogInformation("====================================================");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Đã xảy ra lỗi trong quá trình tạo phiếu tự động.");
                    GhiLogFile($"[ERROR] {DateTime.Now} - {ex.Message}\n{ex.StackTrace}");
                }

                demLanChay++;
                _logger.LogInformation($"[Loop Tick] {DateTime.Now}");

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Bỏ qua nếu token bị hủy trong thời gian chờ
                    break;
                }
            }

        }

        //private async Task TaoPhieuVaNapThungAsync()
        //{
        //    using var scope = _serviceProvider.CreateScope();
        //    var _context = scope.ServiceProvider.GetRequiredService<DataContext>();


        //    //var BKMIS_MySql = "Server=your-server;Database=your-db;User Id=your-user;Password=your-pass;Port=3306;";

        //    var connectionString = "Server=10.192.215.11;Database=bkmis_kcshpsdq;User Id=viewkcs;Password=viewkcs@2024;Port=3307;";

        //    DateTime now = DateTime.Now;
        //    TimeSpan startCa1 = new TimeSpan(8, 0, 0);
        //    string tenCa = now.TimeOfDay >= startCa1 && now.TimeOfDay < new TimeSpan(20, 0, 0) ? "1" : "2";
        //    DateTime ngayLamViec = now.TimeOfDay < startCa1 ? now.Date.AddDays(-1) : now.Date;
        //    string ngayStr = ngayLamViec.ToString("yyyy-MM-dd");

        //    var kip = await _context.Tbl_Kip.FirstOrDefaultAsync(x =>
        //        x.NgayLamViec.Value.Date == ngayLamViec && x.TenCa == tenCa);

        //    if (kip == null)
        //    {
        //        _logger.LogWarning("Không tìm thấy ca/kíp cho {ngay}", ngayLamViec);
        //        return;
        //    }

        //    int idKip = kip.ID_Kip;
        //    string caKipCode = kip.TenCa + kip.TenKip;

        //    for (int idLoCao = 1; idLoCao <= 5; idLoCao++)
        //    {
        //        // Lấy khối lượng xe goòng theo lò cao
        //        var xeGoong = await _context.Tbl_XeGoong.FirstOrDefaultAsync(x => x.ID_LoCao == idLoCao);
        //        decimal? klxegoong = xeGoong?.KL_Xe;

        //        bool daCoPhieu = await _context.Tbl_BM_16_Phieu.AnyAsync(p =>
        //            p.ID_Locao == idLoCao && p.ID_Kip == idKip && p.NgayPhieuGang.Date == ngayLamViec);

        //        string maPhieu = $"GL-LG-L{idLoCao}{caKipCode}{ngayLamViec:yyMMdd}";

        //        // Nếu phiếu chưa có -> tạo mới
        //        if (!daCoPhieu)
        //        {
        //            var phieu = new Tbl_BM_16_Phieu
        //            {
        //                MaPhieu = maPhieu,
        //                NgayTaoPhieu = DateTime.Today,
        //                ThoiGianTao = DateTime.Now,
        //                ID_Locao = idLoCao,
        //                ID_Kip = idKip,
        //                NgayPhieuGang = ngayLamViec
        //            };

        //            await _context.Tbl_BM_16_Phieu.AddAsync(phieu);
        //            await _context.SaveChangesAsync();
        //            _logger.LogInformation("Tạo phiếu: {maPhieu}", maPhieu);
        //        }
        //        // Gọi dữ liệu BKMIS
        //        var danhSachThung = await GetSoMeGangFromBKMIS(idLoCao, caKipCode, ngayStr, connectionString);
        //        int? soCa = int.TryParse(kip.TenCa, out int result) ? result : null;

        //        // Lấy danh sách đã có để kiểm tra trùng mã số mẻ
        //        var bkmisSoMeDaCo = await _context.Tbl_BM_16_GangLong
        //            .Where(x => x.MaPhieu == maPhieu)
        //            .Select(x => x.BKMIS_SoMe)
        //            .ToListAsync();

        //        int sttThung = 1;

        //        foreach (var thung in danhSachThung)
        //        {
        //            if (bkmisSoMeDaCo.Contains(thung.TestPatternCode))
        //            {
        //                _logger.LogInformation("Bỏ qua thùng đã tồn tại: {code}", thung.TestPatternCode);
        //                continue;
        //            }

        //            var soMe = thung.TestPatternCode;
        //            string? thungSo = soMe is { Length: >= 2 } ? soMe[^2..] : soMe;
        //            var maThungGang = GenerateMaThung(ngayLamViec, idLoCao, kip.TenCa, sttThung);

        //            var thungGang = new Tbl_BM_16_GangLong
        //            {
        //                MaPhieu = maPhieu,
        //                MaThungGang = maThungGang,
        //                BKMIS_SoMe = thung.TestPatternCode,
        //                BKMIS_ThungSo = thungSo,
        //                BKMIS_Gio = thung.Patterntime?.ToString(),
        //                BKMIS_PhanLoai = thung.ClassifyName,
        //                Gio_NM = null,
        //                ChuyenDen = null,
        //                G_ID_TrangThai = 1,
        //                ID_TrangThai = 1,
        //                NgayLuyenGang = ngayLamViec,
        //                NgayTao = DateTime.Now,
        //                ID_Locao = idLoCao,
        //                G_ID_Kip = idKip,
        //                G_Ca = soCa,
        //                G_ID_NguoiLuu = 1,
        //                T_copy = false,
        //                KL_XeGoong = klxegoong
        //            };

        //            await _context.Tbl_BM_16_GangLong.AddAsync(thungGang);
        //            sttThung++;
        //        }

        //        await _context.SaveChangesAsync();
        //        _logger.LogInformation("Đã nạp thêm {count} thùng mới cho phiếu {maPhieu}", danhSachThung.Count, maPhieu);
        //    }
        //}
        private async Task TaoPhieuVaNapThungAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var connectionString = "Server=10.192.215.11;Database=bkmis_kcshpsdq;User Id=viewkcs;Password=viewkcs@2024;Port=3307;";

            DateTime now = DateTime.Now;
            TimeSpan startCa1 = new TimeSpan(8, 0, 0);
            string tenCa = now.TimeOfDay >= startCa1 && now.TimeOfDay < new TimeSpan(20, 0, 0) ? "1" : "2";
            DateTime ngayLamViec = now.TimeOfDay < startCa1 ? now.Date.AddDays(-1) : now.Date;
            string ngayStr = ngayLamViec.ToString("yyyy-MM-dd");

            var kip = await _context.Tbl_Kip.FirstOrDefaultAsync(x =>
                x.NgayLamViec.Value.Date == ngayLamViec && x.TenCa == tenCa);

            GhiLogFile($"[INFO] Tìm thấy ca/kíp: Ngày {ngayLamViec:yyyy-MM-dd}, Ca {kip.TenCa}, Kíp {kip.TenKip}, ID_Kip {kip.ID_Kip}");
            if (kip == null)
            {
                _logger.LogWarning("Không tìm thấy ca/kíp cho {ngay}", ngayLamViec);
                GhiLogFile($"Không tìm thấy ca/kíp cho ngày {ngayLamViec}");
                return;
            }

            int idKip = kip.ID_Kip;
            string caKipCode = kip.TenCa + kip.TenKip;
            int? soCa = int.TryParse(kip.TenCa, out int result) ? result : null;

            for (int idLoCao = 1; idLoCao <= 5; idLoCao++)
            {
                var xeGoong = await _context.Tbl_XeGoong.FirstOrDefaultAsync(x => x.ID_LoCao == idLoCao);
                decimal? klxegoong = xeGoong?.KL_Xe;

                string maPhieu = $"GL-LG-L{idLoCao}{caKipCode}{ngayLamViec:yyMMdd}";

                var phieu = await _context.Tbl_BM_16_Phieu.FirstOrDefaultAsync(p =>
                    p.ID_Locao == idLoCao && p.ID_Kip == idKip && p.NgayPhieuGang.Date == ngayLamViec);

                if (phieu == null)
                {
                    phieu = new Tbl_BM_16_Phieu
                    {
                        MaPhieu = maPhieu,
                        NgayTaoPhieu = DateTime.Today,
                        ThoiGianTao = DateTime.Now,
                        ID_Locao = idLoCao,
                        ID_Kip = idKip,
                        NgayPhieuGang = ngayLamViec
                    };

                    await _context.Tbl_BM_16_Phieu.AddAsync(phieu);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Tạo phiếu: {maPhieu}", maPhieu);
                }

                var danhSachThung = await GetSoMeGangFromBKMIS(idLoCao, caKipCode, ngayStr, connectionString);

                var thungDaCoDict = await _context.Tbl_BM_16_GangLong
                    .Where(x => x.MaPhieu == maPhieu)
                    .ToDictionaryAsync(x => x.BKMIS_SoMe);

                int sttThung = thungDaCoDict.Count + 1;

                foreach (var thung in danhSachThung)
                {
                    if (thungDaCoDict.TryGetValue(thung.TestPatternCode, out var thungDaCo))
                    {
                        if (thungDaCo.G_ID_TrangThai == 1 && thungDaCo.ID_TrangThai == 1 && (thungDaCo.XacNhan == false || thungDaCo.XacNhan == null))
                        {
                            thungDaCo.BKMIS_ThungSo = thung.TestPatternCode.Length >= 2 ? thung.TestPatternCode[^2..] : thung.TestPatternCode;
                            thungDaCo.BKMIS_Gio = thung.Patterntime?.ToString();
                            thungDaCo.BKMIS_PhanLoai = thung.ClassifyName;
                            _context.Update(thungDaCo);
                        }
                        else
                        {
                            _logger.LogInformation("Bỏ qua thùng đã xác nhận hoặc chuyển: {code}", thung.TestPatternCode);
                            GhiLogFile($"[INFO] Bỏ qua thùng đã xác nhận hoặc chuyển: {thung.TestPatternCode}");
                        }
                        continue;
                    }

                    var thungSo = thung.TestPatternCode.Length >= 2 ? thung.TestPatternCode[^2..] : thung.TestPatternCode;
                    var maThungGang = GenerateMaThung(ngayLamViec, idLoCao, kip.TenCa, sttThung);

                    var thungGang = new Tbl_BM_16_GangLong
                    {
                        MaPhieu = maPhieu,
                        MaThungGang = maThungGang,
                        BKMIS_SoMe = thung.TestPatternCode,
                        BKMIS_ThungSo = thungSo,
                        BKMIS_Gio = thung.Patterntime?.ToString(),
                        BKMIS_PhanLoai = thung.ClassifyName,
                        Gio_NM = null,
                        ChuyenDen = null,
                        G_ID_TrangThai = 1,
                        T_ID_TrangThai = 2,
                        ID_TrangThai = 1,
                        NgayLuyenGang = ngayLamViec,
                        NgayTao = DateTime.Now,
                        ID_Locao = idLoCao,
                        G_ID_Kip = idKip,
                        G_Ca = soCa,
                        G_ID_NguoiLuu = null,
                        T_copy = false,
                        KL_XeGoong = klxegoong
                    };

                    await _context.Tbl_BM_16_GangLong.AddAsync(thungGang);
                    sttThung++;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Đã nạp/cập nhật {count} thùng cho phiếu {maPhieu}", danhSachThung.Count, maPhieu);
                GhiLogFile($"[INFO] Đã nạp/cập nhật {danhSachThung.Count} thùng cho phiếu {maPhieu}");
            }
        }
        private async Task<List<Bkmis_view>> GetSoMeGangFromBKMIS(int idLoCao, string caKipCode, string ngay, string connectionString)
        {
            var result = new List<Bkmis_view>();
            string table = idLoCao switch
            {
                1 => "view_dq1_lg_daura_lc1",
                2 => "view_dq1_lg_daura_lc2",
                3 => "view_dq1_lg_daura_lc3",
                4 => "view_dq1_lg_daura_lc4",
                5 => "view_dq2_lg_daura_lc5",
                _ => ""
            };

            if (string.IsNullOrEmpty(table))
            {
                _logger.LogWarning("Không xác định được bảng tương ứng với ID_LoCao = {idLoCao}", idLoCao);
                return result;
            }

            string query = "SELECT TestPatternCode,ClassifyName,ProductionDate,ShiftName,InputTime,Patterntime,TestPatternName " +
                           $"FROM bkmis_kcshpsdq.{table} " +
                           $"WHERE bkmis_kcshpsdq.{table}.ProductionDate = '{ngay}' " +
                           $"AND bkmis_kcshpsdq.{table}.ShiftName = '{caKipCode}'";

            _logger.LogInformation("Thực thi truy vấn MySQL: {query}", query);
            GhiLogFile($"Thực thi truy vấn MySQL: {query}");

            try
            {
                using var conn = new MySqlConnection(connectionString);
                await conn.OpenAsync();

                using var cmd = new MySqlCommand(query, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new Bkmis_view
                    {
                        TestPatternCode = reader["TestPatternCode"]?.ToString(),
                        ClassifyName = reader["ClassifyName"]?.ToString(),
                        ProductionDate = reader["ProductionDate"]?.ToString(),
                        ShiftName = reader["ShiftName"]?.ToString(),
                        InputTime = reader["InputTime"]?.ToString(),
                        Patterntime = reader["Patterntime"]?.ToString(),
                        TestPatternName = reader["TestPatternName"]?.ToString(),
                    });
                }

                _logger.LogInformation("Đã đọc {count} dòng từ BKMIS bảng {table}", result.Count, table);
                GhiLogFile($"[INFO] Đã đọc {result.Count} dòng từ BKMIS bảng {table}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đọc dữ liệu BKMIS từ bảng {table}", table);
                GhiLogFile($"[ERROR] Lỗi khi đọc dữ liệu BKMIS từ bảng {table} | Exception: {ex.Message}");
            }

            return result;
        }

        private string GenerateMaThung(DateTime ngay, int loCao, string ca, int stt)
        {
            var dd = ngay.Day.ToString("D2");
            var mm = ngay.Month.ToString("D2");
            var yy = ngay.Year.ToString().Substring(2); // lấy 2 số cuối

            var caChar = (ca == "1") ? "N" : "D";
            var sttFormatted = stt.ToString("D3");

            return $"{dd}{mm}{yy}L{loCao}{caChar}{sttFormatted}.00";
        }
        private void GhiLogFile(string message)
        {
            string basePath = AppContext.BaseDirectory; // Luôn trỏ đúng thư mục đang chạy, ổn định hơn
            string logDir = Path.Combine(basePath, "logs");
            Directory.CreateDirectory(logDir); // Tạo thư mục nếu chưa tồn tại

            string filePath = Path.Combine(logDir, $"log_{DateTime.Now:yyyyMMdd}.txt");
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}";

            File.AppendAllText(filePath, logEntry + Environment.NewLine);


        }

    }
}
