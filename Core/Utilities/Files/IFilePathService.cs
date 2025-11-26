using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Files
{
    public interface IFilePathService
    {
        string WebRootPath { get; }
        string EnsureWebFolder(string relativeFolder);                 // wwwroot/<folder> oluşturur
        string JoinWebPath(string relativeFolder, string fileName);    // "uploads/x/file.png"
    }

}
