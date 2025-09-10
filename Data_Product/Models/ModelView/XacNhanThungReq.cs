using Data_Product.DTO.BM_16_DTO;

namespace Data_Product.Models.ModelView
{
    public class XacNhanThungReq
    {
            public string? MaPhieu { get; set; }
            public List<ThungGangDto>? DsMaThung { get; set; }
    }
}
