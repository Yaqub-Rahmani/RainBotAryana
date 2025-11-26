using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.AccrualSettlement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccrualSettlementsController : ControllerBase
    {
        private readonly IAccrualSettlementService _service;
        public AccrualSettlementsController(IAccrualSettlementService service) => _service = service;

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? accrualId)
            => Ok(_service.GetAll(accrualId).Data);

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
        public IActionResult Add([FromBody] AccrualSettlementCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");
            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] AccrualSettlementUpdateDto dto)
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
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");
            var r = _service.Delete(id, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
