using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class Tbl_NhatKy_SanXuat
    {
        [Key]
        public int ID { get; set; }
        public int ID_NhanVien_SX { get; set; }
        public int ID_PhongBan_SX { get; set; }
        public string SoPhieu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayDungSX { get; set; }
        public string Ca { get; set; }
        public string Kip { get; set; }
        public int ID_Kip { get; set; }
        public int TinhTrang { get; set; }
        public bool IsDelete { get; set; }
        [NotMapped]
        public int TinhTrangCheckPhieu { get; set; }
        [NotMapped]
        public List<Tbl_NhatKy_SanXuat_ChiTiet> NhatKy_SanXuat_ChiTiet { get; set; }

        [NotMapped]
        public Tbl_TaiKhoan Tbl_TaiKhoan { get; set; }


    }
}
