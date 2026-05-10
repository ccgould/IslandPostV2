using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IslandPostPOS.Services
{
    public class ReceiptService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiUrl = "https://api.craftmypdf.com/v1/create";
        private const string ApiKey = "4cb0MjgxODM6MjgzNDM6d3hpcDIyMHg3eVE2V2VvTA="; // replace with your key

        public class PdfResult
        {
            public string file { get; set; }
            public string transaction_ref { get; set; }
            public string status { get; set; }
            public int total_pages { get; set; }
            public int file_size { get; set; }
            public string template_id { get; set; }
        }

        public async Task<PdfResult> GenerateReceiptAsync(object receiptData)
        {
            var payload = new
            {
                template_id = "3ac77b23d2616f9a",
                data = receiptData
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("X-API-KEY", ApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();
            var pdfResult = JsonSerializer.Deserialize<PdfResult>(result);

            return pdfResult;
        }
    }
}