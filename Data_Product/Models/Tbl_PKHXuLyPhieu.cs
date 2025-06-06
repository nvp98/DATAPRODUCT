using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_PKHXuLyPhieu
    {
        [Key]
        public int ID { get; set; }
        public int ID_TaiKhoan { get; set; }
        public int ID_BBGN { get; set; }
        public int ID_NKDSX { get; set; }
        public DateTime NgayXuLy { get; set; }
        public bool TinhTrang { get; set; }
    }
}
