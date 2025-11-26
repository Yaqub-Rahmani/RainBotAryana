using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Utilities.Images
{
    /// <summary>data:image/...;base64,... ayrıştırıcı</summary>
    public static class ImageDataUrl
    {
        private static readonly Regex Rx = new(
            @"^data:(?<mime>image\/(?:png|jpeg|jpg|webp));base64,(?<data>[A-Za-z0-9+/=\r\n]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool TryParse(string? input, out string mime, out byte[] bytes)
        {
            mime = string.Empty;
            bytes = Array.Empty<byte>();
            if (string.IsNullOrWhiteSpace(input)) return false;

            var m = Rx.Match(input.Trim());
            if (!m.Success) return false;

            mime = m.Groups["mime"].Value;
            var b64 = m.Groups["data"].Value;
            try { bytes = Convert.FromBase64String(b64); }
            catch { return false; }
            return true;
        }

        public static string MimeToExtension(string mime) => mime.ToLowerInvariant() switch
        {
            "image/png" => ".png",
            "image/jpeg" => ".jpg",
            "image/jpg" => ".jpg",
            "image/webp" => ".webp",
            _ => ".bin",
        };
    }
}
