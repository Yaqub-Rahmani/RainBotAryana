using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IFloorService
    {
        // Okumalar
        IDataResult<List<Floor>> GetAll(Guid? marketId = null, bool? status = null);
        IDataResult<Floor> GetById(Guid id);
        IDataResult<PagedResult<FloorDetailDto>> GetDetails(QueryOptions opt, Guid? marketId = null, bool? status = null);
        IDataResult<FloorDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(Floor floor);
        IResult Update(Floor floor);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}
