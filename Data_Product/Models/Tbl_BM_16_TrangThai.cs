using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_16_TrangThai
    {
        [Key]
        public int ID { get; set; }
        public string TenTrangThai { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
