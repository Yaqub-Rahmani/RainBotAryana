using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Pdf.Abstractions;

namespace Business.Models.Reports
{   

    public sealed class GenerateContractPeriodStatementRequest
    {
        public List<Guid> ContractIds { get; set; } = new();

        // ISO input example: "2026-01-08"
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public PdfRenderOptions? Options { get; set; }

        // Footer texts (optional)
        public string? StampInfoText { get; set; }
    }

}
