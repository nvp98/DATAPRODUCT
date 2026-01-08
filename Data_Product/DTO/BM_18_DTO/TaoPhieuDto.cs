namespace Data_Product.DTO.BM_18_DTO
{
    public class TaoPhieuDto
    {
        public DateTime NgaySanXuat { get; set; }
        //public int ID_LoCao { get; set; }
        public int ID_Kip { get; set; }
        public int Ca { get; set; }
        public List<int> ID_LoCaos { get; set; } = new();
        
        /// <summary>
        /// Loại cân: 1=KL Gang Lỏng Theo Phiếu, 2=Cân Cẩu Trục + Xỉ, 3=KL Gang Theo Cân Cẩu Trục
        /// </summary>
        public int LoaiCan { get; set; }
    }
}
