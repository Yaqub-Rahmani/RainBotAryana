using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Business.Abstract;
using libzkfpcsharp;

namespace Business.Concrete
{
    public class FingerprintManager : IFingerprintService, IDisposable
    {
        private readonly object _lock = new();
        private IntPtr _dev = IntPtr.Zero;
        private IntPtr _db = IntPtr.Zero;

        private int _imgWidth;
        private int _imgHeight;
        private int _dpi;

        private byte[] _imgBuf = Array.Empty<byte>();      // Gri seviye ham görüntü
        private byte[] _tmplBuf = new byte[2048];          // Template buffer (ZK için tipik)
        private bool _opened;

        public FingerprintManager()
        {
            // Başlangıçta sessiz dene; cihaz yoksa _opened=false kalır (throw etmez)
            Init();
        }

        // ---------------- Init & Params ----------------
        // NOT: Artık throw ETMEZ; başarılıysa true, değilse false döner.
        private bool Init()
        {
            try
            {
                var retInit = zkfp2.Init();
                if (retInit != zkfp.ZKFP_ERR_OK && retInit != zkfp.ZKFP_ERR_ALREADY_INIT)
                {
                    _opened = false; _dev = IntPtr.Zero; _db = IntPtr.Zero;
                    return false;
                }

                var count = zkfp2.GetDeviceCount();
                if (count <= 0)
                {
                    _opened = false; _dev = IntPtr.Zero; _db = IntPtr.Zero;
                    return false;
                }

                _dev = zkfp2.OpenDevice(0);
                if (_dev == IntPtr.Zero)
                {
                    _opened = false; _db = IntPtr.Zero;
                    return false;
                }

                _db = zkfp2.DBInit();
                if (_db == IntPtr.Zero)
                {
                    _opened = false;
                    return false;
                }

                _imgWidth = GetParamIntOrDefault(_dev, 1, 0);   // width
                _imgHeight = GetParamIntOrDefault(_dev, 2, 0);   // height
                _dpi = GetParamIntOrDefault(_dev, 3, 96);  // bazı cihazlarda 0 → 96 kullan

                if (_imgWidth <= 0 || _imgHeight <= 0)
                {
                    _opened = false;
                    return false;
                }

                _imgBuf = new byte[_imgWidth * _imgHeight];
                _opened = true;
                return true;
            }
            catch
            {
                _opened = false; _dev = IntPtr.Zero; _db = IntPtr.Zero;
                return false;
            }
        }

        private static int GetParamIntOrDefault(IntPtr dev, int code, int @default)
        {
            try
            {
                byte[] param = new byte[4];
                int size = 4;
                var ret = zkfp2.GetParameters(dev, code, param, ref size);
                if (ret != zkfp.ZKFP_ERR_OK) return @default;
                return BitConverter.ToInt32(param, 0);
            }
            catch { return @default; }
        }

        // ---------------- Device Connectivity Helpers ----------------
        private bool IsDeviceConnected()
        {
            try
            {
                if (!_opened || _dev == IntPtr.Zero || _db == IntPtr.Zero) return false;
                var count = zkfp2.GetDeviceCount();
                return count > 0;
            }
            catch { return false; }
        }

        private bool EnsureDeviceConnected()
        {
            if (IsDeviceConnected()) return true;

            lock (_lock)
            {
                // temizle ve tekrar dene
                try
                {
                    InternalDisposeUnlocked();
                    try { zkfp2.Terminate(); } catch { /* ignore */ }
                }
                catch { /* ignore */ }

                _opened = false;
                return Init(); // throw etmez; başarılıysa true
            }
        }

        private void InternalDisposeUnlocked()
        {
            if (_db != IntPtr.Zero) { zkfp2.DBFree(_db); _db = IntPtr.Zero; }
            if (_dev != IntPtr.Zero) { zkfp2.CloseDevice(_dev); _dev = IntPtr.Zero; }
        }

        private static FingerprintResult DeviceNotConnectedResult() => new()
        {
            Success = false,
            Message = "Cihaz bağlı değil. Lütfen parmak izi cihazını bağlayınız."
        };

        private static EnrollResult DeviceNotConnectedEnroll() => new()
        {
            Success = false,
            Message = "Cihaz bağlı değil. Lütfen parmak izi cihazını bağlayınız.",
            CollectedSampleCount = 0
        };

        private static MatchResult DeviceNotConnectedMatch() => new()
        {
            Success = false,
            Message = "Cihaz bağlı değil. Lütfen parmak izi cihazını bağlayınız.",
            Score = 0
        };

