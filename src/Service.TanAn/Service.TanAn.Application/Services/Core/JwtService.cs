// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;

namespace Service.TanAn.Application.Services.Core
{
    /// <summary>
    /// Phát hành và validate JWT token.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _opts;
        private readonly byte[] _keyBytes;

        public JwtService(IOptions<JwtOptions> options)
        {
            _opts = options?.Value ?? new JwtOptions();

            if (string.IsNullOrWhiteSpace(_opts.SecretKey))
            {
                _opts.SecretKey = "TanAnCommuneDigitalPlatformSecretKey2026!KeySuperSecretForGraduationProject";
            }

            _keyBytes = Encoding.UTF8.GetBytes(_opts.SecretKey);
        }

        public Task<string> GenerateToken(SsoUserInfo user)
        {
            var handler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim("preferred_username", user.UserName ?? string.Empty),
                new Claim("preferred_token", ""),
                new Claim("login_scheme", "local"),

                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? user.UserName ?? string.Empty),
                new Claim(ClaimTypes.SerialNumber, user.FullName ?? user.UserName ?? string.Empty),
            };

            var key = new SymmetricSecurityKey(_keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claims,
                notBefore: now,
                expires: now.AddMinutes(_opts.ExpirationMinutes > 0 ? _opts.ExpirationMinutes : 60),
                signingCredentials: creds
            );

            return Task.FromResult(handler.WriteToken(token));
        }
    }
}
