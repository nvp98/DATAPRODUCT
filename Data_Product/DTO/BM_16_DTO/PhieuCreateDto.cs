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
    public class MappingCanRayDto
    {
        public int RowId { get; set; }
        public int ID_LoCao { get; set; }

        public DateTime? GioChotGang { get; set; }

        // chuỗi hiển thị giờ (HH:mm)
        public string GioStr { get; set; }

        // số liệu khối lượng / TS1, TS4, TS5, TS6 — decimal? để giữ precision
        public decimal? ThungSo { get; set; }   // TS1
        public decimal? KL_Bi { get; set; }     // TS4
        public decimal? KL_Tong { get; set; }   // TS5
        public decimal? KL_Gang { get; set; }   // TS6
        public int? SanRaGang { get; set; } 

        // các trường gốc nếu cần (tùy chọn)
        public int? BF_no { get; set; }
        public int? Laddle_no { get; set; }
        public int? Shift { get; set; }
        public int? Casthouse { get; set; }
        public string BKMIS_SoMe { get; set; }
    }

}
