using System.ComponentModel.DataAnnotations;
namespace Data_Product.Models
{
    public class Tbl_DonViTinh
    {
        [Key]
        public int ID_DonViTinh { get; set; }
        public string? TenDonVi { get; set; }
    }
}
