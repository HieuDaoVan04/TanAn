// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Service.TanAn.Application;
using Service.TanAn.Infrastructure;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.TanAn.API
{
    /// <summary>
    /// Configuration Startup Class cho Service.TanAn.API
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Khởi tạo Startup
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Cấu hình các Services
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ConfigureServices(IServiceCollection services)
        {
            #region Cấu hình xác thực
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? _configuration["Jwt:SecretKey"] ?? "TanAnCommuneDigitalPlatformSecretKey2026!KeySuperSecretForGraduationProject";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"] ?? "TanAnIssuer",
                    ValidAudience = jwtSettings["Audience"] ?? "TanAnAudience",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });

            // Add Authorization Policy: cho phép JWT hoặc ClientID
            services.AddAuthorization(options =>
            {
                options.AddPolicy("UseClientId", policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                });
            });
            #endregion

            services.AddAuthentication().AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions,
                Service.TanAn.API.Authentication.LoginSessionAuthenticationHandler>(
                Service.TanAn.API.Authentication.LoginSessionAuthenticationHandler.SchemeName, _ => { });
            services.AddControllers();

            // Thêm API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(2, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new HeaderApiVersionReader("X-Api-Version")
                );
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Thêm Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Service TanAn API",
                    Version = "v1",
                    Description = "Danh sách API của dịch vụ TanAn - Nền tảng Quản lý Dân cư & An sinh Xã Tân An.",
                    Contact = new OpenApiContact
                    {
                        Name = "Tân An Team",
                        Email = "hieudv@tanan.gov.vn",
                        Url = new Uri("https://tanan.gov.vn")
                    }
                });

                // Cấu hình Bearer Token cho Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Nhập JWT token vào ô bên dưới. Ví dụ: Bearer <your_token>",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                // Đọc file XML nếu có
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            #region Cấu hình ForwardedHeaders để xử lý proxy
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();

                var proxySettings = _configuration.GetSection("ProxySettings");

                var knownProxies = proxySettings.GetSection("KnownProxies").Get<string[]>();
                if (knownProxies != null)
                {
                    foreach (var proxy in knownProxies)
                    {
                        if (IPAddress.TryParse(proxy, out var ipAddress))
                        {
                            options.KnownProxies.Add(ipAddress);
                        }
                    }
                }

                var networks = proxySettings.GetSection("KnownNetworks").Get<List<NetworkConfig>>();
                if (networks != null)
                {
                    foreach (var network in networks)
                    {
                        if (IPAddress.TryParse(network.Prefix, out var prefix))
                        {
                            options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(prefix, network.PrefixLength));
                        }
                    }
                }
                options.ForwardLimit = 5;
                options.RequireHeaderSymmetry = true;
            });
            #endregion

            // Enable Legacy Timestamp behavior for Npgsql PostgreSQL
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            // Add DbContext (Supports both PostgreSQL and SQLite)
            var connString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=tan_an_db.sqlite";
            services.AddDbContext<TanAnDbContext>(options =>
            {
                if (connString.Contains("Host=") || connString.Contains("Server=") || _configuration["DatabaseProvider"]?.ToLower() == "postgresql")
                {
                    options.UseNpgsql(connString);
                }
                else
                {
                    options.UseSqlite(connString);
                }
            });

            services.AddInfrastructureServices(_configuration);
            services.AddApplicationServices(_configuration);
        }

        /// <summary>
        /// Configure Pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Service TanAn Api V1");
                });
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var options = scope.ServiceProvider
                    .GetRequiredService<DbContextOptions<TanAnDbContext>>();

                using var db = new TanAnDbContext(options);

                DatabaseSchemaValidator.ValidateAsync(db).GetAwaiter().GetResult();
            }
        }
    }

    /// <summary>
    /// Cấu hình mạng Network Proxy
    /// </summary>
    public class NetworkConfig
    {
        public string Prefix { get; set; } = string.Empty;
        public int PrefixLength { get; set; }
    }
}
