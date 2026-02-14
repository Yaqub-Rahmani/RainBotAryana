using Core.Utilities.Pdf.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models.Reports
{
    public sealed class GenerateContractDebtStatusRequest
    {
        public List<Guid> ContractIds { get; set; } = new();
        public PdfRenderOptions? Options { get; set; }
    }
}
