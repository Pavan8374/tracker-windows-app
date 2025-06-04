using AutoMapper;
using Tracker.Models.DPRS;
using Tracker.Models.Projects;
using Tracker.Models.Users;
using Tracker.Services.DPRSService.TokenManagement;
using Newtonsoft.Json;
using System.Globalization;
using System.Net.Http.Json;
using Tracker.Services;
using System.Text;

namespace Tracker.Services.DPRSService.Users
{
    /// <summary>
    /// User service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly ITokenManager _tokenManager;

        /// <summary>
        /// Creates User service
        /// </summary>
        /// <param name="httpClientFactory">Http client factory</param>
        /// <param name="mapper">Mapper</param>
        /// <param name="tokenManager">Token amanger</param>
        public UserService(
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
        /// Get user projects.
        /// </summary>
        /// <returns>User project list.</returns>
        public async Task<List<UserProject>> GetUserProjects()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("NatrixApiClient");

                var tokenDetails = await _tokenManager.GetTokenDetailsAsync();
                if (tokenDetails == null)
                {
                    throw new InvalidOperationException("Token details are not available.");
                }

                var endpoint = Endpoint.UserProjects + $"MemberId={tokenDetails.MemberId}&&Status=Active,Unapproved";

                //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDetails.AccessToken);

                var response = await httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var userResponse = JsonConvert.DeserializeObject<List<UserProjectAPIResponse>>(jsonString);

                if (userResponse == null)
                {
                    return new List<UserProject>();
                }

                return _mapper.Map<List<UserProject>>(userResponse);
            }
            catch (HttpRequestException ex)
            {
                // Log the exception or handle it as appropriate for your application
                //Console.WriteLine($"HTTP request failed: {ex.Message}");
                return new List<UserProject>();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as appropriate for your application
                //Console.WriteLine($"An error occurred while fetching user projects: {ex.Message}");
                return new List<UserProject>();
            }
        }

        /// <summary>
        /// Manage Dprs
        /// </summary>
        /// <param name="model">model</param>
        /// <returns>Tre/false</returns>
        public async Task<bool> ManageDPRS(AddDPRSRequestModel model)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("NatrixApiClient");

                // Convert the model to JSON and create StringContent
                var jsonContent = JsonConvert.SerializeObject(model);
                var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(Endpoint.ManageDPRS, stringContent);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                //var jsonResponse = JsonConvert.DeserializeObject<List<UserProjectAPIResponse>>(jsonString);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Get previous Worked project entry.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<List<ProjectDTO>> GetDPRSEntry()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("NatrixApiClient");

                var tokenDetails = await _tokenManager.GetTokenDetailsAsync();
                if (tokenDetails == null)
                {
                    throw new InvalidOperationException("Token details are not available.");
                }

                //DateTime previousWorkDate = GetValidYesterday(DateTime.Today.Date);
                //string formattedDate = previousWorkDate.ToString("yyyy-MM-dd");

                //// Or using string format
                ////string formattedDate = string.Format("{0:yyyy-MM-dd}", previousWorkDate);
                //var endpoint = Endpoint.GetDPRSByQuery + $"FromDate={formattedDate}&UserID={tokenDetails.UserId}";

                ////httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenDetails.AccessToken);

                DateTime previousWorkDate = GetValidYesterday(DateTime.Today.Date);
                string formattedDate = previousWorkDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                // Construct the URL with proper query parameters
                var baseUrl = Tracker.Services.Endpoint.GetDPRSByQuery;
                var queryString = $"FromDate={Uri.EscapeDataString(formattedDate)}&UserID={Uri.EscapeDataString(tokenDetails.UserId)}";
                var endpoint = baseUrl + queryString;

                // For debugging - log or check the final URL
                var response = await httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var userResponse = JsonConvert.DeserializeObject<List<WorkEntry>>(jsonString);

                if (userResponse == null)
                {
                    return new List<ProjectDTO>();
                }

                var recentProjects = _mapper.Map<List<ProjectDTO>>(userResponse);
                return recentProjects.Where(x => x.MemberId == tokenDetails.MemberId).ToList();
            }
            catch (HttpRequestException ex)
            {
                return new List<ProjectDTO>();
            }
            catch (Exception ex)
            {
                return new List<ProjectDTO>();
            }
        }

        public DateTime GetValidYesterday(DateTime referenceDate)
        {
            DateTime yesterday = referenceDate.AddDays(-1);

            // If yesterday is Sunday or any Saturday, return Friday
            if (yesterday.DayOfWeek == DayOfWeek.Sunday)
            {
                return yesterday.AddDays(-2); // Return Friday
            }
            if(yesterday.DayOfWeek == DayOfWeek.Saturday)
            {
                return yesterday.AddDays(-1); // Return Friday
            }

            // Otherwise, return yesterday
            return yesterday;
        }
    }
}