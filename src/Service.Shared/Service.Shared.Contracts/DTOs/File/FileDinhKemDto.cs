using System;
using System.IO;

namespace Service.Shared.Contracts.DTOs.File
{
    public class FileDinhKemDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? FileExtension { get; set; }
        public string? PathServer { get; set; }
        public string? Bucket { get; set; }
        public string? ContentType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public FileDinhKemDto() { }

        public FileDinhKemDto(UploadedFileResponeDto resp)
        {
            if (resp != null)
            {
                Id = resp.Id != Guid.Empty ? resp.Id : Guid.NewGuid();
                FileName = resp.FileName ?? resp.Name ?? string.Empty;
                FileSize = resp.FileSize > 0 ? resp.FileSize : resp.Size;
                PathServer = resp.PathServer ?? resp.Path;
                Bucket = resp.Bucket;
                FileExtension = resp.FileExtension ?? Path.GetExtension(FileName);
                ContentType = resp.ContentType;
            }
        }
    }

    public class UploadedFileResponeDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? FileName { get; set; }
        public string? Name { get; set; }
        public long FileSize { get; set; }
        public long Size { get; set; }
        public string? FileExtension { get; set; }
        public string? PathServer { get; set; }
        public string? Path { get; set; }
        public string? Bucket { get; set; }
        public string? ContentType { get; set; }
    }
}
