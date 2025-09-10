namespace Data_Product.Models.ModelView
{
    public class BM_16_LoSanXuatModelView
    {
        public int ID { get; set; }
        public string TenLo { get; set; }
        public int MaLo { get; set; }
        public int ID_BoPhan { get; set; }
        public string BoPhan { get; set; }
        public bool IsActived { get; set; }
        public List<TaiKhoanViewModel> ListTaiKhoan { get; set; }
    }

    public class TaiKhoanViewModel
    {
        public int ID { get; set; }
        public string TenTaiKhoan { get; set; }
        public string HoVaTen { get; set; }
    }
}
