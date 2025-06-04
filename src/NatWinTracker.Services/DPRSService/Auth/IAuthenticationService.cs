using NatWinTracker.Models.Auth;

namespace NatWinTracker.Services.DPRSService.Auth
{
    /// <summary>
    /// Authentication service interface
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Log in async
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="password">password</param>
        /// <returns>Toekn response</returns>
        Task<(Login token, bool isSuccess)> LoginAsync(string username, string password);
    }
}
