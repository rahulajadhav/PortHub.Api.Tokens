namespace PortHub.Api.Tokens.Data.Models.viewModels
{
    public class AuthVM
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Emailid { get; set; }
        public string RefreshToken { get;  set; }
    }
}
