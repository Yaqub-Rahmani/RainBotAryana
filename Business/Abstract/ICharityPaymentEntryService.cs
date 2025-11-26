using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos.CharityPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICharityPaymentEntryService
    {
        IDataResult<List<CharityPaymentEntry>> GetAll();
        IDataResult<List<CharityPaymentEntry>> GetAll(CharityPaymentEntryFilterDto filter);
        IDataResult<CharityPaymentEntryDetailDto> GetDetail(Guid id);

        IResult Add(CharityPaymentEntryCreateDto dto, int userId);
        IResult Update(CharityPaymentEntryUpdateDto dto, int userId);
        IResult Delete(Guid id);
        IResult Approve(Guid id, int userId);
    }
}
