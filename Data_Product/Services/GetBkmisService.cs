using Data_Product.Models;
using MySqlConnector;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using Data_Product.Repositorys;

namespace Data_Product.Services
{
    public class GetBkmisService
    {
        private readonly string _connStr;
        private readonly ILogger<GetBkmisService> _logger;
        private readonly DataContext _context;

        public GetBkmisService(IConfiguration config, ILogger<GetBkmisService> logger, DataContext _context)
        {
            _connStr = config.GetConnectionString("BKMIS")
                       ?? throw new InvalidOperationException("Missing ConnectionStrings:BKMIS");
            _logger = logger;
            this._context = _context;
        }

        public async Task<List<Bkmis_view>> GetSoMeBKMisAsync(string ngay, int? idLoCao, string idKip)
        {
            var result = new List<Bkmis_view>();
            string shiftName = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(idKip))
                {
                    var parsedDate = DateTime.Parse(ngay);
                    int kipId = int.Parse(idKip);

                    var ca = await _context.Tbl_Kip.AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ID_Kip == kipId && x.NgayLamViec == parsedDate);

                    shiftName = ca?.TenCa + ca?.TenKip; // VD: 1A, 2A...
                }

                    // Xác định tên bảng dựa theo idLoCao
                    var table = idLoCao switch
                    {
                        1 => "view_dq1_lg_daura_lc1",
                        2 => "view_dq1_lg_daura_lc2",
                        3 => "view_dq1_lg_daura_lc3",
                        4 => "view_dq1_lg_daura_lc4",
                        5 => "view_dq2_kqganglocao",
                        6 => "view_dq2_kqganglocao_6",
                        _ => throw new ArgumentOutOfRangeException(nameof(idLoCao), "ID Lò cao không hợp lệ")
                    };

                    string query = $@" SELECT TestPatternCode, ClassifyName, ProductionDate, ShiftName, 
                                             InputTime, Patterntime, TestPatternName
                                      FROM bkmis_kcshpsdq.{table}
                                      WHERE ProductionDate = @ProductionDate AND ShiftName = @ShiftName
                                     ";
                await using var conn = new MySqlConnection(_connStr);
                {
                    await conn.OpenAsync();
                    Console.WriteLine("Kết nối thành công!");


                    await using var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ProductionDate", DateTime.Parse(ngay).Date);
                    cmd.Parameters.AddWithValue("@ShiftName", shiftName);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new Bkmis_view
                            {
                                TestPatternCode = reader["TestPatternCode"]?.ToString()?.Trim(),
                                ClassifyName = reader["ClassifyName"]?.ToString(),
                                ProductionDate = reader["ProductionDate"]?.ToString(),
                                ShiftName = reader["ShiftName"]?.ToString(),
                                InputTime = reader["InputTime"]?.ToString(),
                                Patterntime = reader["Patterntime"]?.ToString(),
                                TestPatternName = reader["TestPatternName"]?.ToString(),
                            });
                        }
                        reader.Close();
                        await reader.DisposeAsync();
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu BK-MIS: Ngày={Ngay}, Lò cao={LoCao}, Ca={Ca}", ngay, idLoCao, idKip);
                throw;
            }

            return result;
        }
        public async Task<decimal?> GetKhoiLuongXeLoCaoAsync(int idLoCao)
        {
            var xe = await _context.Tbl_XeGoong.FirstOrDefaultAsync(x => x.ID_LoCao == idLoCao);
            return xe?.KL_Xe;
        }

       
    }
}
