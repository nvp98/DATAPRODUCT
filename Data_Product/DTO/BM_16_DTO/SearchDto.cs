namespace Data_Product.DTO.BM_16_DTO
{
    public class SearchLTDto
    {
        public DateTime? NgayLuyenGang { get; set; }
        public int? ID_TrangThai { get; set; }
        public int? ID_Locao { get; set; }
        public string? ChuyenDen { get; set; }
        public int? G_Ca { get; set; }
    }

    public class LuuKRDto
    {
        public int id { get; set; }
        public bool isChecked { get; set; }
    }
}
