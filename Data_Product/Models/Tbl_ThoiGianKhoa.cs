using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_ThoiGianKhoa
    {
        [Key]
        public int ID { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }
        public DateTime NgayXuLy { get; set; }
        public Boolean IsLock { get; set; }

        public string? MaBB { get; set; }
    }
}
