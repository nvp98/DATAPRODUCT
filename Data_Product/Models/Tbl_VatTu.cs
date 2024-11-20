using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Data_Product.Models
{
    public class Tbl_VatTu
    {
        [Key]
        public int ID_VatTu { get; set; }
        public string? TenVatTu { get; set; }
        public string? MaVatTu_Sap { get; set; }
        public string? TenVatTu_Sap { get; set; }
        public string? DonViTinh { get; set; }
        public int? ID_NhomVatTu { get; set; }
        [NotMapped]
        public string? TenNhomVatTu { get; set; }
        public string? PhongBan { get; set; }
        public int? ID_TrangThai { get; set; }
        [NotMapped]
        public string[] ID_PhongBan { get; set; }


    }
}
