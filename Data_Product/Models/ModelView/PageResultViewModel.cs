namespace Data_Product.Models.ModelView
{
    public class PageResultViewModel<T>
    {
        public int TotalRecords { get; set; }
        public decimal SumKLGang { get; set; }
        public decimal SumKLGangLongThep { get; set; }
        public decimal SumKLGangNhan { get; set; }
        public decimal SumKLPhe { get; set; }
        public decimal SumKLVaoLoThoi { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
