using System.ComponentModel.DataAnnotations;

namespace Data_Product.Common.Enums
{
    public enum TinhTrang
    {
        [Display(Name = "Chưa xử lý")]
        ChuaChuyen = 1,
        
        [Display(Name = "Chờ xử lý")]
        ChoXuLy = 2,
        
        [Display(Name = "Đã xử lý")]
        DaChuyen = 3,
        
        [Display(Name = "Đã nhận")]
        DaNhan = 4,

        [Display(Name = "Đã chốt")]
        DaChot = 5
    }
}
