using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterReadingsController : ControllerBase
    {
        private readonly IMeterReadingService _service;
        public MeterReadingsController(IMeterReadingService service) { _service = service; }

        // Okumalar
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? shopId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] bool? finalized)
            => Ok(_service.GetAll(shopId, year, month, finalized).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? shopId, [FromQuery] int? year, [FromQuery] int? month, [FromQuery] bool? finalized)
        {
            var r = _service.GetDetails(opt, shopId, year, month, finalized);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Yazmalar
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] MeterReading dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] MeterReading dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var r = _service.Update(dto);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        // İşlemsel
        [HttpPost("{id:guid}/finalize")]
        [Authorize]
        public IActionResult Finalize(Guid id)
        {
            var r = _service.Finalize(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/unfinalize")]
        [Authorize]
        public IActionResult Unfinalize(Guid id)
        {
            var r = _service.Unfinalize(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/sync-accrual")]
        [Authorize]
        public IActionResult SyncAccrual(Guid id)
        {
            var r = _service.SyncAccrual(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
