namespace Data_Product.DTO.BM_16_DTO
{

    public class LoSanXuatPayloadDTO
    {
        public int? ID { get; set; }
        public string TenLo { get; set; }
        public int MaLo { get; set; }
        public int ID_BoPhan { get; set; }
        public bool IsActived { get; set; }
        public List<int>? TaiKhoanIds { get; set; }
    }
}
