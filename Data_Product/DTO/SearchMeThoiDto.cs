namespace Data_Product.DTO
{
    public class SearchMeThoiDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int? ID_LoThoi { get; set; }
        public int? IDTrangThai { get; set; }
        public string? SearchText { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
    }
}
