using Core.Utilities.Pdf.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Models.Reports
{
    public sealed class GeneratePaymentReceiptsRequest
    {
        public List<Guid> CollectionIds { get; set; } = new();

        // Öneri: A6 yatay fişleri A4’e 4-up basmak için A4_Landscape seç
        //public PdfPageKind TargetPageKind { get; set; } = PdfPageKind.A4_Landscape;

        //public ImpositionMode Imposition { get; set; } = ImpositionMode.NUpNoScale;

        //public float MarginMm { get; set; } = 6f;
        //public float GapMm { get; set; } = 2f;
    }
}
