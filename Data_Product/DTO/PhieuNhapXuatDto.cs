using System.Text.Json.Serialization;

namespace Data_Product.DTO
{
    public class PhieuBBGNDto
    {
        [JsonPropertyName("id_pnx")]
        public int ID_PNX { get; set; }

        [JsonPropertyName("ngay")]
        public DateTime NGAY { get; set; }

        [JsonPropertyName("kip")]
        public string? KIP { get; set; }

        [JsonPropertyName("ca")]
        public string? CA { get; set; }

        [JsonPropertyName("noi_dung")]
        public string? NOI_DUNG { get; set; }

        [JsonPropertyName("ma_vat_tu")]
        public string? Ma_VAT_TU { get; set; }

        [JsonPropertyName("ten_lo")]
        public string? TEN_LO { get; set; }

        [JsonPropertyName("dvt")]
        public string? DVT { get; set; }

        [JsonPropertyName("nhap_kl")]
        public double NHAP_KL { get; set; }

        [JsonPropertyName("nhap_do_am")]
        public double NHAP_DO_AM { get; set; }

        [JsonPropertyName("nhap_quy_kho")]
        public double NHAP_QUY_KHO { get; set; }

        [JsonPropertyName("nhap_xuong")]
        public string? NHAP_XUONG { get; set; }

        [JsonPropertyName("nhap_ma_xuong")]
        public string? NHAP_MA_XUONG { get; set; }

        [JsonPropertyName("nhap_bo_phan")]
        public string? NHAP_BO_PHAN { get; set; }

        [JsonPropertyName("nhap_ma_bo_phan")]
        public string? NHAP_MA_BO_PHAN { get; set; }

        [JsonPropertyName("nhap_ho_ten")]
        public string? NHAP_HO_TEN { get; set; }

        [JsonPropertyName("nhap_ma_nv")]
        public string? NHAP_MA_NV { get; set; }

        [JsonPropertyName("xuat_kl")]
        public double XUAT_KL { get; set; }

        [JsonPropertyName("xuat_do_am")]
        public double XUAT_DO_AM { get; set; }

        [JsonPropertyName("xuat_quy_kho")]
        public double XUAT_QUY_KHO { get; set; }

        [JsonPropertyName("xuat_xuong")]
        public string? XUAT_XUONG { get; set; }

        [JsonPropertyName("xuat_ma_xuong")]
        public string? XUAT_MA_XUONG { get; set; }

        [JsonPropertyName("xuat_bo_phan")]
        public string? XUAT_BO_PHAN { get; set; }

        [JsonPropertyName("xuat_ma_bo_phan")]
        public string? XUAT_MA_BO_PHAN { get; set; }

        [JsonPropertyName("xuat_ho_ten")]
        public string? XUAT_HO_TEN { get; set; }

        [JsonPropertyName("xuat_ma_nv")]
        public string? XUAT_MA_NV { get; set; }

        [JsonPropertyName("ghi_chu")]
        public string? GHI_CHU { get; set; }

        [JsonPropertyName("ma_phieu")]
        public string? MA_PHIEU { get; set; }

        [JsonPropertyName("tinh_trang_phieu")]
        public string? TINH_TRANG_PHIEU { get; set; }

        [JsonPropertyName("ma_tinh_trang_phieu")]
        public string? MA_TINH_TRANG_PHIEU { get; set; }

        [JsonPropertyName("ma_quy_trinh")]
        public string? MA_QUY_TRINH { get; set; }
    }
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? Total { get; set; }
        public T? Data { get; set; }
      
    }
}
