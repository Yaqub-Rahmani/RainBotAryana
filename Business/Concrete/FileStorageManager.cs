using Business.Abstract;
using Core.Utilities.Files;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public sealed class FileStorageManager : IFileStorageService
    {
        private readonly IFilePathService _paths;

        private static readonly HashSet<string> DefaultAllowedMimes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "application/pdf",
                "image/png", "image/jpeg", "image/jpg", "image/webp"
            };

        private static readonly HashSet<string> DefaultAllowedExts =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".pdf", ".png", ".jpg", ".jpeg", ".webp"
            };

        public FileStorageManager(IFilePathService paths) => _paths = paths;

        public async Task<FileSaveResult> SaveAsync(
            string folderUrl,
            IFormFile file,
            FileSaveOptions? options = null,
            CancellationToken ct = default)
        {
            options ??= new FileSaveOptions(
                AllowedMime: DefaultAllowedMimes.ToArray(),
                AllowedExtensions: DefaultAllowedExts.ToArray()
            );

            if (string.IsNullOrWhiteSpace(folderUrl))
                return new FileSaveResult(false, null, null, null, null, "folderUrl is required.");

            if (file is null || file.Length == 0)
                return new FileSaveResult(false, null, null, null, null, "Empty file.");

            if (file.Length > options.MaxBytes)
                return new FileSaveResult(false, null, null, null, file.Length, "Size limit exceeded.");

            // Uzantı & MIME
            var ext = Path.GetExtension(file.FileName);
            var mime = file.ContentType;

            if (options.AllowedExtensions is { Length: > 0 } &&
                !options.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                return new FileSaveResult(false, null, null, null, file.Length, $"Extension not allowed: {ext}");

            if (options.AllowedMime is { Length: > 0 } &&
                !options.AllowedMime.Contains(mime, StringComparer.OrdinalIgnoreCase))
                return new FileSaveResult(false, null, null, null, file.Length, $"MIME not allowed: {mime}");

            // İçerik sign kontrolü (yalın doğrulama)
            using var head = file.OpenReadStream();
            var prefix = new byte[Math.Min(32, (int)Math.Min(file.Length, 32))];
            int read = await head.ReadAsync(prefix, 0, prefix.Length, ct);
            head.Position = 0;

            bool looksPdf = read >= 4 && prefix[0] == 0x25 && prefix[1] == 0x50 && prefix[2] == 0x44 && prefix[3] == 0x46;                          // %PDF
            bool looksPng = read >= 8 && prefix[0] == 0x89 && prefix[1] == 0x50 && prefix[2] == 0x4E && prefix[3] == 0x47;                          // PNG
            bool looksJpg = read >= 2 && prefix[0] == 0xFF && prefix[1] == 0xD8;                                                                     // JPEG SOI
            bool looksWebp = read >= 12 && prefix[0] == 0x52 && prefix[1] == 0x49 && prefix[2] == 0x46 && prefix[3] == 0x46
                                           && prefix[8] == 0x57 && prefix[9] == 0x45 && prefix[10] == 0x42 && prefix[11] == 0x50;                    // RIFF....WEBP

            if (mime.StartsWith("application/pdf", StringComparison.OrdinalIgnoreCase) && !looksPdf)
                return new FileSaveResult(false, null, null, null, file.Length, "Invalid PDF signature.");
            if (mime.StartsWith("image/png", StringComparison.OrdinalIgnoreCase) && !looksPng)
                return new FileSaveResult(false, null, null, null, file.Length, "Invalid PNG signature.");
            if ((mime.StartsWith("image/jpeg", StringComparison.OrdinalIgnoreCase) || mime.EndsWith("/jpg", StringComparison.OrdinalIgnoreCase)) && !looksJpg)
                return new FileSaveResult(false, null, null, null, file.Length, "Invalid JPEG signature.");
            if (mime.StartsWith("image/webp", StringComparison.OrdinalIgnoreCase) && !looksWebp)
                return new FileSaveResult(false, null, null, null, file.Length, "Invalid WEBP signature.");

            try
            {
                // Güvenli klasör (IFilePathService senin projende web-köküne göre garanti veriyor)
                var absFolder = _paths.EnsureWebFolder(folderUrl);

                // Güvenli isim: orijinal uzantıyı koruyarak yeni GUID
                var finalName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
                var absPath = Path.Combine(absFolder, finalName);

                if (!options.Overwrite && File.Exists(absPath))
                    return new FileSaveResult(false, null, null, null, null, "File exists.");

                await using (var fs = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                {
                    await file.CopyToAsync(fs, ct);
                }

                // (Opsiyonel) Hash ile deduplikasyon/sahte kontrolü
                // using var sha = SHA256.Create();
                // await using var fs2 = new FileStream(absPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                // var hash = await sha.ComputeHashAsync(fs2, ct);

                var rel = _paths.JoinWebPath(folderUrl, finalName);
                var size = new FileInfo(absPath).Length;

                return new FileSaveResult(true, rel, finalName, mime, size, null);
            }
            catch (OperationCanceledException)
            {
                return new FileSaveResult(false, null, null, null, null, "Canceled.");
            }
            catch (Exception ex)
            {
                return new FileSaveResult(false, null, null, null, null, ex.Message);
            }
        }
    }
}
