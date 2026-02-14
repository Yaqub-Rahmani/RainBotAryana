using Core.Utilities.Pdf.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Core.Utilities.Pdf.Engine;

public sealed class QuestPdfReportEngine : IPdfReportEngine
{
    private readonly IServiceProvider _sp;
    private readonly QuestPdfFontRegistry _fontRegistry;

    public QuestPdfReportEngine(IServiceProvider sp, QuestPdfFontRegistry fontRegistry)
    {
        _sp = sp;
        _fontRegistry = fontRegistry;
    }

    public byte[] Render<TRequest>(string templateKey, TRequest request, PdfRenderOptions options)
    {
        return RenderMany(templateKey, new List<TRequest> { request }, options);
    }

    public byte[] RenderMany<TRequest>(string templateKey, IReadOnlyList<TRequest> requests, PdfRenderOptions options)
    {
        if (requests is null || requests.Count == 0)
            throw new ArgumentException("requests empty", nameof(requests));

        _fontRegistry.EnsureRegistered();

        var templates = _sp.GetServices(typeof(IReportTemplate<TRequest>)).Cast<IReportTemplate<TRequest>>();
        var tpl = templates.FirstOrDefault(x => x.TemplateKey == templateKey);
        if (tpl is null)
            throw new InvalidOperationException($"Template not found: {templateKey}");

        var sourceSize = MapPageSize(tpl.SourcePageKind);
        var targetSize = MapPageSize(options.TargetPageKind);

        var components = requests.Select(tpl.Build).ToList();

        IDocument doc = new MultiPageDocument(
            components,
            tpl.SourcePageKind,
            options.TargetPageKind,
            sourceSize,
            targetSize,
            options);

        return doc.GeneratePdf();
    }

    private static PageSize MapPageSize(PdfPageKind kind) => kind switch
    {
        PdfPageKind.A4_Portrait => PageSizes.A4,
        PdfPageKind.A4_Landscape => PageSizes.A4.Landscape(),

        PdfPageKind.A5_Portrait => PageSizes.A5,
        PdfPageKind.A5_Landscape => PageSizes.A5.Landscape(),

        PdfPageKind.A6_Portrait => PageSizes.A6,
        PdfPageKind.A6_Landscape => PageSizes.A6.Landscape(),

        PdfPageKind.A7_Portrait => PageSizes.A7,
        PdfPageKind.A7_Landscape => PageSizes.A7.Landscape(),

        _ => PageSizes.A4
    };

    // ✅ MM boyutları (N-up hesapları için tek referans)
    private static (float Wmm, float Hmm) MapPageSizeMm(PdfPageKind kind) => kind switch
    {
        PdfPageKind.A4_Portrait => (210f, 297f),
        PdfPageKind.A4_Landscape => (297f, 210f),

        PdfPageKind.A5_Portrait => (148f, 210f),
        PdfPageKind.A5_Landscape => (210f, 148f),

        PdfPageKind.A6_Portrait => (105f, 148f),
        PdfPageKind.A6_Landscape => (148f, 105f),

        PdfPageKind.A7_Portrait => (74f, 105f),
        PdfPageKind.A7_Landscape => (105f, 74f),

        _ => (210f, 297f)
    };

    private sealed class MultiPageDocument : IDocument
    {
        private readonly IReadOnlyList<IComponent> _components;
        private readonly PdfPageKind _sourceKind;
        private readonly PdfPageKind _targetKind;
        private readonly PageSize _sourceSize;
        private readonly PageSize _targetSize;
        private readonly PdfRenderOptions _opt;

        public MultiPageDocument(
            IReadOnlyList<IComponent> components,
            PdfPageKind sourceKind,
            PdfPageKind targetKind,
            PageSize sourceSize,
            PageSize targetSize,
            PdfRenderOptions opt)
        {
            _components = components;
            _sourceKind = sourceKind;
            _targetKind = targetKind;
            _sourceSize = sourceSize;
            _targetSize = targetSize;
            _opt = opt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            if (_opt.Imposition == ImpositionMode.None)
            {
                foreach (var comp in _components)
                    ComposeSingle(container, comp);

                return;
            }

            // ✅ N-up: çoklu component’i aynı target sayfaya doldur
            ComposeNUpMany(container, _components);
        }

