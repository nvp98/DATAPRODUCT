using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_KL_THUNGXE
    {
        [Key]
        public int ID { get; set; }

        public int ID_LOCAO { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khối lượng xe thùng.")]
        //[RegularExpression(@"^\d*\.?\d+$", ErrorMessage = "Vui lòng nhập số.")]

        public double KL_THUNGXE { get; set; }
        [NotMapped] // EF Core attribute to indicate this isn’t in the DB table
        public string TenLoCao { get; set; }

    }
}
