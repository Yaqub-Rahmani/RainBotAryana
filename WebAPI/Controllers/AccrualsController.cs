using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.Accrual;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccrualsController : ControllerBase
    {
        private readonly IAccrualService _service;
        public AccrualsController(IAccrualService service) => _service = service;

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? shopId, [FromQuery] Guid? tenantId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] AccrualStatus? status)
            => Ok(_service.GetAll(shopId, tenantId, year, month, status).Data);

        [HttpGet("{id:guid}"), AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("{id:guid}/detail"), AllowAnonymous]
        public IActionResult Detail(Guid id)
        {
            var r = _service.GetDetail(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost, Authorize]
        public IActionResult Add([FromBody] AccrualCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        /// <summary>
        /// Bir sözleşmenin (ContractId) açık borçlarını döner.
        /// GET api/Accrual/open-debts-by-contract/{contractId}
        /// </summary>
        [HttpGet("open-debts-by-contract/{contractId:guid}")]
        public IActionResult GetOpenDebtsForContract(Guid contractId)
        {
            var result = _service.GetOpenDebtsForContract(contractId);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] AccrualUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest("شناسه نامعتبر است.");
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Update(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}"), Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/settle"), Authorize]
        public IActionResult Settle(Guid id, [FromBody] AccrualSettleDto dto)
        {
            if (id != dto.AccrualId) return BadRequest("شناسه نامعتبر است.");
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Settle(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/reopen"), Authorize]
        public IActionResult Reopen(Guid id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Reopen(id, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/recalc"), Authorize]
        public IActionResult Recalculate(Guid id)
        {
            var r = _service.RecalculateOutstanding(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
