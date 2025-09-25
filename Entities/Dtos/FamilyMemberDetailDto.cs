using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FamilyMemberDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = "";
        public string Gender { get; set; } = "Unknown";
        public string? RelationTag { get; set; }
        public bool IsActive { get; set; }
    }
}
