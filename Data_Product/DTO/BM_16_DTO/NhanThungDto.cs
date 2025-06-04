namespace Data_Product.DTO.BM_16_DTO
{
    public class NhanThungDto
    {
        public List<int> selectedIds { get; set; }
        public DateTime ngayNhan { get; set; }
        public int idCa { get; set; }
        public int idLoThoi { get; set; }
        public int idNguoiNhan { get; set; }
    }

    public class ThungNhanResultDto
    {
        public int ID { get; set; }
        public string MaThungGang { get; set; }
        public string MaThungThep { get; set; }
        public string? BKMIS_ThungSo { get; set; }
        public int? ID_TrangThai { get; set; }
        public int? T_ID_TrangThai { get; set; }
        public int? ID_LoCao { get; set; }
    }
}
