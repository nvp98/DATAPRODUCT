using System.Text.Json.Serialization;
using Data_Product.Models;
using Newtonsoft.Json;

namespace Data_Product.DTO.BM_16_DTO
{
    public class PhieuCreateDto
    {
        public int ID_Locao { get; set; }
        public int ID_Kip { get; set; }
        public DateTime NgayPhieuGang { get; set; }
        [JsonPropertyName("ThungGangs")]
        public List<ThungGangDto> DanhSachThung { get; set; }
    }
    public class PhieuViewModel
    {
        public string MaPhieu { get; set; }
        public DateTime NgayTaoPhieu { get; set; }
        public DateTime NgayPhieuGang { get; set; }
        public string? TenNguoiTao { get; set; }
        public string? TenCa { get; set; }
        public string? TenLoCao { get; set; } 
        public String? ThoiGianTao { get; set; }
        public int ID_LoCao { get; set; }
    }
    public class BBGN_GangLong_ViewModel
    {
        public List<Tbl_BM_16_GangLong> DanhSachGangLong { get; set; }
        public List<NguoiInfo> NguoiChuyen { get; set; }
        public List<NguoiInfo> NguoiNhan { get; set; }
        public List<NguoiInfo> NguoiXacNhan { get; set; }
    }
    public class AutoMappingItemDto
    {
        public string MaPhieu { get; set; }       // mã phiếu hiện tại
        public string SoMe { get; set; }          // BKMIS_SoMe
        public int ID_LoCao { get; set; }         // Lò cao
        public DateTime? GioChotGang { get; set; }// từ cân ray (có thể dùng làm Gio_NM)

        public decimal? G_KLXeVaThung { get; set; }      // map từ KL_Bi (TS4)
        public decimal? G_KLXeThungVaGang { get; set; }  // map từ KL_Tong (TS5)
        public decimal? G_KLGangLong { get; set; }       // map từ KL_Gang (TS6)
    }
    public class NguoiInfo
    {
        public string HoVaTen { get; set; }
        public string TenViTri { get; set; }
        public string TenPhongBan { get; set; }
        public string ChuKy { get; set; }
    }
    public class LogDataBfDto
    {
        public int id { get; set; }
        public string BF_no { get; set; }
        public string Laddle_no { get; set; }
        public string Shift { get; set; }
        public DateTime BF_Timestap { get; set; }
        public string Casthouse { get; set; }
        public decimal? Weight_no { get; set; }
        public decimal? Weight_TARE { get; set; }
        public decimal? Weight_GROSS { get; set; }
        public decimal? Weight_NET { get; set; }
    }

}
