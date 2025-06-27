using System.Text.Json.Serialization;

namespace Data_Product.DTO.BM_16_DTO
{
    public class SaveThungDto
    {
        public string MaPhieu { get; set; }
        public int ID_Locao { get; set; }
        public int ID_Kip { get; set; }
        public DateTime NgayPhieuGang { get; set; }
        [JsonPropertyName("DanhSachThung")]
        public List<ThungGangDto> DanhSachThung { get; set; }
    }
}
