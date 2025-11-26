namespace ZkBridge
{
    public class DeviceAbstractions
    {
        public interface IDeviceReader : IAsyncDisposable
        {
            Task StartEnrollAsync(int attempts, Action<CaptureDto> onCapture, CancellationToken ct);
        }

        public record CaptureDto(string TemplateBase64, int? Quality, string DeviceSerial);

        public record StartEnrollMsg(string type, StartData data);
        public record StartData(int attempts);

        public record OutMsg(string type, CaptureWrap data);
        public record CaptureWrap(string templateBase64, int? quality, string deviceSerial);

    }
}

