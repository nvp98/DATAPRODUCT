namespace Data_Product.DTO.BM_16_DTO
{
    public class ChiaGangDto
    {
        public int ID { get; set; }
        public string MaChiaGang { get; set; }
        public string MaThungGang { get; set; }
        public string MaThungThep { get; set; }
        public decimal TyLeChia { get; set; }
        public decimal KLChia { get; set; }

    }

    public class GopThungGang
    {
        public List<int> IDs { get; set; }
        public string PhongBan { get; set; }
    }
}
