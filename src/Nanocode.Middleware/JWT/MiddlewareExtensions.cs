using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics;
using System.Text;

namespace Nanocode.Middleware.JWT
{
    public static class MiddlewareExtensions
    {
        public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection @this, IConfiguration configuration)
            => @this.AddJwtAuthentication(configuration["JWT:Secret"], configuration["JWT:Issuer"], configuration["JWT:Audience"]);
        public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection @this, string secret, string issuer, string audience)
        {
            var dev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
            var debug = Debugger.IsAttached;

            return @this
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = !dev;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),

                        // ClockSkew = Debugger.IsAttached ? TimeSpan.Zero : TimeSpan.FromMinutes(10)
                        ClockSkew = TimeSpan.Zero,
                    };
                });
        }

        public static IApplicationBuilder UseJwtAuthentication(this WebApplication @this)
        {
            var dev = @this.Environment.IsDevelopment();
            var debug = Debugger.IsAttached;

            return @this
                .UseAuthentication()
                .UseAuthorization();
        }
    }
}
