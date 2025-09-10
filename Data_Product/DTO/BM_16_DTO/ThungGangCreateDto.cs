using System.ComponentModel.DataAnnotations;

namespace Data_Product.DTO.BM_16_DTO
{
    public class ThungGangDto
    {
        
        public string? MaPhieu { get;set; }
        
        public string? MaThungGang { get; set; }
  
        public string? BKMIS_SoMe { get; set; }
      
        public string? BKMIS_ThungSo { get; set; }
     
        public string? BKMIS_Gio { get; set; }
      
        public string? BKMIS_PhanLoai { get; set; }

        public decimal? KL_XeGoong { get; set; }

        public decimal? G_KLXeThungVaGang { get; set; }

        public decimal? G_KLXeVaThung { get; set; }
        
        public decimal? G_KLThungChua { get; set; }
       
        public decimal? G_KLThungVaGang { get; set; }
      
        public decimal? G_KLGangLong { get; set; }
       
        public string? ChuyenDen { get; set; }
       
        public string? Gio_NM { get; set; }
        public string G_GhiChu { get; set; }

    }
}
