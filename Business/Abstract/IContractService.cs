using Business.ValidationRules.FluentValidation.Contract;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos.contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IContractService
    {
        IDataResult<List<Contract>> GetAll(bool includeArchived = false);
        IDataResult<Contract> GetById(Guid id);
        IDataResult<ContractDetailDto> GetDetail(Guid contractId);

        //IDataResult<List<ContractListItemDto>> GetListItems();
        public IDataResult<PagedResult<ContractListItemDto>> GetListItems(QueryOptions opt);



        IResult Add(ContractCreateDto dto, int userId);
        IResult Update(ContractUpdateDto dto, int userId);
        IResult Delete(Guid id);

        // Durum işlemleri
        IResult Archive(Guid id, int userId, string? reason);
        IResult Unarchive(Guid id, int userId);
    }
}
