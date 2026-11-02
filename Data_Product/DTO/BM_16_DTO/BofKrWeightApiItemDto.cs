using System.Text.Json.Serialization;

namespace Data_Product.DTO.BM_16_DTO
{
    // Khớp 1 item trong response list của API nhà máy:
    // GET /api/reports/bof-heat-crane-weight/view?from=yyyy-MM-dd&to=yyyy-MM-dd
    // AllowReadingFromString vì API trả lẫn lộn số dạng chuỗi ("LADLE_NO": "66", "KIP": "1") và số thuần.
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public class BofKrWeightApiItemDto
    {
        public int PRODUCT_NO { get; set; }
        public string? PLANT { get; set; }
        public string? HEAT_ID { get; set; }
        public string? LADLE_NO { get; set; }
        public string? DISCHARGE_DATE { get; set; }
        public string? SHIFT { get; set; }
        public int? KIP { get; set; }
        public int? BOF_CRANE { get; set; }
        public int PLANT_NO { get; set; }
        public int? BOF_CHARGE_POSITION { get; set; }
        public string? BOF_EVENT_TIME { get; set; }
        public decimal? BOF_WEIGHT_IN { get; set; }
        public decimal? BOF_WEIGHT_OUT { get; set; }
        public decimal? BOF_CHARGED { get; set; }
        public int? KR_CRANE_NAME { get; set; }
        public int? KR_AREA_CODE { get; set; }
        public int? KR_CHARGE_POSITION { get; set; }
        public string? KR_EVENT_TIME { get; set; }
        public decimal? KR_WEIGHT_VALUE { get; set; }
        public decimal? KR_SLAG { get; set; }
    }
}
