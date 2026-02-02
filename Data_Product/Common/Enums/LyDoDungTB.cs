namespace Data_Product.Common.Enums
{
    public class LyDoDungTB
    {
        public static string GetLyDoDungThietBi(int? lyDo)
        {
            if (!lyDo.HasValue)
                return "Không xác định";

            if (!Enum.IsDefined(typeof(LyDoDungThietBiEnum), lyDo.Value))
                return "Không xác định";

            return ((LyDoDungThietBiEnum)lyDo.Value) switch
            {
                LyDoDungThietBiEnum.SuCoCongNghe => "Dừng máy do Sự cố công nghệ",
                LyDoDungThietBiEnum.ChoCongNghe => "Dừng máy do Chờ công nghệ",
                LyDoDungThietBiEnum.KhachQuan => "TG dừng khách quan",
                LyDoDungThietBiEnum.ChoKhac => "TG chờ khác",
                LyDoDungThietBiEnum.BTBDThucTe => "TG dừng BTBD thực tế",
                _ => "Không xác định"
            };
        }
    }
    public enum LyDoDungThietBiEnum
    {
        SuCoCongNghe = 1,
        ChoCongNghe = 2,
        KhachQuan = 3,
        ChoKhac = 4,
        BTBDThucTe = 5
    }

}
