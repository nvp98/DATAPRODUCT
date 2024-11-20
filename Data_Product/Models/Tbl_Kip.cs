using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_Kip
    {
        [Key]
        public int ID_Kip { get; set; }
        public DateTime? NgayLamViec { get; set; }
        public string? TenKip { get; set; }
        public string? TenCa { get; set; }
        [NotMapped]
        public Nullable<DateTime> TuNgay { get; set; }
        [NotMapped]
        public Nullable<DateTime> DenNgay { get; set; }
    }
}
