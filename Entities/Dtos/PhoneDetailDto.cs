using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PhoneDetailDto : IDto
    {
        public Guid Id { get; set; }
        public string OwnerType { get; set; } = "";
        public Guid OwnerId { get; set; }
        public string Number { get; set; } = "";
        public string Label { get; set; } = "";
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
