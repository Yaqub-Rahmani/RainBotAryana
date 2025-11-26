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
    public interface IBiometricService
    {
        // Okumalar
        IDataResult<List<Biometric>> GetAll();
        IDataResult<Biometric> GetById(Guid id);
        IDataResult<PagedResult<BiometricDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<BiometricDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(Biometric biometric);
        IResult Update(Biometric biometric);
        IResult Delete(Guid id);
    }
}
