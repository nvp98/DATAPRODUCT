using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Data_Product.DTO.BM_16_DTO
{
    public class PhieuCreateDto
    {
        public int ID_Locao { get; set; }
        public int ID_Kip { get; set; }
        public DateTime NgayPhieuGang { get; set; }
        [JsonPropertyName("ThungGangs")]
        public List<ThungGangDto> DanhSachThung { get; set; }
    }
    public class PhieuViewModel
    {
        public string MaPhieu { get; set; }
        public DateTime NgayTaoPhieu { get; set; }
        public DateTime NgayPhieuGang { get; set; }
        public string? TenNguoiTao { get; set; }
        public string? TenCa { get; set; }
        public string? TenLoCao { get; set; } 
        public String? ThoiGianTao { get; set; }
    }


}
