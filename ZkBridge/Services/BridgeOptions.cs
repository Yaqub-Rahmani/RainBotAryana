namespace ZkBridge.Services
{
    public sealed class BridgeOptions
    {
        public int Port { get; set; } = 8730;
        public int DefaultQuality { get; set; } = 80;
        public int DefaultTimeoutMs { get; set; } = 10000;
    }
}
