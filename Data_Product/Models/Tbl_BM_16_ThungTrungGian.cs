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

        public int? ID_MeThoi { get; set; }
        public bool IsCopy { get; set; }
        public string? GhiChu { get; set; }
        public DateTime? NgayTao { get; set; }
	    public string? GioNhan { get; set; }
        public string? MaThungTG_Copy { get; set; }
        public int? ID_NguoiNhan { get; set; }
    }
}
