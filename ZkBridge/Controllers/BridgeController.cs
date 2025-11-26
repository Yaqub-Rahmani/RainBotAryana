using Microsoft.AspNetCore.Mvc;
using ZkBridge.Models;
using ZkBridge.Services;

namespace ZkBridge.Controllers
{
    [ApiController]
    [Route("api")]
    public sealed class BridgeController : ControllerBase
    {
        private readonly ZkScanner _scanner;
        public BridgeController(ZkScanner scanner) => _scanner = scanner;

        [HttpGet("devices")]
        public IActionResult Devices() => Ok(_scanner.ListDevices().Select(d => new { serialNumber = d.sn, model = d.model }));

        [HttpPost("capture")]
        public IActionResult Capture([FromBody] CaptureRequest req)
        {
            var (tpl, img, q) = _scanner.Capture(req.Quality, req.TimeoutMs, req.WithImage);
            return Ok(new CaptureResponse { DeviceSn = req.SerialNumber ?? "1985240901743", TemplateBase64 = tpl, ImageBase64 = img, Quality = q });
        }
    }
}
