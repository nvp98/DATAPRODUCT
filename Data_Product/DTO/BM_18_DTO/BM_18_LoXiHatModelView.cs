using Data_Product.Models.ModelView;

namespace Data_Product.DTO.BM_18_DTO
{
    public class BM_18_LoXiHatModelView
    {
        public int ID { get; set; }
        public int MaLo { get; set; }
        public string TenLo { get; set; }
        public int ID_BoPhan { get; set; }
        public string BoPhan { get; set; }
        public bool IsActived { get; set; }
        public List<TaiKhoanViewModel> ListTaiKhoan { get; set; }
    }
}
