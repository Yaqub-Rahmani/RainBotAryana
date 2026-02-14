using System;
using System.Globalization;
using System.Text;

namespace Core.Utilities.KabulDateTime
{
    public interface ITimeProvider
    {
        // Kabul (Asia/Kabul) zaman dilimine göre tarih/saat
        DateOnly Today();   // Kabul, Gregorian
        DateTime Now();     // Kabul, Gregorian
        DateTime UtcNow();  // UTC

        // ---- Persian (Hijri-Şemsi) yardımcıları ----
        (int Year, int Month, int Day) TodayPersian();
        (int Year, int Month, int Day) ToPersian(DateOnly gregorian);
        (int Year, int Month) AddMonthsPersian(int year, int month, int offset);

        // ---- Yeni: Dari Afghanistan formatları ----

        /// <summary>Örn: 08:08AM / 08:08PM (Kabul saatine göre)</summary>
        string FormatTimeAmPm(DateTime utcOrAny);

        /// <summary>Örn: 1404/4</summary>
        string FormatPersianYearMonthSlash(DateTime utcOrAny);

        /// <summary>Örn: 1404-04-14</summary>
        string FormatPersianDateDash(DateTime utcOrAny);

        /// <summary>Örn: 1404-سرطان-14</summary>
        string FormatPersianDateWithMonthNameDash(DateTime utcOrAny);

        /// <summary>
        /// RTL metinde Latin digit hizası için sayıları LRM ile sarar.
        /// PDF/HTML gibi ortamlarda karışmayı azaltır.
        /// </summary>
        string RtlSafeLatin(string text);
    }

    public sealed class KabulTimeProvider : ITimeProvider
    {
#if WINDOWS
        private const string KabulTimeZoneId = "Afghanistan Standard Time";
#else
        private const string KabulTimeZoneId = "Asia/Kabul";
#endif

        private static readonly TimeZoneInfo _tz =
            TimeZoneInfo.FindSystemTimeZoneById(KabulTimeZoneId);

        private readonly PersianCalendar _persianCalendar = new PersianCalendar();

        // Dari Afghanistan month names (1..12)
        private static readonly string[] _dariMonthNames =
        {
            "", "حمل", "ثور", "جوزا", "سرطان", "اسد", "سنبله", "میزان", "عقرب", "قوس", "جدی", "دلو", "حوت"
        };

        // Bidi marks
        private const char LRM = '\u200E'; // Left-to-Right Mark
        private const char RLM = '\u200F'; // Right-to-Left Mark (gerekirse)

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

        // --------- Mevcut Persian yardımcıları (DEĞİŞMEDİ) ---------
        public (int Year, int Month, int Day) TodayPersian()
        {
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

        public (int Year, int Month) AddMonthsPersian(int year, int month, int offset)
        {
            var total = (year * 12) + (month - 1) + offset;

            var newYear = total / 12;
            var newMonth = (total % 12) + 1;

            return (newYear, newMonth);
        }

        // --------- Yeni helper’lar ---------

        /// <summary>
        /// Gelen DateTime’ı Kabul saatine çevirir.
        /// Eğer Kind=Utc ise kesin doğru; Local/Unspecified ise yine de Kabul’e map eder.
        /// </summary>
        private DateTime ToKabul(DateTime utcOrAny)
        {
            // ConvertTime, Kind’e göre davranır; yine de Kabul’i hedefliyoruz.
            return TimeZoneInfo.ConvertTime(utcOrAny, _tz);
        }

        private static string MonthNameDari(int month)
        {
            if (month < 1 || month > 12) throw new ArgumentOutOfRangeException(nameof(month));
            return _dariMonthNames[month];
        }

        /// <summary>Persian/Arabic digits -> Latin digits</summary>
        private static string ToLatinDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var sb = new StringBuilder(input.Length);
            foreach (var ch in input)
            {
                sb.Append(ch switch
                {
                    // Persian digits
                    '\u06F0' => '0',
                    '\u06F1' => '1',
                    '\u06F2' => '2',
                    '\u06F3' => '3',
                    '\u06F4' => '4',
                    '\u06F5' => '5',
                    '\u06F6' => '6',
                    '\u06F7' => '7',
                    '\u06F8' => '8',
                    '\u06F9' => '9',

                    // Arabic-Indic digits
                    '\u0660' => '0',
                    '\u0661' => '1',
                    '\u0662' => '2',
                    '\u0663' => '3',
                    '\u0664' => '4',
                    '\u0665' => '5',
                    '\u0666' => '6',
                    '\u0667' => '7',
                    '\u0668' => '8',
                    '\u0669' => '9',

                    _ => ch
                });
            }
            return sb.ToString();
        }

        /// <summary>
        /// RTL metinde Latin sayı/format karışmasını azaltır (LRM ile sarar).
        /// Örn: "1404-04-14" -> "\u200E1404-04-14\u200E"
        /// </summary>
        public string RtlSafeLatin(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var latin = ToLatinDigits(text);
            return $"{LRM}{latin}{LRM}";
        }

        /// <summary>Örn: 08:08AM / 08:08PM (Kabul saatine göre)</summary>
        public string FormatTimeAmPm(DateTime utcOrAny)
        {
            var k = ToKabul(utcOrAny);

            // 12-hour clock
            int hour12 = k.Hour % 12;
            if (hour12 == 0) hour12 = 12;

            var ampm = k.Hour < 12 ? "AM" : "PM";
            var s = $"{hour12:00}:{k.Minute:00}{ampm}"; // 08:08AM

            return RtlSafeLatin(s);
        }

