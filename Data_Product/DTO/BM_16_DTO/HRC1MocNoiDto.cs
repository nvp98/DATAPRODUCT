namespace Data_Product.DTO.BM_16_DTO
{
    public class MocNoiTheoLoDto
    {
        public int IdLoThoi { get; set; }
        public List<int> IdThungGangATs { get; set; }
    }

    public class TaoMoiMeGangVaoLoDto
    {
        public int? ID { get; set; }   
        public int? ID_MeThoi { get; set; }
        //public string? MeThoi { get; set; }
        public decimal KLThungVaGang { get; set; }
        public decimal KLThung { get; set; }
        public decimal KLGang { get; set; }
        public string? SoThung { get; set; }
        public int IdLoThoi { get; set; }
        public int Ca { get; set; }
        public DateTime NgaySanXuat { get; set; }
        public string? ThoiDiemRot { get; set; }
        public string? LyDo { get; set; }

        public int? ParentID { get; set; }
    }

    public class ChuyenMeGangDto
    {
        public int FromLo { get; set; }
        public int FromID { get; set; }

        public int ToLo { get; set; }
        public DateTime NgaySanXuatTo { get; set; }
        public int CaTo { get; set; }
    }

    public class ThuHoiMeDto
    {
        public int tuLoID { get; set; }
        public int tuMeID { get; set; }
    }
}
