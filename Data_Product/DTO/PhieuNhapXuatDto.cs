namespace Data_Product.DTO
{
    public class PhieuBBGNDto
    {
        public int ID_PNX { get; set; }
        public DateTime NGAY { get; set; }
        public string? KIP { get; set; }
        public string? CA { get; set; }
        public string? NOI_DUNG { get; set; }
        public string? TEN_LO { get; set; }
        public string? DVT { get; set; }

        public double NHAP_KL { get; set; }
        public double NHAP_DO_AM { get; set; }
        public double NHAP_QUY_KHO { get; set; }
        public string? NHAP_XUONG { get; set; }
        public string? NHAP_BO_PHAN { get; set; }

        public double XUAT_KL { get; set; }
        public double XUAT_DO_AM { get; set; }
        public double XUAT_QUY_KHO { get; set; }
        public string? XUAT_XUONG { get; set; }
        public string? XUAT_BO_PHAN { get; set; }

        public string? GHI_CHU { get; set; }
        public string? MA_PHIEU { get; set; }
        public string? TINH_TRANG_PHIEU { get; set; }
    }
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? Total { get; set; }
        public T? Data { get; set; }
      
    }
}
