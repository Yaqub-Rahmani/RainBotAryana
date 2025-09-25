using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class MarketMemberDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = "";
        public string? MemberType { get; set; }
        public bool IsActive { get; set; }
    }
}
