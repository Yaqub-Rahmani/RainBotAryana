using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class BiometricDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string OwnerType { get; set; } = "";
        public Guid OwnerId { get; set; }
        public string Type { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? RelatedMediaAssetId { get; set; }
        public int TemplateSize { get; set; } // byte uzunluğu
    }
}
