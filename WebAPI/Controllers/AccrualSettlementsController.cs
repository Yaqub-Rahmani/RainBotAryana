using Business.Abstract;
using Entities.Dtos.AccrualSettlement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccrualSettlementController : ControllerBase
    {
        private readonly IAccrualSettlementService _service;

        public AccrualSettlementController(IAccrualSettlementService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}"), Authorize]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("by-accrual/{accrualId:guid}"), Authorize]
        public IActionResult GetByAccrual(Guid accrualId)
        {
            var r = _service.GetByAccrual(accrualId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("by-allocation/{allocationId:guid}"), Authorize]
        public IActionResult GetByAllocation(Guid allocationId)
        {
            var r = _service.GetByAllocation(allocationId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpPost, Authorize]
        public IActionResult Add([FromBody] AccrualSettlementCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
