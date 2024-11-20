namespace Data_Product.Models
{
    using Microsoft.AspNetCore.Http;
    public class MyAuthentication
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public MyAuthentication(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
    }
}
