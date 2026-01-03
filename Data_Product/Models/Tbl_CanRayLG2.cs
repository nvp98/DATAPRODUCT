using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_CanRayLG2
    {
            [ Key]
            public int ID { get; set; }
            public int BF_ID { get; set; }
            public int? BF_no { get; set; }       // Số Lò Cao
            public int? Laddle_no { get; set; }   // Số thùng
            public int? Shift { get; set; }       // Ca Sản Xuất
            public int? Casthouse { get; set; }   // Nhà Đúc/Khu Vực Ra Gang
            public int? Weight_no { get; set; }   // Số Lần Cân

            public DateTime? BF_Timestap { get; set; } // Thời Gian Ghi Nhận

            public decimal? Weight_TARE { get; set; }  // Trọng Lượng Bì
            public decimal? Weight_GROSS { get; set; } // Trọng Lượng Tổng
            public decimal? Weight_NET { get; set; }   // Trọng Lượng Tịnh
            public string? BKMIS_SoMe { get; set; } // Mã số theo dõi BKMIS

            public bool Is_Nhap { get; set; }

            public string? Ghi_Chu { get; set; }
            // Flag để đánh dấu Số mẻ đã bị xóa/clear bởi UI/backend
            public bool? SoMe_Cleared { get; set; }
            public string? OriginalSoMe { get; set; }


    }

   
}
