using Core.Utilities.Pdf.Abstractions;

namespace Core.Utilities.Pdf.Engine;

public readonly record struct PageSpec(float WidthMm, float HeightMm)
{
    public bool IsLandscape => WidthMm > HeightMm;
}

public static class PageSpecs
{
    // ISO 216 mm ölçüleri
    private static readonly PageSpec A4P = new(210, 297);
    private static readonly PageSpec A5P = new(148, 210);
    private static readonly PageSpec A6P = new(105, 148);
    private static readonly PageSpec A7P = new(74, 105);

    public static PageSpec Get(PdfPageKind kind) => kind switch
    {
        PdfPageKind.A4_Portrait => A4P,
        PdfPageKind.A4_Landscape => new PageSpec(A4P.HeightMm, A4P.WidthMm),

        PdfPageKind.A5_Portrait => A5P,
        PdfPageKind.A5_Landscape => new PageSpec(A5P.HeightMm, A5P.WidthMm),

        PdfPageKind.A6_Portrait => A6P,
        PdfPageKind.A6_Landscape => new PageSpec(A6P.HeightMm, A6P.WidthMm),

        PdfPageKind.A7_Portrait => A7P,
        PdfPageKind.A7_Landscape => new PageSpec(A7P.HeightMm, A7P.WidthMm),

        _ => A4P
    };
}
