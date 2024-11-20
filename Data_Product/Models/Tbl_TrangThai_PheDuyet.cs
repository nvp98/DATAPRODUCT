using System.ComponentModel.DataAnnotations;
namespace Data_Product.Models
{
    public class Tbl_TrangThai_PheDuyet
    {
        [Key]
        public int ID_TrangThai_PheDuyet { get; set; }
        public string? TenTrangThai { get; set; }
    }
}
