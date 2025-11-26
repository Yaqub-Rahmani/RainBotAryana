namespace WebAPI.Models
{
    public class SaveImageResponse
    {
        public bool Saved { get; set; }
        public string? Path { get; set; }       // "uploads/owners/<guid>.png"
        public string? FileName { get; set; }   // "<guid>.png"
        public string? Mime { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public long? SizeBytes { get; set; }
        public string? Error { get; set; }
    }
}
