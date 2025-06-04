namespace Tracker.Models.Auth
{
    public class LoginResponseModel
    {
        public string userId { get; set; }
        public int memberId { get; set; }
        public string userName { get; set; }
        public string role { get; set; }
        public string token { get; set; }
        public DateTime expiration { get; set; }
        public string tokenType { get; set; }
    }
}
