using Microsoft.EntityFrameworkCore;
using Data_Product.Models;

namespace Data_Product.Repositorys
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<Tbl_TaiKhoan> Tbl_TaiKhoan { get; set; }
        public DbSet<Tbl_Quyen> Tbl_Quyen { get; set; }
        public DbSet<Tbl_PhongBan> Tbl_PhongBan { get; set; }
        public DbSet<Tbl_Xuong> Tbl_Xuong { get; set; }
        public DbSet<Tbl_Kip> Tbl_Kip { get; set; }
        public DbSet<Tbl_NhomVatTu> Tbl_NhomVatTu { get; set; }
        public DbSet<Tbl_VatTu> Tbl_VatTu { get; set; }
        public DbSet<Tbl_BienBanGiaoNhan> Tbl_BienBanGiaoNhan { get; set; }
        public DbSet<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan { get; set; }
        public DbSet<Tbl_TrinhKyBoSung> Tbl_TrinhKyBoSung { get; set; }      
        public DbSet<Tbl_ViTri> Tbl_ViTri { get; set; }
        public DbSet<Tbl_YeuCauHieuChinh> Tbl_YeuCauHieuChinh { get; set; }
        public DbSet<Tbl_DonViTinh> Tbl_DonViTinh { get; set; }
        public DbSet<Tbl_MaLo> Tbl_MaLo { get; set; }
        public DbSet<Tbl_TrangThai> Tbl_TrangThai { get; set; }
        public DbSet<Tbl_TrangThai_PheDuyet> Tbl_TrangThai_PheDuyet { get; set; }
        public DbSet<Tbl_VatTuMaLo> Tbl_VatTuMaLo { get; set; }
        public DbSet<Tbl_ThongKeXuong> Tbl_ThongKeXuong { get; set; }
        public DbSet<Tbl_PKHXuLyPhieu> Tbl_PKHXuLyPhieu { get; set; }
        public DbSet<Tbl_XuLyXoaPhieu> Tbl_XuLyXoaPhieu { get; set; }
        public DbSet<Tbl_ThoiGianKhoa> Tbl_ThoiGianKhoa { get; set; }

    }
}
