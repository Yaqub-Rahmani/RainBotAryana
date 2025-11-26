using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class MediaAssetDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string OwnerType { get; set; } = "";
        public Guid OwnerId { get; set; }
        public string Kind { get; set; } = "";
        public string StorageUrl { get; set; } = "";
        public string? HashChecksum { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
