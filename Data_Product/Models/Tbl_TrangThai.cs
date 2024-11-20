using System.ComponentModel.DataAnnotations;
namespace Data_Product.Models
{
    public class Tbl_TrangThai
    {
        [Key]
        public int ID_TrangThai { get; set; }
        public string? TenTrangThai { get; set; }
    }
}