        private void ComposeSingle(IDocumentContainer container, IComponent comp)
        {
            container.Page(page =>
            {
                page.Size(_sourceSize);
                page.Margin(0);
                page.DefaultTextStyle(x => x.FontFamily("Vazirmatn"));
                page.Content().Element(c => c.Component(comp));
            });
        }

        private void ComposeNUpMany(IDocumentContainer container, IReadOnlyList<IComponent> comps)
        {
            var (srcWmm, srcHmm) = MapPageSizeMm(_sourceKind);
            var (tgtWmm, tgtHmm) = MapPageSizeMm(_targetKind);

            var marginMm = _opt.MarginMm;
            var gapMm = _opt.GapMm;

            var usableWmm = tgtWmm - 2 * marginMm;
            var usableHmm = tgtHmm - 2 * marginMm;

            int cols = Math.Max(1, (int)Math.Floor((usableWmm + gapMm) / (srcWmm + gapMm)));
            int rows = Math.Max(1, (int)Math.Floor((usableHmm + gapMm) / (srcHmm + gapMm)));
            int cellCount = cols * rows;

            int idx = 0;

            while (idx < comps.Count)
            {
                var slice = comps.Skip(idx).Take(cellCount).ToList();
                idx += slice.Count;

                container.Page(page =>
                {
                    page.Size(_targetSize);
                    page.Margin(marginMm, Unit.Millimetre);
                    page.DefaultTextStyle(x => x.FontFamily("Vazirmatn"));

                    page.Content().Table(t =>
                    {
                        t.ColumnsDefinition(colsDef =>
                        {
                            for (int c = 0; c < cols; c++)
                                colsDef.ConstantColumn(srcWmm); // proje yaklaşımı: mm sayısal değer
                        });

                        for (int i = 0; i < cellCount; i++)
                        {
                            bool isLastCol = (i % cols) == (cols - 1);
                            bool isLastRow = (i / cols) == (rows - 1);

                            t.Cell()
                                .Height(srcHmm, Unit.Millimetre)
                                .PaddingRight(isLastCol ? 0 : gapMm, Unit.Millimetre)
                                .PaddingBottom(isLastRow ? 0 : gapMm, Unit.Millimetre)
                                .Element(cell =>
                                {
                                    if (i < slice.Count)
                                        cell.Component(slice[i]);
                                    else
                                        cell.MinimalBox();
                                });
                        }
                    });
                });
            }
        }
    }
}


//using Core.Utilities.Pdf.Abstractions;
//using Microsoft.Extensions.DependencyInjection;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;

//namespace Core.Utilities.Pdf.Engine;

//public sealed class QuestPdfReportEngine : IPdfReportEngine
//{
//    private readonly IServiceProvider _sp;
//    private readonly QuestPdfFontRegistry _fontRegistry;

//    public QuestPdfReportEngine(IServiceProvider sp, QuestPdfFontRegistry fontRegistry)
//    {
//        _sp = sp;
//        _fontRegistry = fontRegistry;
//    }

//    public byte[] Render<TRequest>(string templateKey, TRequest request, PdfRenderOptions options)
//    {
//        return RenderMany(templateKey, new List<TRequest> { request }, options);
//    }

//    public byte[] RenderMany<TRequest>(string templateKey, IReadOnlyList<TRequest> requests, PdfRenderOptions options)
//    {
//        if (requests is null || requests.Count == 0)
//            throw new ArgumentException("requests empty", nameof(requests));

//        _fontRegistry.EnsureRegistered();

//        var templates = _sp.GetServices(typeof(IReportTemplate<TRequest>)).Cast<IReportTemplate<TRequest>>();
//        var tpl = templates.FirstOrDefault(x => x.TemplateKey == templateKey);
//        if (tpl is null)
//            throw new InvalidOperationException($"Template not found: {templateKey}");

//        var sourceSize = MapPageSize(tpl.SourcePageKind);
//        var targetSize = MapPageSize(options.TargetPageKind);

