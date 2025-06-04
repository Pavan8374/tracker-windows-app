using Tracker.Models.Auth;
using Tracker.Services.DPRSService.Auth;

namespace Tracker.Services.DPRSService.TokenManagement
{
    /// <summary>
    /// Token manager interface
    /// </summary>
    public interface ITokenManager
    {
        /// <summary>
        /// Save token async
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        Task SaveTokenAsync(Login token);

        /// <summary>
        /// Get token async
        /// </summary>
        /// <returns>Token</returns>
        Task<string> GetTokenAsync();

        /// <summary>
        /// Get token details async.
        /// </summary>
        /// <returns>Token response</returns>
        Task<Login> GetTokenDetailsAsync();

        /// <summary>
        /// Is token valid async.
        /// </summary>
        /// <returns>True/Flse</returns>
        Task<bool> IsTokenValidAsync();

        /// <summary>
        /// Clear tokn async.
        /// </summary>
        /// <returns></returns>
        Task ClearTokenAsync();
    }
}
