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
    public interface IAddressService
    {
        // Okumalar
        IDataResult<List<Address>> GetAll();
        IDataResult<Address> GetById(Guid id);
        IDataResult<PagedResult<AddressDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<List<AddressDetailDto>> GetByOwner(MediaOwnerType ownerType, Guid ownerId);

        // Yazmalar
        IResult Add(Address address);          // IsPrimary=true ise diğerlerini kapatır
        IResult Update(Address address);       // IsPrimary=true ise diğerlerini kapatır
        IResult Delete(Guid id);

        // İşlemsel
        IResult SetPrimary(Guid id);           // Adresi birincil yapar (owner’daki diğerlerini false)
    }
}