//        // her request -> component
//        var components = requests.Select(tpl.Build).ToList();

//        IDocument doc = new MultiPageDocument(components, sourceSize, targetSize, options);
//        return doc.GeneratePdf();
//    }

//    private static PageSize MapPageSize(PdfPageKind kind) => kind switch
//    {
//        PdfPageKind.A4_Portrait => PageSizes.A4,
//        PdfPageKind.A4_Landscape => PageSizes.A4.Landscape(),

//        PdfPageKind.A5_Portrait => PageSizes.A5,
//        PdfPageKind.A5_Landscape => PageSizes.A5.Landscape(),

//        PdfPageKind.A6_Portrait => PageSizes.A6,
//        PdfPageKind.A6_Landscape => PageSizes.A6.Landscape(),

//        PdfPageKind.A7_Portrait => PageSizes.A7,
//        PdfPageKind.A7_Landscape => PageSizes.A7.Landscape(),

//        _ => PageSizes.A4
//    };


//    private sealed class MultiPageDocument : IDocument
//    {
//        private readonly IReadOnlyList<IComponent> _components;
//        private readonly PageSize _sourceSize;
//        private readonly PageSize _targetSize;
//        private readonly PdfRenderOptions _opt;

//        public MultiPageDocument(IReadOnlyList<IComponent> components, PageSize sourceSize, PageSize targetSize, PdfRenderOptions opt)
//        {
//            _components = components;
//            _sourceSize = sourceSize;
//            _targetSize = targetSize;
//            _opt = opt;
//        }

//        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

//        public void Compose(IDocumentContainer container)
//        {
//            foreach (var comp in _components)
//            {
//                if (_opt.Imposition == ImpositionMode.None)
//                    ComposeSingle(container, comp);
//                else
//                    ComposeNUp(container, comp);
//            }
//        }

//        private void ComposeSingle(IDocumentContainer container, IComponent comp)
//        {
//            container.Page(page =>
//            {
//                page.Size(_sourceSize);
//                page.Margin(0);
//                page.DefaultTextStyle(x => x.FontFamily("Vazirmatn"));

//                page.Content().Element(c => c.Component(comp));
//            });
//        }

//        private void ComposeNUp(IDocumentContainer container, IComponent comp)
//        {
//            // tek fişi büyütmeden target page'e koy (ilk cell)
//            var srcW = _sourceSize.Width;
//            var srcH = _sourceSize.Height;

//            var tgtW = _targetSize.Width;
//            var tgtH = _targetSize.Height;

//            var margin = _opt.MarginMm;
//            var gap = _opt.GapMm;

//            var usableW = tgtW - 2 * margin;
//            var usableH = tgtH - 2 * margin;

//            int cols = Math.Max(1, (int)Math.Floor((usableW + gap) / (srcW + gap)));
//            int rows = Math.Max(1, (int)Math.Floor((usableH + gap) / (srcH + gap)));
//            int cellCount = cols * rows;

//            container.Page(page =>
//            {
//                page.Size(_targetSize);
//                page.Margin(margin, Unit.Millimetre);
//                page.DefaultTextStyle(x => x.FontFamily("Vazirmatn"));

//                page.Content().Table(table =>
//                {
//                    table.ColumnsDefinition(columns =>
//                    {
//                        for (int i = 0; i < cols; i++)
//                            columns.ConstantColumn(srcW);
//                    });

//                    for (int i = 0; i < cellCount; i++)
//                    {
//                        bool isLastCol = (i % cols) == (cols - 1);
//                        bool isLastRow = (i / cols) == (rows - 1);

//                        table.Cell()
//                            .Height(srcH, Unit.Millimetre)
//                            .PaddingRight(isLastCol ? 0 : gap, Unit.Millimetre)
//                            .PaddingBottom(isLastRow ? 0 : gap, Unit.Millimetre)
//                            .Element(c =>
//                            {
//                                if (i == 0) c.Component(comp);  // sadece ilk hücre
//                                else c.Padding(0);
//                            });
//                    }
//                });
//            });
//        }
//    }
//}
