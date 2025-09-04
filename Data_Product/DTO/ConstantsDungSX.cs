namespace Data_Product.DTO
{
    public class ConstantsDungSX
    {
        // Constant ID
        public const int KhongXacNhan = -1;
        public const int DaGui = 0;
        public const int HoanTat = 1;
        public const int DangXoaPhieu = 2;
        public const int DaXoaPhieu = 3;
        public const int ChoXuLy = 4;
        public const int KhongDinhTre = 5;

        // Dictionary mapping ID → (Tên, Màu)
        public static readonly Dictionary<int, (int status, string Ten, string Color, string statusBTBD)> Map =
            new Dictionary<int, (int status,string Ten, string Color, string statusBTBD)>
            {
                { KhongXacNhan, (KhongXacNhan,"Không xác nhận", "#dc3545","Không xác nhận") },   // đỏ
                { DaGui,        (DaGui,"Đã gửi", "#fd7e14","Chờ xử lý") },          // cam
                { HoanTat,      (HoanTat,"Hoàn tất", "#28a745","Hoàn tất") },        // xanh lá
                { DangXoaPhieu, (DangXoaPhieu,"Đang xóa phiếu", "#17a2b8","Đang xóa phiếu") },  // xanh dương nhạt
                { DaXoaPhieu,   (DaXoaPhieu,"Đã xóa phiếu", "#6c757d","Đã xóa phiếu") },    // xám
                { ChoXuLy,      (ChoXuLy,"Chờ xử lý", "#007bff","Chưa xử lý") },       // xanh nước biển
                { KhongDinhTre, (KhongDinhTre,"Không đình trệ", "#ffc107","Không đình trệ") }   // vàng
            };

        // Hàm lấy tên tình trạng
        public static string TinhTrang(int statusId)
        {
            return Map.ContainsKey(statusId) ? Map[statusId].Ten : "Không rõ";
        }

        public static string TinhTrangBTBT(int statusId)
        {
            return Map.ContainsKey(statusId) ? Map[statusId].statusBTBD : "Không rõ";
        }

        // Hàm lấy màu
        public static string Mau(int statusId)
        {
            return Map.ContainsKey(statusId) ? Map[statusId].Color : "đen";
        }
        
    }
}
