using System;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WooCommerceLicenseManagerClient
{
    public class LicenseClient
    {
        private readonly string _baseUrl;
        private readonly string _authorizationHeader;
        private readonly string _cookie;

        public string MachineNode { get; private set; }

        public LicenseClient(string baseUrl, string authorizationHeader, string cookie, string machineNode)
        {
            _baseUrl = baseUrl;
            _authorizationHeader = authorizationHeader;
            _cookie = cookie;
            this.MachineNode = machineNode;
        }

        public async Task<LicenseResponse> ValidateLicenseAsync(string licenseKey)
        {
            return await ExecuteLicenseRequestAsync($"/wp-json/lmfwc/v2/licenses/validate/{licenseKey}", Method.GET);
        }
        public   LicenseResponse ValidateLicense(string licenseKey)
        {
            return ExecuteLicenseRequest($"/wp-json/lmfwc/v2/licenses/validate/{licenseKey}", Method.GET);
        }



        public async Task<LicenseResponse> DeactivateLicenseAsync(string licenseKey)
        {
            return await ExecuteLicenseRequestAsync($"/wp-json/lmfwc/v2/licenses/deactivate/{licenseKey}", Method.GET);
        }

        public LicenseResponse DeactivateLicense(string licenseKey)
        {
            return ExecuteLicenseRequest($"/wp-json/lmfwc/v2/licenses/deactivate/{licenseKey}", Method.GET);
        }

        public async Task<LicenseResponse> ActivateLicenseAsync(string licenseKey)
        {
            return await ExecuteLicenseRequestAsync($"/wp-json/lmfwc/v2/licenses/activate/{licenseKey}", Method.GET);
        }

        public   LicenseResponse ActivateLicense(string licenseKey)
        {
            return   ExecuteLicenseRequest($"/wp-json/lmfwc/v2/licenses/activate/{licenseKey}", Method.GET);
        }

        private async Task<LicenseResponse> ExecuteLicenseRequestAsync(string endpoint, Method method)
        {
            var client = new RestClient(_baseUrl);
            if (string.IsNullOrWhiteSpace(MachineNode) == false)
                client.UserAgent = MachineNode;
            var request = new RestRequest(endpoint, method);
            request.AddHeader("Authorization", _authorizationHeader);

            if (string.IsNullOrWhiteSpace(MachineNode) == false)
                request.AddHeader("User-Agent", MachineNode);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                // Clean the response content by removing non-JSON content (e.g., HTML warnings)
                var jsonResponse = ExtractJson(response.Content);
                var licenseResponse = default(LicenseResponse);
                try
                {
                    
                      licenseResponse = JsonConvert.DeserializeObject<LicenseResponse>(jsonResponse);
                }
                catch (Exception e)
                {

                }
              
                if (licenseResponse != null && licenseResponse.success)
                {
                    return licenseResponse;
                }
                else
                {
                    throw new Exception($"License request failed: {string.Join(", ", licenseResponse?.data?.errors?.LmfwcRestDataError)}");
                }
            }
            else
            {
                throw new Exception($"Failed to execute license request. Status Code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        public LicenseResponse ExecuteLicenseRequest(string endpoint, Method method)
        {
            var client = new RestClient(_baseUrl);
            if (string.IsNullOrWhiteSpace(MachineNode) == false)
                client.UserAgent = MachineNode;
            var request = new RestRequest(endpoint, method);
            request.AddHeader("Authorization", _authorizationHeader);

            if (string.IsNullOrWhiteSpace(MachineNode) == false)
                request.AddHeader("User-Agent", MachineNode);

            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                // Clean the response content by removing non-JSON content (e.g., HTML warnings)
                var jsonResponse = ExtractJson(response.Content);
                var licenseResponse = default(LicenseResponse);
                try
                {

                    licenseResponse = JsonConvert.DeserializeObject<LicenseResponse>(jsonResponse);
                }
                catch (Exception e)
                {

                }

                if (licenseResponse != null && licenseResponse.success)
                {
                    return licenseResponse;
                }
                else
                {
                    throw new Exception($"License request failed: {string.Join(", ", licenseResponse?.data?.errors?.LmfwcRestDataError)}");
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
        public bool success { get; set; }
        public LicenseData data { get; set; }
    }

    public class LicenseData
    {
        public int? Id { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? UserId { get; set; }
        public string LicenseKey { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? ValidFor { get; set; }
        public int? Source { get; set; }
        public int? Status { get; set; }
        public int? TimesActivated { get; set; }
        public int? TimesActivatedMax { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        [JsonConverter(typeof(SingleValueArrayConverter<ActivationData>))]
        public List<ActivationData> ActivationData { get; set; }

        public ErrorDetails errors { get; set; }
        
        public ErrorStatus errorData { get; set; }
    }


    public class SingleValueArrayConverter<T> : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            
             
            object retVal = new Object();

            if (reader.TokenType == JsonToken.Null)
            {
                // Handle null value
                return objectType == typeof(List<T>) ? new List<T>() : null;
            }
            if (reader.TokenType == JsonToken.StartObject)
            {
                T instance = (T)serializer.Deserialize(reader, typeof(T));
                retVal = new List<T>() { instance };
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                retVal = serializer.Deserialize(reader, objectType);
            }
            return retVal;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }
    }

    public class ActivationData
    {
       


        public int id { get; set; }
        public string token { get; set; }
        public int license_id { get; set; }
        public string label { get; set; }
        public int source { get; set; }
        public string ip_address { get; set; }
        public string user_agent { get; set; }
        public string meta_data { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deactivated_at { get; set; }
    }

    public class ErrorDetails
    {
        [JsonProperty("lmfwc_rest_data_error")]
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
