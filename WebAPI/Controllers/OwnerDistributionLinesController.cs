using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerDistributionLinesController : ControllerBase
    {
        private readonly IOwnerDistributionLineService _service;
        public OwnerDistributionLinesController(IOwnerDistributionLineService service) { _service = service; }

        // Okumalar
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? distributionId)
            => Ok(_service.GetAll(distributionId).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? distributionId)
        {
            var r = _service.GetDetails(opt, distributionId);
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
        public IActionResult Add([FromBody] OwnerDistributionLine dto, [FromQuery] bool alignWithHeader = true)
        {
            var r = _service.Add(dto, alignWithHeader);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] OwnerDistributionLine dto, [FromQuery] bool alignWithHeader = true)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var r = _service.Update(dto, alignWithHeader);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        // Recalc
        [HttpPost("{distributionId:guid}/recalc")]
        [Authorize]
        public IActionResult Recalc(Guid distributionId)
        {
            var r = _service.RecalcFromHeader(distributionId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
