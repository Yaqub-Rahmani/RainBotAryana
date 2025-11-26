using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public sealed class FileUploadForm
    {
        [Required]
        public string FolderUrl { get; set; } = default!;

        [Required]
        public IFormFile File { get; set; } = default!;
    }
}
