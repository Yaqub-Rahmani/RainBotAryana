using libzkfpcsharp;
using static ZkBridge.DeviceAbstractions;
using System.Text;

namespace ZkBridge
{
    public sealed class ZkDeviceReader : IDeviceReader
    {
        private IntPtr _dev = IntPtr.Zero;
        private IntPtr _db = IntPtr.Zero;
        private int _w, _h;
        private string _sn = "";

        // Sadece senin cihazınla çalışsın istersen bunu set et:
        private const string AllowedSerial = "1985240901743"; // senin verdiğin SN

        public ZkDeviceReader()
        {
            // 1) SDK init
            var r = zkfp2.Init();
            if (r != zkfp.ZKFP_ERR_OK && r != zkfp.ZKFP_ERR_ALREADY_INIT)
                throw new InvalidOperationException($"zkfp2.Init failed: {r}");

            // 2) Cihaz sayısı
            var count = zkfp2.GetDeviceCount();
            if (count <= 0) throw new InvalidOperationException("ZKTeco cihaz bulunamadı (GetDeviceCount<=0).");

            // 3) İlk cihazı aç
            _dev = zkfp2.OpenDevice(0);
            if (_dev == IntPtr.Zero) throw new InvalidOperationException("OpenDevice(0) başarısız.");

            // 4) DB init (template/feature işlemleri için)
            _db = zkfp2.DBInit();
            if (_db == IntPtr.Zero) throw new InvalidOperationException("DBInit başarısız.");

            // 5) Görüntü genişlik/yükseklik (param code 1 & 2)
            _w = GetIntParam(1);
            _h = GetIntParam(2);
            if (_w <= 0 || _h <= 0) throw new InvalidOperationException("Sensör görüntü boyutu alınamadı (param 1/2).");

            // 6) Seri numarası (param code 1103)
            _sn = GetStringParam(1103) ?? "";
            Console.WriteLine($"[ZKTeco] Device SN: {_sn}");

            // (opsiyonel) Tek cihaz kilidi: yalnızca AllowedSerial çalışsın
            if (!string.IsNullOrWhiteSpace(AllowedSerial) && !string.IsNullOrWhiteSpace(_sn))
            {
                if (!_sn.Contains(AllowedSerial))
                    throw new InvalidOperationException($"Beklenen SN ({AllowedSerial}) ile eşleşmedi: {_sn}");
            }
        }

        public async Task StartEnrollAsync(int attempts, Action<CaptureDto> onCapture, CancellationToken ct)
        {
            var img = new byte[_w * _h];
            var tpl = new byte[2048];

            for (int i = 0; i < attempts && !ct.IsCancellationRequested; i++)
            {
                int len = tpl.Length;
                int rc;
                // Parmağı bekle: rc==0 (OK) olana kadar dene
                do
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(30, ct); // busy-wait azalt
                    rc = zkfp2.AcquireFingerprint(_dev, img, tpl, ref len);
                } while (rc != zkfp.ZKFP_ERR_OK);

                // Template'i Base64'e çevir
                var b64 = Convert.ToBase64String(tpl, 0, len);

                // Kaliteyi SDK direkt vermezse null geçiyoruz (gerekirse img'ten hesaplanabilir)
                onCapture(new CaptureDto(b64, null, string.IsNullOrWhiteSpace(_sn) ? "ZKTeco" : _sn));
            }
        }

        private int GetIntParam(int code)
        {
            var buf = new byte[4];
            int size = 4;
            var rc = zkfp2.GetParameters(_dev, code, buf, ref size);
            if (rc != zkfp.ZKFP_ERR_OK) return 0;
            int val = 0;
            zkfp2.ByteArray2Int(buf, ref val);
            return val;
        }

        private string GetStringParam(int code)
        {
            var buf = new byte[256];
            int size = buf.Length;
            var rc = zkfp2.GetParameters(_dev, code, buf, ref size);
            if (rc != zkfp.ZKFP_ERR_OK || size <= 0) return "";
            return Encoding.ASCII.GetString(buf, 0, size).TrimEnd('\0');
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_db != IntPtr.Zero) zkfp2.DBFree(_db);
                if (_dev != IntPtr.Zero) zkfp2.CloseDevice(_dev);
                zkfp2.Terminate();
            }
            catch { /* yut */ }
            finally
            {
                _db = IntPtr.Zero;
                _dev = IntPtr.Zero;
            }
            await Task.CompletedTask;
        }
    }
}
