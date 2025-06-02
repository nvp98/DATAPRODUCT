using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Data_Product.DTO.BM_16_DTO
{
    public class PhieuCreateDto
    {
        public int ID_Locao { get; set; }
        public int ID_Kip { get; set; }
        [JsonPropertyName("ThungGangs")]
        public List<ThungGangDto> DanhSachThung { get; set; }
    }
}
