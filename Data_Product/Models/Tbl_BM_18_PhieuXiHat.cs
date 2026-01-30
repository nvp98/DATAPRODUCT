using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_BM_18_PhieuXiHat
    {
        [Key]
        public int ID { get; set; }

        public string MaPhieu { get; set; }
        public int? Ca { get; set; }
        public DateTime NgayTaoPhieu { get; set; }

        public int ID_Locao { get; set; }

        public int? ID_Kip { get; set; }

        public int? ID_NguoiTao { get; set; }

        public DateTime NgaySanXuat { get; set; }

        public int? ID_NguoiGiao { get; set; }

        public int? ID_TrangThaiBG { get; set; }

        public int? ID_NguoiNhan { get; set; }

        public int? ID_TrangThaiBN { get; set; }

        public int? TrangThai { get; set; }

        /// <summary>
        /// Loại cân được chọn khi tạo phiếu:
        /// 1 = Tổng KL Gang Lỏng Theo Phiếu
        /// 2 = Tổng Cân Cẩu Trục + Xỉ
        /// 3 = Tổng KL Gang Theo Cân Cẩu Trục
        /// </summary>
        //public int? ID_HeSoXi { get; set; }
        public int? LoaiCan { get; set; }

    }
}
