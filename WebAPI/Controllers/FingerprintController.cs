using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FingerprintController : ControllerBase
    {
        private readonly IFingerprintService _svc;

        public FingerprintController(IFingerprintService svc) => _svc = svc;

        // ----------- Request DTO'ları -----------
        public record ScanRequest([Range(1000, 60000)] int TimeoutMs = 12000, bool IncludeImage = false);
        public record EnrollRequest([Required] int UserId, [Range(3, 5)] int RequiredSamples = 3, [Range(3000, 60000)] int TimeoutMs = 20000, bool IncludeImage = false);
        public record VerifyRequest([Required] string EnrolledTemplateBase64, [Range(2000, 60000)] int TimeoutMs = 8000, bool IncludeImage = false);
        public record IdentifyRequest([Range(2000, 60000)] int TimeoutMs = 8000, bool IncludeImage = false);
        public record AddTemplateRequest([Required] int UserId, [Required] string TemplateBase64);

        // ----------- Endpoints -----------

        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] ScanRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var result = await _svc.ScanOnceAsync(req.IncludeImage, ct);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                templateBase64 = result.TemplateBase64,
                imagePngBase64 = result.ImagePngBase64,
                deviceId = result.DeviceId,
                serialNumber = result.SerialNumber
            });
        }

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] EnrollRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var res = await _svc.EnrollAsync(req.UserId, req.RequiredSamples, req.TimeoutMs, req.IncludeImage, ct);

            if (!res.Success)
                return BadRequest(new { message = res.Message, collectedSampleCount = res.CollectedSampleCount });

            return Ok(new
            {
                message = res.Message,
                mergedTemplateBase64 = res.MergedTemplateBase64,
                collectedSampleCount = res.CollectedSampleCount,
                lastSampleImagePngBase64 = res.LastSampleImagePngBase64
            });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var res = await _svc.VerifyAsync(req.EnrolledTemplateBase64, req.TimeoutMs, req.IncludeImage, ct);

            if (!res.Success)
                return Unauthorized(new { message = res.Message, score = res.Score, capturedImagePngBase64 = res.CapturedImagePngBase64 });

            return Ok(new { message = res.Message, score = res.Score, capturedImagePngBase64 = res.CapturedImagePngBase64 });
        }

        [HttpPost("identify")]
        public async Task<IActionResult> Identify([FromBody] IdentifyRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var res = await _svc.IdentifyAsync(req.TimeoutMs, req.IncludeImage, ct);

            if (!res.Success)
                return NotFound(new { message = res.Message, capturedImagePngBase64 = res.CapturedImagePngBase64 });

            return Ok(new
            {
                message = res.Message,
                matchedUserId = res.MatchedUserId,
                score = res.Score,
                capturedImagePngBase64 = res.CapturedImagePngBase64
            });
        }

        [HttpPost("db/add")]
        public IActionResult AddTemplate([FromBody] AddTemplateRequest req)
        {
            _svc.AddTemplateToDb(req.UserId, req.TemplateBase64);
            return Ok(new { message = "Template RAM/SDK DB’ye eklendi.", userId = req.UserId });
        }

        [HttpPost("db/clear")]
        public IActionResult ClearDb()
        {
            _svc.ClearDb();
            return Ok(new { message = "SDK DB temizlendi." });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var s = _svc.GetStatus();
            return Ok(new
            {
                deviceOpened = s.opened, // IsDeviceConnected() sonucu
                imageWidth = s.width,
                imageHeight = s.height,
                dpi = s.dpi,
                deviceCount = s.deviceCount
            });
        }

        // ----------- Template Araçları -----------
        public record CompareTemplatesRequest([Required] string TemplateA, [Required] string TemplateB);
        public record MergeTemplatesRequest([Required] string[] TemplatesBase64);
        private const int SAME_FINGER_THRESHOLD = 0;

        [HttpPost("templates/compare")]
        public async Task<IActionResult> CompareTemplates([FromBody] CompareTemplatesRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var (ok, score, msg) = await _svc.CompareTemplatesAsync(req.TemplateA, req.TemplateB, ct);
            if (!ok) return BadRequest(new { message = msg });

            return Ok(new
            {
                message = "OK",
                score,
                isSameFinger = score > SAME_FINGER_THRESHOLD
            });
        }

        [HttpPost("templates/merge")]
        public async Task<IActionResult> MergeTemplates([FromBody] MergeTemplatesRequest req)
        {
            var ct = HttpContext.RequestAborted;
            var (ok, merged, msg) = await _svc.MergeTemplatesAsync(req.TemplatesBase64, ct);
            if (!ok) return BadRequest(new { message = msg });

            return Ok(new { message = "OK", templateBase64 = merged });
        }
    }
}



//using Business.Abstract;
//using Microsoft.AspNetCore.Mvc;

//namespace WebAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class FingerprintController : ControllerBase
//    {
//        private readonly IFingerprintService _svc;

//        public FingerprintController(IFingerprintService svc) => _svc = svc;

//        // Swagger’dan tarama: Parmağını cihaza koyup Execute'a bas.
//        [HttpPost("scan")]
//        public async Task<IActionResult> Scan()
//        {
//            var result = await _svc.ScanOnceAsync();
//            if (!result.Success) return BadRequest(new { message = result.Message });
//            return Ok(new
//            {
//                message = result.Message,
//                templateBase64 = result.TemplateBase64,
//                deviceId = result.DeviceId,
//                serialNumber = result.SerialNumber
//            });
//        }

//        // Tanı için küçük bir endpoint
//        [HttpGet("status")]
//        public IActionResult Status()
//        {
//            var s = _svc.GetStatus();
//            return Ok(new
//            {
//                deviceOpened = s.opened,
//                imageWidth = s.width,
//                imageHeight = s.height,
//                dpi = s.dpi,
//                deviceCount = s.deviceCount
//            });
//        }
//    }
//}