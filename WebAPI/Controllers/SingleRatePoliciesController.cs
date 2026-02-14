using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.SingleRatePolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SingleRatePolicyController : ControllerBase
    {
        private readonly ISingleRatePolicyService _service;

        public SingleRatePolicyController(ISingleRatePolicyService service)
        {
            _service = service;
        }

        [HttpGet, Authorize]
        public IActionResult GetAll()
        {
            var r = _service.GetAll();
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}"), Authorize]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("by-type/{type:int}"), Authorize]
        public IActionResult GetByType(int type)
        {
            if (!Enum.IsDefined(typeof(SingleRateType), type))
                return BadRequest("نوع نامعتبر است.");

            var r = _service.GetByType((SingleRateType)type);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("current/{type:int}"), AllowAnonymous]
        public IActionResult GetCurrent(int type, [FromQuery] DateTime? asOfUtc = null)
        {
            if (!Enum.IsDefined(typeof(SingleRateType), type))
                return BadRequest("نوع نامعتبر است.");

            var r = _service.GetCurrentRate((SingleRateType)type, asOfUtc);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost, Authorize]
        public IActionResult Add([FromBody] SingleRatePolicyCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] SingleRatePolicyUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("شناسه با مدل هم‌خوانی ندارد.");

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
    }
}
