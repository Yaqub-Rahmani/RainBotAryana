using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.OwnerDistributionPolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IOwnerDistributionPolicyService
    {
        IDataResult<List<OwnerDistributionPolicyDto>> GetAll();
        IDataResult<OwnerDistributionPolicyDto> GetById(Guid id);

        /// <summary>
        /// Belirli bir tarih için (veya bugün) geçerli owner dağıtım politikası.
        /// </summary>
        IDataResult<OwnerDistributionPolicyDto> GetCurrent();

        IResult Add(OwnerDistributionPolicyCreateDto dto, int userId);
        IResult Update(OwnerDistributionPolicyUpdateDto dto, int userId);
        IResult Delete(Guid id);
    }
}
