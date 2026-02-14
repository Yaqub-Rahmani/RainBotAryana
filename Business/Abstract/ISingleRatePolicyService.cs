using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.SingleRatePolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ISingleRatePolicyService
    {
        IDataResult<List<SingleRatePolicyDto>> GetAll();
        IDataResult<List<SingleRatePolicyDto>> GetByType(SingleRateType type);
        IDataResult<SingleRatePolicyDto> GetById(Guid id);

        /// <summary>
        /// Belirli bir tarih için (veya bugün) geçerli oran.
        /// </summary>
        IDataResult<decimal> GetCurrentRate(SingleRateType type, DateTime? asOfUtc = null);

        IResult Add(SingleRatePolicyCreateDto dto, int userId);
        IResult Update(SingleRatePolicyUpdateDto dto, int userId);
        IResult Delete(Guid id);
    }
}
