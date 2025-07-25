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
    }
}
