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
    public interface IAccrualService
    {
        // Basit
        IDataResult<List<Accrual>> GetAll();
        IDataResult<Accrual> GetById(Guid id);

        // Zengin
        IDataResult<PagedResult<AccrualDetailDto>> GetDetails(QueryOptions opt);
        IDataResult<AccrualDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(Accrual accrual);
        IResult Update(Accrual accrual);
        IResult Delete(Guid id);

        // İşlemler
        IResult InitializeOutstanding(Guid id); // Amount => Outstanding
        IResult ApplySettlement(Guid id, decimal amount); // borç düşür
    }
}
