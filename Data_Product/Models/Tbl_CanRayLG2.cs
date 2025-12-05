using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Office2019.Excel.RichData;

namespace Data_Product.Models
{
    public class Tbl_CanRayLG2
    {
        [Key]
        public int ID { get; set; }

        // Các trường int (nullable)
        public int? BF_no { get; set; }       // Số Lò Cao
        public int? Laddle_no { get; set; }   // Số Xô/Nồi
        public int? Shift { get; set; }       // Ca Sản Xuất
        public int? Casthouse { get; set; }   // Nhà Đúc/Khu Vực Ra Gang
        public int? Weight_no { get; set; }   // Số Lần Cân

        // Trường DateTime (nullable)
        public DateTime? BF_Timestap { get; set; } // Thời Gian Ghi Nhận

        // Các trường Trọng Lượng (Double? cho độ chính xác cao)
        // Ánh xạ tốt nhất với kiểu 'float' (8 byte) trong SQL Server
        public decimal? Weight_TARE { get; set; }  // Trọng Lượng Bì
        public decimal? Weight_GROSS { get; set; } // Trọng Lượng Tổng
        public decimal? Weight_NET { get; set; }   // Trọng Lượng Tịnh

        // Trường Chuỗi (String) - Cột BKMIS_SoMe (nvarchar(20))
        // Kiểu string trong C# mặc định là nullable
        public string? BKMIS_SoMe { get; set; } // Mã số theo dõi BKMIS

        public string? LyDo { get; set; }

        public bool Is_Nhap { get; set; }
    }
}
    
