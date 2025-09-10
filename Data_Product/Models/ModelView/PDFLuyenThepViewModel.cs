namespace Data_Product.Models.ModelView
{

    public class PDFLuyenThepViewModel
    {
        public List<ThungTrungGianGroupViewModel> Data { get; set; }
        public ThongTinPDFViewModel? ThongTin { get; set; }
    }
    public class ThongTinPDFViewModel
    {
        public string T_Ca { get; set; }
        public string T_Kip { get; set; }
        public DateTime NgayLuyenThep { get; set; }
    }
}
