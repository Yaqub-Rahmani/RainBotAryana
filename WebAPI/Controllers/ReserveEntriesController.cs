using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReserveEntriesController : ControllerBase
    {
        private readonly IReserveEntryService _service;
        public ReserveEntriesController(IReserveEntryService service) { _service = service; }

        // Okumalar
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? accountId, [FromQuery] BalanceDirection? dir, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
            => Ok(_service.GetAll(accountId, dir, fromUtc, toUtc).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? accountId, [FromQuery] BalanceDirection? dir, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
        {
            var r = _service.GetDetails(opt, accountId, dir, fromUtc, toUtc);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Bakiye
        [HttpGet("balance/{accountId:guid}")]
        [AllowAnonymous]
        public IActionResult GetBalance(Guid accountId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
        {
            var r = _service.GetBalance(accountId, fromUtc, toUtc);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // Yazmalar
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] ReserveEntry dto, [FromQuery] bool allowOverdraw = false)
        {
            var r = _service.Add(dto, allowOverdraw);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] ReserveEntry dto, [FromQuery] bool allowOverdraw = false)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var r = _service.Update(dto, allowOverdraw);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
