using System.Text;
using System.Text.Json;

namespace Data_Product.Services
{
    public interface IV2AuthService
    {
        // Trả về JWT nếu đăng nhập v2 thành công, null nếu lỗi / sai tài khoản
        Task<string?> LoginAsync(string username, string password);
    }

    public class V2AuthService : IV2AuthService
    {
        // Tên cookie lưu token v2 trên trình duyệt
        public const string TokenCookieName = "_v2token";

        private const string DefaultLoginUrl = "https://report.hoaphatdungquat.vn:60528/api/TaiKhoan/login";
        private static readonly string[] TokenProps = { "token", "accessToken", "access_token", "Token", "AccessToken" };

        private readonly HttpClient _httpClient;
        private readonly string _loginUrl;
        private readonly ILogger<V2AuthService> _logger;

        public V2AuthService(HttpClient httpClient, IConfiguration configuration, ILogger<V2AuthService> logger)
        {
            _httpClient = httpClient;
            _loginUrl = configuration["V2Api:LoginUrl"] ?? DefaultLoginUrl;
            _logger = logger;
        }

        public async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                var payload = JsonSerializer.Serialize(new { username, password });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_loginUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Login v2 thất bại cho {User}: HTTP {Status}", username, (int)response.StatusCode);
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                foreach (var prop in TokenProps)
                {
                    if (root.TryGetProperty(prop, out var val) && val.ValueKind == JsonValueKind.String)
                    {
                        var token = val.GetString();
                        return string.IsNullOrEmpty(token) ? null : token;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không gọi được API login v2 cho {User}", username);
                return null;
            }
        }
    }
}
