using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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
        public decimal? KLGangChia { get; set; }
        public int? T_ID_NguoiNhan { get; set; }


        public int? G_ID_NguoiLuu { get; set; }
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
        public string? MaChiaGang { get; set; }
        public int? T_ReceiveSeq { get; set; }

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
        public string? GioChonMe { get; set; }

        public DateTime? NgayTaoTTG { get; set; }

        public int? ID_MeThoi { get; set; }
        public string MaMeThoi { get; set; }
        [JsonIgnore] public int __Sort_HasMeThoi { get; set; }
        [JsonIgnore] public int __Sort_MinReceiveSeq { get; set; }
        public List<GangLongItemViewModel> DanhSachThungGang { get; set; } = new();
    }



    public class ThungTrungGianRenderViewModel
    {
        public int ID_TTG { get; set; }
        public string? MaThungTG { get; set; }
        public string? MaThungTG_Copy { get; set; }
        public string? SoThungTG { get; set; }
        public string? GhiChu { get; set; }
        public bool IsCopy { get; set; }
        public decimal? KLThung_Thoi { get; set; }
        public decimal? KLThungVaGang_Thoi { get; set; }
        public decimal? KL_phe { get; set; }
        public decimal? KLGang_Thoi { get; set; }
        public decimal? Tong_KLGangNhan { get; set; }
        public int? ID_MeThoi { get; set; }
        public string? MaMeThoi { get; set; }
        public DateTime? NgayTaoTTG { get; set; }
        public string? GioChonMe { get; set; }
        public List<ItemRowViewModel> DanhSachThungGang { get; set; } = new();
        public List<GroupBlockViewModel> Groups { get; set; } = new();
        public int TtgRowspan { get; set; }
    }

    public class ItemRowViewModel
    {
        public int ID { get; set; }
        public string? MaThungGang { get; set; }
        public string? MaThungThep { get; set; }
        public string? BKMIS_ThungSo { get; set; }
        public int? ID_LoCao { get; set; } // note: map từ x.ID_Locao
        public int? ID_TrangThai { get; set; }
        public decimal? T_KLGangLong { get; set; }
        public decimal? T_KLThungChua { get; set; }
        public decimal? T_KLThungVaGang { get; set; }
        public int? G_ID_NguoiChuyen { get; set; }
        public int? G_ID_NguoiLuu { get; set; }
        public string? ChuyenDen { get; set; }
        public string? BKMIS_SoMe { get; set; }
        public string? BKMIS_Gio { get; set; }
        public DateTime? NgayTaoG { get; set; }
        public decimal? KLGangChia { get; set; }
        public int? T_ID_NguoiNhan { get; set; }
        public string? MaChiaGang { get; set; } // sẽ set khi enrich
    }

    public class GroupBlockViewModel
    {
        public string MaChiaGang { get; set; } = "";
        public int GroupRowspan { get; set; }
        public GroupValuesViewModel Values { get; set; } = new();
        public List<ItemRowViewModel> Items { get; set; } = new();
    }

    public class GroupValuesViewModel
    {
        public decimal? T_KLThungVaGang { get; set; }
        public decimal? T_KLThungChua { get; set; }
        public decimal? T_KLGangLong { get; set; }
    }

    internal class GangEnriched
    {
        public int ID_TTG { get; set; }
        public ItemRowViewModel Item { get; set; } = default!;
        public string? MaChiaGang { get; set; }
    }

}
