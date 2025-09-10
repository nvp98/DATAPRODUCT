using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_LoCao
    {
        [Key]
        public int ID { get; set; }
        public string TenLoCao { get; set; }
        public int ID_PhongBan { get; set; }
    }
}
