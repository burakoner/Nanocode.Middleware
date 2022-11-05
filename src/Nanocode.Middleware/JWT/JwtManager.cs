using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Nanocode.Middleware.JWT
{
    /// <summary>
    /// USAGE:
    /// 
    /// Program.cs
    /// ====================================================================================================
    /// Aşağıdakilerden birisi ile JwtAuthentication EKLE
    /// - builder.Services.AddJwtAuthentication(builder.Configuration);
    /// - builder.Services.AddJwtAuthentication(builder.Configuration["Jwt:Secret"], builder.Configuration["Jwt:Issuer"], builder.Configuration["Jwt:Audience"]);
    /// 
    /// Devamında aşağıdaki satır ile JwtAuthentication KULLAN
    /// - app.UseJwtAuthentication();
    /// ----------------------------------------------------------------------------------------------------
    /// 
    /// appsettings.json
    /// ====================================================================================================
    /// Eğer configuration dosyası kullanılacaksa aşağıdakine benzer bir yapıda olmalı.
    ///   "JWT": {
    ///     "Secret": "D3p6PaerfJPKbe4t76dQwmZEHWmKMh63xTc8kMGf2j6Ym6hzyrtEzuLrSLNJbtiHZTjNTU2gwf4vUKbncHm6W5vJx7DDJvig6d4vkTAGNbNuB9CcfNVuqYZSdXdZRHLB",
    ///     "Issuer": "JWTAuthenticationServer",          -> sso.domain.com gibi bir şey de olabilir.
    ///     "Audience": "JWTServicePostmanClient",        -> www.domain.com gibi bir şey de olabilir.
    ///     "Subject": "JWTServiceAccessToken",           -> JWT Konusu. Boş olabilir. https://www.rfc-editor.org/rfc/rfc7519#page-9
    ///     "AccessTokenValidity": "600",
    ///     "RefreshTokenValidity": "86400"
    ///   },
    /// ----------------------------------------------------------------------------------------------------
    /// 
    /// [Controller].cs
    /// ====================================================================================================
    /// JWT Token ile yetki kontrolü yapılmak istenen Controller CLASS veya METHOD için aşağıdaki attribute ekle
    ///  => Rol tanımı olacaksa bunlardan birisini ya da custom role adı ile kullan
    ///  -> [Authorize(Roles = "User")]
    ///  -> [Authorize(Roles = "Moderator")]
    ///  -> [Authorize(Roles = "SuperAdmin")]
    ///     veya
    ///  => Rol tanımına gerek yoksa bu yeterli
    ///  -> [Authorize]
    ///     [Route("api/employee")]
    ///     [ApiController]
    ///     public class EmployeeController : ControllerBase
    ///     {
    ///     ...
    /// ----------------------------------------------------------------------------------------------------
    /// 
    /// [Controller].cs
    /// ====================================================================================================
    /// JWT Token üretimi aşağıdaki şekilde yapılabilir.
    /// Sadece tercih edilen Claim'ler gönderilebilir. Aşağıda yer almayan ama JWT içine eklenmek istenen tüm bilgiler eklenebilir.
    /// Roller için yalnızca kullanıcıya ait roller eklenmeli. Rol yoksa boş veya null gönderilebilir.
    /// 
    /// var jwt = new JwtManager(_configuration);
    /// var token = jwt.CreateToken(new Dictionary<string, string>
    /// {
    ///     { "UserId", user.UserId.ToString() },
    ///     { "DisplayName", user.DisplayName },
    ///     { "UserName", user.UserName },
    ///     { "Email", user.Email }
    /// }, new List<string> { "User", "Moderator", "SuperAdmin" });
    /// 
    /// ...
    /// 
    /// Ayrıca JwtManager ile geçerli olup olmadığına bakılmaksızın 
    /// (Issuer, Audience, Lifetime validasyonları devre dışı bırakılarak sadece IssuerSigningKey validasyonu yapılarak)
    /// GetPrincipals methodu ile principals verileri elde edilebilir.
    /// 
    /// var jwt = new JwtManager(_configuration);
    /// var principals = jwt.GetPrincipals("ACCESS-TOKEN")
    /// ----------------------------------------------------------------------------------------------------
    /// 
    /// [Controller].cs
    /// ====================================================================================================
    /// Authorize edilmiş method içinde kullanıcı bilgilerine aşağıdaki şekilde ulaşılabilir.
    /// ...
    /// var user = this.User;
    /// var claims = this.User.Claims.ToList();
    /// ...
    /// ----------------------------------------------------------------------------------------------------
    /// 
    /// [Controller].cs
    /// ====================================================================================================
    /// Authorize edilmiş method içinde kullanıcı bilgilerine aşağıdaki şekilde ulaşılabilir.
    /// ...
    /// var user = this.User;
    /// var claims = this.User.Claims.ToList();
    /// ...
    /// ----------------------------------------------------------------------------------------------------
    /// </summary>
    public class JwtManager
    {
        protected string Secret { get; set; } // Minimum 32 karakter
        protected string Issuer { get; set; }
        protected string Audience { get; set; }
        protected string Subject { get; set; }
        protected int AccessTokenValidity { get; set; }
        protected int RefreshTokenValidity { get; set; }

        public JwtManager(IConfiguration configuration)
        {
            this.Secret = configuration["JWT:Secret"];
            this.Issuer = configuration["JWT:Issuer"];
            this.Audience = configuration["JWT:Audience"];
            this.Subject = configuration["JWT:Subject"];
            this.AccessTokenValidity = Convert.ToInt32(configuration["JWT:AccessTokenValidity"]);
            this.RefreshTokenValidity = Convert.ToInt32(configuration["JWT:RefreshTokenValidity"]);
        }

        public JwtManager(JwtManagerOptions options)
        {
            this.Secret = options.Key;
            this.Issuer = options.Issuer;
            this.Audience = options.Audience;
            this.Subject = options.Subject;
            this.AccessTokenValidity = options.AccessTokenValidity;
            this.RefreshTokenValidity = options.RefreshTokenValidity;
        }

        public JwtManager(string key, string issuer, string audience, string subject, int accessTokenValidity, int refreshTokenValidity)
        {
            this.Secret = key;
            this.Issuer = issuer;
            this.Audience = audience;
            this.Subject = subject;
            this.AccessTokenValidity = accessTokenValidity;
            this.RefreshTokenValidity = refreshTokenValidity;
        }

        public JwtCredentials CreateToken(Dictionary<string, string> claims, List<string> roles)
        {
            var time = DateTime.UtcNow;
            var epochTime = Convert.ToInt32((time - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

            // Create Common Claims
            var guid = Guid.NewGuid().ToString();
            var claimList = new List<Claim> {
                // Subject
                new Claim(JwtRegisteredClaimNames.Sub, this.Subject),

                // JWT ID
                new Claim(JwtRegisteredClaimNames.Jti, guid),

                // Issued At
                new Claim(JwtRegisteredClaimNames.Iat, epochTime.ToString(), ClaimValueTypes.Integer),
            };

            // Add Passed Claims
            if (claims != null)
                foreach (var claim in claims)
                    claimList.Add(new Claim(claim.Key, claim.Value));

            // Add Passed Roles
            if (roles != null)
                foreach (var role in roles)
                    claimList.Add(new Claim(ClaimTypes.Role, role));

            // Generate JWT
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Secret));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessExpires = DateTime.UtcNow.AddSeconds(this.AccessTokenValidity);
            var securityToken = new JwtSecurityToken(this.Issuer, this.Audience, claimList, expires: accessExpires, signingCredentials: signIn);
            var accessToken = new JwtSecurityTokenHandler().WriteToken(securityToken);

            var refreshToken = GenerateRefreshToken();
            var refreshExpires = accessExpires.AddSeconds(this.RefreshTokenValidity);

            // Return
            return new JwtCredentials
            {
                Identifier = guid,
                Claims = claims,
                Roles = roles,
                AccessToken = accessToken,
                AccessExpires = accessExpires,
                RefreshToken = refreshToken,
                RefreshExpires = refreshExpires,
            };
        }

        public ClaimsPrincipal GetPrincipals(string accessToken, bool validateToken = false)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = validateToken,
                ValidateAudience = validateToken,
                ValidateLifetime = validateToken,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Secret)),
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public void Extend()
        {
            // TODO
        }

        public void Revoke()
        {
            // TODO
        }
    }
}
