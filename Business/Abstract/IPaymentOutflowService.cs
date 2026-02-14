using Core.Utilities.Results;
using Entities.Dtos.PaymentOutflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IPaymentOutflowService
    {
        Task<IDataResult<PaymentOutflowCreateResultDto>> CreateAsync(
            PaymentOutflowCreateFromUiDto dto,
            int userId,
            CancellationToken ct);
    }
}
