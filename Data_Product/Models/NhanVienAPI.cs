namespace Data_Product.Models
{
    public class NhanVienAPI
    {
        public string result { get; set; }
        public string content { get; set; }
        public List<dataAPI> data { get; set; }
        public class dataAPI
        {
            public string manv { get; set; }
            public string hoten { get; set; }
            public string cmnd { get; set; }
            public string diachi { get; set; }
            public string sodienthoai { get; set; }
            public string email { get; set; }
            public string ngayvaolam { get; set; }
            public int tinhtranglamviec { get; set; }
            public string ngaynghiviec { get; set; }
            public string phongban { get; set; }
            public string maphongban { get; set; }
            public string mavitri { get; set; }
            public string vitri { get; set; }
            public string makip { get; set; }
            public string tenkip { get; set; }
            public string tolamviec { get; set; }
            public string phanxuong { get; set; }
        }
    }
}
