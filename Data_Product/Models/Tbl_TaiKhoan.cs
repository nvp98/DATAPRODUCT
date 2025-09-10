using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_TaiKhoan
    {
        [Key]
        public int ID_TaiKhoan { get; set; }
        public string? TenTaiKhoan { get; set; }
        public string? MatKhau { get; set; }
        [NotMapped]
        public string? MatKhauCu { get; set; }
        [NotMapped]
        public string? MatKhauNhapLai { get; set; }
        public string? HoVaTen { get; set; }

        public int ID_PhongBan { get; set; }
        [NotMapped]
        public string? TenPhongBan { get; set; }

        public int? ID_PhanXuong { get; set; }
        [NotMapped]
        public string? TenXuong { get; set; 
        }
        public int ID_ChucVu { get; set; }
        [NotMapped]
        public string? TenChucVu { get; set; }

        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public DateTime NgayTao { get; set; }
        public Nullable<int> ID_Quyen { get; set; }
        [NotMapped]
        public string? TenQuyen { get; set; }
        public string? ChuKy { get; set; }
        public string? PhongBan_Them { get; set; }
        public string? Quyen_Them { get; set; }
        [NotMapped]
        public string[] ListPhongBan_Them { get; set; }
        [NotMapped]
        public string[] ListQuyen_Them { get; set; }
        public Nullable<int> ID_TrangThai { get; set; }
        public string? PhongBan_API { get; set; }
        public string? Xuong_API { get; set; }
    }
}
