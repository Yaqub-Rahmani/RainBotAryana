using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public enum BiometricType
    {
        Fingerprint = 1
    }

    public class Biometric : IEntity
    {
        public Guid Id { get; set; }

        public MediaOwnerType OwnerType { get; set; }
        public Guid OwnerId { get; set; }

        public BiometricType Type { get; set; }
        public byte[] Template { get; set; } = null!;     
        public Guid? RelatedMediaAssetId { get; set; } 
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }

}
