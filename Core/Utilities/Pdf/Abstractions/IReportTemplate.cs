using QuestPDF.Infrastructure;

namespace Core.Utilities.Pdf.Abstractions;

public interface IReportTemplate<in TRequest>
{
    string TemplateKey { get; }
    PdfPageKind SourcePageKind { get; } // template’in doğal boyutu (örn A6 landscape)

    /// <summary>
    /// Template’in "içeriği". Engine bunu tek sayfa veya N-up içine yerleştirir.
    /// </summary>
    IComponent Build(TRequest request);
}
