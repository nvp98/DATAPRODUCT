using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_TrinhKyBoSung
    {
        [Key]
        public int ID_TrinhKy { get; set; }
        public int ID_BBGN { get; set; }
        [NotMapped]
        public string SoPhieu { get; set; }
        public int ID_TaiKhoan { get; set; }
        public int ID_TaiKhoan_View { get; set; }
        public DateTime? NgayTrinhKy { get; set; }
        public  Nullable<DateTime> NgayXuLy { get; set; }
        
        public int? ID_TrangThai { get; set; }
    }
}
