namespace NatWinTracker.Services.DPRSService.Auth
{
    /// <summary>
    /// Token response
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Access token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Token type
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// Expires in
        /// </summary>
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User identity.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Member Identity.
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Issued
        /// </summary>
        public DateTime Issued { get; set; }

        /// <summary>
        /// Expired
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// Refresh
        /// </summary>
        public bool Refresh { get; set; }
    }
}
