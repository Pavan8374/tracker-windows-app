using Tracker.Services.DPRSService.TokenManagement;
using System.Net.Http.Headers;

/// <summary>
/// Authentication Delegation handler.
/// </summary>
public class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly ITokenManager _tokenManager;

    /// <summary>
    /// Creates Authenticatuion delegation handler.
    /// </summary>
    /// <param name="tokenManager">Token manager</param>
    public AuthenticationDelegatingHandler(ITokenManager tokenManager)
    {
        _tokenManager = tokenManager;
    }

    /// <summary>
    /// Send async
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenManager.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}