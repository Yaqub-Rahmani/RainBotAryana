using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Utilities.Pdf.Abstractions;
using Core.Utilities.Pdf.Contracts;
using Core.Utilities.Pdf.Resources;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Core.Utilities.Pdf.Templates
{
    public sealed class ContractDebtStatusA6Template : IReportTemplate<ContractDebtStatusA6Model>
    {
        private readonly PdfResourceLoader _res;
        public ContractDebtStatusA6Template(PdfResourceLoader res) => _res = res;

        public string TemplateKey => "contract-debt-a6";
        public PdfPageKind SourcePageKind => PdfPageKind.A6_Landscape;

        public IComponent Build(ContractDebtStatusA6Model m) => new Component(m, _res);

        private sealed class Component : IComponent
        {
            private readonly ContractDebtStatusA6Model _m;
            private readonly PdfResourceLoader _res;

            public Component(ContractDebtStatusA6Model m, PdfResourceLoader res)
            {
                _m = m;
                _res = res;
            }

            public void Compose(IContainer container)
            {
                container
                    .Border(1)
                    .Padding(8)
                    .DefaultTextStyle(x => x.FontFamily("Vazirmatn").FontSize(8))
                    .Column(col =>
                    {
                        // Header: logo + meta
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(72).Height(72)
                                .AlignLeft()
                                .Element(x => x.Image(_res.LogoPath).FitArea());

                            r.RelativeItem().PaddingTop(10).AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().PaddingBottom(9)
                                    .Text($"{_m.ReportWeekday}  {_m.ReportDateBig}  {_m.ReportTime}")
                                    .FontSize(7);

                                c.Item().AlignRight()
                                    .Text($"فعالیت: {_m.BusinessActivity}")
                                    .FontSize(8);

                                c.Item().AlignRight()
                                    .Text($"مستاجر: {_m.TenantName}")
                                    .FontSize(8);

                                if (_m.HasFranchiseOwners && !string.IsNullOrWhiteSpace(_m.FranchiseOwnersText))
                                    c.Item().AlignRight().Text(_m.FranchiseOwnersText).FontSize(7);
                            });

                        });

                        col.Item().PaddingTop(-38).AlignCenter()
                            .Text("گزارش باقی")
                            .FontSize(16)
                            .SemiBold();

                        col.Item().PaddingTop(2).LineHorizontal(1);


                        col.Item().PaddingTop(4).Row(r =>
                        {
                            // Left block: Amount + Method
                            r.RelativeItem().AlignLeft().Column(c =>
                            {

                                c.Item().Row(rr =>
                                {
                                    c.Item().Text($"جمله باقی: {_m.TotalDebt}").FontSize(13).SemiBold();
                                    c.Item().Text($"باقی گذشته: {_m.OldDebtTotal}  - باقی این دوره: {_m.CurrentDebtTotal}").FontSize(9);
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


                        col.Item().PaddingTop(2).LineHorizontal(0.5f);

                        col.Item().PaddingTop(3).Table(t =>
                        {
                            // 4 columns (RTL layout with AlignRight):
                            // [Value] [Label] [Period] [RowNo]
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(4);  // value
                                cols.RelativeColumn(3);  // label
                                cols.RelativeColumn(4);  // period
                                cols.ConstantColumn(16); // row no
                            });

                            // Header row (same order as columns)
                            t.Cell().BorderBottom(0.5f).PaddingBottom(2).AlignRight().Text("مقدار").SemiBold();
                            t.Cell().BorderBottom(0.5f).PaddingBottom(2).AlignRight().Text("جزئیات").SemiBold();
                            t.Cell().BorderBottom(0.5f).PaddingBottom(2).AlignRight().Text("دوره").SemiBold();
                            t.Cell().BorderBottom(0.5f).PaddingBottom(2).AlignRight().Text("#").SemiBold();

                            int i = 1;
                            var period = _m.CurrentPeriod;

                            Row4(t, i++, "سهم مالک مارکیت", _m.CurrentOwnerPortion, period);
                            Row4(t, i++, "سهم مارکیت", _m.CurrentMarketPortion, period);
                            Row4(t, i++, "سهم امتیاز", _m.CurrentFranchisePortion, period);

                            // Electricity: kWh next to name (label), value is ONLY money
                            var electricityLabel = $"({_m.CurrentElectricityKwh})  برق ";
                            Row4(t, i++, electricityLabel, _m.CurrentElectricityAmount, period);

                            static void Row4(TableDescriptor t, int rowNo, string label, string value, string period)
                            {
                                // value
                                t.Cell().BorderBottom(0.3f).PaddingVertical(1).AlignRight().Text(value).SemiBold();
                                // label
                                t.Cell().BorderBottom(0.3f).PaddingVertical(1).AlignRight().Text(label);
                                // period
                                t.Cell().BorderBottom(0.3f).PaddingVertical(1).AlignRight().Text(period);
                                // row no
                                t.Cell().BorderBottom(0.3f).PaddingVertical(1).AlignRight().Text(rowNo.ToString());
                            }
                        });


                        // Footer
                        col.Item().PaddingTop(3).LineHorizontal(0.5f);

                        col.Item().PaddingTop(3).Row(r =>
                        {

                            r.RelativeItem().AlignLeft().Text(_m.DisclaimerText).FontSize(7);
                            r.RelativeItem().AlignRight().Text($"ادمین: {_m.PreparedByUserName}").FontSize(7);
                        });
                    });

            }
        }
    }

}
