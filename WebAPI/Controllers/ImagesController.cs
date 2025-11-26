using Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageStorageService _storage;

        public ImagesController(IImageStorageService storage) => _storage = storage;

        [HttpPost("save-if-dataurl")]
        public async Task<ActionResult<SaveImageResponse>> SaveIfDataUrl([FromBody] SaveImageRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.PhotoFolderUrl) || string.IsNullOrWhiteSpace(req.BaseFile))
                return BadRequest("PhotoFolderUrl ve BaseFile zorunlu.");

            var res = await _storage.SaveIfDataUrlAsync(req.PhotoFolderUrl, req.BaseFile, null, ct);

            return Ok(new SaveImageResponse
            {
                Saved = res.Saved,
                Path = res.RelativePath,
                FileName = res.FileName,
                Mime = res.Mime,
                Width = res.Width,
                Height = res.Height,
                SizeBytes = res.SizeBytes,
                Error = res.Error
            });
        }
    }
}
