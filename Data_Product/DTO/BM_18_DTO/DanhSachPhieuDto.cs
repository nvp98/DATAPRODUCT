namespace Data_Product.DTO.BM_18_DTO
{
    public class DanhSachPhieuDto
    {
        public string MaPhieu { get; set; }
        public DateTime NgayTaoPhieu { get; set; }
        public DateTime NgaySanXuat { get; set; }
        public string? TenNguoiTao { get; set; }
        public string? TenCa { get; set; }
        public string? TenLoCao { get; set; }
        public String? ThoiGianTao { get; set; }
        public int ID_LoCao { get; set; }
        public int? TrangThai { get; set; }
    }
    public class ResetPhieuRequest
    {
        public string MaPhieu { get; set; }
    }
    public class KLGangChiaTheoMe
    {
        public string SoMe { get; set; }        // Số mẻ
        public decimal SumChiaRaw { get; set; } // Tổng KLGangChia thô
        public bool HasChia { get; set; }       // Có KLGangChia hợp lệ
        public decimal SumChia { get; set; }    // Tổng KLGangChia sau filter
    }

}
