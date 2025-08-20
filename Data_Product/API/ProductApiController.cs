using Data_Product.DTO;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Data_Product.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductApiController : ControllerBase
    {
        private readonly DataContext _context;
        public ProductApiController(DataContext _context)
        {
            this._context = _context;
        }
        [HttpGet("GetBBGN")]
        public async Task<IActionResult> Get(DateTime? tuNgay, DateTime? denNgay,int? IDPhongBan,int? IDXuong,int? IDPhongBan_BG,int? IDXuong_BG,int? IDPhongBan_BN,int? IDXuongBN,int? IDTinhTrangPhieu)
        {
           
            try
            {
                if (!BasicAuth.IsAuthorized(HttpContext, "api", "123456a@"))
                {
                    Response.Headers["WWW-Authenticate"] = "Basic";
                    return Unauthorized("Bạn không có quyền truy cập.");
                }
                var result = new List<PhieuBBGNDto>();

                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_GetBienBanGiaoNhan";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDPhongBan", IDPhongBan ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDXuong", IDXuong ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDPhongBanBG", IDPhongBan_BG ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDXuongBG", IDXuong_BG ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDPhongBanBN", IDPhongBan_BN ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDXuongBN", IDXuongBN ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDTinhtrang", IDTinhTrangPhieu ?? (object)DBNull.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dto = new PhieuBBGNDto
                    {
                        ID_PNX = reader.GetInt32(reader.GetOrdinal("ID_PNX")),
                        NGAY = reader.GetDateTime(reader.GetOrdinal("NGAY")),
                        KIP = reader["KIP"]?.ToString(),
                        CA = reader["CA"]?.ToString(),
                        NOI_DUNG = reader["NOI_DUNG"]?.ToString(),
                        TEN_LO = reader["TEN_LO"]?.ToString(),
                        DVT = reader["DVT"]?.ToString(),
                        NHAP_KL = reader.GetDouble(reader.GetOrdinal("NHAP_KL")),
                        NHAP_DO_AM = reader.GetDouble(reader.GetOrdinal("NHAP_DO_AM")),
                        NHAP_QUY_KHO = reader.GetDouble(reader.GetOrdinal("NHAP_QUY_KHO")),
                        NHAP_XUONG = reader["NHAP_XUONG"]?.ToString(),
                        NHAP_BO_PHAN = reader["NHAP_BO_PHAN"]?.ToString(),
                        XUAT_KL = reader.GetDouble(reader.GetOrdinal("XUAT_KL")),
                        XUAT_DO_AM = reader.GetDouble(reader.GetOrdinal("XUAT_DO_AM")),
                        XUAT_QUY_KHO = reader.GetDouble(reader.GetOrdinal("XUAT_QUY_KHO")),
                        XUAT_XUONG = reader["XUAT_XUONG"]?.ToString(),
                        XUAT_BO_PHAN = reader["XUAT_BO_PHAN"]?.ToString(),
                        GHI_CHU = reader["GHI_CHU"]?.ToString(),
                        MA_PHIEU = reader["MA_PHIEU"]?.ToString(),
                        TINH_TRANG_PHIEU = GetTinhTrangPhieu(reader["TINH_TRANG_PHIEU"]?.ToString())
                    };

                    result.Add(dto);
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    Total = result != null? result.Count():0,
                    Message = "Thành công!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Đã xảy ra lỗi trong quá trình xử lý.",
                    error = ex.Message 
                });
            }
        }
        public string GetTinhTrangPhieu(string tinhtrang)
        {
            switch (tinhtrang)
            {
                case "0":
                    return "Chưa xử lý";
                case "1":
                    return  "Đã xử lý";
                case "2":
                    return "BN Hủy Phiếu";
                case "3":
                    return "Đề nghị hiệu chỉnh";
                case "4":
                    return "PKH Hủy phiếu";
                case "5":
                    return "Xóa Phiếu";

                default:
                    return "";
            }
        }
    }
    
    public static class DataReaderExtensions
    {
        public static T? SafeGet<T>(this DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(ordinal)) return default;
            return (T)reader.GetValue(ordinal);
        }
    }
    public static class BasicAuth
    {
        public static bool IsAuthorized(HttpContext context, string expectedUser, string expectedPass)
        {
            var authHeader = context.Request.Headers["Authorization"];
            if (authHeader.Count == 0 || !authHeader.ToString().StartsWith("Basic ")) return false;

            var encoded = authHeader.ToString().Substring("Basic ".Length).Trim();
            var credentialBytes = Convert.FromBase64String(encoded);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var username = credentials[0];
            var password = credentials[1];

            return username == expectedUser && password == expectedPass;
        }
    }
}
