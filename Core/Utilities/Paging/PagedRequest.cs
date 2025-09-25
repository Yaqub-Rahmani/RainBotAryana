using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utilities.Paging
{
    public class PagedRequest
    {
        // 1-based page
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // ör: "name" veya "-unitPrice" (başa - koymak DESC demek)
        public string? Sort { get; set; }

        // Basit arama anahtarı (ör. name contains)
        public string? Search { get; set; }
    }
}
