using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class FloorDetailDto : IDto
    {
        public Guid Id { get; set; }
        public Guid MarketId { get; set; }
        public string MarketName { get; set; } = "";
        public string FloorName { get; set; } = "";
        public int Level { get; set; }
        public bool Status { get; set; }
    }
}
