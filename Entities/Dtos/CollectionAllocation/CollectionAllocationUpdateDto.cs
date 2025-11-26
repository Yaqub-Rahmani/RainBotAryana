using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.CollectionAllocation
{
    public class CollectionAllocationUpdateDto : CollectionAllocationCreateDto
    {
        public Guid Id { get; set; }
    }
}
