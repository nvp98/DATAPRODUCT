using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_NhatKy_CumTB_Xuong
    {
        public int ID { get; set; }
        public int CumTB_ID { get; set; }
        public int Xuong_ID { get; set; }
    }
}
