using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public enum MediaOwnerType { User = 1, Tenant = 2, FranchiseOwner = 3, FamilyMember = 4, Employee = 5, Contract =6, Worker = 7}

    public enum MediaKind
    {
        Avatar = 1,
        IdFront = 2,
        IdBack = 3,
        FingerBitmap = 4,
        FranchiseContract = 5,
        TenantContract = 6,
        Other = 99
    }

    public class MediaAsset : IEntity
    {
        public Guid Id { get; set; }


        // Polimorfik sahiplik
        public MediaOwnerType OwnerType { get; set; }
        public Guid OwnerId { get; set; }

        // İçerik
        public MediaKind Kind { get; set; }
        public string StorageUrl { get; set; } = null!;   // S3/Blob URL, base64 yerine
        public string? HashChecksum { get; set; }
        public bool IsPrimary { get; set; }

        public DateTime CreatedAt { get; set; }
    }

}