        private static IdentifyResult DeviceNotConnectedIdentify() => new()
        {
            Success = false,
            Message = "Cihaz bağlı değil. Lütfen parmak izi cihazını bağlayınız.",
            MatchedUserId = -1,
            Score = 0
        };

        // ---------------- Public: Status ----------------
        public (bool opened, int width, int height, int dpi, int deviceCount) GetStatus()
        {
            int count = 0;
            try { count = zkfp2.GetDeviceCount(); } catch { /* ignore */ }
            return (IsDeviceConnected(), _imgWidth, _imgHeight, _dpi, count);
        }

        // ---------------- Public: Scan ----------------
        public async Task<FingerprintResult> ScanOnceAsync(bool includeImage = false, CancellationToken ct = default)
        {
            if (!EnsureDeviceConnected()) return DeviceNotConnectedResult();

            var (ok, capTpl, tplSize, err) = await CaptureTemplateAsync(timeoutMs: 12000, ct);
            if (!ok || tplSize <= 0)
                return new FingerprintResult { Success = false, Message = err ?? "No finger detected or template empty." };

            var res = new FingerprintResult
            {
                Success = true,
                Message = "OK",
                TemplateBase64 = Convert.ToBase64String(capTpl, 0, tplSize),
                DeviceId = "ZK9500",
                SerialNumber = "1985240901743"
            };

            if (includeImage)
            {
                res.ImagePngBase64 = TryMakePngBase64();
            }

            return res;
        }

        // ---------------- Public: Enroll ----------------
        public async Task<EnrollResult> EnrollAsync(
            int userId,
            int requiredSamples = 3,
            int timeoutMs = 20000,
            bool includeImage = false,
            CancellationToken ct = default)
        {
            if (!EnsureDeviceConnected()) return DeviceNotConnectedEnroll();

            if (requiredSamples < 3) requiredSamples = 3;

            byte[][] samples = new byte[requiredSamples][];
            int captured = 0;
            string lastPng = null;
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

            while (DateTime.UtcNow < deadline && captured < requiredSamples && !ct.IsCancellationRequested)
            {
                var (ok, capTpl, tplSize, _) = await CaptureTemplateAsync(timeoutMs: 4000, ct);
                if (!ok || tplSize <= 0) continue;

                // Aynı parmak tutarlılığı (önceki sample ile Match > 0)
                if (captured > 0)
                {
                    int score;
                    lock (_lock)
                    {
                        score = zkfp2.DBMatch(_db, capTpl, samples[captured - 1]);
                    }
                    if (score <= 0) continue; // farklı parmak olabilir, örneği yoksay
                }

                var store = new byte[tplSize];
                Buffer.BlockCopy(capTpl, 0, store, 0, tplSize);
                samples[captured++] = store;

                if (includeImage)
                    lastPng = TryMakePngBase64();
            }

            if (captured < requiredSamples)
                return new EnrollResult { Success = false, Message = "Yeterli örnek alınamadı.", CollectedSampleCount = captured };

            // Merge
            var regTmp = new byte[2048];
            int regSize = regTmp.Length;
            int retMerge;
            lock (_lock)
            {
                retMerge = zkfp2.DBMerge(_db, samples[0], samples[1], samples[2], regTmp, ref regSize);
            }
            if (retMerge != zkfp.ZKFP_ERR_OK)
                return new EnrollResult { Success = false, Message = $"DBMerge hata: {retMerge}", CollectedSampleCount = captured };

            // RAM DB’ye ekle
            int retAdd;
            lock (_lock)
            {
                retAdd = zkfp2.DBAdd(_db, userId, regTmp);
            }
            if (retAdd != zkfp.ZKFP_ERR_OK)
                return new EnrollResult { Success = false, Message = $"DBAdd hata: {retAdd}", CollectedSampleCount = captured };

            return new EnrollResult
            {
                Success = true,
                Message = "Kayıt başarılı.",
                MergedTemplateBase64 = Convert.ToBase64String(regTmp, 0, regSize),
                CollectedSampleCount = captured,
                LastSampleImagePngBase64 = includeImage ? lastPng : null
            };
        }

