using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_NhatKy_CumTB
    {
        public int ID { get; set; }
        public string TenCumTB { get; set; }
        public bool IsLock { get; set; }
        public bool IsDelete { get; set; }
        [NotMapped]
        public List<Tbl_Xuong> Xuong { get; set; }
    }
}
