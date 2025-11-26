using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos.ZakatPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IZakatPaymentEntryService
    {
        IDataResult<List<ZakatPaymentEntry>> GetAll();
        IDataResult<List<ZakatPaymentEntry>> GetAll(ZakatPaymentEntryFilterDto filter);
        IDataResult<ZakatPaymentEntryDetailDto> GetDetail(Guid id);

        IResult Add(ZakatPaymentEntryCreateDto dto, int userId);
        IResult Update(ZakatPaymentEntryUpdateDto dto, int userId);
        IResult Delete(Guid id);
        IResult Approve(Guid id, int userId);
    }
}
