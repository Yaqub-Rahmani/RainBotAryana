using Core.Utilities.Pdf.Abstractions;
using Core.Utilities.Pdf.Contracts;
using Core.Utilities.Pdf.Resources;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Core.Utilities.Pdf.Templates;

public sealed class PaymentReceiptA6Template : IReportTemplate<PaymentReceiptA6Model>
{
    private readonly PdfResourceLoader _res;

    public PaymentReceiptA6Template(PdfResourceLoader res) => _res = res;

    public string TemplateKey => "payment-receipt-a6";
    public PdfPageKind SourcePageKind => PdfPageKind.A6_Landscape;

    public IComponent Build(PaymentReceiptA6Model m) => new ReceiptComponent(m, _res);

    private sealed class ReceiptComponent : IComponent
    {
        private readonly PaymentReceiptA6Model _m;
        private readonly PdfResourceLoader _res;

        public ReceiptComponent(PaymentReceiptA6Model m, PdfResourceLoader res)
        {
            _m = m;
            _res = res;
        }

        public void Compose(IContainer container)
        {
            container
                .Border(1)
                .Padding(10) // dış boşluk artırıldı
                .DefaultTextStyle(x => x.FontFamily("Vazirmatn").FontSize(9))
                .Column(col =>
                {
                    // 1) Top: Big Logo + (DateTime small) + (Title big)
                    col.Item().Row(r =>
                    {
                        r.ConstantItem(72).Height(72)
                            .AlignLeft()
                            .Element(x => x.Image(_res.LogoPath).FitArea());

                        r.RelativeItem().PaddingTop(10).AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().PaddingBottom(9)
                                .Text($"{_m.PaidWeekday}  {_m.PaidDateBig}  {_m.PaidTime}")
                                .FontSize(7);

                            c.Item().AlignRight()
                                .Text($"فعالیت: {_m.BusinessActivity}")
                                .FontSize(8);

                            c.Item().AlignRight()
                                .Text($"مستاجر: {_m.TenantName}")
                                .FontSize(8);

                            if (!string.IsNullOrWhiteSpace(_m.FranchiseOwnerText))
                                c.Item().AlignRight().Text($"امتیاز: {_m.FranchiseOwnerText}").FontSize(7);

                        });
                    });

                    // ✅ Başlık ayrı satır: tam ortada
                    col.Item().PaddingTop(-50).AlignCenter()
                       .Text("چیک رسید")
                       .FontSize(21)
                       .SemiBold();



                    // 2) Under title: single line tenant | franchise (tek satır)
                    //col.Item().PaddingTop(2).AlignRight()
                    //    .Text($"مستاجر: {_m.TenantName} | امتیاز: {_m.FranchiseOwnerText}")
                    //    .FontSize(9);

                    // 3) Line under logo area
                    col.Item().PaddingTop(3).LineHorizontal(1);

                    // 4) Amount big (left side) + method small next to it
                    //    Opposite: Shop name big + Market/Floor under it
                    col.Item().PaddingTop(4).Row(r =>
                    {
                        // Left block: Amount + Method
                        r.RelativeItem().AlignLeft().Column(c =>
                        {

                            c.Item().Row(rr =>
                            {
                                rr.AutoItem().AlignLeft()
                                    .Text(_m.AmountBig)
                                    .FontSize(18)
                                    .SemiBold();

                                rr.ConstantItem(10); // küçük boşluk

                                rr.RelativeItem().PaddingTop(6).AlignLeft()
                                    .Text(_m.MethodText)
                                    .FontSize(9);
                            });
                        });

                        // Right block: Shop + Market/Floor
                        r.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().AlignRight()
                                .Text($"{_m.ShopName} | :دوکان")
                                .FontSize(13)
                                .SemiBold();

                            c.Item().AlignRight()
                                .Text($"مارکیت: {_m.MarketName} | طبقه: {_m.FloorText}")
                                .FontSize(9);
                        });
                    });

                    col.Item().PaddingTop(3).LineHorizontal(1);

                    // 5) Table: # | مبلغ | دلیل | دوره  (satır numarası eklendi)
                    col.Item().PaddingTop(4).Element(BuildAllocationTable);

                    // Spacer -> footer en alta insin
                    col.Item().Height(1).Extend();


                    col.Item().PaddingTop(3).LineHorizontal(1);

                    // 6) Footer: left note, right user name
                    col.Item().PaddingTop(2).Row(r =>
                    {
                        r.RelativeItem().AlignLeft()
                            .Text("بدون امضا/مهر این چک معتبر نیست")
                            .FontSize(8);

                        r.RelativeItem().AlignRight()
                            .Text(_m.CreatedByName)
                            .FontSize(8)
                            .SemiBold();
                    });
                });
        }

        private void BuildAllocationTable(IContainer container)
        {
            var lines = _m.Lines.Take(7).ToList(); // A6 yatay için limit
            if (lines.Count == 0)
                lines.Add(new PaymentReceiptA6LineModel { Period = "-", Kind = "-", Amount = "-" });

            container.Table(t =>
            {
                t.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(30); // مبلغ
                    cols.RelativeColumn(18); // دوره
                    cols.RelativeColumn(42); // دلیل
                    cols.RelativeColumn(10); // #

                });


                HeaderCell(t, "مبلغ");
                HeaderCell(t, "دوره");
                HeaderCell(t, "دلیل");
                HeaderCell(t, "#");

                for (int i = 0; i < lines.Count; i++)
                {
                    var ln = lines[i];

                    RowCell(t, ln.Amount);
                    RowCell(t, ln.Period);
                    RowCell(t, ln.Kind);
                    RowCell(t, (i + 1).ToString()); // satır sayısı
                }
            });
        }

        private static void HeaderCell(TableDescriptor t, string text)
        {
            t.Cell()
                .BorderBottom(1)
                .PaddingVertical(2)
                .AlignRight()
                .Text(text)
                .SemiBold();
        }

        private static void RowCell(TableDescriptor t, string text)
        {
            t.Cell()
                .BorderBottom(0.5f)
                .PaddingVertical(2)
                .AlignRight()
                .Text(text);
        }
    }
}
