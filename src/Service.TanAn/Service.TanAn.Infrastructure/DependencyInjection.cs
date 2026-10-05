// "Một sản phẩm của HieuDV"

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Interfaces.Elasticsearch;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Interfaces;
using Service.TanAn.Domain.Interfaces.Elastic;
using Service.TanAn.Infrastructure.Persistence;
using Service.TanAn.Infrastructure.Repositories.Bases;
using Service.TanAn.Infrastructure.Repositories.Elastic;
using Service.TanAn.Infrastructure.Repositories.Elasticsearch;
using Service.TanAn.Infrastructure.Services;

namespace Service.TanAn.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<DataActor>();
            if (configuration.GetValue<bool>("Redis:Enabled"))
            {
                services.AddSingleton<RedisCacheService>();
                services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<RedisCacheService>());
                services.AddSingleton<ILoginSessionStore>(sp => sp.GetRequiredService<RedisCacheService>());
            }
            services.AddScoped<ITanAnDbContext>(provider => provider.GetRequiredService<TanAnDbContext>());
            services.AddScoped<IRequestContext, RequestContext>();

            services.AddScoped(typeof(IElasticsearchRepository<>), typeof(ElasticsearchRepository<>));
            services.AddScoped<ILogDangNhapIndexRepository, LogDangNhapIndexRepository>();
            services.AddScoped<ILogHeThongIndexRepository, LogHeThongIndexRepository>();
            services.AddScoped<IDuLieuTinKiemIndexRepository, DuLieuTinKiemIndexRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUnitOfWorkQuanTriHeThong>(provider => (UnitOfWork)provider.GetRequiredService<IUnitOfWork>());
            services.AddScoped<IUnitOfWorkDanCu>(provider => (UnitOfWork)provider.GetRequiredService<IUnitOfWork>());
            services.AddScoped<IUnitOfWorkTTHC>(provider => (UnitOfWork)provider.GetRequiredService<IUnitOfWork>());
            services.AddScoped<IUnitOfWorkAnSinh>(provider => (UnitOfWork)provider.GetRequiredService<IUnitOfWork>());
            services.AddScoped<IUnitOfWorkChatBot>(provider => (UnitOfWork)provider.GetRequiredService<IUnitOfWork>());

            return services;
        }
    }
}
