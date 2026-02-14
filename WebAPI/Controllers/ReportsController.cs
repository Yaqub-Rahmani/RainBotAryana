using Business.Abstract;
using Business.Models.Reports;
using Core.Utilities.Pdf.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reports) => _reportService = reports;

        private static PdfRenderOptions DefaultA6Landscape() => new()
        {
            TargetPageKind = PdfPageKind.A6_Landscape,
            Imposition = ImpositionMode.None
        };

        [HttpGet("GeneratePaymentReceiptA6/{collectionId:guid}")]
        public IActionResult GeneratePaymentReceiptA6(Guid collectionId)
        {
            var opt = DefaultA6Landscape();

            var r = _reportService.GeneratePaymentReceiptA6(collectionId, opt);
            if (!r.Success || r.Data is null) return NotFound(r.Message);

            return File(r.Data, "application/pdf", $"payment-receipt-{collectionId}.pdf");
        }

        [HttpPost("GeneratePaymentReceiptsA6")]
        public IActionResult GeneratePaymentReceiptsA6([FromBody] GeneratePaymentReceiptsRequest req)
        {
            if (req?.CollectionIds is null || req.CollectionIds.Count == 0)
                return BadRequest("CollectionIds boş olamaz.");

            var opt = DefaultA6Landscape();

            var r = _reportService.GeneratePaymentReceiptsA6(req.CollectionIds, opt);
            if (!r.Success || r.Data is null) return NotFound(r.Message);

            return File(r.Data, "application/pdf", $"payment-receipts-{req.CollectionIds.Count}.pdf");
        }

        // Tekil: A6 Landscape (default)
        [HttpPost("contract-debt-a6/{contractId:guid}")]
        [AllowAnonymous]
        public IActionResult ContractDebtA6(Guid contractId, [FromBody] PdfRenderOptions? options)
        {
            options ??= DefaultA6Landscape();

            var r = _reportService.GenerateContractDebtStatusA6(contractId, options);
            if (!r.Success || r.Data is null) return NotFound(r.Message);

            return File(r.Data, "application/pdf", $"contract-debt-{contractId}.pdf");
        }

        // Çoğul: default da A6 Landscape (tekil ile aynı boyut)
        [HttpPost("contract-debt-a6/batch")]
        [AllowAnonymous]
        public IActionResult ContractDebtA6Batch([FromBody] GenerateContractDebtStatusRequest req)
        {
            if (req?.ContractIds is null || req.ContractIds.Count == 0)
                return BadRequest("ContractIds boş olamaz.");

            var options = req.Options ?? DefaultA6Landscape();

            var r = _reportService.GenerateContractDebtStatusesA6(req.ContractIds, options);
            if (!r.Success || r.Data is null) return NotFound(r.Message);

            return File(r.Data, "application/pdf", $"contract-debt-batch-{req.ContractIds.Count}.pdf");
        }

        // Tekil: A6 Landscape (default)
        [HttpPost("contract-period-a6/{contractId:guid}")]
        [AllowAnonymous]
        public IActionResult ContractPeriodA6(Guid contractId, [FromBody] GenerateContractPeriodStatementRequest req)
        {
            if (req is null)
                return BadRequest("Request boş olamaz.");

            var options = req.Options ?? DefaultA6Landscape();

            var r = _reportService.GenerateContractPeriodStatementA6(
                contractId,
                req.StartDate,
                req.EndDate,
                options,
                req.StampInfoText // null olabilir, sorun değil
            );

            if (!r.Success || r.Data is null) return NotFound(r.Message);

            return File(r.Data, "application/pdf", $"contract-period-{contractId}.pdf");
        }

        // Çoğul: default da A6 Landscape (tekil ile aynı boyut)
        [HttpPost("contract-period-a6/batch")]
        [AllowAnonymous]
        public IActionResult ContractPeriodA6Batch([FromBody] GenerateContractPeriodStatementRequest req)
        {
            if (req?.ContractIds is null || req.ContractIds.Count == 0)
                return BadRequest("ContractIds boş olamaz.");

            var options = req.Options ?? DefaultA6Landscape();

            var r = _reportService.GenerateContractPeriodStatementsA6(
                req.ContractIds,
                req.StartDate,
                req.EndDate,
                options,
                req.StampInfoText // null olabilir, sorun değil
            );

            if (!r.Success || r.Data is null) return NotFound(r.Message);

            return File(r.Data, "application/pdf", $"contract-period-batch-{req.ContractIds.Count}.pdf");
        }
    }
}



//using Autofac.Core;
//using Business.Abstract;
//using Business.Models.Reports;
//using Core.Utilities.Pdf.Abstractions;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;

//namespace WebAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public sealed class ReportsController : ControllerBase
//    {
//        private readonly IReportService _reportService;

//        public ReportsController(IReportService reports) => _reportService = reports;

