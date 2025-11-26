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
    public interface IFranchiseOwnerBalanceService
    {
        // Reads
        IDataResult<List<FranchiseOwnerBalance>> GetAll(bool? onlyActiveOwners = null); // onlyActiveOwners: ileride Owner.IsActive filtrelenebilir
        IDataResult<FranchiseOwnerBalance> GetById(Guid id);
        IDataResult<FranchiseOwnerBalance> GetByOwnerId(Guid ownerId);
        IDataResult<List<FranchiseOwnerBalance>> ListByOwnerId(Guid ownerId); // liste gerekirse
        IDataResult<PagedResult<FranchiseOwnerBalanceDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<FranchiseOwnerBalanceDetailDto> GetDetailById(Guid id);

        // Mutations
        IResult EnsureCreatedForOwner(Guid ownerId, Guid updatedBy); // idempotent oluştur
        IResult UpdateIn(Guid ownerId, decimal amount, Guid updatedBy, DateTime? whenUtc = null, string? note = null);
        IResult UpdateOut(Guid ownerId, decimal amount, Guid updatedBy, DateTime? whenUtc = null, string? note = null);

        // İsteğe bağlı tam güncelleme (Balance set)
        IResult SetBalance(Guid ownerId, decimal balance, Guid updatedBy, DateTime? whenUtc = null);
    }
}
