namespace Data_Product.DTO
{
    public class ThoiGianThungGang
    {
        public string BKMIS_SoMe { get; set; }
        public string BKMIS_ThungSo { get; set; }
        public string BKMIS_Gio { get; set; }
        public int G_Ca { get; set; }
        public string Gio_NM { get; set; }
        public DateTime NgayTao { get; set; }
        public int ID_LoCao { get; set; }
        public string ChuyenDen { get; set; }
        public decimal G_KLGangLong { get; set; }
        public string GioChonMe { get; set; }
    }

    public class ThoiGianThungGangThoiDiem
    {
        public int ID { get; set; }
        public string BKMIS_SoMe { get; set; }
        public string BKMIS_ThungSo { get; set; }
        public string BKMIS_Gio { get; set; }
        public int G_Ca { get; set; }
        public int G_ID_Kip {  get; set; }
        public string Gio_NM { get; set; }
        public DateTime NgayTao { get; set; }
        public int ID_LoCao { get; set; }
        public string ChuyenDen { get; set; }
        public decimal G_KLGangLong { get; set; }
        public string GioChonMe { get; set; }
        public int G_ID_TrangThai { get; set; }
        public int T_ID_TrangThai { get; set; }
        public decimal NhietDo { get; set; }
        public decimal KL_XeGoong { get; set; }
        public TPHHDto TPHH { get; set; }
    }
    public class TPHHDto
    {
        public decimal C { get; set; }
        public decimal Si { get; set; }
        public decimal Mn { get; set; }
        public decimal S { get; set; }
        public decimal P { get; set; }
        public decimal Ti { get; set; }
        public decimal Temp { get; set; }
    }

    class ToHopGang
    {
        public int TtgId { get; set; }
        public string NoiNhan { get; set; }
        public string SoThungTG { get; set; }
        public HashSet<string> GangSet { get; set; }
    }

    public class TtgKrErrorDto
    {
        public int ID_TTG { get; set; }
        public string NoiNhan { get; set; }
        public string SoThungTG { get; set; }
        public string Reason { get; set; }
    }
}
