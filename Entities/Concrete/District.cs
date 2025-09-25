using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class District : IEntity
    {
        public int Id { get; set; }

        public int ProvinceId { get; set; }   // FK
        public string Name { get; set; } = null!;
        public string? Code { get; set; }
        public bool IsActive { get; set; } = true;

        // navigation
        public Province? Province { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();

    }
}
