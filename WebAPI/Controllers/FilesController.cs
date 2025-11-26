using Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _files;

        public FilesController(IFileStorageService files) => _files = files;

        /// <summary>Genel dosya yükleme (multipart/form-data)</summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(25 * 1024 * 1024)] // 25MB: örnek üst sınır (Kestrel/IIS ile uyumlu tut)
        [ProducesResponseType(typeof(FileSaveResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] FileUploadForm form, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (form.File is null || form.File.Length == 0)
                return BadRequest("File is empty.");

            var opts = new FileSaveOptions(
                MaxBytes: 10 * 1024 * 1024,
                AllowedMime: new[] { "application/pdf", "image/png", "image/jpeg", "image/webp" },
                AllowedExtensions: new[] { ".pdf", ".png", ".jpg", ".jpeg", ".webp" }
            );

            var res = await _files.SaveAsync(form.FolderUrl, form.File, opts, ct);
            if (!res.Saved) return BadRequest(res);

            return Ok(res);
        }
    }
}
