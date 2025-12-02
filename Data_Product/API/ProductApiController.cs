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
                        Ma_VAT_TU = reader["Ma_VAT_TU"]?.ToString(),
                        TEN_LO = reader["TEN_LO"]?.ToString(),
                        DVT = reader["DVT"]?.ToString(),
                        NHAP_KL = reader.GetDouble(reader.GetOrdinal("NHAP_KL")),
                        NHAP_DO_AM = reader.GetDouble(reader.GetOrdinal("NHAP_DO_AM")),
                        NHAP_QUY_KHO = reader.GetDouble(reader.GetOrdinal("NHAP_QUY_KHO")),
                        NHAP_XUONG = reader["NHAP_XUONG"]?.ToString(),
                        NHAP_MA_XUONG = reader["NHAP_MA_XUONG"]?.ToString(),
                        NHAP_BO_PHAN = reader["NHAP_BO_PHAN"]?.ToString(),
                        NHAP_MA_BO_PHAN = reader["NHAP_MA_BO_PHAN"]?.ToString(),
                        NHAP_HO_TEN = reader["NHAP_HO_TEN"]?.ToString(),
                        NHAP_MA_NV = reader["NHAP_MA_NV"]?.ToString(),
                        XUAT_KL = reader.GetDouble(reader.GetOrdinal("XUAT_KL")),
                        XUAT_DO_AM = reader.GetDouble(reader.GetOrdinal("XUAT_DO_AM")),
                        XUAT_QUY_KHO = reader.GetDouble(reader.GetOrdinal("XUAT_QUY_KHO")),
                        XUAT_XUONG = reader["XUAT_XUONG"]?.ToString(),
                        XUAT_MA_XUONG = reader["XUAT_MA_XUONG"]?.ToString(),
                        XUAT_BO_PHAN = reader["XUAT_BO_PHAN"]?.ToString(),
                        XUAT_MA_BO_PHAN = reader["XUAT_MA_BO_PHAN"]?.ToString(),
                        XUAT_HO_TEN = reader["XUAT_HO_TEN"]?.ToString(),
                        XUAT_MA_NV = reader["XUAT_MA_NV"]?.ToString(),
                        GHI_CHU = reader["GHI_CHU"]?.ToString(),
                        MA_PHIEU = reader["MA_PHIEU"]?.ToString(),
                        MA_TINH_TRANG_PHIEU = reader["TINH_TRANG_PHIEU"]?.ToString(),
                        MA_QUY_TRINH = reader["MA_QUY_TRINH"]?.ToString(),
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

        [HttpGet("GetKhoiLuongGang")]

        public async Task<IActionResult> GetKhoiLuongGang(DateTime? tuNgay, DateTime? denNgay, int? IDLoCao)
        {

            try
            {
                if (!BasicAuth.IsAuthorized(HttpContext, "api", "123456a@"))
                {
                    Response.Headers["WWW-Authenticate"] = "Basic";
                    return Unauthorized("Bạn không có quyền truy cập.");
                }
                var result = new List<KhoiLuongGangDto>();

                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_GetKhoiLuongGang";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@IDLoCao", IDLoCao ?? (object)DBNull.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dto = new KhoiLuongGangDto
                    {
                        ID_LOCAO = reader.GetInt32(reader.GetOrdinal("ID_LOCAO")),
                        NGAY_TAO = reader.GetDateTime(reader.GetOrdinal("NGAY_TAO")),
                        G_KLGANGLONG = reader.IsDBNull(reader.GetOrdinal("G_KLGANGLONG"))? 0 : reader.GetDecimal(reader.GetOrdinal("G_KLGANGLONG")),
                        SO_ME = reader["SO_ME"]?.ToString(),
                    };

                    result.Add(dto);
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    Total = result != null ? result.Count() : 0,
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


        [HttpGet("GetDuLieuThungGang")]
        public async Task<IActionResult> GetDuLieuThungGang(DateTime? tuNgay, DateTime? denNgay, int? ca)
        {
            var result = new List<ThoiGianThungGang>();

            try
            {
                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_GetGangLongTheoNgayCa";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new SqlParameter("@TuNgay", tuNgay ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@DenNgay", denNgay ?? (object)DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Ca", ca ?? (object)DBNull.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dto = new ThoiGianThungGang
                    {
                        BKMIS_SoMe = reader["BKMIS_SoMe"]?.ToString(),
                        BKMIS_ThungSo = reader["BKMIS_ThungSo"]?.ToString(),
                        BKMIS_Gio = reader["BKMIS_Gio"]?.ToString(),
                        G_Ca = reader.IsDBNull(reader.GetOrdinal("G_Ca")) ? 0 : reader.GetInt32(reader.GetOrdinal("G_Ca")),
                        Gio_NM = reader["Gio_NM"]?.ToString(),
                        NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                        ID_LoCao = reader.GetInt32(reader.GetOrdinal("ID_Locao")),
                        ChuyenDen = reader["ChuyenDen"]?.ToString(),
                        G_KLGangLong = reader.IsDBNull(reader.GetOrdinal("G_KLGangLong")) ? 0 : reader.GetDecimal(reader.GetOrdinal("G_KLGangLong")),
                        GioChonMe = reader["GioChonMe"]?.ToString()
                    };

                    result.Add(dto);
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    Total = result != null ? result.Count() : 0,
                    Message = "Thành công!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Đã xảy ra lỗi trong quá trình lấy dữ liệu.",
                    error = ex.Message
                });
            }
        }


        [HttpGet("GetDuLieuThungGangThoiDiem")]
        public async Task<IActionResult> GetDuLieuThungGangThoiDiem(DateTime? date)
        {
            var result = new List<ThoiGianThungGangThoiDiem>();

            try
            {
                using var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "sp_GetGangLongTheoThoiDiem";
                cmd.CommandType = CommandType.StoredProcedure;

                // Nếu muốn truyền ngày, bạn có thể thêm param
                // cmd.Parameters.Add(new SqlParameter("@Ngay", date ?? (object)DBNull.Value));

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dto = new ThoiGianThungGangThoiDiem
                    {
                        BKMIS_SoMe = reader["BKMIS_SoMe"]?.ToString(),
                        BKMIS_ThungSo = reader["BKMIS_ThungSo"]?.ToString(),
                        BKMIS_Gio = reader["BKMIS_Gio"]?.ToString(),
                        G_Ca = reader.GetInt32(reader.GetOrdinal("G_Ca")),
                        Gio_NM = reader["Gio_NM"]?.ToString(),
                        NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                        ID_LoCao = reader.GetInt32(reader.GetOrdinal("ID_LoCao")),
                        ChuyenDen = reader["ChuyenDen"]?.ToString(),
                        G_KLGangLong = reader.IsDBNull(reader.GetOrdinal("G_KLGangLong"))
                                        ? 0
                                        : reader.GetDecimal(reader.GetOrdinal("G_KLGangLong")),
                        GioChonMe = reader["GioChonMe"]?.ToString(),
                        G_ID_TrangThai = reader.GetInt32(reader.GetOrdinal("G_ID_TrangThai")),
                        T_ID_TrangThai = reader.GetInt32(reader.GetOrdinal("T_ID_TrangThai"))
                    };
                    result.Add(dto);
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    Total = result != null ? result.Count() : 0,
                    Message = "Thành công!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Đã xảy ra lỗi khi lấy dữ liệu.",
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
