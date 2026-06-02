using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.Json.Serialization;

namespace Data_Product.DTO
{
    public class KhoiLuongGangDto
    {
        [JsonPropertyName("so_me")]
        public string SO_ME { get; set; }

        [JsonPropertyName("ngay")]
        public DateTime NGAY_TAO { get; set; }

        [JsonPropertyName("kl_gang_nhap")]
        public decimal? G_KLGANGLONG { get; set; }

        [JsonPropertyName("id_locao")]
        public int ID_LOCAO { get; set; }

        [JsonPropertyName("kl_gang_thoi")] // Đặt tên JSON nếu cần
        public decimal? KLGang_Thoi { get; set; }

        [JsonPropertyName("nhiet_do")]
        public decimal? NhietDo { get; set; }


        [JsonPropertyName("nhiet_do_lg")]
        public int? NhietDo_LG { get; set; }

        // Dùng JsonIgnore để không xuất hiện ở JSON cha
        [JsonIgnore]
        public decimal? KLGangTheoMe { get; set; }

        [JsonIgnore]
        public string? MaMeThoi { get; set; }

        [JsonPropertyName("MeThoi")]
        public List<MeThoiDto> MeThoi { get; set; } = new();
    }
    public class MeThoiDto
    {
        [JsonPropertyName("kl_gang_thoi_theo_me")]
        public decimal? KLGangTheoMe { get; set; }

        [JsonPropertyName("maMeThoi")]
        public string? MaMeThoi { get; set; }
    }
}
