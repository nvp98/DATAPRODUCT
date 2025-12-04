namespace Data_Product.DTO.BM_18_DTO
{
    public class BM18ChiTietDto
    {
        public int ID { get; set; }
        public string MaPhieu { get; set; }
        public int ID_LoCao { get; set; }
        public int ID_Ca { get; set; }
        public int ID_Kip { get; set; }
        public DateTime? NgaySanXuat { get; set; }
        public string Ten_NVL { get; set; }
        public string DVT { get; set; }
        public string Lo { get; set; }
        public decimal HeSo { get; set; }

        public decimal KL_Gang_Giao { get; set; }
        public decimal KL_Xi_Giao { get; set; }
        public decimal KL_Gang_Nhan { get; set; }
        public decimal KL_Xi_Nhan { get; set; }
        public string GhiChu { get; set; }
    }
    public class BM18Request
    {
        public string MaPhieu { get; set; }
        public List<BM18ChiTietDto> ChiTiet { get; set; }
    }
}
