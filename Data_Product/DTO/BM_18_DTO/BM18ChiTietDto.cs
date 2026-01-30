namespace Data_Product.DTO.BM_18_DTO
{
    public class BM18ChiTietDto
    {
        public int ID { get; set; }
        public string MaPhieu { get; set; }
        public int ID_LoCao { get; set; }
        public int ID_Ca { get; set; }
        public int ID_Kip { get; set; }
        public DateTime? NgaySanXuat { get; set; }
        public string Ten_NVL { get; set; }
        public string DVT { get; set; }
        public int? ID_Lo { get; set; }
        public decimal HeSo { get; set; }

        public decimal KL_Gang_Giao { get; set; }
        public decimal KL_Xi_Giao { get; set; }
        public decimal KL_Gang_Nhan { get; set; }
        public decimal KL_Xi_Nhan { get; set; }
        public string GhiChu { get; set; }
    }
    public class BM18Request
    {
        public string MaPhieu { get; set; }
        public List<BM18ChiTietDto> ChiTiet { get; set; }
        public int ID_NguoiGiao { get; set; }
        public int ID_NguoiNhan { get; set; }
    }
    public class XacNhanPhieuBNRequest
    {
        public string MaPhieu { get; set; }
        public int TrangThai { get; set; }
    }
    public class UpdateMaLoDto
    {
        public string MaPhieu { get; set; }
        public int ID_MaLo { get; set; }
    }
    public class CreateHeSoXiDto
    {
        public int ID_LoCao { get; set; }
        public DateTime NgaySanXuat { get; set; }

        public int? Ca { get; set; }
        public int? ID_Kip { get; set; }

        public decimal HeSoXi { get; set; }
        public int ID_NguoiXacNhan { get; set; }
    }
    public class DanhSachHeSoXiDto
    {
        public int ID { get; set; }

        public int ID_LoCao { get; set; }
        public string TenLoCao { get; set; }

        public DateTime NgaySanXuat { get; set; }
        public int? Ca { get; set; }
        public int? Kip { get; set; }

        public decimal HeSoXi { get; set; }

        public string NguoiXacNhan { get; set; }
        public DateTime ThoiGianXacNhan { get; set; }

        public int TrangThai { get; set; }
    }

}
