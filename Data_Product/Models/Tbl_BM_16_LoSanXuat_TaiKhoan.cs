using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_16_LoSanXuat_TaiKhoan
    {
        [Key]
        public int ID { get; set; }
        public int ID_TaiKhoan { get; set; }
        public int ID_LoSanXuat { get; set; }
    }
}
