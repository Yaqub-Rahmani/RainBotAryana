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
    public interface IProvinceService
    {
        IDataResult<List<Province>> GetAll(bool? onlyActive = null);
        IDataResult<Province> GetById(int id);
        IDataResult<PagedResult<ProvinceDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<ProvinceDetailDto> GetDetailById(int id);

        IResult Add(Province entity);
        IResult Update(Province entity);
        IResult Delete(int id);
    }
}
