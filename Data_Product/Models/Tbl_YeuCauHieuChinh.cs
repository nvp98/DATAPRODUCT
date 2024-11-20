using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Data_Product.Models
{
    public class Tbl_YeuCauHieuChinh
    {
        [Key]
        public int ID_YeuCau { get; set; }
        public string? NoiDungYeuCau { get; set; }
        public int ID_BBGN { get; set; }
    }
}
