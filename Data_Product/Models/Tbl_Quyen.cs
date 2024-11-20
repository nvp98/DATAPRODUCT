using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_Quyen
    {
        [Key]
        public int ID_Quyen { get; set; }
        public string? TenQuyen { get; set; }
    }
}
