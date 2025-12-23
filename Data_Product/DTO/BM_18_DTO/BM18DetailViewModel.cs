using Data_Product.Models;

namespace Data_Product.DTO.BM_18_DTO
{
    public class BM18DetailViewModel
    {
        public Tbl_BM_18_PhieuXiHat Phieu { get; set; } = null!;
        public List<Tbl_BM_18_XiHatLoCao> ChiTiet { get; set; } = new();
        public Tbl_TaiKhoan ThongTinBenGiao { get; set; }
        public Tbl_PhongBan PhongBanBenGiao { get; set; }
        public Tbl_Xuong PhanXuongBenGiao { get; set; }
        public Tbl_ViTri ViTriBenGiao { get; set; }

        // Thông tin bên nhận
        public Tbl_TaiKhoan ThongTinBenNhan { get; set; }
        public Tbl_PhongBan PhongBanBenNhan { get; set; }
        public Tbl_Xuong PhanXuongBenNhan { get; set; }
        public Tbl_ViTri ViTriBenNhan { get; set; }
    }
}
