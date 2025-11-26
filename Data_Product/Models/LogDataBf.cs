using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Office2019.Excel.RichData;

namespace Data_Product.Models
{
    public class LogDataBf
    {
        [Key]
        public int ID { get; set; }              // int NOT NULL

        public int? BF_no { get; set; }
        public int? Laddle_no { get; set; }
        public int? Shift { get; set; }

        public DateTime? BF_Timestap { get; set; }
        public int? Casthouse { get; set; }

        // CHỈNH TỪ float? -> double?
        public int? Weight_no { get; set; }
        public double? Weight_TARE { get; set; }
        public double? Weight_GROSS { get; set; }
        public double? Weight_NET { get; set; }
    }
}
    
