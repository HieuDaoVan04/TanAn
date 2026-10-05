// "Một sản phẩm của HieuDV"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Services;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Application.Services;

namespace Service.TanAn.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<Service.TanAn.Application.Services.Core.AdministrationService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.SystemConfigurationService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.AccountPasswordService>();
            services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
            services.AddSingleton<ICacheService, MemoryCacheService>();
            services.AddHttpClient<IAIService, AIService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<IAuthService, Service.TanAn.Application.Services.Core.AuthService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.IAuthService, Service.TanAn.Application.Services.Core.AuthService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.IGroupsService, Service.TanAn.Application.Services.Core.GroupsService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.GroupsService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.IJwtService, Service.TanAn.Application.Services.Core.JwtService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.JwtService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.ILogHeThongService, Service.TanAn.Application.Services.Core.LogHeThongService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.LogHeThongService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.ILogThaoTacNguoiDungService, Service.TanAn.Application.Services.Core.LogThaoTacNguoiDungService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.LogThaoTacNguoiDungService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.IModuleService, Service.TanAn.Application.Services.Core.ModuleService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.ModuleService>();
            services.AddSingleton<LoggingThaoTacQueue>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.IPermissionService, Service.TanAn.Application.Services.Core.PermissionService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.PermissionService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.IRoleService, Service.TanAn.Application.Services.Core.RoleService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.RoleService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.ISystemParameterService, Service.TanAn.Application.Services.Core.SystemParameterService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.SystemParameterService>();
            services.AddScoped<Service.TanAn.Application.Interfaces.Core.IUsersService, Service.TanAn.Application.Services.Core.UsersService>();
            services.AddScoped<Service.TanAn.Application.Services.Core.UsersService>();
            services.AddScoped<ICitizenRequestService, CitizenRequestService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IPopulationService, PopulationService>();
            services.AddScoped<IWelfareService, WelfareService>();

            return services;
        }
    }
}
