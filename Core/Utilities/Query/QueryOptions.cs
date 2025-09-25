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

        // Sorting: "name" veya "-createdAt"
        public string? Sort { get; set; }

        // Basit arama
        public string? Search { get; set; }

        // Dinamik include yolları: ["Districts", "Province", "Province.Districts"]
        public List<string> Includes { get; set; } = new();

        // Basit alan=değer filtreleri (eşitlik)
        // Örn: ["isActive=true","provinceId=1"]
        public List<string> Filters { get; set; } = new();
    }
}
