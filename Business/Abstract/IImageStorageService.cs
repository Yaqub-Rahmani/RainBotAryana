using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public record ImageSaveOptions(
        bool ReencodeToPng = true,        // içerik ne olursa olsun PNG’e yeniden kodla (tavsiye)
        int MaxBytes = 10 * 1024 * 1024  // 10MB sınır
    );

    public record ImageSaveResult(
        bool Saved,
        string? RelativePath,
        string? FileName,
        string? Mime,
        int? Width,
        int? Height,
        long? SizeBytes,
        string? Error
    );

    public interface IImageStorageService
    {
        /// <summary>
        /// baseFile: data URL (base64) ise kaydeder; değilse hiç dokunmaz.
        /// </summary>
        Task<ImageSaveResult> SaveIfDataUrlAsync(
            string photoFolderUrl,   // örn: "uploads/owners"
            string baseFile,         // data:image/...;base64,... | (başka şeyse kaydetmez)
            ImageSaveOptions? options = null,
            CancellationToken ct = default);
    }
}
