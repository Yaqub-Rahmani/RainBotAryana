using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class Market : IEntity
    {
        public Guid Id { get; set; }
        public string MarketName { get; set; } = null!;
        public bool Status { get; set; } = true; // varsayılan aktif
    }
}
