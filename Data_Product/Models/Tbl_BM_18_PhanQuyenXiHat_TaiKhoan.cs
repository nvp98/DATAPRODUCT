using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_18_PhanQuyenXiHat_TaiKhoan
    {
        [Key]
        public int ID { get; set; }
        public int ID_TaiKhoan { get; set; }
        public int ID_LoSanXuat { get; set; }
    }
}
