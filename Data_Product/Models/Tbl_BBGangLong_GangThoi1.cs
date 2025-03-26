using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BBGangLong_GangThoi1
    {
        [Key]
        public int ID_BBGL { get; set; }
        public int ID_NhanVien_BG { get; set; }
            public int ID_PhongBan_BG { get; set; }
            public int ID_Xuong_BG { get; set; }
            public int ID_ViTri_BG { get; set; }

            
            public int? ID_NhanVien_HRC { get; set; }
            public int? ID_PhongBan_HRC { get; set; }
            public int? ID_Xuong_HRC { get; set; }
            public int? ID_ViTri_HRC { get; set; }

            public int? ID_NhanVien_QLCL { get; set; }
            public int? ID_PhongBan_QLCL { get; set; }

            public int? ID_Xuong_QLCL { get; set; }
            public int? ID_ViTri_QLCL { get; set; }
            public DateTime? NgayXuly_BG { get; set; }

            public int? ID_Kip { get; set; }
            public string? Kip { get; set; }
            public string? Ca { get; set; }

            public string? NoiDungTrichYeu { get; set; }
    }
}
