using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_BBGangLong_GangThoi
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

        public int? TinhTrang_BG { get; set; }
        public int? TinhTrang_HRC { get; set; }
        public int? TinhTrang_QLCL { get; set; }
        public int? TinhTrang_BBGN { get; set; }

        public DateTime? NgayXuly_BG { get; set; }
        public DateTime? NgayXuly_HRC { get; set; }
        public DateTime? NgayXuly_QLCL { get; set; }

        [StringLength(50)]
        public string? SoPhieu { get; set; }

        public int? ID_Kip { get; set; }

        [StringLength(1)]
        public string? Kip { get; set; }

        [StringLength(1)]
        public string? Ca { get; set; }

        public int? IDBBGL_Cu { get; set; }

        [StringLength(250)]
        public string? FileBBGL { get; set; }

        public DateTime? NgayTao { get; set; }

        [StringLength(250)]
        public string? NoiDungTrichYeu { get; set; }

        public int? ID_QuyTrinh { get; set; }
        [NotMapped]
        public int? ID_LOCAO { get; set; }
        [NotMapped]
        public int ID_TaiKhoan { get; set; }
        [NotMapped]
        public int ID_TaiKhoan_View { get; set; }
        [NotMapped]
        public DateTime Ngaytrinhky { get; set; }


    }
    

}
