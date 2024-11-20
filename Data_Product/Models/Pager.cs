namespace Data_Product.Models
{
    public class Pager
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }

        public int TotalPages { get; set; }
        public int StartPages { get; set; }
        public int EndPages { get; set; }

        public Pager()
        {

        }
        public Pager(int totalItems, int page, int pageSize = 10)
        {
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            int currentPage = page;

            int startPages = currentPage - 5;
            int endPages = currentPage + 4;

            if (startPages <= 0)
            {
                endPages = endPages - (startPages - 1);
                startPages = 1;
            }
            if (endPages > totalPages)
            {
                endPages = totalPages;
                if (endPages > 10)
                {
                    startPages = endPages - 9;
                }
            }

            TotalItems = totalPages;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPages = startPages;
            EndPages = endPages;
        }
    }
}
