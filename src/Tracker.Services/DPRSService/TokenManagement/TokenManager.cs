using Microsoft.Win32;
using Tracker.Models.Auth;
using Tracker.Services.DPRSService.Auth;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tracker.Services.DPRSService.TokenManagement
{

    /// <summary>
    /// Token manager 
    /// </summary>
    public class TokenManager : ITokenManager
    {
        private const string RegistryPath = @"SOFTWARE\Tracker";
        private const string TokenKey = "AuthToken";
        //private static readonly byte[] Entropy = new byte[] { 12, 45, 67, 89, 32, 12, 45, 67 }; // Change this in production
        private static readonly byte[] Entropy = EntropyManager.GetProductionEntropy();

        /// <summary>
        /// Save token async
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        /// <exception cref="Exception">Eception</exception>
        public async Task SaveTokenAsync(Login token)
        {
            try
            {
                // Serialize the token object to JSON
                string jsonToken = JsonSerializer.Serialize(token);
                byte[] tokenBytes = Encoding.UTF8.GetBytes(jsonToken);

                // Encrypt the token data
                byte[] encryptedData = ProtectedData.Protect(
                    tokenBytes,
                    Entropy,
                    DataProtectionScope.CurrentUser);

                // Store in Registry
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key.SetValue(TokenKey, Convert.ToBase64String(encryptedData));
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving token: " + ex.Message);
            }
        }

        /// <summary>
        /// Get token async
        /// </summary>
        /// <returns>Token</returns>
        public async Task<string> GetTokenAsync()
        {
            try
            {
                var tokenDetails = await GetTokenDetailsAsync();
                return tokenDetails?.Token;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving access token: " + ex.Message);
            }
        }

        /// <summary>
        /// Get token details async.
        /// </summary>
        /// <returns>Token response</returns>
        public async Task<Login> GetTokenDetailsAsync()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key == null)
                        return null;

                    string encryptedToken = key.GetValue(TokenKey) as string;
                    if (string.IsNullOrEmpty(encryptedToken))
                        return null;

                    // Decrypt the token
                    byte[] encryptedData = Convert.FromBase64String(encryptedToken);
                    byte[] tokenBytes = ProtectedData.Unprotect(
                        encryptedData,
                        Entropy,
                        DataProtectionScope.CurrentUser);

                    string jsonToken = Encoding.UTF8.GetString(tokenBytes);
                    var logInResponse = JsonSerializer.Deserialize<Login>(jsonToken);

                    await Task.CompletedTask;
                    return logInResponse;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving token details: " + ex.Message);
            }
        }

        /// <summary>
        /// Is token valid async.
        /// </summary>
        /// <returns>True/Flse</returns>
        public async Task<bool> IsTokenValidAsync()
        {
            try
            {
                var tokenDetails = await GetTokenDetailsAsync();
                if (tokenDetails == null)
                    return false;

                // Check if token is expired
                if (DateTime.UtcNow >= tokenDetails.Expiration)
                {
                    // If refresh token is available and refresh is allowed
                    //if (!string.IsNullOrEmpty(tokenDetails.RefreshToken) && tokenDetails.Refresh)
                    //{
                    //    // Here you would typically implement refresh token logic
                    //    // For now, we'll just return false
                    //    return false;
                    //}
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Clear tokn async.
        /// </summary>
        /// <returns></returns>
        public async Task ClearTokenAsync()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(TokenKey, false);
                    }
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new Exception("Error clearing token: " + ex.Message);
            }
        }

        private bool IsTokenExpired(TokenResponse token)
        {
            if (token == null)
                return true;

            return DateTime.UtcNow >= token.Expires;
        }
    }
}