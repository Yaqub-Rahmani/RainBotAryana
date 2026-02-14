using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICollectionService
    {
        IDataResult<CollectionDto> GetById(Guid id);
        IDataResult<List<CollectionDto>> GetByTenant(Guid tenantId);

        /// <summary>
        /// Tahsilat kaydı + MoneyEntry (dolar izi) oluşturur, IsApproved = false.
        /// </summary>
        IResult Add(CollectionCreateDto dto, int userId);

        /// <summary>
        /// Tahsilatı onaylar (B kullanıcısı).
        /// Dağıtım / rezerv operasyonları ileride bu aşamaya bağlanacak.
        /// </summary>
        IResult Approve(CollectionApproveDto dto, int userId);

        IResult AddContractDebtPayment(ContractDebtPaymentRequestDto dto, int userId);
    }
}
