using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data_Product.Models
{
    public class Tbl_ChiTiet_BienBanGiaoNhan
    {
        [Key]
        public int ID_CT_BBGN { get; set; }
        public int ID_VatTu { get; set; }
        [NotMapped]
        public string? TenVatTu { get; set; }
        [NotMapped]
        public string? DonViTinh { get; set; }
        public string? MaLo { get; set; }
        public double DoAm_W { get; set; }
        public double KhoiLuong_BG { get; set; }
        public double KL_QuyKho_BG { get; set; }
        public double KhoiLuong_BN { get; set; }
        public double KL_QuyKho_BN { get; set; }
        public string? GhiChu { get; set; }
        public int ID_BBGN { get; set; }
    }
}
