using Core.Utilities.Pdf.Contracts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Core.Utilities.Pdf.Engine;

public static class A6ReceiptBatchDocument
{
    // A4 landscape (mm): 297 x 210
    // A6 landscape (mm): 148 x 105
    public static byte[] RenderA4Landscape_2x2(
        IReadOnlyList<PaymentReceiptA6Model> items,
        Func<PaymentReceiptA6Model, IComponent> receiptComponentFactory,
        float marginMm = 6f,
        float gapMm = 2f)
    {
        if (items.Count == 0)
            throw new InvalidOperationException("No receipts to render.");

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(marginMm, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontFamily("Vazirmatn"));

                page.Content().Column(col =>
                {
                    int idx = 0;

                    while (idx < items.Count)
                    {
                        // one A4 page block: 2x2 table with up to 4 receipts
                        var slice = items.Skip(idx).Take(4).ToList();
                        idx += slice.Count;

                        col.Item().Element(c =>
                        {
                            c.Table(t =>
                            {
                                t.ColumnsDefinition(cols =>
                                {
                                    cols.ConstantColumn(148); // A6 landscape width
                                    cols.ConstantColumn(148);
                                });

                                // 4 cells auto-flow
                                for (int i = 0; i < 4; i++)
                                {
                                    bool isLastCol = (i % 2) == 1;
                                    bool isLastRow = (i / 2) == 1;

                                    t.Cell()
                                     .Height(105, Unit.Millimetre) // A6 landscape height
                                     .PaddingRight(isLastCol ? 0 : gapMm, Unit.Millimetre)
                                     .PaddingBottom(isLastRow ? 0 : gapMm, Unit.Millimetre)
                                     .Element(cell =>
                                     {
                                         if (i < slice.Count)
                                             cell.Component(receiptComponentFactory(slice[i]));
                                         else
                                             cell.MinimalBox();
                                     });
                                }
                            });
                        });

                        if (idx < items.Count)
                            col.Item().PageBreak();
                    }
                });
            });
        });

        return doc.GeneratePdf();
    }
}
