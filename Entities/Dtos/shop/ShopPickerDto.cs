using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.shop
{
    public sealed class ShopPickerDto: IDto
    {
        public Guid Id { get; set; }
        public string? UnitNumber { get; set; }
        public string? Name { get; set; }
        public Guid MarketId { get; set; }
        public Guid FloorId { get; set; }

        public bool HasActiveFranchiseOwner { get; set; }
    }

}
