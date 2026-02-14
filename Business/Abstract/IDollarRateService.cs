using Core.Utilities.Results;
using Entities.Dtos.DollarRate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IDollarRateService
    {
        IDataResult<List<DollarRateDto>> GetAll();
        IDataResult<DollarRateDto> GetById(int id);
        IDataResult<DollarRateDto> GetLatest();   // En son kur

        IResult Add(DollarRateCreateDto dto, int userId);
        IResult Update(DollarRateUpdateDto dto, int userId);
        IResult Delete(int id);
    }
}
