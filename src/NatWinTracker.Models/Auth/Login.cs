namespace NatWinTracker.Models.Auth
{
    public class Login
    {
        public string UserId { get; set; }
        public int MemberId { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string TokenType { get; set; }
    }
}
