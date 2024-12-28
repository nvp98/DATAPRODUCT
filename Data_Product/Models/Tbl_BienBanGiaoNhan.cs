using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_BienBanGiaoNhan
    {
        [Key]
        public int ID_BBGN { get; set; }
        public int ID_NhanVien_BG { get; set; }
        public int ID_PhongBan_BG { get; set; }
        public int ID_Xuong_BG { get; set; }
        public int ID_ViTri_BG { get; set; }
        public DateTime ThoiGianXuLyBG { get; set; }
        public int ID_TrangThai_BG { get; set; }


        public int ID_NhanVien_BN { get; set; }
        public int ID_PhongBan_BN { get; set; }
        public int ID_Xuong_BN { get; set; }
        public int ID_ViTri_BN { get; set; }
        public Nullable<DateTime> ThoiGianXuLyBN { get; set; }
        public int ID_TrangThai_BN { get; set; }


        public string SoPhieu { get; set; }
        public int ID_Kip { get; set; }
        public int ID_TrangThai_BBGN { get; set; }
        public int ID_QuyTrinh { get; set; }
        public Nullable<int> ID_BBGN_Cu { get; set; }
        public string? FileBBGN { get; set; }
        public string? FileDinhKem { get; set; }
        public DateTime NgayTao { get; set; }
        public string Kip { get; set; }
        public string Ca { get; set; }
        public string? NoiDungTrichYeu { get; set; }
        public Boolean IsDelete { get; set; }
        public Boolean IsLock { get; set; }
        [NotMapped]
        public string[] IDNhanVien { get; set; }
    }
}
