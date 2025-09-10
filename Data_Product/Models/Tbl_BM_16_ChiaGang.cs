using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_16_ChiaGang
    {
        [Key]
        public int ID { get; set; }
        public int? ID_Thung { get; set; }
        public string MaChiaGang { get; set; }
        public string MaThungGang { get; set; }
        public string MaThungThep { get; set; }
        public decimal? PhanTram { get; set; }
        public decimal? KLGangChia { get; set; }
        public int? ID_NguoiChia { get; set; }
    }
}
