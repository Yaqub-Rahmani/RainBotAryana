using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class RefDto: IDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";   // ör. numara/kod
        public string? Code { get; set; }
    }
}
