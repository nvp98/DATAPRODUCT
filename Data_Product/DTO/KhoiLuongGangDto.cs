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

        public decimal? KLGang_Thoi {get; set; }

        public decimal? NhietDo { get; set; }

        [JsonPropertyName("Tong_KLGangNhan")]
        public decimal? KlGangNhan { get; set; }

        public string? MaMeThoi { get; set; }
    }
}
