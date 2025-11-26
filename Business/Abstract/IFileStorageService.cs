using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public record FileSaveOptions(
        int MaxBytes = 10 * 1024 * 1024,            // 10MB
        string[]? AllowedMime = null,               // null => tüm MIME’lar serbest
        string[]? AllowedExtensions = null,         // null => uzantı kısıtlaması yok
        bool Overwrite = false                      // aynı ada denk gelirse üstüne yazma
    );

    public record FileSaveResult(
        bool Saved,
        string? RelativePath,
        string? FileName,
        string? Mime,
        long? SizeBytes,
        string? Error
    );

    public interface IFileStorageService
    {
        /// <summary>
        /// Tek bir dosyayı güvenli şekilde kaydeder.
        /// folderUrl: web köküne göre göreli klasör ("uploads/contracts" gibi).
        /// </summary>
        Task<FileSaveResult> SaveAsync(
            string folderUrl,
            IFormFile file,
            FileSaveOptions? options = null,
            CancellationToken ct = default);
    }
}
