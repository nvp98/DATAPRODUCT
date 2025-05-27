using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data_Product.Models
{
    public class Tbl_NhatKy_LyDo
    {
        [Key]
        public int ID { get; set; }
        public string TenLyDo { get; set; }
    }
}
