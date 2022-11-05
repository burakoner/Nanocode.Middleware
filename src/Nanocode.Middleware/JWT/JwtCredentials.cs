using System;
using System.Collections.Generic;

namespace Nanocode.Middleware.JWT
{
    public class JwtCredentials
    {
        public string Identifier { get; set; }
        public Dictionary<string, string> Claims { get; set; }
        public List<string> Roles { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessExpires { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshExpires { get; set; }
    }
}
