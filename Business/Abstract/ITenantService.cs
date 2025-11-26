using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ITenantService
    {
        // Okuma
        IDataResult<List<Tenant>> GetAll(bool? onlyActive = null);
        IDataResult<Tenant> GetById(Guid id);
        IDataResult<PagedResult<TenantDetailDto>> GetDetails(QueryOptions opt, bool? onlyActive = null);
        IDataResult<TenantDetailDto> GetDetailById(Guid id);

        IDataResult<List<TenantViewDto>> GetAllTenantView(bool? active = null);

        // Yazma
        IResult Add(Tenant entity);
        IResult Update(Tenant entity);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Activate(Guid id);
        IResult Deactivate(Guid id);
    }
}
