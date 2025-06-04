using Newtonsoft.Json;

namespace Tracker.Models.Auth
{
    /// <summary>
    /// Auth token api response
    /// </summary>
    public class AuthTokenAPIResponse
    {
        /// <summary>
        /// Access token
        /// </summary>
        public string access_token { get; set; }

        /// <summary>
        /// Token type
        /// </summary>
        public string token_type { get; set; }

        /// <summary>
        /// Expires in
        /// </summary>
        public int expires_in { get; set; }

        /// <summary>
        /// Refresh token 
        /// </summary>
        public string refresh_token { get; set; }

        /// <summary>
        /// User name
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// issued
        /// </summary>
        [JsonProperty(".issued")]
        public DateTime issued { get; set; }

        /// <summary>
        /// Expires
        /// </summary>
        [JsonProperty(".expires")]
        public DateTime expires { get; set; }

        /// <summary>
        /// refresh
        /// </summary>
        [JsonProperty(".refresh")]
        public bool refresh { get; set; }
    }
}
