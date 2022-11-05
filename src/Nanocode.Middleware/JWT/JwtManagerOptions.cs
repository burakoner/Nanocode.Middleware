namespace Nanocode.Middleware.JWT
{
    public class JwtManagerOptions
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Subject { get; set; }
        public int AccessTokenValidity { get; set; }
        public int RefreshTokenValidity { get; set; }
    }
}
