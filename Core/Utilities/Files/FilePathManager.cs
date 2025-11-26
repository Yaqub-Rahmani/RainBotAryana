using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Files
{
    public sealed class FilePathManager : IFilePathService
    {
        private readonly IPathProvider _paths;
        public FilePathManager(IPathProvider paths) => _paths = paths;

        public string WebRootPath =>
            string.IsNullOrWhiteSpace(_paths.WebRootPath)
                ? System.IO.Path.Combine(_paths.ContentRootPath, "wwwroot")
                : _paths.WebRootPath;

        public string EnsureWebFolder(string relativeFolder)
        {
            // null/boş güvenli hale getir
            if (string.IsNullOrWhiteSpace(relativeFolder))
                relativeFolder = string.Empty;

            // "users/idCard" → Windows ayıracıyla normalize et
            var rf = relativeFolder.Trim().TrimStart('\\', '/')
                .Replace('/', System.IO.Path.DirectorySeparatorChar)
                .Replace('\\', System.IO.Path.DirectorySeparatorChar);

            // basit traversal koruması
            rf = rf.Replace("..", string.Empty);

            // wwwroot garanti altına al
            var webRoot = WebRootPath;
            if (!System.IO.Directory.Exists(webRoot))
                System.IO.Directory.CreateDirectory(webRoot);

            // wwwroot + rf → mutlak yol
            var abs = System.IO.Path.GetFullPath(System.IO.Path.Combine(webRoot, rf));

            // klasörü (yoksa) oluştur
            System.IO.Directory.CreateDirectory(abs);
            return abs;
        }
        public string JoinWebPath(string relativeFolder, string fileName)
        {
            var rf = (relativeFolder ?? "").Trim().TrimStart('\\', '/').Replace('\\', '/');
            var n = (fileName ?? "").Trim();
            return $"{rf}/{n}".Replace("//", "/");
        }
    }
}
