namespace Core.Utilities.Pdf.Resources;

public sealed class PdfAssetsOptions
{
    /// <summary>Örn: {ContentRoot}/wwwroot/pdf</summary>
    public string RootPath { get; set; } = default!;
    public string LogoFileName { get; set; } = "AryanaLogo.png";
    public string DefaultFontFileName { get; set; } = "Vazirmatn-Regular.ttf";
}
