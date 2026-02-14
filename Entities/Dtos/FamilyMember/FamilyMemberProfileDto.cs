using Core.Entities;
using Entities.Concrete;

namespace Entities.Dtos.FamilyMember
{
    public class FamilyMemberProfileDto : IDto
    {
        public string Id { get; set; } = "";

        public string DisplayName { get; set; } = "";
        public string FatherName { get; set; } = "";
        public Gender Gender { get; set; }
        public string NationalId { get; set; } = "";

        public string RelationTag { get; set; } = "";
        public bool IsActive { get; set; }
        public bool IsParent { get; set; }
        public bool IncludeInShare { get; set; }

        public string PhonePrimary { get; set; } = "";
        public string PhoneSecondary { get; set; } = "";

        public string Province { get; set; } = "";
        public string District { get; set; } = "";
        public string AddressLine { get; set; } = "";
        public string AddressFull { get; set; } = "";

        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
        public decimal Balance => TotalIn - TotalOut;

        // Media
        public string AvatarUrl { get; set; } = "";
        public string IdFrontUrl { get; set; } = "";
        public string IdBackUrl { get; set; } = "";
        public string FingerBitmapUrl { get; set; } = "";
    }
}
