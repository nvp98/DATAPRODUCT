using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    
    public class Tbl_BM_16_GangLong
    {
        [Key]
        public int ID { get; set; }

        [StringLength(20)]
        public string? MaPhieu { get; set; }

        [StringLength(20)]
        public string? MaThungGang { get; set; }

        [StringLength(20)]
        public string? BKMIS_SoMe { get; set; }

        [StringLength(10)]
        public string? BKMIS_ThungSo { get; set; }

        [StringLength(10)]
        public string? BKMIS_Gio { get; set; }

        [StringLength(20)]
        public string? BKMIS_PhanLoai { get; set; }
        [Column(TypeName = "decimal(5, 2)")]
        public decimal? KL_XeGoong { get; set; }

        public DateTime? NgayLuyenGang { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? G_KLThungChua { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? G_KLThungVaGang { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? G_KLGangLong { get; set; }

        [StringLength(20)]
        public string? ChuyenDen { get; set; }

        [StringLength(10)]
        public string? Gio_NM { get; set; }

        [StringLength(255)]
        public string? G_GhiChu { get; set; }

        public DateTime? NgayLuyenThep { get; set; }

        public bool? KR { get; set; }

        [StringLength(20)]
        public string? MaThungThep { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? T_KLThungVaGang { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? T_KLThungChua { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? T_KLGangLong { get; set; }

        public int? ThungTrungGian { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? T_KLThungVaGang_Thoi { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? T_KLThungChua_Thoi { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? T_KLGangLongThoi { get; set; }

        [StringLength(255)]
        public string? T_GhiChu { get; set; }
        public int? G_Ca { get; set; }
        public int? T_Ca { get; set; }

        public DateTime? NgayTao { get; set; }

        public int? G_ID_TrangThai { get; set; }
        public int? T_ID_TrangThai { get; set; }
        public int? ID_TrangThai { get; set; }

        public int? G_ID_NguoiLuu { get; set; }
        public int? G_ID_NguoiChuyen { get; set; }
        public int? G_ID_NguoiThuHoi { get; set; }

        public int? T_ID_NguoiLuu { get; set; }

        public int? T_ID_NguoiHuyNhan { get; set; }
        public int? ID_NguoiChot { get; set; }
        public int? G_ID_Kip { get; set; }
        public int? T_ID_Kip { get; set; }

        public int? ID_Locao { get; set; }
        [NotMapped]
        public string TenLoCao { get; set; }

        public int? ID_LoThoi { get; set; }
        [NotMapped]
        public string TenLoThoi { get; set; }
        [NotMapped]
        public string MaLoThoi { get; set; }

        public int? ID_MeThoi { get; set; }
        [NotMapped]
        public string MaMeThoi { get; set; }

        public int? ID_Phieu { get; set; }
    }
}
