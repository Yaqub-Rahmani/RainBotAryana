using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.FamilySharePolicy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IFamilySharePolicyService
    {
        IDataResult<List<FamilySharePolicyDto>> GetAll();
        IDataResult<FamilySharePolicyDto> GetById(Guid id);

        /// <summary>Belirli bir tarih için (veya bugün) geçerli aile paylaşım politikası.</summary>
        IDataResult<FamilySharePolicyDto> GetCurrent(DateTime? asOfUtc = null);

        IResult Add(FamilySharePolicyCreateDto dto, int userId);
        IResult Update(FamilySharePolicyUpdateDto dto, int userId);
        IResult Delete(Guid id);
    }
}
