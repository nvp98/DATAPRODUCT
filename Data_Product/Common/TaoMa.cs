namespace Data_Product.Common
{

    public static class TaoMa
    {
        // Giả sử bạn có một bộ mã đã tạo trước đó (có thể thay thế bằng cơ sở dữ liệu hoặc bộ nhớ)
        private static readonly HashSet<string> ExistingCodes = new HashSet<string>();

        // Bộ ký tự có thể sử dụng để tạo mã
        private static readonly string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        // Phương thức tạo mã không trùng lặp
        public static string GenerateSafeCode(int length)
        {
            string code;
            do
            {
                code = GenerateRandomCode(length);
            }
            while (ExistingCodes.Contains(code)); // Kiểm tra xem mã đã tồn tại hay chưa

            ExistingCodes.Add(code); // Lưu mã vào danh sách mã đã tạo
            return code;
        }

        // Phương thức tạo mã ngẫu nhiên
        private static string GenerateRandomCode(int length)
        {
            var random = new Random();
            var code = new string(Enumerable.Range(0, length)
                .Select(_ => Characters[random.Next(Characters.Length)])
                .ToArray());
            return code;
        }
    }
}
