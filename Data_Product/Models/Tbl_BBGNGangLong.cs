using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_BBGNGangLong
    {
        [Key]
        public int ID_BBGL { get; set; }
        public int ID_NhanVien_BG { get; set; }
        public int ID_PhongBan_BG { get; set; }
        public int ID_Xuong_BG { get; set; }
        public int ID_NhanVien_BN { get; set; }
        public int ID_PhongBan_BN { get; set; }
        public int ID_Xuong_BN { get; set; }
        public int TinhTrang_BG { get; set; }
        public int TinhTrang_BN { get; set; }
        public DateTime NgayXuLy_BG { get; set; }
        public DateTime? NgayXuLy_BN { get; set; }
        public string SoPhieu { get; set; }
        public int? ID_Kip { get; set; }
        public string? Kip { get; set; }
        public string? Ca { get; set; }
        public int? IDBBGL_Cu { get; set; }
        public int? FileBBGL { get; set; }
        public DateTime? NgayTao { get; set; }
        public string? NoiDungTrichYeu { get; set; }
        public int? ID_QuyTrinh { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsLock { get; set; }
        public string? FileDinhKem { get; set; }
        public int? TinhTrang_BBGN { get; set; }

        [NotMapped]
        public List<Tbl_ChiTiet_BBGNGangLong> chitietBBGNGangLong { get; set; }

    }
}
