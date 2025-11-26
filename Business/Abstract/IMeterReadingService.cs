using Core.Utilities.Paging;
using Core.Utilities.Query;
using Core.Utilities.Results;
using Entities.Concrete;
using Entities.Dtos;
using Entities.Dtos.MeterReadings;
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
        IDataResult<List<MeterReading>> GetAll(Guid? shopId = null, Guid? contractId = null, int? year = null, int? month = null, bool? finalized = null);
        IDataResult<MeterReading> GetById(Guid id);
        IDataResult<MeterReadingDetailDto> GetDetail(Guid id);

        IDataResult<MeterReadingNewContextDto> GetNewContext(Guid contractId);


        IResult Add(MeterReadingCreateDto dto, int userId);
        IResult Update(MeterReadingUpdateDto dto, int userId);
        IResult Delete(Guid id);


        IResult Finalize(Guid id, int userId);
        IResult Unfinalize(Guid id, int userId);
    }
}
