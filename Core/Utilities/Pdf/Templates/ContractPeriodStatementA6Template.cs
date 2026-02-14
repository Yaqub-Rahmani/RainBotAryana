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

    public sealed class ContractPeriodStatementA6Template : IReportTemplate<ContractPeriodStatementA6Model>
    {
        private readonly PdfResourceLoader _res;
        public ContractPeriodStatementA6Template(PdfResourceLoader res) => _res = res;

        public string TemplateKey => "contract-period-statement-a6";
        public PdfPageKind SourcePageKind => PdfPageKind.A6_Landscape;

        public IComponent Build(ContractPeriodStatementA6Model m) => new Component(m, _res);

        private sealed class Component : IComponent
        {
            private readonly ContractPeriodStatementA6Model _m;
            private readonly PdfResourceLoader _res;

            public Component(ContractPeriodStatementA6Model m, PdfResourceLoader res)
            {
                _m = m;
                _res = res;
            }

            public void Compose(IContainer container)
            {
                container
                    .Border(1)
                    .Padding(8)
                    .DefaultTextStyle(x => x.FontFamily("Vazirmatn").FontSize(7))
                    .Column(col =>
                    {
                        // Header: logo + meta
                        col.Item().Row(r =>
                        {
                            r.ConstantItem(60).Height(60)
                                .AlignLeft()
                                .Element(x => x.Image(_res.LogoPath).FitArea());

                            r.RelativeItem().AlignRight().PaddingTop(3).Column(c =>
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

                                if (!string.IsNullOrWhiteSpace(_m.FranchiseOwnerText))
                                    c.Item().AlignRight().Text($"امتیاز: {_m.FranchiseOwnerText}").FontSize(7);
                            });


                        });

                        col.Item().PaddingTop(-34).AlignCenter()
                            .Text("گزارش دوره‌ای قرارداد")
                            .FontSize(14)
                            .SemiBold();

                        col.Item().PaddingTop(1).LineHorizontal(0.8f);

                        // Subheader
                        col.Item().PaddingTop(2).Row(r =>
                        {
                            // Left block: Amount + Method
                            r.RelativeItem().AlignLeft().Column(c =>
                            {

                                c.Item().Row(rr =>
                                {
                                    c.Item().Text($"{_m.DebtStatusText}").FontSize(13).SemiBold();
                                    c.Item().Text($"{_m.RangeText}").FontSize(8);
                                });
                            });

                            r.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight()
                                    .Text($"{_m.ShopName} | :دوکان")
                                    .FontSize(13)
                                    .SemiBold();

                                c.Item().AlignRight()
                                    .Text($"مارکیت: {_m.MarketName} | طبقه: {_m.FloorText}")
                                    .FontSize(8);
                            });

                        });

                        col.Item().PaddingTop(2).LineHorizontal(0.5f);

                        // Table: RTL, columns order (left->right in code, visually right-aligned)
                        // [باقی] [پرداخت] [مجموع] [kWh] [دوره] [#]
                        col.Item().PaddingTop(3).Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(2); // Remaining
                                cols.RelativeColumn(2); // Paid
                                cols.RelativeColumn(2); // Total
                                cols.RelativeColumn(2); // kWh
                                cols.RelativeColumn(2); // Period
                                cols.ConstantColumn(14); // #
                            });

                            Header(t, "باقی");
                            Header(t, "پرداخت");
                            Header(t, "مجموع");
                            Header(t, "برق (kWh)");
                            Header(t, "دوره");
                            Header(t, "#");

                            foreach (var line in _m.Lines)
                            {
                                Cell(t, line.Remaining, bold: true);
                                Cell(t, line.Paid);
                                Cell(t, line.TotalDebt);
                                Cell(t, line.Kwh);
                                Cell(t, line.Period);
                                Cell(t, line.RowNo);
                            }

                            static void Header(TableDescriptor t, string text)
                            {
                                t.Cell().BorderBottom(0.6f).PaddingVertical(0.5f).AlignRight().Text(text).SemiBold();
                            }

                            static void Cell(TableDescriptor t, string text, bool bold = false)
                            {
                                var c = t.Cell().BorderBottom(0.3f).PaddingVertical(0.5f).AlignRight();
                                if (bold) c.Text(text).SemiBold();
                                else c.Text(text);
                            }
                        });

                        col.Item().PaddingTop(1).LineHorizontal(0.5f);

                        // Footer
                        col.Item().PaddingTop(1).Row(r =>
                        {
                            r.RelativeItem().AlignLeft().Text(_m.StampInfoText).FontSize(6);
                            r.RelativeItem().AlignRight().Text($"ادمین: {_m.PreparedByUserName}").FontSize(6);
                        });
                    });
            }
        }
    }

}
