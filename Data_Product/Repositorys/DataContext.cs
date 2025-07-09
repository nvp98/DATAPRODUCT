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
        public DbSet<Tbl_NhatKy_LyDo> Tbl_NhatKy_LyDo { get; set; }
        public DbSet<Tbl_NhatKy_SanXuat> Tbl_NhatKy_SanXuat { get; set; }
        public DbSet<Tbl_NhatKy_SanXuat_ChiTiet> Tbl_NhatKy_SanXuat_ChiTiet { get; set; }
        public DbSet<Tbl_BM_16_GangLong> Tbl_BM_16_GangLong { get; set; }
        public DbSet<Tbl_BM_16_TrangThai> Tbl_BM_16_TrangThai { get; set; }
        public DbSet<Tbl_LoThoi> Tbl_LoThoi { get; set; }
        public DbSet<Tbl_MeThoi> Tbl_MeThoi { get; set; }
        public DbSet<Tbl_LoCao> Tbl_LoCao { get; set; }
        public DbSet<Tbl_BM_16_Phieu> Tbl_BM_16_Phieu { get; set; }
        public DbSet<Tbl_BM_16_TaiKhoan_Thung> Tbl_BM_16_TaiKhoan_Thung { get; set; }
        public DbSet<Tbl_Counter_MeThoi> Tbl_Counter_MeThoi { get; set; }
        public DbSet<Tbl_XeGoong> Tbl_XeGoong { get; set; } 
        public DbSet<Tbl_BM_16_ThungTrungGian> Tbl_BM_16_ThungTrungGian { get; set; }
        public DbSet<Tbl_BM_16_LoSanXuat_TaiKhoan> Tbl_BM_16_LoSanXuat_TaiKhoan { get; set; }
        public DbSet<Tbl_BM_16_LoSanXuat> Tbl_BM_16_LoSanXuat { get; set; }
        public DbSet<Tbl_BM_16_PhanTramDuc> Tbl_BM_16_PhanTramDuc { get; set; }
    }
}
