namespace Data_Product.DTO
{
    public class TongHopKLGangLongDto
    {
        public DateTime Ngay { get; set; }
        public string TenLoCao { get; set; }
        public int ID_Locao { get; set; }
        public string TenKip { get; set; }
        public string TenCa { get; set; }
        public int ID_Kip { get; set; }
        public int SoLuongPhieu { get; set; }
        public int SoLuongMe { get; set; }
        public int SoLuongThung { get; set; }
        public decimal Tong_KL_GangLong_CanRay { get; set; }
        public decimal Tong_TongKL_TheoMe { get; set; }
        public decimal Tong_KLDuc { get; set; }
    }
}
