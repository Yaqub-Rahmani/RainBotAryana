using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public enum PhoneLabel { Primary = 1, Secondary = 2, Mobile = 3, Office = 4 }

    public class Phone:IEntity
    {
        public Guid Id { get; set; }

        public MediaOwnerType OwnerType { get; set; }
        public Guid OwnerId { get; set; }

        public string Number { get; set; } = null!;
        public PhoneLabel Label { get; set; }
        public bool IsVerified { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
