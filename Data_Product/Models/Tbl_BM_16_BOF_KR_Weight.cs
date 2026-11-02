using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    // Bảng lưu dữ liệu cân Cân Ray (KR) + BOF đồng bộ từ API nhà máy
    // (endpoint /api/reports/bof-kr-weight), dùng để tự fill số liệu khi HRC2 chọn mẻ thổi.
    public class Tbl_BM_16_BOF_KR_Weight
    {
        [Key]
        public int ID { get; set; }

        public string MeThoi { get; set; }      // BOF_HEAT_ID
        public int ReportNo { get; set; }        // BOF_REPORT_NO - khóa tự nhiên để upsert
        public int? SoThung { get; set; }        // LADLE_NO
        public string? Plant { get; set; }       // PLANT
        public int LoThoi { get; set; }          // PLANT_NO (6, 7, ...)

        public DateTime? ThoiGianBatDauBOF { get; set; }   // BOF_START_TIME
        public DateTime? ThoiGianKetThucBOF { get; set; }  // BOF_END_TIME
        public DateTime? HmTime { get; set; }              // HM_TIME
        public string? MacThep { get; set; }               // GRADE_ID

        public string? MeThoiKR { get; set; }              // KR_HEAT_ID
        public int? ReportNoKR { get; set; }                // KR_REPORT_NO
        public DateTime? ThoiGianBatDauKR { get; set; }     // KR_START_TIME
        public DateTime? ThoiGianKetThucKR { get; set; }    // KR_END_TIME
        public int? CanKR { get; set; }                     // KR_CRANE_NAME
        public int? KhuVucKR { get; set; }                  // KR_AREA_CODE
        public decimal? KLThungVaGangTruocKR { get; set; }  // KR_WEIGHT

        public int? CanThoi { get; set; }                   // BOF_CRANE_NAME
        public decimal? KLThungVaGangTruocBOF { get; set; } // BOF_WEIGHT_IN
        public decimal? KLThung { get; set; }                // BOF_WEIGHT_OUT
        public decimal? KLChenhLech { get; set; }            // WEIGHT_DIFF
        public decimal? KLGangLongBOF { get; set; }          // WEIGHT_CHARGE

        public string? Kip { get; set; }   // SHIFT
        public int? Ca { get; set; }        // KIP

        public DateTime? NgaySanXuat { get; set; }
        public DateTime NgayDongBo { get; set; }
    }
}
