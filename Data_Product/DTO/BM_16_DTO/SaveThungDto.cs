using System.Text.Json.Serialization;

namespace Data_Product.DTO.BM_16_DTO
{
    public class SaveThungDto
    {
        public int PhieuID { get; set; }
        [JsonPropertyName("ThungGangs")]
        public List<ThungGangDto> DanhSachThung { get; set; }
    }
}
