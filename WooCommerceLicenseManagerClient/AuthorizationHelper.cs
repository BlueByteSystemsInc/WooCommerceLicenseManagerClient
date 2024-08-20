using System;
using System.Text;

namespace WooCommerceLicenseManagerClient
{
    public class AuthorizationHelper
    {
        public static string GetBasicAuthorizationToken(string username, string password)
        {
            string credentials = $"{username}:{password}";
            byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
            string base64Credentials = Convert.ToBase64String(credentialsBytes);
            return $"Basic {base64Credentials}";
        }
    }



    
}
