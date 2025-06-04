using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_MeThoi
    {
        [Key]
        public int ID { get; set; }
        public DateTime NgayTao { get; set; }
        public string MaMeThoi { get; set; }
        public int ID_NguoiTao { get; set; }
        public int ID_TrangThai { get; set; }
        public int ID_LoThoi { get; set; }

        [NotMapped]
        public string? MaLoThoi { get; set; }
    }
}
