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
    public interface IPhoneService
    {
        // Okumalar
        IDataResult<List<Phone>> GetAll(Guid? ownerId = null, MediaOwnerType? ownerType = null);
        IDataResult<Phone> GetById(Guid id);
        IDataResult<PagedResult<PhoneDetailDto>> GetDetails(QueryOptions opt, Guid? ownerId = null, MediaOwnerType? ownerType = null);
        IDataResult<PhoneDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(Phone phone);
        IResult Update(Phone phone);
        IResult Delete(Guid id);

        // Yardımcı
        string Normalize(string raw);
    }
}
