using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_BM_18_XiHatLoCao
    {
        [Key]
        public int ID { get; set; }

        public int? Ca { get; set; }
        public int? Kip { get; set; }
        public DateTime? NgaySanXuat { get; set; }
        public string MaPhieu { get; set; }
        public int ID_LoCao { get; set; }

        public string? Ten_NVL { get; set; }
        public string? DVT { get; set; }
        public int? ID_Lo  { get; set; }

        [Column(TypeName = "decimal(10,4)")]
        public decimal? HeSo { get; set; }
        [Column(TypeName = "decimal(10,3)")]
        public decimal? KL_Gang_Giao { get; set; }
        [Column(TypeName = "decimal(10,3)")]
        public decimal? KL_Xi_Giao { get; set; }
        [Column(TypeName = "decimal(10,3)")]
        public decimal? KL_Gang_Nhan { get; set; }
        [Column(TypeName = "decimal(10,3)")]
        public decimal? KL_Xi_Nhan { get; set; }

        public string? GhiChu { get; set; }
        [NotMapped]
        public string TenMaLo { get; set; }
        [NotMapped]
        public string CaKip { get; set; }
    }
}
