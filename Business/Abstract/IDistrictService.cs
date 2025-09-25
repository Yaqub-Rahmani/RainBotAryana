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
    public interface IDistrictService
    {
        IDataResult<List<District>> GetAll();
        IDataResult<District> GetById(int id);
        IDataResult<PagedResult<DistrictDetailDto>> GetDetails(QueryOptions opt, int? provinceId = null);
        IDataResult<DistrictDetailDto> GetDetailById(int id);

        IResult Add(District district);
        IResult Update(District district);
        IResult Delete(int id);
    }
}
