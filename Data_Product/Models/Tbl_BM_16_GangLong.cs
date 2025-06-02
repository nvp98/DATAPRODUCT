using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data_Product.Models
{
    public class NguoiNhanModel
    {
        public int ID_NguoiNhan { get; set; }
        public int TongSoLanNhan { get; set; }
    }
    public class Tbl_BM_16_GangLong
    {
        [Key]
        public int ID { get; set; }
        public string MaThungGang { get; set; }
        public string BKMIS_SoMe { get; set; }
        public string BKMIS_ThungSo { get; set; }
        public string BKMIS_Gio { get; set; }
        public string BKMIS_PhanLoai { get; set; }
        public float KL_XeGoong { get; set; }
        public DateTime NgayLuyenGang { get; set; }
        public float G_KLThungChua { get; set; }
        public float G_KLThungVaGang { get; set; }
        public float G_KLGangLong { get; set; }
        public string ChuyenDen { get; set; }
        public string Gio_NM { get; set; }
        public string? G_GhiChu { get; set; }
        public DateTime NgayLuyenThep { get; set; }
        public bool KR { get; set; }
        public string MaThungThep { get; set; }
        public float T_KLThungVaGang { get; set; }
        public float T_KLThungChua { get; set; }
        public float T_KLGangLong { get; set; }
        public int ThungTrungGian { get; set; }
        public float T_KLThungVaGang_Thoi { get; set; }
        public float T_KLThungChua_Thoi { get; set; }
        public float T_KLGangLongThoi { get; set; }
        public string? T_GhiChu { get; set; }

        //Các liên kết với các bảng khác
        public int ID_TrangThai { get; set; } // trang thai chung cua P KH
        public int G_ID_TrangThai { get; set; } // trang thai cua gang
        public int T_ID_TrangThai { get; set; } // trang thai cua thep
        public int G_ID_NguoiLuu { get; set; }
        public int? G_ID_NguoiChuyen { get; set; }
        public int G_ID_NguoiThuHoi { get; set; }
        public int T_ID_NguoiLuu { get; set; }
        public int T_ID_NguoiHuyNhan { get; set; }
        public int ID_NguoiChot { get; set; }
        public List<NguoiNhanModel> T_NguoiNhan { get; set; } // List<Any> Dạng JSON [{"ID_NguoiNhan": 3, "TongSoLanNhan": 2},{"ID_NguoiNhan": 4, "TongSoLanNhan": 1}]
        public int ID_Locao { get; set; }
        public int G_ID_Kip { get; set; }
        public int T_ID_Kip { get; set; }
        public int ID_LoThoi { get; set; }
        public int ID_MeThoi { get; set; }
        public int ID_Phieu { get; set; }
    }
}
