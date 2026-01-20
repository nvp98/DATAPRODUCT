using Data_Product.Common.Enums;
using Data_Product.DTO;
using Data_Product.Models;
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
        public async Task<IActionResult> Get(DateTime? tuNgay, DateTime? denNgay, int? IDPhongBan, int? IDXuong, int? IDPhongBan_BG, int? IDXuong_BG, int? IDPhongBan_BN, int? IDXuongBN, int? IDTinhTrangPhieu)
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
                        G_KLGANGLONG = reader.IsDBNull(reader.GetOrdinal("G_KLGANGLONG")) ? 0 : reader.GetDecimal(reader.GetOrdinal("G_KLGANGLONG")),
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
            var result = new List<ThoiGianThungGangThoiDiem>();

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
                    var dto = new ThoiGianThungGangThoiDiem
                    {
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        BKMIS_SoMe = reader["BKMIS_SoMe"]?.ToString(),
                        BKMIS_ThungSo = reader["BKMIS_ThungSo"]?.ToString(),
                        BKMIS_Gio = reader["BKMIS_Gio"]?.ToString(),
                        G_Ca = reader.GetInt32(reader.GetOrdinal("G_Ca")),
                        G_ID_Kip = reader.GetInt32(reader.GetOrdinal("G_ID_Kip")),
                        Gio_NM = reader["Gio_NM"]?.ToString(),
                        NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                        ID_LoCao = reader.GetInt32(reader.GetOrdinal("ID_LoCao")),
                        ChuyenDen = reader["ChuyenDen"]?.ToString(),
                        G_KLGangLong = reader.IsDBNull(reader.GetOrdinal("G_KLGangLong"))
                ? 0
                : reader.GetDecimal(reader.GetOrdinal("G_KLGangLong")),
                        GioChonMe = reader["GioChonMe"]?.ToString(),
                        G_ID_TrangThai = reader.GetInt32(reader.GetOrdinal("G_ID_TrangThai")),
                        T_ID_TrangThai = reader.GetInt32(reader.GetOrdinal("T_ID_TrangThai")),
                        NhietDo = reader.IsDBNull(reader.GetOrdinal("NhietDo"))
                ? 0
                : reader.GetDecimal(reader.GetOrdinal("NhietDo")),
                        KL_XeGoong = reader.IsDBNull(reader.GetOrdinal("KL_XeGoong"))
                ? 0
                : reader.GetDecimal(reader.GetOrdinal("KL_XeGoong"))

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
                        ID = reader.GetInt32(reader.GetOrdinal("ID")),
                        BKMIS_SoMe = reader["BKMIS_SoMe"]?.ToString(),
                        BKMIS_ThungSo = reader["BKMIS_ThungSo"]?.ToString(),
                        BKMIS_Gio = reader["BKMIS_Gio"]?.ToString(),
                        G_Ca = reader.GetInt32(reader.GetOrdinal("G_Ca")),
                        G_ID_Kip = reader.GetInt32(reader.GetOrdinal("G_ID_Kip")),
                        Gio_NM = reader["Gio_NM"]?.ToString(),
                        NgayTao = reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                        ID_LoCao = reader.GetInt32(reader.GetOrdinal("ID_LoCao")),
                        ChuyenDen = reader["ChuyenDen"]?.ToString(),
                        G_KLGangLong = reader.IsDBNull(reader.GetOrdinal("G_KLGangLong"))
                ? 0
                : reader.GetDecimal(reader.GetOrdinal("G_KLGangLong")),
                        GioChonMe = reader["GioChonMe"]?.ToString(),
                        G_ID_TrangThai = reader.GetInt32(reader.GetOrdinal("G_ID_TrangThai")),
                        T_ID_TrangThai = reader.GetInt32(reader.GetOrdinal("T_ID_TrangThai")),
                        NhietDo = reader.IsDBNull(reader.GetOrdinal("NhietDo"))
                ? 0
                : reader.GetDecimal(reader.GetOrdinal("NhietDo")),
                        KL_XeGoong = reader.IsDBNull(reader.GetOrdinal("KL_XeGoong"))
                ? 0
                : reader.GetDecimal(reader.GetOrdinal("KL_XeGoong"))

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
                    return "Đã xử lý";
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

        public string GetTinhTrangNhatKySX(int tinhTrang)
        {
            switch (tinhTrang)
            {
                case 0:
                    return "Chưa xác nhận";
                case 1:
                    return "Đã xác nhận";
                case 2:
                    return "Đã duyệt";
                case 3:
                    return "Hủy";
                default:
                    return "Không xác định";
            }
        }
        [HttpGet("GetKLGangLongDetailsTongHop")]
        public async Task<IActionResult> GetTongHopKLGangLong(
                [FromQuery] DateTime? ngayBatDau = null,
                [FromQuery] DateTime? ngayKetThuc = null,
                [FromQuery] int? idLocao = null,
                [FromQuery] int? idKip = null)
        {
            try
            {
                if (!BasicAuth.IsAuthorized(HttpContext, "apilg", "123456a@"))
                {
                    Response.Headers["WWW-Authenticate"] = "Basic";
                    return Unauthorized("Bạn không có quyền truy cập.");
                }

                var result = new List<TongHopKLGangLongDto>();

                // Lấy connection string từ DbContext
                var connection = _context.Database.GetDbConnection();

                // Tạo command
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_GetKLGangLongDetails_TongHop";
                    command.CommandType = CommandType.StoredProcedure;

                    // Thêm parameters
                    command.Parameters.Add(new SqlParameter("@NgayBatDau", SqlDbType.Date)
                    {
                        Value = ngayBatDau.HasValue ? (object)ngayBatDau.Value : DBNull.Value
                    });
                    command.Parameters.Add(new SqlParameter("@NgayKetThuc", SqlDbType.Date)
                    {
                        Value = ngayKetThuc.HasValue ? (object)ngayKetThuc.Value : DBNull.Value
                    });
                    command.Parameters.Add(new SqlParameter("@ID_Locao", SqlDbType.Int)
                    {
                        Value = idLocao.HasValue ? (object)idLocao.Value : DBNull.Value
                    });
                    command.Parameters.Add(new SqlParameter("@ID_Kip", SqlDbType.Int)
                    {
                        Value = idKip.HasValue ? (object)idKip.Value : DBNull.Value
                    });

                    // Mở connection nếu chưa mở
                    if (connection.State != ConnectionState.Open)
                    {
                        await connection.OpenAsync();
                    }

                    // Thực thi và đọc dữ liệu
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dto = new TongHopKLGangLongDto
                            {
                                Ngay = reader.GetDateTime(reader.GetOrdinal("Ngay")),
                                TenLoCao = reader.IsDBNull(reader.GetOrdinal("TenLoCao"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("TenLoCao")),
                                ID_Locao = reader.GetInt32(reader.GetOrdinal("ID_Locao")),
                                TenKip = reader.IsDBNull(reader.GetOrdinal("TenKip"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("TenKip")),
                                TenCa = reader.IsDBNull(reader.GetOrdinal("TenCa"))
                                    ? null
                                    : reader.GetString(reader.GetOrdinal("TenCa")),
                                ID_Kip = reader.GetInt32(reader.GetOrdinal("ID_Kip")),
                                SoLuongPhieu = reader.GetInt32(reader.GetOrdinal("SoLuongPhieu")),
                                SoLuongMe = reader.GetInt32(reader.GetOrdinal("SoLuongMe")),
                                SoLuongThung = reader.GetInt32(reader.GetOrdinal("SoLuongThung")),
                                Tong_KL_GangLong_CanRay = reader.GetDecimal(reader.GetOrdinal("Tong_KL_GangLong_CanRay")),
                                Tong_TongKL_TheoMe = reader.GetDecimal(reader.GetOrdinal("Tong_TongKL_TheoMe")),
                                Tong_KLDuc = reader.GetDecimal(reader.GetOrdinal("Tong_KLDuc"))
                            };

                            result.Add(dto);
                        }
                    }
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    total = result.Count,
                    filters = new
                    {
                        ngayBatDau,
                        ngayKetThuc,
                        idLocao,
                        idKip
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Lỗi khi lấy dữ liệu tổng hợp",
                    error = ex.Message
                });
            }
        }


        [HttpGet("GetNhatKySanXuat")]
        public async Task<IActionResult> GetNhatKySanXuat(
            [FromQuery] DateTime? tuNgay = null,
            [FromQuery] DateTime? denNgay = null,
            [FromQuery] int? tinhTrang = null)
        {
            try
            {
                if (!BasicAuth.IsAuthorized(HttpContext, "api", "123456a@"))
                {
                    Response.Headers["WWW-Authenticate"] = "Basic";
                    return Unauthorized("Bạn không có quyền truy cập.");
                }

                // Lấy dữ liệu Nhật ký sản xuất theo điều kiện lọc với join bảng Xuong và PhongBan
                var query = from nk in _context.Tbl_NhatKy_SanXuat
                            join xuong in _context.Tbl_Xuong on nk.ID_Xuong_SX equals xuong.ID_Xuong into xuongJoin
                            from xuong in xuongJoin.DefaultIfEmpty()
                            join phongban in _context.Tbl_PhongBan on nk.ID_PhongBan_SX equals phongban.ID_PhongBan into phongbanJoin
                            from phongban in phongbanJoin.DefaultIfEmpty()
                            where nk.IsDelete == false
                            select new
                            {
                                Phieu = nk,
                                TenXuong = xuong != null ? xuong.TenXuong : null,
                                TenPhongBan = phongban != null ? phongban.TenPhongBan : null
                            };

                if (tuNgay.HasValue)
                {
                    query = query.Where(x => x.Phieu.NgayDungSX >= tuNgay.Value);
                }

                if (denNgay.HasValue)
                {
                    var endDate = denNgay.Value.AddDays(1);
                    query = query.Where(x => x.Phieu.NgayDungSX < endDate);
                }

                if (tinhTrang.HasValue)
                {
                    query = query.Where(x => x.Phieu.TinhTrang == tinhTrang.Value);
                }

                var phieus = await query
                    .OrderByDescending(x => x.Phieu.NgayDungSX)
                    .ToListAsync();

                // Lấy ID của các phiếu
                var phieuIds = phieus.Select(x => x.Phieu.ID).ToList();

                // Lấy chi tiết cho tất cả các phiếu
                var chiTiets = await _context.Tbl_NhatKy_SanXuat_ChiTiet
                    .Where(x => phieuIds.Contains(x.ID_NhatKy))
                    .ToListAsync();
                var CumTBs = await _context.Tbl_NhatKy_CumTB.ToListAsync();

                // Nhóm chi tiết theo ID phiếu
                var chiTietGrouped = chiTiets.GroupBy(x => x.ID_NhatKy).ToDictionary(x => x.Key, x => x.ToList());
            
                // Tạo kết quả trả về
                var result = new List<dynamic>();

                foreach (var item in phieus)
                {
                    var phieu = item.Phieu;
                    var chiTietPhieu = chiTietGrouped.ContainsKey(phieu.ID)
                        ? chiTietGrouped[phieu.ID]
                        : new List<Tbl_NhatKy_SanXuat_ChiTiet>();

                    result.Add(new
                    {
                        phieu = new
                        {
                            id = phieu.ID,
                            soPhieu = phieu.SoPhieu,
                            ngayTao = phieu.NgayTao,
                            ngayDungSX = phieu.NgayDungSX,
                            ca = phieu.Ca,
                            kip = phieu.Kip,
                            //idKip = phieu.ID_Kip,
                            //idPhongBanSX = phieu.ID_PhongBan_SX,
                            tenPhongBanSX = item.TenPhongBan,
                            //idXuongSX = phieu.ID_Xuong_SX,
                            tenXuongSX = item.TenXuong,
                            tinhTrang = GetTinhTrangNhatKySX(phieu.TinhTrang),
                            //tinhTrangText = GetTinhTrangNhatKySX(phieu.TinhTrang),
                            //idNhanVienSX = phieu.ID_NhanVien_SX,
                            //idNhanVienBTBD = phieu.ID_NhanVien_BTBD,
                            //hoTenNhanVienBTBD = phieu.HoTen_NhanVien_BTBD,
                            //fileBB = phieu.FileBB,
                            ghiChu = phieu.GhiChu,
                            //isLock = phieu.IsLock
                        },
                        chiTiets = chiTietPhieu.Select(ct => new
                        {
                            //idct = ct.IDCT,
                            //idNhatKy = ct.ID_NhatKy,
                            //idXuong = ct.ID_Xuong,
                            thoiDiemDung = ct.ThoiDiemDung.ToString(@"hh\:mm\:ss"),
                            thoiDiemChay = ct.ThoiDiemChay.ToString(@"hh\:mm\:ss"),
                            lyDoDungThietBi = LyDoDungTB.GetLyDoDungThietBi(ct.LyDo_DungThietBi),
                            ghiChu = ct.GhiChu,
                            noiDungDung = ct.NoiDungDung,
                            thoiGianDung = ct.ThoiGianDung,
                            TenCumTB = CumTBs.FirstOrDefault(x=>x.ID ==ct.ID_CumTB)?.TenCumTB,
                            coDienSoLan = ct.CoDien_SoLan,
                            coDienChoXL = ct.CoDien_ChoXL,
                            coDienTGianXL = ct.CoDien_TGianXL,
                            coDienTGianSC = ct.CoDien_TGianSC,
                            dungDayChuyen = ct.DungDayChuyen,
                            tGianKHBTBD = ct.TGian_KH_BTBD
                        }).ToList()
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result,
                    Total = result.Count,
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
