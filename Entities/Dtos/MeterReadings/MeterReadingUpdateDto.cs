using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos.MeterReadings
{
    public class MeterReadingUpdateDto : MeterReadingCreateDto
    {
        public Guid Id { get; set; }
        public bool IsFinalized { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
