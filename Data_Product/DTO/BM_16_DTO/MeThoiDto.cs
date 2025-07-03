namespace Data_Product.DTO.BM_16_DTO
{
    public class MeThoiSearchDto
    {
        public int id_LoThoi { get; set; }
        public string searchText { get; set; }
        public DateTime ngayLamViec { get; set; }
    }
    public class TaoMeThoiDto
    {
        public int id_LoThoi { get; set; }
        public string maLoThoi { get; set; }
        public int soLuong { get; set; }
        public DateTime ngayTao{ get; set; }
    }

    public class FilterMeThoiDto
    {
        public int id_LoThoi { get; set; }
    }
}
