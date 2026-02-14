using Business.Models.Reports;
using Core.Utilities.Pdf.Abstractions;
using Core.Utilities.Results;

namespace Business.Abstract;

public interface IReportService
{
    IDataResult<byte[]> GeneratePaymentReceiptA6(Guid collectionId, PdfRenderOptions options);

    IDataResult<byte[]> GeneratePaymentReceiptsA6(IReadOnlyList<Guid> collectionIds, PdfRenderOptions options);


    // NEW: Contract debt status (A6 landscape)
    IDataResult<byte[]> GenerateContractDebtStatusA6(Guid contractId, PdfRenderOptions options);
    IDataResult<byte[]> GenerateContractDebtStatusesA6(IReadOnlyList<Guid> contractIds, PdfRenderOptions options);

    IDataResult<byte[]> GenerateContractPeriodStatementA6(Guid contractId, DateOnly startDate, DateOnly endDate, PdfRenderOptions options, string? stampInfoText);
    IDataResult<byte[]> GenerateContractPeriodStatementsA6(IReadOnlyList<Guid> contractIds, DateOnly startDate, DateOnly endDate, PdfRenderOptions options, string? stampInfoText);


}
