using System;
using System.Globalization;

namespace Core.Utilities.KabulDateTime
{
    public interface ITimeProvider
    {
        // Kabul (Asia/Kabul) zaman dilimine göre tarih/saat
        DateOnly Today();   // Kabul, Gregorian
        DateTime Now();     // Kabul, Gregorian
        DateTime UtcNow();  // UTC

        // ---- Persian (Hijri-Şemsi) yardımcıları ----

        /// <summary>
        /// Kabul saat dilimine göre bugünün Farsî (Persian) tarihini döner.
        /// Örn: (1404, 8, 15)
        /// </summary>
        (int Year, int Month, int Day) TodayPersian();

        /// <summary>
        /// Verilen Gregorian DateOnly değerini Farsî (Persian) takvime çevirir.
        /// </summary>
        (int Year, int Month, int Day) ToPersian(DateOnly gregorian);

        /// <summary>
        /// Farsî (Persian) yıl/ay üzerinde offset kadar ay ekler/çıkarır.
        /// Örn: AddMonthsPersian(1404, 8, -2) => (1404, 6)
        /// </summary>
        (int Year, int Month) AddMonthsPersian(int year, int month, int offset);
    }

    public sealed class KabulTimeProvider : ITimeProvider
    {
        // Windows:  "Afghanistan Standard Time"
        // Linux:    "Asia/Kabul"
#if WINDOWS
        private const string KabulTimeZoneId = "Afghanistan Standard Time";
#else
        private const string KabulTimeZoneId = "Asia/Kabul";
#endif

        private static readonly TimeZoneInfo _tz =
            TimeZoneInfo.FindSystemTimeZoneById(KabulTimeZoneId);

        private readonly PersianCalendar _persianCalendar = new PersianCalendar();

        // --------- Mevcut (Gregorian) metodlar ---------

        public DateOnly Today()
        {
            var local = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);
            return DateOnly.FromDateTime(local);
        }

        public DateTime Now()
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);
        }

        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        // --------- Yeni Persian yardımcıları ---------

        public (int Year, int Month, int Day) TodayPersian()
        {
            // Today() zaten Kabul timezone’da Gregorian DateOnly veriyor
            var todayGregorian = Today().ToDateTime(TimeOnly.MinValue);

            var y = _persianCalendar.GetYear(todayGregorian);
            var m = _persianCalendar.GetMonth(todayGregorian);
            var d = _persianCalendar.GetDayOfMonth(todayGregorian);

            return (y, m, d);
        }

        public (int Year, int Month, int Day) ToPersian(DateOnly gregorian)
        {
            var dt = gregorian.ToDateTime(TimeOnly.MinValue);

            var y = _persianCalendar.GetYear(dt);
            var m = _persianCalendar.GetMonth(dt);
            var d = _persianCalendar.GetDayOfMonth(dt);

            return (y, m, d);
        }

        /// <summary>
        /// Persian yıl/ay üzerinde offset kadar ay ekleme/çıkarma.
        /// Örn: AddMonthsPersian(1404, 8, -2) => (1404, 6)
        /// </summary>
        public (int Year, int Month) AddMonthsPersian(int year, int month, int offset)
        {
            // Basit lineer hesaplama: her yılı 12 ay kabul ediyoruz.
            var total = (year * 12) + (month - 1) + offset;

            var newYear = total / 12;
            var newMonth = (total % 12) + 1;

            return (newYear, newMonth);
        }
    }
}




//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Core.Utilities.KabulDateTime
//{
//    public interface ITimeProvider
//    {
//        DateOnly Today();
//        DateTime Now();
//        DateTime UtcNow();
//    }

//    public sealed class KabulTimeProvider : ITimeProvider
//    {
//        // Windows:  "Afghanistan Standard Time"
//        // Linux:    "Asia/Kabul"
//        #if WINDOWS
//            private const string KabulTimeZoneId = "Afghanistan Standard Time";
//        #else
//                private const string KabulTimeZoneId = "Asia/Kabul";
//        #endif

//        private static readonly TimeZoneInfo _tz =
//            TimeZoneInfo.FindSystemTimeZoneById(KabulTimeZoneId);

//        public DateOnly Today()
//        {
//            var local = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);
//            return DateOnly.FromDateTime(local);
//        }

//        public DateTime Now()
//        {
//            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, _tz);
//        }

//        public DateTime UtcNow()
//        {
//            return DateTime.UtcNow;
//        }
//    }


//}
