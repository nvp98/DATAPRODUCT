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
    public static class GlobalVariables
    {
        public static int ID_PKH = 70;
        public static string HPDQ02384 = "HPDQ02384";
    }
}
