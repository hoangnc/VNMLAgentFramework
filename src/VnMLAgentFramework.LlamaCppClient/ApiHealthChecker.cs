namespace VnMLAgentFramework.LlamaCppClient
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Lớp tiện ích để kiểm tra trạng thái hoạt động của API Health.
    /// </summary>
    public static class ApiHealthChecker
    {
        // Sử dụng chung một instance HttpClient để tránh lỗi cạn kiệt Socket (Socket Exhaustion)
        private static readonly HttpClient HttpClientInstance = new()
        {
            // Đặt timeout tổng thể thấp để tránh ứng dụng bị treo lâu khi API chết hẳn
            Timeout = TimeSpan.FromSeconds(5)
        };

        /// <summary>
        /// Kiểm tra xem API Health có hoạt động bình thường hay không (Mặc định check status code 200-299)
        /// </summary>
        /// <param name="url">Đường dẫn đầy đủ của API Health (ví dụ: http://localhost:8080/health)</param>
        /// <param name="timeoutSeconds">Thời gian chờ phản hồi tối đa (giây)</param>
        /// <returns>True nếu API sống, False nếu API chết hoặc phản hồi lỗi</returns>
        public static async Task<bool> IsAliveAsync(string url, int timeoutSeconds = 5)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            // Tạo CancellationToken tương ứng với thời gian timeout cấu hình riêng cho lượt check này
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                // Sử dụng HttpMethod.Get kết hợp ResponseHeadersRead giúp hàm trả về kết quả ngay khi 
                // nhận được Status Code từ server mà không cần mất thời gian tải toàn bộ Body về bộ nhớ.
                using var response = await HttpClientInstance.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead,
                    cts.Token);

                // Trả về true nếu status code nằm trong dải 2xx (ví dụ: 200 OK)
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or TimeoutException)
            {
                // Trình quản lý log lỗi nếu cần (Ví dụ: Console.WriteLine($"API Dead: {ex.Message}"))
                return false;
            }
        }
    }
}
