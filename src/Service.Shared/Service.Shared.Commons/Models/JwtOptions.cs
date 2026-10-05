// "Một sản phẩm của HieuDV"

namespace Service.Shared.Commons.Models
{
    public class JwtOptions
    {
        public string SecretKey { get; set; } = "TanAnCommuneDigitalPlatformSecretKey2026!KeySuperSecretForGraduationProject";
        public string SecretKeyRefresh { get; set; } = "TanAnCommuneDigitalPlatformSecretKey2026!KeySuperSecretForGraduationProject";
        public string Issuer { get; set; } = "TanAnIssuer";
        public string Audience { get; set; } = "TanAnAudience";
        public int ExpirationMinutes { get; set; } = 60;
    }
}
