// "Một sản phẩm của HieuDV"

using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace Service.TanAn.API
{
    /// <summary>
    /// Cấu hình Serilog Logger cho API Service.TanAn.API
    /// </summary>
    public static class SerilogConfig
    {
        /// <summary>
        /// Khởi tạo và cấu hình Serilog Logger từ Configuration
        /// </summary>
        public static (Serilog.ILogger Logger, LoggingLevelSwitch LevelSwitch) ConfigureLogger(IConfiguration configuration)
        {
            var levelSwitch = new LoggingLevelSwitch();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            return (logger, levelSwitch);
        }
    }
}
