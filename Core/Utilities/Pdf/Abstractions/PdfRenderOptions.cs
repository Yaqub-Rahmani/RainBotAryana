namespace Core.Utilities.Pdf.Abstractions;

public enum ImpositionMode
{
    None = 0,
    NUpNoScale = 1
}

public sealed class PdfRenderOptions
{
    public PdfPageKind TargetPageKind { get; set; } = PdfPageKind.A4_Portrait;
    public ImpositionMode Imposition { get; set; } = ImpositionMode.None;

    /// <summary>Target sayfada kenar boşluğu (mm)</summary>
    public float MarginMm { get; set; } = 6f;

    /// <summary>Grid hücre arası boşluk (mm)</summary>
    public float GapMm { get; set; } = 2f;
}
