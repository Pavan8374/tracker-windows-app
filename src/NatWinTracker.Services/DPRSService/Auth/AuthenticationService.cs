using AutoMapper;
using NatWinTracker.Models.Auth;
using NatWinTracker.Modelsusers;
using NatWinTracker.Services.DPRSService.TokenManagement;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace NatWinTracker.Services.DPRSService.Auth
{
    /// <summary>
    /// Authentication service
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly ITokenManager _tokenManager;

        /// <summary>
        /// Creates Authenticaion sevice
        /// </summary>
        /// <param name="httpClientFactory">Http client factory.</param>
        /// <param name="mapper">Mapper.</param>
        /// <param name="tokenManager">Token manager.</param>
        public AuthenticationService(
            IHttpClientFactory httpClientFactory, 
            IMapper mapper, 
            ITokenManager tokenManager
            )
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Log in async
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="password">password</param>
        /// <returns>Token response and success status</returns>
        public async Task<(Login token, bool isSuccess)> LoginAsync(string username, string password)
        {
            var httpClient = _httpClientFactory.CreateClient("NatrixApiClient");

            var loginData = new Dictionary<string, string>
            {
                { "userName", username },
                { "password", password },
                //{ "grant_type", "password" }
            };

            var content = new FormUrlEncodedContent(loginData);
            var response = await httpClient.PostAsync(Endpoint.Login, content);

            if (!response.IsSuccessStatusCode)
            {
                return (null, false);
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<LoginResponseModel>(jsonString);
            var data = _mapper.Map<Login>(tokenResponse);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.Token);

            var getActiveUsersResponse = await httpClient.GetAsync(Endpoint.ActiveMembers);
            if (!getActiveUsersResponse.IsSuccessStatusCode)
            {
                return (null, false);
            }

            var activeUsersJsonString = await getActiveUsersResponse.Content.ReadAsStringAsync();
            var activeUsersJson = JsonConvert.DeserializeObject<List<UserResponse>>(activeUsersJsonString);

            if (activeUsersJson != null && activeUsersJson.Any())
            {
                var user = activeUsersJson.FirstOrDefault(x => x.userName == data.UserName);
                if (user != null)
                {
                    data.MemberId = user.id;
                    data.UserName = user.memberName;
                    data.UserId = user.userID;
                }
            }

            await _tokenManager.SaveTokenAsync(data);
            return (data, true);
        }


    }

}
