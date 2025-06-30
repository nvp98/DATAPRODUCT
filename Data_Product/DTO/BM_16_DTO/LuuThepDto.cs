namespace Data_Product.DTO.BM_16_DTO
{
    //public class LuuThepDto
    //{
    //    public int ID { get; set; }
    //    public decimal? T_KLThungVaGang { get; set; }
    //    public decimal? T_KLThungChua { get; set; }
    //    public decimal? T_KLGangLong { get; set; }
    //    public int? ThungTrungGian { get; set; }
    //    public decimal? T_KLThungVaGang_Thoi { get; set; }
    //    public decimal? T_KLThungChua_Thoi { get; set; }
    //    public decimal? T_KLGangLongThoi { get; set; }
    //    public decimal? T_KL_phe { get; set; }
    //    public int? ID_MeThoi { get; set; }
    //    public string? GhiChu { get; set; }
    //}

    public class ThungGangConDto
    {
        public string MaThungThep { get; set; }
        public decimal? T_KLThungVaGang { get; set; }
        public decimal? T_KLThungChua { get; set; }
        public decimal? T_KLGangLong { get; set; }
    }

    public class ThungTrungGianDto
    {
        public DateTime NgayNhan { get; set; }
        public int CaNhan { get; set; }
        public int ID_LoThoi { get; set; }
        public string MaThungTG { get; set; }
        public string SoThungTG { get; set; }
        public bool IsCopy { get; set; }
        public string GhiChu { get; set; }
        public decimal? KLThung_Thoi { get; set; }
        public decimal? KLThungVaGang_Thoi { get; set; }
        public decimal? KLGang_Thoi { get; set; }
        public decimal? KLPhe { get; set; }
        public decimal? Tong_KLGangNhan { get; set; }
        public int? ID_MeThoi { get; set; }
        public string? GioChonMe { get; set; }
        public string? MaThungTG_Copy { get; set; }

        public List<ThungGangConDto> DanhSachThungGang { get; set; }
    }
}
