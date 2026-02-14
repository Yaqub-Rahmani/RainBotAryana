using Core.Utilities.Pdf.Resources;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace Core.Utilities.Pdf.Engine;

public sealed class QuestPdfFontRegistry
{
    private static volatile bool _registered;
    private static readonly object _lock = new();

    private readonly PdfResourceLoader _res;

    public QuestPdfFontRegistry(PdfResourceLoader res) => _res = res;

    public void EnsureRegistered()
    {
        if (_registered) return;

        lock (_lock)
        {
            if (_registered) return;

            _res.ValidateOrThrow();

            // Font register
            FontManager.RegisterFont(File.OpenRead(_res.DefaultFontPath));

            // İstersen burada global settings:
            QuestPDF.Settings.License = LicenseType.Community; // gerekiyorsa/projende nasıl kullanıyorsan
            QuestPDF.Settings.EnableCaching = true;

            _registered = true;
        }
    }
}
