using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_16_ThungTrungGian
    {
        [Key]
        public int ID { get; set; }
        public string SoThungTG { get; set; }
        public string MaThungTG { get; set; }
        public DateTime NgayNhan { get; set; }
        public int CaNhan { get; set; }
        public int ID_LoThoi { get; set; }

        public decimal? KLThungVaGang_Thoi { get; set; }
        public decimal? KLThung_Thoi { get; set; }
        public decimal? KLGang_Thoi { get; set; }
        public decimal? KL_phe { get; set; }
        public decimal? Tong_KLGangNhan { get; set; }
        public string? GioChonMe { get; set; }

        public int? ID_MeThoi { get; set; }
        public bool IsCopy { get; set; }
        public string? GhiChu { get; set; }
        public DateTime? NgayTaoTTG { get; set; }
	    public string? GioNhan { get; set; }
        public string? MaThungTG_Copy { get; set; }
        public int? ID_NguoiNhan { get; set; }

        public decimal? KLThungVaGangTruocKR { get; set; }

        // Snapshot dữ liệu cân BOF/KR tự động tại thời điểm Lưu (theo mẻ thổi đang chọn),
        // dùng để so sánh với 3 cột phía trên nhằm highlight ô đã bị sửa tay + hiển thị tooltip giá trị gốc.
        public decimal? KLThungVaGangTruocKR_Auto { get; set; }
        public decimal? KLThungVaGang_Thoi_Auto { get; set; }
        public decimal? KLThung_Thoi_Auto { get; set; }
        public int? ID_BOF_KR_Weight { get; set; }

        public bool? IsNhap { get; set; }
        public string? NoiNhan { get; set; }
        //public string? MeThoi { get; set; }
    }
}
