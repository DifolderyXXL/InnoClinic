using MicroserviceApiKernel.Results;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DocumentsAPI.Application;

public class PdfMedicalResultGenerator : IPdfMedicalResultGenerator
{
    public byte[] Generate(MedicalResultPdfData data)
    {
        return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(20));

                    page.Header()
                        .Text("Medical result")
                        .SemiBold().FontSize(36).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Doctor {data.Doctor}");
                            x.Item().Text($"Patient {data.Patient}");
                            x.Item().Text($"Complaints {data.Complaints}");
                            x.Item().Text($"Recommendations {data.Recommendations}");
                            x.Item().Text($"Conclusion {data.Conclusion}");
                        });

                    page.Footer()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);
                            x.Item().AlignCenter().Text($"Date {data.Date}");

                            x.Item().AlignCenter()
                                .Text(e =>
                                {
                                    e.Span("Page ");
                                    e.CurrentPageNumber();
                                });
                        });
                });
            })
            .GeneratePdf();
    }
}