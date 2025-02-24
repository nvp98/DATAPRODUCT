using System.ComponentModel.DataAnnotations;

namespace Data_Product.Models
{
    public class Tbl_ChiTiet_BBGNGangLong
    {
        [Key]
        public int IDCTGL { get; set; }
        public int ID_BBGL { get; set; }
        public int ID_LoCao { get; set; }
        public string SoMeGang { get; set; }
        public string SoThung { get; set; }
        public string ThoiGian { get; set; }
        public string SoID { get; set; }
        public double KL_ThungGang { get; set; }
        public double KL_Thung { get; set; }
        public double KL_Gang { get; set; }
        public string GhiChu { get; set; }
        public Tbl_ChiTiet_BBGNGangLong() { }  // Bắt buộc để EF Core hoạt động

        public Tbl_ChiTiet_BBGNGangLong(int IDCTGL_, int ID_BBGL_, int ID_LoCao_, string SoMeGang_, string SoThung_, string ThoiGian_, string SoID_, double KL_ThungGang_, double KL_Thung_, double KL_Gang_, string GhiChu_)
        {
            this.IDCTGL = IDCTGL_;
            this.ID_BBGL = ID_BBGL_;
            this.ID_LoCao = ID_LoCao_;
            this.SoMeGang = SoMeGang_;
            this.SoThung = SoThung_;
            this.ThoiGian = ThoiGian_;
            this.SoID = SoID_;
            this.KL_ThungGang = KL_ThungGang_;
            this.KL_Thung = KL_Thung_;
            this.KL_Gang = KL_Gang_;
            this.GhiChu = GhiChu_;
        }

    }
}
