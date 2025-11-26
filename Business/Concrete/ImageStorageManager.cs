using Business.Abstract;
using Core.Utilities.Files;
using Core.Utilities.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public sealed class ImageStorageManager : IImageStorageService
    {
        private static readonly HashSet<string> AllowedMime = new(StringComparer.OrdinalIgnoreCase)
    { "image/png", "image/jpeg", "image/jpg", "image/webp" };

        private readonly IFilePathService _paths;

        public ImageStorageManager(IFilePathService paths) => _paths = paths;

        public async Task<ImageSaveResult> SaveIfDataUrlAsync(
            string photoFolderUrl,
            string baseFile,
            ImageSaveOptions? options = null,
            CancellationToken ct = default)
                {
                    options ??= new ImageSaveOptions();

                    // 1) data-URL mi?
                    if (!ImageDataUrl.TryParse(baseFile, out var mime, out var bytes))
                        return new ImageSaveResult(false, null, null, null, null, null, null, null);

                    // 2) güvenlik kontrolleri
                    if (!AllowedMime.Contains(mime))
                        return new ImageSaveResult(false, null, null, null, null, null, null, "Unsupported mime");

                    if (bytes.Length == 0 || bytes.Length > options.MaxBytes)
                        return new ImageSaveResult(false, null, null, null, null, null, bytes.Length, "Size limit");

                    try
                    {
                        // --- BURASI DÜZELTİLDİ ---
                        // Artık 'out IImageFormat' kullanmıyoruz.
                        using Image<Rgba32> image = Image.Load<Rgba32>(bytes);

                        // 4) hedef klasör
                        var absFolder = _paths.EnsureWebFolder(photoFolderUrl);

                        // 5) isim + uzantı
                        var ext = options.ReencodeToPng ? ".png" : ImageDataUrl.MimeToExtension(mime);
                        var fileName = $"{Guid.NewGuid():N}{ext}";
                        var absPath = Path.Combine(absFolder, fileName);

                        await using var fs = new FileStream(absPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);

                        // 6) yaz (re-encode PNG ya da mime’a göre uygun encoder)
                        if (options.ReencodeToPng)
                        {
                            // Her şeyi PNG'e yeniden kodla (tutarlılık için önerilir)
                            await image.SaveAsPngAsync(fs, new PngEncoder(), ct);
                        }
                        else
                        {
                            // Orijinal MIME'a göre kaydet
                            switch (mime.ToLowerInvariant())
                            {
                                case "image/png":
                                    await image.SaveAsPngAsync(fs, new PngEncoder(), ct);
                                    break;
                                case "image/jpeg":
                                case "image/jpg":
                                    await image.SaveAsJpegAsync(fs, new JpegEncoder
                                    {
                                        // kalite vs. istersen ayarlayabilirsin
                                        Quality = 90
                                    }, ct);
                                    break;
                                case "image/webp":
                                    await image.SaveAsWebpAsync(fs, new WebpEncoder
                                    {
                                        // lossy/lossless seçenekleri mevcut
                                        FileFormat = WebpFileFormatType.Lossy,
                                        Quality = 90
                                    }, ct);
                                    break;
                                default:
                                    // Fallback: png
                                    await image.SaveAsPngAsync(fs, new PngEncoder(), ct);
                                    break;
                            }
                        }

                        var rel = _paths.JoinWebPath(photoFolderUrl, fileName);

                        return new ImageSaveResult(
                            Saved: true,
                            RelativePath: rel,
                            FileName: fileName,
                            Mime: options.ReencodeToPng ? "image/png" : mime,
                            Width: image.Width,
                            Height: image.Height,
                            SizeBytes: new FileInfo(absPath).Length,
                            Error: null
                        );
                    }
                    catch (Exception ex)
                    {
                        return new ImageSaveResult(false, null, null, null, null, null, null, ex.Message);
                    }
                }
    }
}
