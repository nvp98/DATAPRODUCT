using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Office2019.Excel.RichData;

namespace Data_Product.Models
{
    public class LogDataBf
    {
        [Key]
        public int ID { get; set; }
        public string BF_no { get; set; }
        public string Laddle_no { get; set; }
        public string Shift { get; set; }
        public DateTime BF_Timestap { get; set; }
        public string Casthouse { get; set; }
        public decimal? Weight_no { get; set; }
        public decimal? Weight_TARE { get; set; }
        public decimal? Weight_GROSS { get; set; }
        public decimal? Weight_NET { get; set; }
    }
}
