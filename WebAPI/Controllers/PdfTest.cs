using Core.Utilities.KabulDateTime;
using Core.Utilities.Pdf;
using Entities.Dtos.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/pdf")]
public sealed class PdfTestController : ControllerBase
{
    private readonly IPdfRenderer _pdf;
    private readonly ITimeProvider _time;

    public PdfTestController(IPdfRenderer pdf, ITimeProvider time)
    {
        _pdf = pdf;
        _time = time;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        var nowUtc = _time.UtcNow();

        var dto = new PdfTestDto
        {
            TitleFa = "رسید آزمایشی",
            DateLine = $"تاریخ: {_time.FormatPersianDateWithMonthNameDash(nowUtc)}  ساعت: {_time.FormatTimeAmPm(nowUtc)}",
            AmountLine = $"مبلغ: {_time.RtlSafeLatin("12345.60")} ؋"
        };

        var bytes = _pdf.Render("pdf-test", dto);
        return File(bytes, "application/pdf", "pdf-test-a6.pdf");
    }
}
