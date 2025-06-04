using System.ComponentModel.DataAnnotations;

namespace Data_Product.Common.Enums
{
    public enum TinhTrang
    {
        [Display(Name = "Chưa chuyển")]
        ChuaChuyen = 1,
        
        [Display(Name = "Chờ xử lý")]
        ChoXuLy = 2,
        
        [Display(Name = "Đã chuyển")]
        DaChuyen = 3,
        
        [Display(Name = "Đã nhận")]
        DaNhan = 4,

        [Display(Name = "Đã chốt")]
        DaChot = 5
    }
}
