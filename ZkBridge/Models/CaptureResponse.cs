namespace ZkBridge.Models
{
    public sealed class CaptureResponse
    {
        public string DeviceSn { get; set; } = "";
        public string TemplateBase64 { get; set; } = "";
        public string? ImageBase64 { get; set; }
        public int Quality { get; set; }
    }
}
