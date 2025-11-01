namespace Data_Product.Models.ModelView
{
    public class ThungGangChiaModel
    {
        public int ID { get; set; }
        public string MaThungGang { get; set; }
        public int? ID_Locao { get; set; }
        public string BKMIS_ThungSo { get; set; }
        public string MaThungThep { get; set; }
        public decimal? G_KLGangLong { get; set; }
        public decimal? T_KLGangLong { get; set; }
        public decimal? T_KLThungChua { get; set; }
        public decimal? T_KLThungVaGang { get; set; }
        public decimal? KL_Phe { get; set; }
        public decimal? KLGangChia { get; set; }
        public bool? T_copy { get; set; }

        // 2 trường thêm cho xử lý chia
        public decimal? TyLeChia { get; set; }
        public decimal? KLChia { get; set; }
        public string? MaChiaGang { get; set; }
    }


    public class ChiaGangResultModel
    {
        public List<ThungGangChiaModel> ThungGoc { get; set; }
        public List<ThungGangChiaModel> ThungDaCoKL { get; set; }
        public List<ThungGangChiaModel> ThungAll { get; set; }
        public List<ThungGangChiaModel> ListAll { get; set; }

    }

    public class ChiTietChiaGangResponse
    {
        public string MaThungGang { get; set; }
        public List<TtgBasicDto> listThung { get; set; } = new();
    }

    public class ThungGangDetailDto
    {
        public int Id { get; set; }
        public string MaThungGang { get; set; }
        public string MaThungThep { get; set; }
        public bool IsCopy { get; set; }
        public decimal? GKLGangLong { get; set; }
        public decimal? TKLGangLong { get; set; }
        public decimal? KLGangChia { get; set; }
        public int? IdTTG { get; set; }
    }

    public class PhanBoRowDto
    {
        public int IdTtg { get; set; }
        public string MaThungTG { get; set; }
        public bool IsTTGCopy { get; set; }
        public decimal? TyLeTrongMaTTG { get; set; }
        public decimal? KLPhanBoCR { get; set; }
    }

    public class TtgBasicDto
    {
        public int Id { get; set; }
        public string MaThungTG { get; set; }
        public bool IsCopy { get; set; }
        public decimal? KLGangThoi { get; set; }
        public ThungGangDetailDto thungGang { get; set; }
        public PhanBoRowDto PhanBo { get; set; } = new();
    }

}
