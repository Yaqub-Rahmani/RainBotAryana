namespace Core.Utilities.Pdf.Abstractions;

public interface IPdfReportEngine
{
    byte[] Render<TRequest>(string templateKey, TRequest request, PdfRenderOptions options);

    byte[] RenderMany<TRequest>(string templateKey, IReadOnlyList<TRequest> requests, PdfRenderOptions options);

}
