using Data_Product.Models;

namespace Data_Product.DTO.BM_18_DTO
{
    public class BM18DetailViewModel
    {
        public Tbl_BM_18_Phieu Phieu { get; set; } = null!;
        public List<Tbl_BM_18_XiHatLoCao> ChiTiet { get; set; } = new();
    }
}
