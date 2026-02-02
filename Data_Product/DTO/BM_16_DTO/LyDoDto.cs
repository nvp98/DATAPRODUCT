namespace Data_Product.DTO.BM_16_DTO
{
    public class SearchLyDoDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool? TrangThai { get; set; }
        public string? SearchText { get; set; }
        public DateTime? TuNgay{ get; set; }
        public DateTime? DenNgay { get; set; }
    }

    public class LyDoXuLyMeDto
    {
        public int? ID { get; set; }
        public string LyDo { get; set; }
        public bool TrangThai { get; set; }
    }
}
