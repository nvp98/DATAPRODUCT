namespace Data_Product.DTO.BM_16_DTO
{
    public class NhanThungDto
    {
        public List<selectedThungsDto> selectedThungs { get; set; }
        public DateTime ngayNhan { get; set; }
        public int idCa { get; set; }
        public int idLoThoi { get; set; }
        public int idNguoiNhan { get; set; }
        public string thungTrungGian { get; set; }
    }

    public class selectedThungsDto
    {
        public int clientSeq { get; set; }
        public string maThungGang { get; set; }
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


    public class ChuyenThungReq
    {
        public string? MaPhieu { get; set; }
        public List<ThungGangDto>? DsMaThung { get; set; } 
    }
    public class XoaSoMeRequest
    {
        public string MaPhieu { get; set; }
        public List<string> SoMes { get; set; }
    }
    public class CapNhatRequest
    {
        public string MaPhieu { get; set; }
        public List<ThungGangDto> DsMaThung { get; set; }
    }
}
