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
    public interface IMeterReadingService
    {
        // Okumalar
        IDataResult<List<MeterReading>> GetAll(Guid? shopId = null, int? year = null, int? month = null, bool? finalized = null);
        IDataResult<MeterReading> GetById(Guid id);
        IDataResult<PagedResult<MeterReadingDetailDto>> GetDetails(QueryOptions opt, Guid? shopId = null, int? year = null, int? month = null, bool? finalized = null);
        IDataResult<MeterReadingDetailDto> GetDetailById(Guid id);

        // Yazmalar
        IResult Add(MeterReading reading);
        IResult Update(MeterReading reading);
        IResult Delete(Guid id);

        // İşlemsel
        IResult Finalize(Guid id);     // Accrual oluştur/güncelle
        IResult Unfinalize(Guid id);   // Mümkünse Accrual’ı geri al
        IResult SyncAccrual(Guid id);  // elle senkron
    }
}
