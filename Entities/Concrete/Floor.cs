using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Floor : IEntity
    {
        public Guid Id { get; set; }

        // ilişki
        public Guid MarketId { get; set; }

        // alanlar
        public string FloorName { get; set; } = null!; // örn: "Zemin", "1. Kat"
        public int Level { get; set; }                 // örn: 0,1,2,-1
        public bool Status { get; set; } = true;
        public Market? Market { get; set; }

    }
}
