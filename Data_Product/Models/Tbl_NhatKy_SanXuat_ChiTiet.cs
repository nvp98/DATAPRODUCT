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
}
