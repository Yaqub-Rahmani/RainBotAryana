namespace Core.Utilities.Pdf.Contracts;

public sealed class PaymentReceiptA6Model
{
    // Header
    public string MarketName { get; set; } = "";
    public string FloorText { get; set; } = "";
    public string ShopName { get; set; } = "";
    public string BusinessActivity { get; set; } = "";


    public string FranchiseOwnerText { get; set; } = ""; // "Ahmet" veya "Ahmet و 3 نفر دیگر"
    public string TenantName { get; set; } = "";

    // Date/time (KabulTimeProvider output)
    public string PaidWeekday { get; set; } = "";  // دوشنبه
    public string PaidDateBig { get; set; } = "";  // 1404-سرطان-14
    public string PaidTime { get; set; } = "";     // 08:08AM

    // Amount + method
    public string AmountBig { get; set; } = "";    // 12,000 ؋ (Latin safe)
    public string MethodText { get; set; } = "";   // نقد/بانک/...

    // Body (Allocation only)
    public List<PaymentReceiptA6LineModel> Lines { get; set; } = new();

    // Footer
    public string CreatedByName { get; set; } = ""; // "کاربر: Ahmad"
}

public sealed class PaymentReceiptA6LineModel
{
    public string Period { get; set; } = "";  // 1404/4
    public string Kind { get; set; } = "";    // برق/مارکیت/مالک/امتیاز
    public string Amount { get; set; } = "";  // 1,200 ؋ (Latin safe)
}

