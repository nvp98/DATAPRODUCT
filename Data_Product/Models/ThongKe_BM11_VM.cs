namespace Data_Product.Models
{
    public class ThongKe_BM11_VM
    {
        public int ID_BBGN { get; set; }
        public string? SoPhieu { get; set; }
        public DateTime ThoiGianXuLyBG { get; set; }
        public string? Kip { get; set; }
        public string? Ca { get; set; }
        public string? TenNhanVien_BG { get; set; }
        public string? HoVaTen_BG { get; set; }
        public string? TenPhongBan_BG { get; set; }
        public string? TenXuong_BG { get; set; }
        public string? TenNhanVien_BN { get; set; }
        public string? HoVaTen_BN { get; set; }
        public string? TenPhongBan_BN { get; set; }
        public string? TenXuong_BN { get; set; }
        public string? TenVatTu { get; set; }
        public string? DonViTinh { get; set; }
        public string? MaLo { get; set; }
        public double DoAm_W { get; set; }
        public double KhoiLuong_BG { get; set; }
        public double KL_QuyKho_BG { get; set; }
        public double KhoiLuong_BN { get; set; }
        public double KL_QuyKho_BN { get; set; }
        public string? GhiChu { get; set; }
        public int ID_TrangThai_BBGN { get; set; }
        public string? TenTrangThai_BBGN { get; set; }
        public bool IsLock { get; set; }
    }
}
