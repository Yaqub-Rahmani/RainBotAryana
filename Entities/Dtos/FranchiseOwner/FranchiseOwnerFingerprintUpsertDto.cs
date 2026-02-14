using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.FranchiseOwner
{
    public class FranchiseOwnerFingerprintUpsertDto : IDto
    {
        public string TemplateBase64 { get; set; } = default!;
        public Guid? RelatedMediaAssetId { get; set; }
    }
}
