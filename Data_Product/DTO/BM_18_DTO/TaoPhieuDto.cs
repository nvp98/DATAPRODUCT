namespace Data_Product.DTO.BM_18_DTO
{
    public class TaoPhieuDto
    {
        public DateTime NgaySanXuat { get; set; }
        //public int ID_LoCao { get; set; }
        public int ID_Kip { get; set; }
        public int Ca { get; set; }
        public List<int> ID_LoCaos { get; set; } = new();
    }
}
