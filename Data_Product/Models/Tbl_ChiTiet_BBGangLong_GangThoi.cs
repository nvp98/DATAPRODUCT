using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_ChiTiet_BBGangLong_GangThoi
    {
        [Key]
        public int ID { get; set; }

        [Column("ID_BBGL")]
        public int Id_BBGL { get; set; }

        [Column("SoMeGang")]
        public string SoMe { get; set; }

        public string ThungSo { get; set; }

        [Column("KL_XeGoong")]
        public double? KhoiLuongXeGoong { get; set; }

        [Column("KL_Thung")]
        public double? KhoiLuongThung { get; set; }

        [Column("KL_ThungGang")]
        public double? KLThungGangLong { get; set; }

        [Column("KL_Gang_Ray")]
        public double? KLGangLongCanRay { get; set; }

        [Column("VanChuyen_Thep")]
        public int VanChuyenHRC1 { get; set; }

        [Column("VanChuyen_Gang")]
        public int VanChuyenHRC2 { get; set; }

        public string PhanLoai { get; set; }

        [NotMapped]
        public string Gio { get; set; }

        public string GhiChu { get; set; }

    }
 
   
}
