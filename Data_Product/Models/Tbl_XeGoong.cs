
using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_XeGoong
    {   
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn Lò Cao")]
        public int? ID_LoCao {get;set;}
        [Required(ErrorMessage = "Vui lòng nhập khối lượng")]
        [Range(1, double.MaxValue, ErrorMessage = "Khối lượng phải lớn hơn 0")]
        public decimal? KL_Xe { get; set; }

    }
}
