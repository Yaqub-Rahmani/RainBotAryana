using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Query
{
    public class QueryOptions
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Sort { get; set; }

        // Basit arama
        public string? Search { get; set; }
        public List<string> Includes { get; set; } = new();
        public List<string> Filters { get; set; } = new();
    }
}
