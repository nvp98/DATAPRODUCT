using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BOF_ChuyenMe
    {
        [Key]
        public int ID { get; set; }
        public int TuLoID { get; set; }
        public int TuMeID { get; set; }
        public int DenLoID { get; set; }
        public int DenMeID { get; set; }

        public DateTime NgayTao { get; set; }
    }
}
