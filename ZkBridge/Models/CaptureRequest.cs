namespace ZkBridge.Models
{
    public sealed class CaptureRequest
    {
        public int Quality { get; set; } = 80;
        public int TimeoutMs { get; set; } = 10000;
        public bool WithImage { get; set; } = false;
        public string? SerialNumber { get; set; } // 1985240901743
    }
}
