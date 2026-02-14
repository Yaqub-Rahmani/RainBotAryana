namespace Core.Utilities.Pdf.Resources;

public sealed class PdfResourceLoader
{
    private readonly PdfAssetsOptions _opt;

    public PdfResourceLoader(PdfAssetsOptions opt) => _opt = opt;

    public string LogoPath => Path.Combine(_opt.RootPath, _opt.LogoFileName);
    public string DefaultFontPath => Path.Combine(_opt.RootPath, "fonts", _opt.DefaultFontFileName);

    public void ValidateOrThrow()
    {
        if (!Directory.Exists(_opt.RootPath))
            throw new DirectoryNotFoundException($"Pdf root not found: {_opt.RootPath}");

        if (!File.Exists(LogoPath))
            throw new FileNotFoundException($"Logo not found: {LogoPath}");

        if (!File.Exists(DefaultFontPath))
            throw new FileNotFoundException($"Font not found: {DefaultFontPath}");
    }
}