        // ---------------- Public: Verify (1:1) ----------------
        public async Task<MatchResult> VerifyAsync(
            string enrolledTemplateBase64,
            int timeoutMs = 8000,
            bool includeImage = false,
            CancellationToken ct = default)
        {
            if (!EnsureDeviceConnected()) return DeviceNotConnectedMatch();

            if (string.IsNullOrWhiteSpace(enrolledTemplateBase64))
                return MatchResult.Fail("Kayıtlı template boş.");

            byte[] enrolled;
            try { enrolled = Convert.FromBase64String(enrolledTemplateBase64); }
            catch { return MatchResult.Fail("Template Base64 formatı geçersiz."); }

            var (ok, capTpl, tplSize, err) = await CaptureTemplateAsync(timeoutMs, ct);
            if (!ok || tplSize <= 0)
                return MatchResult.Fail(err ?? "Yakalama başarısız.");

            int score;
            lock (_lock)
            {
                score = zkfp2.DBMatch(_db, capTpl, enrolled);
            }

            return new MatchResult
            {
                Success = score > 0,
                Score = score,
                Message = score > 0 ? "Doğrulama başarılı." : "Eşleşme yok.",
                CapturedImagePngBase64 = includeImage ? TryMakePngBase64() : null
            };
        }

        // ---------------- Public: Identify (1:N) ----------------
        public async Task<IdentifyResult> IdentifyAsync(
            int timeoutMs = 8000,
            bool includeImage = false,
            CancellationToken ct = default)
        {
            if (!EnsureDeviceConnected()) return DeviceNotConnectedIdentify();

            var (ok, capTpl, tplSize, err) = await CaptureTemplateAsync(timeoutMs, ct);
            if (!ok || tplSize <= 0)
                return IdentifyResult.Fail(err ?? "Yakalama başarısız.");

            int fid = 0, score = 0, ret;
            lock (_lock)
            {
                ret = zkfp2.DBIdentify(_db, capTpl, ref fid, ref score);
            }

            return new IdentifyResult
            {
                Success = ret == zkfp.ZKFP_ERR_OK,
                MatchedUserId = ret == zkfp.ZKFP_ERR_OK ? fid : -1,
                Score = score,
                Message = ret == zkfp.ZKFP_ERR_OK ? "Kimlik tespiti başarılı." : $"Kimlik tespiti başarısız, ret={ret}",
                CapturedImagePngBase64 = includeImage ? TryMakePngBase64() : null
            };
        }

        // ---------------- Public: DB Management ----------------
        public void AddTemplateToDb(int userId, string templateBase64)
        {
            if (!EnsureDeviceConnected())
                throw new InvalidOperationException("Cihaz bağlı değil. Lütfen cihazı bağlayınız.");

            if (string.IsNullOrWhiteSpace(templateBase64))
                throw new ArgumentException("templateBase64 boş.");

            byte[] tpl = Convert.FromBase64String(templateBase64);
            int ret;
            lock (_lock)
            {
                ret = zkfp2.DBAdd(_db, userId, tpl);
            }
            if (ret != zkfp.ZKFP_ERR_OK)
                throw new Exception($"DBAdd failed: {ret}");
        }

        public void ClearDb()
        {
            if (!EnsureDeviceConnected())
                throw new InvalidOperationException("Cihaz bağlı değil. Lütfen cihazı bağlayınız.");

            lock (_lock)
            {
                if (_db != IntPtr.Zero) { zkfp2.DBFree(_db); _db = IntPtr.Zero; }
                _db = zkfp2.DBInit();
                if (_db == IntPtr.Zero)
                    throw new Exception("DBInit failed during ClearDb.");
            }
        }

        // ---------------- Helpers ----------------
        private async Task<(bool ok, byte[] tpl, int size, string err)> CaptureTemplateAsync(int timeoutMs, CancellationToken ct)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            int size;

            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                int ret;
                lock (_lock)
                {
                    size = _tmplBuf.Length;
                    ret = zkfp2.AcquireFingerprint(_dev, _imgBuf, _tmplBuf, ref size);
                }

                if (ret == zkfp.ZKFP_ERR_OK && size > 0)
                {
                    var outBuf = new byte[size];
                    Buffer.BlockCopy(_tmplBuf, 0, outBuf, 0, size);
                    return (true, outBuf, size, null);
                }

                try { await Task.Delay(120, ct).ConfigureAwait(false); }
                catch (TaskCanceledException) { break; }
            }

