using Core.Entities;
using Entities.Concrete;

namespace Entities.Dtos.tenant
{
    public class TenantProfileDto : IDto
    {
        public string Id { get; set; } = "";

        public string DisplayName { get; set; } = "";
        public string FatherName { get; set; } = "";
        public Gender Gender { get; set; }
        public string NationalId { get; set; } = "";

        public bool IsActive { get; set; }

        public string PhonePrimary { get; set; } = "";
        public string PhoneSecondary { get; set; } = "";

        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string AddressLine { get; set; } = "";
        public string AddressFull { get; set; } = "";

        // Tenant için "Balance" = toplam kalan borç (Outstanding)
        public decimal Balance { get; set; }

        // Media (primary)
        public string AvatarUrl { get; set; } = "";
        public string IdFrontUrl { get; set; } = "";
        public string IdBackUrl { get; set; } = "";
        public string FingerBitmapUrl { get; set; } = "";
        public string ContractUrl { get; set; } = ""; // TenantContract (pdf)
    }

    public class TenantFingerprintUpsertDto
    {
        public string TemplateBase64 { get; set; } = "";
        public Guid? RelatedMediaAssetId { get; set; }
    }

    public class TenantContractBriefDto
    {
        public Guid ContractId { get; set; }
        public Guid ShopId { get; set; }

        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public string ShopName { get; set; } = "";
        public string? UnitNumber { get; set; }

        public string StartDate { get; set; } = ""; // yyyy-MM-dd
        public string EndDate { get; set; } = "";   // yyyy-MM-dd
        public string Status { get; set; } = "";
    }
}
