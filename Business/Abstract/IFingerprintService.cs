using System;
using System.Threading;
using System.Threading.Tasks;

namespace Business.Abstract
{
    // -------- Temel Sonuçlar --------
    public class FingerprintResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        // Biyometrik template (eşleştirme için gereklidir)
        public string TemplateBase64 { get; set; }

        // Opsiyonel: Görsel önizleme için yakalanan parmak izinin PNG Base64 temsili
        // includeImage=true gönderildiğinde doldurulur.
        public string ImagePngBase64 { get; set; }

        public string DeviceId { get; set; }
        public string SerialNumber { get; set; }
    }

    // Enroll (kayıt) sonucu
    public class EnrollResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        // DBAdd öncesi/sırası için birleşik template (Base64). Kalıcı DB’de saklanabilir.
        public string MergedTemplateBase64 { get; set; }

        // Toplanan örnek sayısı (telemetri/UX için)
        public int CollectedSampleCount { get; set; }

        // Opsiyonel: Son toplanan örneğin PNG Base64 görüntüsü (önizleme amaçlı)
        public string LastSampleImagePngBase64 { get; set; }
    }

    // 1:1 doğrulama sonucu (DBMatch)
    public class MatchResult
    {
        public bool Success { get; set; }
        public int Score { get; set; }
        public string Message { get; set; }

        // Opsiyonel: Bu doğrulamada yakalanan son görüntü (PNG Base64)
        public string CapturedImagePngBase64 { get; set; }

        public static MatchResult Fail(string msg = null) =>
            new MatchResult { Success = false, Message = msg ?? "Fail" };
    }

    // 1:N kimlik tespiti sonucu (DBIdentify)
    public class IdentifyResult
    {
        public bool Success { get; set; }
        public int MatchedUserId { get; set; } // DBAdd sırasında verdiğin kimlik (fid)
        public int Score { get; set; }
        public string Message { get; set; }

        // Opsiyonel: Bu kimlik tespitinde yakalanan son görüntü (PNG Base64)
        public string CapturedImagePngBase64 { get; set; }

        public static IdentifyResult Fail(string msg = null) =>
            new IdentifyResult { Success = false, MatchedUserId = -1, Message = msg ?? "Fail" };
    }

    // -------- Servis Sözleşmesi --------
    // Not: IDisposable’ı interface’e ekliyoruz ki yaşam döngüsü kontrolü garanti olsun.
    public interface IFingerprintService : IDisposable
    {
        // Mevcut metodlar (geri uyumlu) + PNG talebi:
        // includeImage=true ise sonuç DTO’larındaki PNG Base64 alanları doldurulur.
        Task<FingerprintResult> ScanOnceAsync(bool includeImage = false, CancellationToken ct = default);

        (bool opened, int width, int height, int dpi, int deviceCount) GetStatus();

        // Kayıt – requiredSamples adet örnek toplayıp DBMerge (+ DBAdd) için birleşik template üretir.
        Task<EnrollResult> EnrollAsync(
            int userId,
            int requiredSamples = 3,
            int timeoutMs = 20000,
            bool includeImage = false,
            CancellationToken ct = default);

        // 1:1 doğrulama – kayıtlı template (Base64) ile eşleştirir.
        Task<MatchResult> VerifyAsync(
            string enrolledTemplateBase64,
            int timeoutMs = 8000,
            bool includeImage = false,
            CancellationToken ct = default);

        // 1:N kimlik tespiti – RAM/SDK DB’ye (DBAdd) yüklenmiş kayıtlar içinde arar.
        Task<IdentifyResult> IdentifyAsync(
            int timeoutMs = 8000,
            bool includeImage = false,
            CancellationToken ct = default);

        // (Opsiyonel) SDK DB yönetimi – kalıcı DB’den okunan template’leri RAM/SDK DB’sine yüklemek için
        void AddTemplateToDb(int userId, string templateBase64);

        // SDK DB’sini temizler.
        void ClearDb();

        // 🔵 YENİ: Template ↔ Template kıyas
        Task<(bool Success, int Score, string Message)> CompareTemplatesAsync(
            string templateABase64,
            string templateBBase64,
            CancellationToken ct = default);

        // 🟢 YENİ (opsiyonel): İlk 3 örneği merge edip tek template üret
        Task<(bool Success, string MergedTemplateBase64, string Message)> MergeTemplatesAsync(
            string[] templatesBase64,
            CancellationToken ct = default);
    }
}




//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Business.Abstract
//{
//    public class FingerprintResult
//    {
//        public bool Success { get; set; }
//        public string Message { get; set; }
//        public string TemplateBase64 { get; set; }
//        public string DeviceId { get; set; }
//        public string SerialNumber { get; set; }
//    }

//    public interface IFingerprintService
//    {
//        Task<FingerprintResult> ScanOnceAsync();
//        (bool opened, int width, int height, int dpi, int deviceCount) GetStatus();
//    }
//}
