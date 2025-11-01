using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_16_PhanBoGangCR
    {
        [Key]
        public int ID { get; set; }
        public int ID_GangLong { get; set; }
        public int ID_TTG_Target { get; set; }
        public string MaThungGang { get; set; }
        public string MaThungTG {  get; set; }
        public bool IsTTGCopy { get; set; }
        public decimal? TyLeTrongMaTTG { get; set; }
        public decimal? KL_PhanBo_CR { get; set; }
        public bool? IsSaiChuyenDen { get; set; }
    }
}
