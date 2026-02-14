using Core.Utilities.Results;
using Entities.Dtos.MoneyEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IMoneyEntryService
    {
        IResult Add(MoneyEntryCreateDto dto, int userId);
    }
}
