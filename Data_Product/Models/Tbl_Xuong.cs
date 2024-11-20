using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_Xuong
    {
        [Key]
        public int ID_Xuong { get; set; }
        public string? TenXuong { get; set; }
        public int? ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }
        public int? ID_TrangThai { get; set; }
    }
}
