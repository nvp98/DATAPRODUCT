using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_XuLyXoaPhieu
    {
        [Key]
        public int ID { get; set; }
        public int ID_BBGN { get; set; }
        public int ID_TaiKhoanBN { get; set; }
        public DateTime? NgayXuLy_BN { get; set; }
        public int TinhTrang_BN { get; set; }
        public int ID_TaiKhoanKH { get; set; }
        public DateTime? NgayXuLy_KH { get; set; }
        public int TinhTrang_KH { get; set; }
        public int ID_TrangThai { get; set; }
        public int? ID_TaiKhoanKH_View { get; set; }
    }
}
