using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Data_Product.Models
{
    public class Tbl_BM_16_Phieu
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(20)]
        public string MaPhieu { get; set; } = string.Empty;

        [Required]
        public DateTime NgayTaoPhieu { get; set; }

        [Required]
        public DateTime ThoiGianTao { get; set; }

        [Required]
        public int ID_Locao { get; set; }

        [Required]
        public int ID_Kip { get; set; }

        [Required]
        public int ID_NguoiTao { get; set; }
        [Required]
        public DateTime NgayPhieuGang { get; set; }

    } 
}
