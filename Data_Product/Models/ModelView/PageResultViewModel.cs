namespace Data_Product.Models.ModelView
{
    public class PageResultViewModel<T>
    {
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; } = new();
    }
}
