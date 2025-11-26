using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Files
{
    public interface IPathProvider
    {
        string ContentRootPath { get; } // Proje kökü
        string WebRootPath { get; }     // wwwroot (yoksa ContentRoot altında wwwroot varsay
    }
}
