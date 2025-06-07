using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data_Product.Models
{
    public class Tbl_NhatKy_SanXuat_ChiTiet
    {
        [Key]
        public int IDCT { get; set; }
        public int ID_NhatKy { get; set; }
        public int ID_Xuong { get; set; }
        public TimeSpan ThoiDiemDung { get; set; }
        public TimeSpan ThoiDiemChay { get; set; }
        public int LyDo_DungThietBi { get; set; }
        public string? GhiChu { get; set; }
        public string? NoiDungDung { get; set; }
        public int? ThoiGianDung { get; set; }
    }
    public class Tbl_NhatKy_SanXuat_ChiTietExport : Tbl_NhatKy_SanXuat_ChiTiet
    {
        //public Tbl_NhatKy_SanXuat Tbl_NhatKy_SanXuat { get; set; }
        public string SoPhieu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayDungSX { get; set; }
        public string Ca { get; set; }
        public string Kip { get; set; }
        public string TenXuong { get; set; }
        public string TenLyDoDung { get; set; }
        public float SoGioDung { get; set; }
        public string TenPhongBan { get; set; }
        public int TinhTrang { get; set; }
    }
}
