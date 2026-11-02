using Data_Product.DTO.BM_16_DTO;
using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Data_Product.Services
{
    public interface IBofKrWeightSyncService
    {
        Task<int> SyncAsync(DateTime from, DateTime to, CancellationToken ct = default);
    }

    public class BofKrWeightSyncService : IBofKrWeightSyncService
    {
        private static readonly int[] PlantNos = { 6, 7 };
        private const string ApiDateTimeFormat = "HH:mm:ss dd/MM/yyyy";

        private readonly HttpClient _httpClient;
        private readonly DataContext _context;
        private readonly ILogger<BofKrWeightSyncService> _logger;
        private readonly string _baseUrl;

        public BofKrWeightSyncService(HttpClient httpClient, IConfiguration config, DataContext context, ILogger<BofKrWeightSyncService> logger)
        {
            _httpClient = httpClient;
            _context = context;
            _logger = logger;
            _baseUrl = config["BofKrWeightApi:BaseUrl"]
                       ?? throw new InvalidOperationException("Missing BofKrWeightApi:BaseUrl");
        }

        public async Task<int> SyncAsync(DateTime from, DateTime to, CancellationToken ct = default)
        {
            var items = await FetchAsync(from, to, ct);
            return await InsertNewAsync(items, ct);
        }

        private async Task<List<BofKrWeightApiItemDto>> FetchAsync(DateTime from, DateTime to, CancellationToken ct)
        {
            var url = $"{_baseUrl}?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            using var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<List<BofKrWeightApiItemDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<BofKrWeightApiItemDto>();
        }

        // Dữ liệu API là append-only, không đổi giá trị sau khi đã trả về (đã xác nhận
        // nghiệp vụ) -> không cần update lại, chỉ insert bản ghi chưa có.
        // LƯU Ý: PRODUCT_NO KHÔNG đảm bảo đến theo đúng thứ tự tăng dần (mẻ số lớn có
        // thể nấu xong & đẩy dữ liệu lên trước mẻ số nhỏ hơn) -> KHÔNG được dùng
        // MAX(ReportNo) làm ngưỡng lọc (sẽ bỏ sót vĩnh viễn các mẻ đến trễ có số nhỏ
        // hơn ngưỡng). Phải check tồn tại theo đúng từng key (LoThoi, ReportNo).
        // Cần tạo index (khuyến khích UNIQUE) trên (LoThoi, ReportNo) ở DB để câu
        // IN(...) dưới đây seek thay vì scan toàn bảng.
        private async Task<int> InsertNewAsync(List<BofKrWeightApiItemDto> items, CancellationToken ct)
        {
            var validItems = items
                .Where(x => PlantNos.Contains(x.PLANT_NO) && !string.IsNullOrWhiteSpace(x.HEAT_ID))
                .ToList();

            if (validItems.Count == 0)
                return 0;

            var reportNos = validItems.Select(x => x.PRODUCT_NO).Distinct().ToList();
            var existingKeys = await _context.Tbl_BM_16_BOF_KR_Weight
                .Where(x => reportNos.Contains(x.ReportNo) && PlantNos.Contains(x.LoThoi))
                .Select(x => new { x.LoThoi, x.ReportNo })
                .ToListAsync(ct);
            var existingSet = existingKeys.Select(x => (x.LoThoi, x.ReportNo)).ToHashSet();

            var now = DateTime.Now;
            var toInsert = new List<Tbl_BM_16_BOF_KR_Weight>();

            foreach (var item in validItems)
            {
                if (!existingSet.Add((item.PLANT_NO, item.PRODUCT_NO)))
                    continue; // đã có trong DB (hoặc trùng trong cùng response) -> bỏ qua

                toInsert.Add(new Tbl_BM_16_BOF_KR_Weight
                {
                    ReportNo = item.PRODUCT_NO,
                    LoThoi = item.PLANT_NO,
                    MeThoi = item.HEAT_ID!,
                    SoThung = ParseIntOrNull(item.LADLE_NO),
                    Plant = item.PLANT,
                    ThoiGianBatDauBOF = ParseApiDateTime(item.DISCHARGE_DATE),
                    ThoiGianKetThucBOF = ParseApiDateTime(item.BOF_EVENT_TIME),
                    HmTime = null,
                    MacThep = null,

                    MeThoiKR = null,
                    ReportNoKR = null,
                    ThoiGianBatDauKR = null,
                    ThoiGianKetThucKR = ParseApiDateTime(item.KR_EVENT_TIME),
                    CanKR = item.KR_CRANE_NAME,
                    KhuVucKR = item.KR_AREA_CODE,
                    KLThungVaGangTruocKR = item.KR_WEIGHT_VALUE,

                    CanThoi = item.BOF_CRANE,
                    KLThungVaGangTruocBOF = item.BOF_WEIGHT_IN,
                    KLThung = item.BOF_WEIGHT_OUT,
                    KLChenhLech = null,
                    KLGangLongBOF = item.BOF_CHARGED,

                    Kip = item.SHIFT,
                    Ca = item.KIP,

                    NgaySanXuat = (ParseApiDateTime(item.DISCHARGE_DATE) ?? ParseApiDateTime(item.KR_EVENT_TIME))?.Date,
                    NgayDongBo = now
                });
            }

            if (toInsert.Count == 0)
                return 0;

            _context.Tbl_BM_16_BOF_KR_Weight.AddRange(toInsert);
            await _context.SaveChangesAsync(ct);
            return toInsert.Count;
        }

        private static int? ParseIntOrNull(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
        }

        private static DateTime? ParseApiDateTime(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return DateTime.TryParseExact(raw, ApiDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)
                ? dt
                : null;
        }
    }
}
