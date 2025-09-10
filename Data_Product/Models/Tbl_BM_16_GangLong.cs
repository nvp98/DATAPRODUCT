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
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? KL_XeGoong { get; set; }

        public DateTime? NgayLuyenGang { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? G_KLXeVaThung { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? G_KLXeThungVaGang { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? G_KLThungChua { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? G_KLThungVaGang { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? G_KLGangLong { get; set; }

        [StringLength(20)]
        public string? ChuyenDen { get; set; }

        [StringLength(10)]
        public string? Gio_NM { get; set; }

        [StringLength(255)]
        public string? G_GhiChu { get; set; }

        public DateTime? NgayLuyenThep { get; set; }

        public bool? KR { get; set; }

        public bool? T_copy{ get; set; }

        [StringLength(20)]
        public string? MaThungThep { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? T_KLThungVaGang { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? T_KLThungChua { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? T_KLGangLong { get; set; }

        public int? ThungTrungGian { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? T_KLThungVaGang_Thoi { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? T_KLThungChua_Thoi { get; set; }


        [Column(TypeName = "decimal(10, 2)")]
        public decimal? T_KL_phe { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? T_KLGangLongThoi { get; set; }

        public decimal? KLGangChia { get; set; }

        [StringLength(255)]
        public string? T_GhiChu { get; set; }
        public int? G_Ca { get; set; }
        public int? T_Ca { get; set; }

        public int? T_ReceiveSeq { get; set; }

        public DateTime? NgayTao { get; set; }

        public int? G_ID_TrangThai { get; set; }
        [NotMapped]
        public string TrangThaiLG { get; set; }
        public int? T_ID_TrangThai { get; set; }
        [NotMapped]
        public string TrangThaiLT { get; set; }
        public int? ID_TrangThai { get; set; }
        [NotMapped]
        public string TrangThai { get; set; }

        public int? G_ID_NguoiLuu { get; set; }
        public int? G_ID_NguoiChuyen { get; set; }
        public int? G_ID_NguoiThuHoi { get; set; }

        public int? T_ID_NguoiLuu { get; set; }
        public int? T_ID_NguoiNhan { get; set; }

        public int? T_ID_NguoiHuyNhan { get; set; }
        public int? ID_NguoiChot { get; set; }
        public int? G_ID_Kip { get; set; }
        [NotMapped]
        public string G_TenKip { get; set; }
        public int? T_ID_Kip { get; set; }
        [NotMapped]
        public string T_TenKip { get; set; }

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

        [NotMapped]
        public string HoVaTen { get; set; }

        [NotMapped]
        public string TenTaiKhoan { get; set; }

        [NotMapped]
        public string ChuKy { get; set; }

        [NotMapped]
        public string TenPhongBan { get; set; }

        [NotMapped]
        public string TenViTri { get; set; }

        public int? ID_TTG { get; set; }


        [NotMapped]
        public string? SoThungTG { get; set; }
        [NotMapped]
        public string? MaThungTG { get; set; }
        [NotMapped]
        public decimal? KLThungVaGang_Thoi { get; set; }
        [NotMapped]
        public decimal? KLThung_Thoi { get; set; }
        [NotMapped]
        public decimal? KLGang_Thoi { get; set; }
        [NotMapped]
        public decimal? KL_phe { get; set; }
        [NotMapped]
        public decimal? Tong_KLGangNhan { get; set; }

        public bool? XacNhan { get; set; } = false;

        public int? ID_NguoiXacNhan { get; set; }
        [NotMapped]
        public bool? IsCopy { get; set; }

        [NotMapped]
        public string? GioChonMe { get; set; }

        [NotMapped]
        public string? HoTenNguoiChot { get; set; }
    }
}
