using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_LoThoi
    {
        [Key]
        public int ID { get; set; }
        public string TenLoThoi { get; set; }
        public DateTime NgayTao { get; set; }
        public string MaLoThoi { get; set; }
    }
}
