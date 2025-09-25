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
    public interface IOwnerDistributionLineService
    {
        // Okuma
        IDataResult<List<OwnerDistributionLine>> GetAll(Guid? distributionId = null);
        IDataResult<OwnerDistributionLine> GetById(Guid id);
        IDataResult<PagedResult<OwnerDistributionLineDetailDto>> GetDetails(QueryOptions opt, Guid? distributionId = null);
        IDataResult<OwnerDistributionLineDetailDto> GetDetailById(Guid id);

        // Yazma
        IResult Add(OwnerDistributionLine line, bool alignWithHeader = true);
        IResult Update(OwnerDistributionLine line, bool alignWithHeader = true);
        IResult Delete(Guid id);

        // Yardımcı
        IResult RecalcFromHeader(Guid distributionId); // header snapshot’ına göre satırları yeniden yazar
    }
}
