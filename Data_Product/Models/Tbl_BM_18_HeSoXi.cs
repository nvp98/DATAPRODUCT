using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_BM_18_HeSoXi
    {
        public int ID { get; set; }
        public int ID_LoCao { get; set; }

        public DateTime NgaySanXuat { get; set; }

        public int? Ca { get; set; }
        public int? Kip { get; set; }
        [Column(TypeName = "decimal(10,4)")]
        public decimal HeSoXi { get; set; }

        public int ID_NguoiXacNhan { get; set; }
        public DateTime ThoiGianXacNhan { get; set; }

        public int TrangThai { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
