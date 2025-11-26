using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.MeterReadings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterReadingsController : ControllerBase
    {
        private readonly IMeterReadingService _service;
        public MeterReadingsController(IMeterReadingService service) => _service = service;

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? shopId, [FromQuery] Guid? contractId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] bool? finalized)
            => Ok(_service.GetAll(shopId, contractId, year, month, finalized).Data);

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

        [HttpPost] // veya [HttpPost("create")] çakışma varsa
        [Authorize]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public IActionResult Add([FromBody] MeterReadingCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");


            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }


        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] MeterReadingUpdateDto dto)
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

        [HttpPost("{id:guid}/finalize"), Authorize]
        public IActionResult Finalize(Guid id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");
            var r = _service.Finalize(id, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/unfinalize"), Authorize]
        public IActionResult Unfinalize(Guid id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");
            var r = _service.Unfinalize(id, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpGet("new-context/{contractId:guid}")]
        [AllowAnonymous]
        public IActionResult GetNewContext(Guid contractId)
        {
            var r = _service.GetNewContext(contractId);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

    }
}
