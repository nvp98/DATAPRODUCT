namespace Data_Product.DTO.BM_16_DTO
{
        public class CanRayLG1Dto
        {
            public int ID_LoCao { get; set; }
            public int? SanRaGang { get; set; }
            public int? ThungSo { get; set; }
            public string Gio { get; set; }
            public decimal? KL_Bi { get; set; }
            public decimal? KL_Tong { get; set; }
            public decimal? KL_Gang { get; set; }
            public string BKMIS_SoMe { get; set; }
            public string GhiChu { get; set; }
            public int? Ray { get; set; }
        }
        public class CanRayLG2Dto
        {
            public int BF_no { get; set; }
            public int? ThungSo { get; set; }
            // public int? Laddle_no { get; set; }
            public int? Shift { get; set; }
            public string Gio { get; set; }
            //public int? Casthouse { get; set; }
            public int? SanRaGang { get; set; }
            public decimal? KL_Bi { get; set; }
            public decimal? KL_Tong { get; set; }
            public decimal? KL_Gang { get; set; }
            public string BKMIS_SoMe { get; set; }
            public string GhiChu { get; set; }
            // Các trường khác nếu cần!
        }
        // DTO class
        public class ClearSoMeRequest
        {
            public int CanRayId { get; set; }
            public int ID_LoCao { get; set; }
        }
}
