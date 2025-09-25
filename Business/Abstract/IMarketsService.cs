using Core.Utilities.Results;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IMarketsService
    {
        IDataResult<List<Market>> GetAll(bool? onlyActive = null);
        IDataResult<Market> GetById(Guid id);
        IResult Add(Market market);
        IResult Update(Market market);
        IResult Delete(Guid id);
        IResult Activate(Guid id);   // opsiyonel
        IResult Deactivate(Guid id); // opsiyonel
    }
}
