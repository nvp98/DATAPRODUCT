using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_KLGangVaoBOFBase
    {
        [Key]
        public int ID { get; set; }
        public int? ID_NM { get; set; }
        public string? NgayLaySoLieuAT { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? CanCauTruc { get; set; }
        public string? MeThoi { get; set; }
        public decimal? KLThungVaGang { get; set; }
        public decimal? KLThung { get; set; }
        public decimal? KLGang { get; set; }
        public string? SoThung { get; set; }
        public int Ca { get; set; }
        public DateTime NgaySanXuat { get; set; }
        public bool? IsUsed { get; set; }
        public string? ThoiDiemRot { get; set; }
        public bool? IsNM { get; set; }
        public string? LyDo { get; set; }
        public int? ID_MeThoi { get; set; }
        public bool? IsChuyenMe { get; set; }
        public int? ParentID { get; set; }
    }
}