            return (false, Array.Empty<byte>(), 0, ct.IsCancellationRequested ? "İptal edildi." : "Zaman aşımı / parmak algılanmadı.");
        }

        private string TryMakePngBase64()
        {
            try
            {
                using var bmp = Create8bppGrayscaleBitmap(_imgBuf, _imgWidth, _imgHeight);
                using var ms = new MemoryStream();
                bmp.SetResolution(_dpi > 0 ? _dpi : 96, _dpi > 0 ? _dpi : 96);
                bmp.Save(ms, ImageFormat.Png);
                return Convert.ToBase64String(ms.ToArray());
            }
            catch
            {
                return null; // Görsel zorunlu değil; hata durumunda null bırak
            }
        }

        // 8bpp indexed gri paletli Bitmap oluşturur ve _imgBuf’u stride dikkate alarak kopyalar
        private static Bitmap Create8bppGrayscaleBitmap(byte[] src, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Gri palet
            var pal = bmp.Palette;
            for (int i = 0; i < 256; i++) pal.Entries[i] = Color.FromArgb(i, i, i);
            bmp.Palette = pal;

            var rect = new Rectangle(0, 0, width, height);
            var data = bmp.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            try
            {
                int stride = data.Stride;
                IntPtr dest = data.Scan0;
                int srcOffset = 0;

                // Negatif stride (top-down) ise başlangıcı son satıra çekip stride'ı pozitif gibi ilerlet
                if (stride < 0)
                {
                    dest = AddIntPtr(dest, stride * (height - 1));
                    stride = -stride;
                }

                for (int y = 0; y < height; y++)
                {
                    // width = satırdaki gerçek byte sayısı (8bpp)
                    Marshal.Copy(src, srcOffset, dest, width);
                    dest = AddIntPtr(dest, stride);
                    srcOffset += width;
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return bmp;
        }

        private static IntPtr AddIntPtr(IntPtr ptr, int offset)
        {
            unchecked { return new IntPtr(ptr.ToInt64() + offset); }
        }

        // ---------------- Dispose ----------------
        public void Dispose()
        {
            lock (_lock)
            {
                InternalDisposeUnlocked();
                try { zkfp2.Terminate(); } catch { /* ignore */ }
                _opened = false;
            }
            GC.SuppressFinalize(this);
        }

        // ---------------- Ek: Template Karşılaştırma / Merge ----------------
        public async Task<(bool Success, int Score, string Message)> CompareTemplatesAsync(
            string templateABase64,
            string templateBBase64,
            CancellationToken ct = default)
        {
            if (!EnsureDeviceConnected())
                return (false, 0, "Cihaz bağlı değil. Lütfen parmak izi cihazını bağlayınız.");

            if (string.IsNullOrWhiteSpace(templateABase64) || string.IsNullOrWhiteSpace(templateBBase64))
                return (false, 0, "TemplateA/TemplateB boş.");

            byte[] a, b;
            try { a = Convert.FromBase64String(templateABase64); }
            catch { return (false, 0, "TemplateA Base64 geçersiz."); }
            try { b = Convert.FromBase64String(templateBBase64); }
            catch { return (false, 0, "TemplateB Base64 geçersiz."); }

            int score;
            lock (_lock)
            {
                score = zkfp2.DBMatch(_db, a, b);
            }

            return (true, score, "OK");
        }

        public async Task<(bool Success, string MergedTemplateBase64, string Message)> MergeTemplatesAsync(
            string[] templatesBase64,
            CancellationToken ct = default)
        {
            if (!EnsureDeviceConnected())
                return (false, null, "Cihaz bağlı değil. Lütfen parmak izi cihazını bağlayınız.");

            if (templatesBase64 == null || templatesBase64.Length < 3)
                return (false, null, "En az 3 örnek gerekli.");

            // ZK DBMerge tam 3 örnek ister → ilk 3'ü al
            byte[][] src = new byte[3][];
            for (int i = 0; i < 3; i++)
            {
                try { src[i] = Convert.FromBase64String(templatesBase64[i]); }
                catch { return (false, null, $"Template[{i}] Base64 geçersiz."); }
                if (src[i] == null || src[i].Length == 0)
                    return (false, null, $"Template[{i}] boş.");
            }

            var regTmp = new byte[2048];
            int regSize = regTmp.Length;
            int ret;
            lock (_lock)
            {
                ret = zkfp2.DBMerge(_db, src[0], src[1], src[2], regTmp, ref regSize);
            }
            if (ret != zkfp.ZKFP_ERR_OK)
                return (false, null, $"DBMerge hata: {ret}");

            var base64 = Convert.ToBase64String(regTmp, 0, regSize);
            return (true, base64, "OK");
        }
    }
}



//using Business.Abstract;
//using libzkfpcsharp;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Business.Concrete
//{
//    public class FingerprintManager: IFingerprintService, IDisposable
//    {
//        private readonly object _lock = new();
//        private IntPtr _dev = IntPtr.Zero;
//        private IntPtr _db = IntPtr.Zero;

