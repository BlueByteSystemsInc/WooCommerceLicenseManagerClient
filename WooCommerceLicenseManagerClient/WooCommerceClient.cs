using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerceLicenseManagerClient
{
    using System;
    using System.Threading.Tasks;
    using RestSharp;
    using Newtonsoft.Json;

    namespace WooCommerceClient
    {
        public class WooCommerceApiClient
        {
            private readonly string _baseUrl;
            private readonly string _authorizationHeader;

            public string UserAgent { get; set; }

            public WooCommerceApiClient(string baseUrl, string authorizationHeader)
            {
                _baseUrl = baseUrl;
                _authorizationHeader = authorizationHeader;
              
            }

            public async Task<string> GetCustomerNameFromOrderAsync(int orderId)
            {
                var order = await ExecuteApiRequestAsync<OrderResponse>($"/wp-json/wc/v3/orders/{orderId}", Method.GET);
                return order?.Billing?.Company ?? $"{order?.Billing?.FirstName} {order?.Billing?.LastName}";
            }

            public async Task<string> GetProductNameFromProductIdAsync(int productId)
            {
                var product = await ExecuteApiRequestAsync<ProductResponse>($"/wp-json/wc/v3/products/{productId}", Method.GET);
                return product?.Name;
            }

            private async Task<T> ExecuteApiRequestAsync<T>(string endpoint, Method method) where T : class
            {
                var client = new RestClient(_baseUrl);
                var request = new RestRequest(endpoint, method);
                
                request.AddHeader("Authorization", _authorizationHeader);
           

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    // Clean the response content by removing non-JSON content (e.g., HTML warnings)
                    var jsonResponse = ExtractJson(response.Content);
                    return JsonConvert.DeserializeObject<T>(jsonResponse);
                }
                else
                {
                    throw new Exception($"Failed to execute API request. Status Code: {response.StatusCode}, Response: {response.Content}");
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

        // Product response class
        public class ProductResponse
        {
            public int? Id { get; set; }
            public string Name { get; set; }
            // Add other relevant properties as needed
        }

        // Order response class
        public class OrderResponse
        {
            public int? Id { get; set; }
            public BillingInfo Billing { get; set; }
            // Add other relevant properties as needed
        }

        public class BillingInfo
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Company { get; set; }
        }

       
    }


   

}
