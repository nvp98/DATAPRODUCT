namespace Data_Product.DTO.BM_16_DTO
{
    public class SearchDto 
    {
        public int? ID_LoCao { get; set; }
        public int? ID_LoThoi { get; set; }
        public DateTime? TuNgay_LG { get; set; }
        public DateTime? DenNgay_LG { get; set; }
        public DateTime? TuNgay_LT { get; set; }
        public DateTime? DenNgay_LT { get; set; }
        public int? Ca_LT { get; set; }
        public int? Ca_LG { get; set; }
        public string? ID_Kip_LT { get; set; }
        public string? ID_Kip_LG { get; set; }
        public string? ChuyenDen { get; set; }
        public string? ThungSo { get; set; }
        public int? ID_TinhTrang { get; set; }
        public int? ID_TinhTrang_LT { get; set; }
        public int? ID_TinhTrang_LG { get; set; }
        public string? MaThungGang { get; set; }
        public string? MaThungThep { get; set; }
    }

    public class SearchLTDto
    {
        public DateTime? TuNgay{ get; set; }
        public DateTime? DenNgay { get; set; }
        public int? ID_TrangThai { get; set; }
        public int? ID_Locao { get; set; }
        public List<string>? ChuyenDen { get; set; }
        public int? G_Ca { get; set; }
    }


    public class SearchThungDaNhanDto
    {
        public DateTime? NgayLamViec { get; set; } 
        public int? T_Ca { get; set; }
        public int? ID_LoThoi { get; set; }
    }

    public class LuuKRDto
    {
        public string maThungGang { get; set; }
        public bool isChecked { get; set; }
    }


    public class ChotThungDto
    {
        public int id { get; set; }
    }
}
