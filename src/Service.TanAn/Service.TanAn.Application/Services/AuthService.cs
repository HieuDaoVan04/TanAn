// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Service.Shared.Commons.Helpers;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using PermissionDto = Service.Shared.Commons.Models.PermissionDto;
using Service.TanAn.Application.Interfaces;
using IRoleService = Service.TanAn.Application.Interfaces.Core.IRoleService;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Domain.Interfaces;
using Service.TanAn.Domain.Interfaces.Elastic;

namespace Service.TanAn.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWorkQuanTriHeThong _UnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly IRoleService _roleService;
        private readonly ILogDangNhapIndexRepository _logDangNhapIndexRepository;
        private readonly int MinuteExpireToken = 60;

        public AuthService(
            ILogDangNhapIndexRepository LogDangNhapIndexRepository,
            IUnitOfWorkQuanTriHeThong UnitOfWork,
            ICacheService CacheService,
            IConfiguration Configuration,
            IRoleService roleService)
        {
            _UnitOfWork = UnitOfWork;
            _configuration = Configuration;
            _cacheService = CacheService;
            _roleService = roleService;
            _logDangNhapIndexRepository = LogDangNhapIndexRepository;
        }

        public Task<bool> Check2FAUser(string UserName, string Password)
        {
            return Task.FromResult(true);
        }

        // Endpoint tương thích cũ cũng phải xác thực mật khẩu thật.
        public Task<LoginDto?> DangNhapDev(string username, string password, string? OTP)
            => DangNhap(username, password, OTP);

        public async Task<LoginDto?> DangNhap(string username, string password, string? OTP)
        {
            var nguoiDung = await _UnitOfWork.UserRepository.FindAsync(x => x.UserName == username);
            if (nguoiDung == null || nguoiDung.ModerationStatus != Service.Shared.Commons.Model.SQL.ModerationStatus.Approved) return null;
            var valid = nguoiDung.PasswordHash.StartsWith("pbkdf2$")
                ? PasswordHashing.Verify(password, nguoiDung.PasswordHash)
                : nguoiDung.PasswordHash == password || nguoiDung.PasswordHash == AESCrypto.EncryptNoAutoGen(password);
            if (!valid) return null;

            string keyCache = $"AuthorizationUser:{nguoiDung.Id}_{nguoiDung.Username}";
            string uidSession = $"{nguoiDung.Username}:{Guid.NewGuid()}";
            CurrentUserDto currentUserDto;

            if (await _cacheService.KeyExistsAsync(RedisTypeKey.Session, keyCache))
            {
                currentUserDto = await _cacheService.GetAsync<CurrentUserDto>(RedisTypeKey.Session, keyCache)
                                 ?? new CurrentUserDto();
            }
            else
            {
                currentUserDto = new CurrentUserDto
                {
                    UserName = nguoiDung.Username,
                    FullName = nguoiDung.FullName,
                    Email = nguoiDung.Email ?? string.Empty,
                    UserId = nguoiDung.Id,
                    Role = nguoiDung.Role.ToString(),
                    ApThon = nguoiDung.ApThon ?? string.Empty,
                    IsAuthenticated = true,
                    PhanQuyen = await LayDanhSachVaiTroDuocPhan(nguoiDung.Id)
                };

                List<MenuItemDto> menuItems = new List<MenuItemDto>();

                foreach (var chuyenTrang in currentUserDto.PhanQuyen)
                {
                    if (chuyenTrang.Roles != null && chuyenTrang.Roles.Any())
                    {
                        var listIdRole = chuyenTrang.Roles.Select(x => x.RoleId).ToList();
                        chuyenTrang.Permissions = await _roleService.GetPermissionFromLstRoleId(listIdRole);
                        menuItems.AddRange(await _roleService.GetModuleFromLstRoleId(listIdRole));
                    }
                    else
                    {
                        chuyenTrang.Permissions = new List<PermissionDto>();
                    }
                }

                if (!menuItems.Any())
                {
                    menuItems = await _roleService.GetModuleFromLstRoleId(new List<Guid> { Guid.NewGuid() });
                }

                menuItems = menuItems
                    .GroupBy(m => m.Id)
                    .Select(g => g.First())
                    .ToList();

                currentUserDto.Menus = menuItems;
                currentUserDto.Token = GenerateJwtToken(uidSession, currentUserDto);

                await _cacheService.SetAsync(RedisTypeKey.Session, keyCache, currentUserDto, TimeSpan.FromMinutes(MinuteExpireToken));
            }

            return new LoginDto
            {
                access_token = currentUserDto.Token,
                expires_in = MinuteExpireToken * 60,
                refresh_token = GenerateRefreshToken(uidSession, currentUserDto, currentUserDto.Token)
            };
        }

        public async Task<List<LuotTruyCap>> GetThongTinBLV()
        {
            string cacheKey = "ThongTinBLV";
            List<LuotTruyCap>? data = await _cacheService.GetAsync<List<LuotTruyCap>>(RedisTypeKey.Session, cacheKey);
            if (data != null && data.Count > 0)
            {
                return data;
            }

            data = new List<LuotTruyCap>();

            for (int i = 6; i >= 0; i--)
            {
                DateTime ngay = DateTime.Now.Date.AddDays(-i);
                DateTime tuNgay = ngay;
                DateTime denNgay = ngay.AddDays(1).AddTicks(-1);

                var query = new LogDangNhapIndexQuery
                {
                    SearchTuNgay = tuNgay,
                    SearchDenNgay = denNgay
                };

                int soLuong = await _logDangNhapIndexRepository.GetCountData(query);

                data.Add(new LuotTruyCap
                {
                    Ngay = ngay,
                    SoLuong = soLuong
                });
            }

            await _cacheService.SetAsync(RedisTypeKey.Session, cacheKey, data, TimeSpan.FromHours(2));
            return data;
        }

        public async Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var loginDto = await DangNhap(request.Username, request.Password, request.OTP);
            if (loginDto == null)
            {
                return ApiResult<LoginResponse>.Fail("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            var user = await _UnitOfWork.UserRepository.FindAsync(u => u.UserName == request.Username);
            return ApiResult<LoginResponse>.Ok(new LoginResponse
            {
                Token = loginDto.access_token,
                Username = user?.Username ?? request.Username,
                FullName = user?.FullName ?? request.Username,
                Role = user?.Role ?? RoleEnum.CanBoXa,
                ApThon = user?.ApThon
            });
        }

        public async Task<ApiResult<bool>> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _UnitOfWork.UserRepository.FindAsync(u => u.UserName == request.Username);
            if (existingUser != null)
            {
                return ApiResult<bool>.Fail("Tên tài khoản đã tồn tại.");
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = AESCrypto.EncryptNoAutoGen(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Role = request.Role,
                ApThon = request.ApThon,
                CreatedAt = DateTime.Now
            };

            await _UnitOfWork.UserRepository.AddAsync(user);
            await _UnitOfWork.SaveChangesAsync();

            return ApiResult<bool>.Ok(true);
        }

        private async Task<List<SitePermissionDto>> LayDanhSachVaiTroDuocPhan(Guid userId)
        {
            var siteId = Guid.NewGuid();
            var roleId = Guid.NewGuid();

            var dtos = new List<SitePermissionDto>
            {
                new SitePermissionDto
                {
                    SiteId = siteId,
                    SiteName = "Portal Cổng Thông Tin & Quản Lý Xã Tân An",
                    Categories = new List<CategoryPermissionDto>
                    {
                        new CategoryPermissionDto { CategoryId = Guid.NewGuid(), CategoryName = "Quản lý Hộ khẩu" },
                        new CategoryPermissionDto { CategoryId = Guid.NewGuid(), CategoryName = "Dịch vụ công" }
                    },
                    Roles = new List<RolePermissionDto>
                    {
                        new RolePermissionDto { RoleId = roleId, RoldeCode = "ADMIN_XA", RoleName = "Quản trị viên Cấp Xã" }
                    }
                }
            };

            return await Task.FromResult(dtos);
        }

        private string GenerateJwtToken(string uidSession, CurrentUserDto currentUser)
        {
            if (string.IsNullOrEmpty(currentUser.UserName))
                return string.Empty;

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? _configuration["Jwt:SecretKey"] ?? "TanAnEnterpriseSecretKeyForJWTAuthentication2026SuperSecureString!!";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey.PadRight(32)));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, currentUser.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, currentUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, uidSession),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "TanAnApi",
                audience: jwtSettings["Audience"] ?? "TanAnClients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            var activeTokenKey = $"{uidSession}-token-active";
            _cacheService.SetAsync(RedisTypeKey.Session, activeTokenKey, tokenString, TimeSpan.FromMinutes(MinuteExpireToken));

            var activeSessionKey = $"{uidSession}-session-active";
            currentUser.Token = tokenString;
            _cacheService.SetAsync(RedisTypeKey.Session, activeSessionKey, currentUser, TimeSpan.FromMinutes(MinuteExpireToken));

            return tokenString;
        }

        private string GenerateRefreshToken(string uidSession, CurrentUserDto oCurrentUser, string activeToken)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKeyRefresh = jwtSettings["SecretKeyRefresh"] ?? _configuration["Jwt:SecretKeyRefresh"] ?? "TanAnEnterpriseSecretKeyRefresh2026SuperSecureString!!";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyRefresh.PadRight(32)));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, oCurrentUser.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, string.IsNullOrEmpty(oCurrentUser.UserName) ? string.Empty : oCurrentUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, uidSession),
                new Claim(JwtRegisteredClaimNames.UniqueName, activeToken),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "TanAnApi",
                audience: jwtSettings["Audience"] ?? "TanAnClients",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(MinuteExpireToken * 50),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}
