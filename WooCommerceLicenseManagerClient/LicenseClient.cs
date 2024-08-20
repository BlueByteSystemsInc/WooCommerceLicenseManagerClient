using System;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;

namespace WooCommerceLicenseManagerClient
{
    public class LicenseClient
    {
        private readonly string _baseUrl;
        private readonly string _authorizationHeader;
        private readonly string _cookie;

        public LicenseClient(string baseUrl, string authorizationHeader, string cookie)
        {
            _baseUrl = baseUrl;
            _authorizationHeader = authorizationHeader;
            _cookie = cookie;
        }

        public async Task<LicenseResponse> ValidateLicenseAsync(string licenseKey)
        {
            return await ExecuteLicenseRequestAsync($"/wp-json/lmfwc/v2/licenses/validate/{licenseKey}", Method.GET);
        }

        public async Task<LicenseResponse> DeactivateLicenseAsync(string licenseKey)
        {
            return await ExecuteLicenseRequestAsync($"/wp-json/lmfwc/v2/licenses/deactivate/{licenseKey}", Method.POST);
        }

        private async Task<LicenseResponse> ExecuteLicenseRequestAsync(string endpoint, Method method)
        {
            var client = new RestClient(_baseUrl);
            var request = new RestRequest(endpoint, method);
            request.AddHeader("Authorization", _authorizationHeader);
            request.AddHeader("Cookie", _cookie);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                // Clean the response content by removing non-JSON content (e.g., HTML warnings)
                var jsonResponse = ExtractJson(response.Content);

                var licenseResponse = JsonConvert.DeserializeObject<LicenseResponse>(jsonResponse);
                if (licenseResponse != null && licenseResponse.Success)
                {
                    return licenseResponse;
                }
                else
                {
                    throw new Exception($"License request failed: {string.Join(", ", licenseResponse?.Data?.Errors?.LmfwcRestDataError)}");
                }
            }
            else
            {
                throw new Exception($"Failed to execute license request. Status Code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        // Method to extract the JSON portion from the mixed content response
        private string ExtractJson(string content)
        {
            // Assuming JSON content starts with '{' and ends with '}'
            int jsonStartIndex = content.IndexOf('{');
            int jsonEndIndex = content.LastIndexOf('}');

            if (jsonStartIndex != -1 && jsonEndIndex != -1)
            {
                return content.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
            }

            throw new Exception("Invalid response format: JSON not found in the response content.");
        }
    }

    public class LicenseResponse
    {
        public bool Success { get; set; }
        public LicenseData Data { get; set; }
    }

    public class LicenseData
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string LicenseKey { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int ValidFor { get; set; }
        public int Source { get; set; }
        public int Status { get; set; }
        public int? TimesActivated { get; set; }
        public int TimesActivatedMax { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public ActivationData[] ActivationData { get; set; }
        public ErrorDetails Errors { get; set; }
        public ErrorStatus ErrorData { get; set; }
    }

    public class ActivationData
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int LicenseId { get; set; }
        public string Label { get; set; }
        public int Source { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string MetaData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }

    public class ErrorDetails
    {
        public string[] LmfwcRestDataError { get; set; }
    }

    public class ErrorStatus
    {
        public ErrorCode LmfwcRestDataError { get; set; }
    }

    public class ErrorCode
    {
        public int Status { get; set; }
    }



    
}
