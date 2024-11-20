using System.ComponentModel.DataAnnotations;
namespace Data_Product.Models
{
    public class Tbl_NhomVatTu
    {
        [Key]
        public int ID_NhomVatTu { get; set; }
        public string? MaNhom { get; set; }
        public string? TenNhomVatTu { get; set; }
        public int? ID_TrangThai { get; set; }
    }
}
