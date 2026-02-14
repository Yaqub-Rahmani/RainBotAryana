namespace Core.Utilities.Pdf;

public sealed class PdfOptions
{
    public string FontFileName { get; init; } = "Vazirmatn-Regular.ttf";
    public string LogoFileName { get; init; } = "AryanaLogo.png";

    // Assets kök klasörü (WebAPI ContentRoot altında)
    public string AssetsRootRelativePath { get; init; } = "Assets/Pdf";
}