//        [HttpGet("GeneratePaymentReceiptA6/{collectionId:guid}")]
//        public IActionResult GeneratePaymentReceiptA6(Guid collectionId)
//        {
//            var opt = new PdfRenderOptions
//            {
//                TargetPageKind = PdfPageKind.A6_Landscape,
//                Imposition = ImpositionMode.None
//            };

//            var r = _reportService.GeneratePaymentReceiptA6(collectionId, opt);
//            if (!r.Success || r.Data is null) return NotFound(r.Message);

//            return File(r.Data, "application/pdf", $"payment-receipt-{collectionId}.pdf");
//        }


//        [HttpPost("GeneratePaymentReceiptsA6")]
//        public IActionResult GeneratePaymentReceiptsA6([FromBody] GeneratePaymentReceiptsRequest req)
//        {

//            var opt = new PdfRenderOptions
//            {
//                TargetPageKind = PdfPageKind.A6_Landscape,
//                Imposition = ImpositionMode.None
//            };

//            var r = _reportService.GeneratePaymentReceiptsA6(req.CollectionIds, opt);
//            if (!r.Success || r.Data is null) return NotFound(r.Message);

//            return File(r.Data, "application/pdf", "payment-receipts.pdf");
//        }

//        // Tekil: default A6 tek sayfa
//        [HttpPost("contract-debt-a6/{contractId:guid}")]
//        [AllowAnonymous]
//        public IActionResult ContractDebtA6(Guid contractId, [FromBody] PdfRenderOptions? options)
//        {
//            options ??= new PdfRenderOptions
//            {
//                TargetPageKind = PdfPageKind.A6_Landscape,
//                Imposition = ImpositionMode.None
//            };

//            var r = _reportService.GenerateContractDebtStatusA6(contractId, options);
//            if (!r.Success) return NotFound(r.Message);

//            return File(r.Data!, "application/pdf", $"contract-debt-{contractId}.pdf");
//        }

//        // Çoğul: default hedef A4 Landscape + N-Up (çoklu A6 basım)
//        [HttpPost("contract-debt-a6/batch")]
//        [AllowAnonymous]
//        public IActionResult ContractDebtA6Batch([FromBody] GenerateContractDebtStatusRequest req)
//        {
//            if (req?.ContractIds is null || req.ContractIds.Count == 0)
//                return BadRequest("ContractIds boş olamaz.");

//            //var options = req.Options ?? new PdfRenderOptions
//            //{
//            //    TargetPageKind = PdfPageKind.A4_Landscape,
//            //    Imposition = ImpositionMode.NUpNoScale,
//            //    MarginMm = 6f,
//            //    GapMm = 2f
//            //};

//            var options = req.Options ?? new PdfRenderOptions
//            {
//                TargetPageKind = PdfPageKind.A6_Landscape,
//                Imposition = ImpositionMode.None
//            };


//            var r = _reportService.GenerateContractDebtStatusesA6(req.ContractIds, options);
//            if (!r.Success) return NotFound(r.Message);

//            return File(r.Data!, "application/pdf", $"contract-debt-batch-{req.ContractIds.Count}.pdf");
//        }

//        [HttpPost("contract-period-a6/{contractId:guid}")]
//        [AllowAnonymous]
//        public IActionResult ContractPeriodA6(Guid contractId, [FromBody] GenerateContractPeriodStatementRequest req)
//        {
//            var options = req.Options ?? new PdfRenderOptions
//            {
//                TargetPageKind = PdfPageKind.A6_Landscape,
//                Imposition = ImpositionMode.None
//            };

//            var r = _reportService.GenerateContractPeriodStatementA6(contractId, req.StartDate, req.EndDate, options, req.StampInfoText);
//            if (!r.Success) return NotFound(r.Message);

//            return File(r.Data!, "application/pdf", $"contract-period-{contractId}.pdf");
//        }

//        [HttpPost("contract-period-a6/batch")]
//        [AllowAnonymous]
//        public IActionResult ContractPeriodA6Batch([FromBody] GenerateContractPeriodStatementRequest req)
//        {
//            if (req?.ContractIds is null || req.ContractIds.Count == 0)
//                return BadRequest("ContractIds boş olamaz.");

//            var options = req.Options ?? new PdfRenderOptions
//            {
//                TargetPageKind = PdfPageKind.A4_Landscape,
//                Imposition = ImpositionMode.NUpNoScale,
//                MarginMm = 6f,
//                GapMm = 2f
//            };

//            var r = _reportService.GenerateContractPeriodStatementsA6(req.ContractIds, req.StartDate, req.EndDate, options, req.StampInfoText);
//            if (!r.Success) return NotFound(r.Message);

//            return File(r.Data!, "application/pdf", $"contract-period-batch-{req.ContractIds.Count}.pdf");
//        }
//    }
//}
