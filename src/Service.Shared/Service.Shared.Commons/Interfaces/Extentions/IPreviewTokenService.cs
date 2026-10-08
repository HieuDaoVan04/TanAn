using System;

namespace Service.Shared.Commons.Interfaces.Extentions
{
    public interface IPreviewTokenService
    {
        (string? token, DateTime expiresAt) CreateToken(string? bucket, string? pathServer, Guid appId, TimeSpan ttl);
    }
}

namespace Service.Shared.Commons.Services
{
    public class PreviewTokenService : Service.Shared.Commons.Interfaces.Extentions.IPreviewTokenService
    {
        public (string? token, DateTime expiresAt) CreateToken(string? bucket, string? pathServer, Guid appId, TimeSpan ttl)
        {
            var token = Guid.NewGuid().ToString("N");
            return (token, DateTime.UtcNow.Add(ttl));
        }
    }
}
