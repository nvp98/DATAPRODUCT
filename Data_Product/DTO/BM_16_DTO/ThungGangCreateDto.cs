using System.ComponentModel.DataAnnotations;

namespace Data_Product.DTO.BM_16_DTO
{
    public class ThungGangDto
    {
        
        public string? MaPhieu { get;set; }
        
        public string? MaThungGang { get; set; }
  
        public string? BKMIS_SoMe { get; set; }
      
        public string? BKMIS_ThungSo { get; set; }
        [Required]
        public string? BKMIS_Gio { get; set; }
        [Required]
        public string? BKMIS_PhanLoai { get; set; }
        [Required]
        public decimal? KL_XeGoong { get; set; }
        [Required]
        public decimal? G_KLThungChua { get; set; }
        [Required]
        public decimal? G_KLThungVaGang { get; set; }
        [Required]
        public decimal? G_KLGangLong { get; set; }
        [Required]
        public string? ChuyenDen { get; set; }
        [Required]
        public string? Gio_NM { get; set; }
        public string G_GhiChu { get; set; }

    }
}
