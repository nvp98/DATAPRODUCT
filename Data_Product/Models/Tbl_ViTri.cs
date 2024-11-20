using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Data_Product.Models
{
    public class Tbl_ViTri
    {
        [Key]
        public int ID_ViTri { get; set; }
        public string? TenViTri { get; set; }
        public int ID_TrangThai { get; set; }
    }

}
