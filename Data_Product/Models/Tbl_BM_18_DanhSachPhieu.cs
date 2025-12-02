using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_18_Phieu
    {
        [Key]
        public int ID { get; set; }

        public string MaPhieu { get; set; }  // nvarchar(25)

        public DateTime NgayTaoPhieu { get; set; }

        public int ID_Locao { get; set; }

        public int ID_Kip { get; set; }

        public int ID_NguoiTao { get; set; }

        public DateTime NgayPhieu { get; set; }

        public int ID_NguoiGiao { get; set; }

        public int ID_NguoiNhan { get; set; }
    }
}
