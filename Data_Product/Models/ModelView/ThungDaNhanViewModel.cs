using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models.ModelView
{
    public class GangLongItemViewModel
    {
        public int ID { get; set; }
        public string MaThungGang { get; set; }
        public string MaThungThep { get; set; }
        public int? ID_LoCao { get; set; }
        public string? BKMIS_ThungSo { get; set; }
        public int? ID_TrangThai { get; set; }
        public decimal? T_KLGangLong { get; set; }
        public decimal? T_KLThungChua { get; set; }
        public decimal? T_KLThungVaGang { get; set; }

        public int? G_ID_NguoiChuyen { get; set; }
        public string? ChuyenDen { get; set; }
        public string? BKMIS_SoMe { get; set; }
        public string? BKMIS_Gio { get; set; }

        public string? HoVaTen { get; set; }
        public string? TenTaiKhoan { get; set; }
        public string? TenPhongBan { get; set; }
        public string? ChuKy { get; set; }
        public string? TenViTri { get; set; }

        public DateTime? NgayTaoG { get; set; }

    }

    public class ThungTrungGianGroupViewModel
    {
        public int ID_TTG { get; set; }
        public string MaThungTG { get; set; }
        public string MaThungTG_Copy { get; set; }
        public string SoThungTG { get; set; }
        public string GhiChu { get; set; }
        public bool IsCopy { get; set; }

        public decimal? KLThung_Thoi { get; set; }
        public decimal? KLThungVaGang_Thoi { get; set; }
        public decimal? KL_phe { get; set; }
        public decimal? KLGang_Thoi { get; set; }
        public decimal? Tong_KLGangNhan { get; set; }

        public DateTime? NgayTaoTTG { get; set; }

        public int? ID_MeThoi { get; set; }
        public string MaMeThoi { get; set; }

        public List<GangLongItemViewModel> DanhSachThungGang { get; set; } = new();
    }
}
