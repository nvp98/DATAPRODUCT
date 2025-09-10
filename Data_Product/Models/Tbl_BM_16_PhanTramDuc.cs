using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_16_PhanTramDuc
    {
        [Key]
        public int ID { get; set; }
        public int PhanTram { get; set; }
    }
}
