using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_CanRayLG1
    {
        [Key]
        public int ID { get; set; }

        public int? ID_LoCao { get; set; }

        public DateTime? Gio { get; set; }

        public int? ThungSo { get; set; }

        public int? Ray { get; set; }

        public int? SanRaGang { get; set; }

        public decimal? KL_Bi { get; set; }       
        public decimal? KL_Tong { get; set; }   
        public decimal? KL_Gang { get; set; }      

        public string? BKMIS_SoMe { get; set; }
    }
}
