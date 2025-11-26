using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos.TaxPaymentEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ITaxPaymentEntryService
    {
        IDataResult<List<TaxPaymentEntry>> GetAll();
        IDataResult<List<TaxPaymentEntry>> GetAll(TaxPaymentEntryFilterDto filter);

        IDataResult<TaxPaymentEntry> GetById(Guid id);
        IDataResult<TaxPaymentEntryDetailDto> GetDetail(Guid id);

        IDataResult<TaxPaymentSummaryDto> GetSummary(TaxPaymentEntryFilterDto filter);

        IResult Add(TaxPaymentEntryCreateDto dto, int userId);
        IResult Update(TaxPaymentEntryUpdateDto dto, int userId);
        IResult Delete(Guid id);
        IResult Approve(Guid id, int userId);
    }
}