        /// <summary>Örn: 1404/4 (Kabul saatine göre Persian yıl/ay)</summary>
        public string FormatPersianYearMonthSlash(DateTime utcOrAny)
        {
            var k = ToKabul(utcOrAny);

            var y = _persianCalendar.GetYear(k);
            var m = _persianCalendar.GetMonth(k);

            var s = $"{y}/{m}";
            return RtlSafeLatin(s);
        }

        /// <summary>Örn: 1404-04-14 (Kabul saatine göre Persian tarih)</summary>
        public string FormatPersianDateDash(DateTime utcOrAny)
        {
            var k = ToKabul(utcOrAny);

            var y = _persianCalendar.GetYear(k);
            var m = _persianCalendar.GetMonth(k);
            var d = _persianCalendar.GetDayOfMonth(k);

            var s = $"{y}-{m:00}-{d:00}";
            return RtlSafeLatin(s);
        }

        /// <summary>Örn: 1404-سرطان-14 (Kabul saatine göre Persian tarih)</summary>
        public string FormatPersianDateWithMonthNameDash(DateTime utcOrAny)
        {
            var k = ToKabul(utcOrAny);

            var y = _persianCalendar.GetYear(k);
            var m = _persianCalendar.GetMonth(k);
            var d = _persianCalendar.GetDayOfMonth(k);

            var monthName = MonthNameDari(m);

            // Ay adı RTL, ama yıl/gün Latin olmalı. Karışmayı azaltmak için yıl/gün bloklarını LRM ile sarıyoruz.
            // Sonuç: 1404-سرطان-14
            var s = $"{y}-{monthName}-{d}";
            // Burada tüm string’i LRM ile sarmıyoruz, çünkü ay adı RTL.
            // Yıl ve günün Latin kalması için sadece digit dönüşümü yapıp, numeric parçaları LRM ile koruyoruz.
            // Basit ve sağlam:
            var yPart = RtlSafeLatin(y.ToString());
            var dPart = RtlSafeLatin(d.ToString());
            return $"{yPart}-{monthName}-{dPart}";
        }
    }
}


//using System;
//using System.Globalization;

//namespace Core.Utilities.KabulDateTime
//{
//    public interface ITimeProvider
//    {
//        // Kabul (Asia/Kabul) zaman dilimine göre tarih/saat
//        DateOnly Today();   // Kabul, Gregorian
//        DateTime Now();     // Kabul, Gregorian
//        DateTime UtcNow();  // UTC

//        // ---- Persian (Hijri-Şemsi) yardımcıları ----

//        /// <summary>
//        /// Kabul saat dilimine göre bugünün Farsî (Persian) tarihini döner.
//        /// Örn: (1404, 8, 15)
//        /// </summary>
//        (int Year, int Month, int Day) TodayPersian();

//        /// <summary>
//        /// Verilen Gregorian DateOnly değerini Farsî (Persian) takvime çevirir.
//        /// </summary>
//        (int Year, int Month, int Day) ToPersian(DateOnly gregorian);

//        /// <summary>
//        /// Farsî (Persian) yıl/ay üzerinde offset kadar ay ekler/çıkarır.
//        /// Örn: AddMonthsPersian(1404, 8, -2) => (1404, 6)
//        /// </summary>
//        (int Year, int Month) AddMonthsPersian(int year, int month, int offset);
//    }

//    public sealed class KabulTimeProvider : ITimeProvider
//    {
//        // Windows:  "Afghanistan Standard Time"
//        // Linux:    "Asia/Kabul"
//#if WINDOWS
//        private const string KabulTimeZoneId = "Afghanistan Standard Time";
//#else
//        private const string KabulTimeZoneId = "Asia/Kabul";
//#endif

//        private static readonly TimeZoneInfo _tz =
//            TimeZoneInfo.FindSystemTimeZoneById(KabulTimeZoneId);

//        private readonly PersianCalendar _persianCalendar = new PersianCalendar();

//        // --------- Mevcut (Gregorian) metodlar ---------

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

//        // --------- Yeni Persian yardımcıları ---------

//        public (int Year, int Month, int Day) TodayPersian()
//        {
//            // Today() zaten Kabul timezone’da Gregorian DateOnly veriyor
//            var todayGregorian = Today().ToDateTime(TimeOnly.MinValue);

//            var y = _persianCalendar.GetYear(todayGregorian);
//            var m = _persianCalendar.GetMonth(todayGregorian);
//            var d = _persianCalendar.GetDayOfMonth(todayGregorian);

//            return (y, m, d);
//        }

//        public (int Year, int Month, int Day) ToPersian(DateOnly gregorian)
//        {
//            var dt = gregorian.ToDateTime(TimeOnly.MinValue);

//            var y = _persianCalendar.GetYear(dt);
//            var m = _persianCalendar.GetMonth(dt);
//            var d = _persianCalendar.GetDayOfMonth(dt);

//            return (y, m, d);
//        }

//        /// <summary>
//        /// Persian yıl/ay üzerinde offset kadar ay ekleme/çıkarma.
//        /// Örn: AddMonthsPersian(1404, 8, -2) => (1404, 6)
//        /// </summary>
//        public (int Year, int Month) AddMonthsPersian(int year, int month, int offset)
//        {
//            // Basit lineer hesaplama: her yılı 12 ay kabul ediyoruz.
//            var total = (year * 12) + (month - 1) + offset;

//            var newYear = total / 12;
//            var newMonth = (total % 12) + 1;

//            return (newYear, newMonth);
//        }
//    }
//}




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