//        private int _imgWidth;
//        private int _imgHeight;
//        private int _dpi;

//        private byte[] _imgBuf;               // görüntü buffer
//        private byte[] _tmplBuf = new byte[2048]; // template buffer (ZK için genelde 2048 yeter)
//        private int _tmplSize = 2048;

//        private bool _initialized;
//        private bool _opened;

//        public FingerprintManager()
//        {
//            Init();
//        }

//        private void Init()
//        {
//            // 1) SDK init
//            var ret = zkfp2.Init();
//            if (ret != zkfp.ZKFP_ERR_OK && ret != zkfp.ZKFP_ERR_ALREADY_INIT)
//                throw new Exception($"ZK SDK init error: {ret}");

//            // 2) Cihaz sayısı
//            var count = zkfp2.GetDeviceCount();
//            if (count <= 0)
//                throw new Exception("No fingerprint device found.");

//            // 3) İlk cihazı aç
//            _dev = zkfp2.OpenDevice(0);
//            if (_dev == IntPtr.Zero)
//                throw new Exception("OpenDevice failed.");

//            // 4) DB (template işlemleri) init
//            _db = zkfp2.DBInit();
//            if (_db == IntPtr.Zero)
//                throw new Exception("DBInit failed.");

//            // 5) Parametreler: width, height, dpi
//            //_imgWidth = GetParamInt(zkfp.ZKFP_IMAGE_WIDTH);
//            //_imgHeight = GetParamInt(zkfp.ZKFP_IMAGE_HEIGHT);
//            //_dpi = GetParamInt(zkfp.ZKFP_IMAGE_DPI);

//            byte[] param = new byte[4];
//            int size = 4;

//            // Width
//            zkfp2.GetParameters(_dev, 1, param, ref size);
//            _imgWidth = BitConverter.ToInt32(param, 0);

//            // Height
//            zkfp2.GetParameters(_dev, 2, param, ref size);
//            _imgHeight = BitConverter.ToInt32(param, 0);

//            // DPI
//            zkfp2.GetParameters(_dev, 3, param, ref size);
//            _dpi = BitConverter.ToInt32(param, 0);


//            _imgBuf = new byte[_imgWidth * _imgHeight];

//            _initialized = true;
//            _opened = true;
//        }

//        private int GetParamInt(int code)
//        {
//            byte[] param = new byte[4];
//            int size = 4;
//            var ret = zkfp2.GetParameters(_dev, code, param, ref size);
//            if (ret != zkfp.ZKFP_ERR_OK) throw new Exception($"GetParameters({code}) failed: {ret}");
//            return BitConverter.ToInt32(param, 0);
//        }

//        public async Task<FingerprintResult> ScanOnceAsync()
//        {
//            // EnsureInitialized(); // kurucuda Init() yapıyorsan KALDIR

//            var timeoutMs = 12000;
//            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

//            while (DateTime.UtcNow < deadline)
//            {
//                int ret;
//                int tmplSizeLocal;

//                lock (_lock)
//                {
//                    tmplSizeLocal = _tmplBuf.Length; // her denemede sıfırla
//                    ret = zkfp2.AcquireFingerprint(_dev, _imgBuf, _tmplBuf, ref tmplSizeLocal);
//                    _tmplSize = tmplSizeLocal;
//                }

//                if (ret == zkfp.ZKFP_ERR_OK && _tmplSize > 0)
//                {
//                    var base64 = Convert.ToBase64String(_tmplBuf, 0, _tmplSize);
//                    return new FingerprintResult
//                    {
//                        Success = true,
//                        Message = "OK",
//                        TemplateBase64 = base64,
//                        DeviceId = "ZK9500",
//                        SerialNumber = "1985240901743"
//                    };
//                }

//                await Task.Delay(120);
//            }

//            return new FingerprintResult { Success = false, Message = "No finger detected or template empty." };
//        }


//        // Tanı amacıyla
//        public (bool opened, int width, int height, int dpi, int deviceCount) GetStatus()
//        {
//            int count = 0;
//            try { count = zkfp2.GetDeviceCount(); } catch { /* ignore */ }
//            return (_opened, _imgWidth, _imgHeight, _dpi, count);
//        }

//        public void Dispose()
//        {
//            if (_db != IntPtr.Zero) { zkfp2.DBFree(_db); _db = IntPtr.Zero; }
//            if (_dev != IntPtr.Zero) { zkfp2.CloseDevice(_dev); _dev = IntPtr.Zero; }
//            zkfp2.Terminate();
//            _opened = false;
//            _initialized = false;
//        }
//    }
//}
