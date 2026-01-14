using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_MocNoiThungGangAT
    {
        [Key]
        public int ID { get; set; }
        public int Ca { get; set; }
        public int IdLoThoi { get; set; }
        public DateTime NgayMocNoi { get; set; }
        public int IdThungGang { get; set; }
        public int IdThungGangAT { get; set; }
    }
}
