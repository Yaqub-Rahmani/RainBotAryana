
namespace ZkBridge.Services
{
    public sealed class ZkScanner
    {
        private bool _initialized;
        private string _sn = "1985240901743";

        public ZkScanner()
        {
            // TODO: SDK init: zkfp.OpenDevice / Init lib etc.
            _initialized = true;
        }

        public IEnumerable<(string sn, string model)> ListDevices()
            => new[] { (_sn, "ZKTeco ZK9500") }; // TODO: gerçek liste

        public (string templateBase64, string? imageBase64, int quality) Capture(int quality, int timeoutMs, bool withImage)
        {
            if (!_initialized) throw new InvalidOperationException("Scanner not initialized");

            // TODO: SDK ile gerçek yakalama (template byte[] al)
            // DEMO: sahte bir base64 template üretelim
            var fakeBytes = Guid.NewGuid().ToByteArray();
            var tpl = Convert.ToBase64String(fakeBytes);
            return (tpl, null, Math.Max(quality, 70));
        }
    }

}
