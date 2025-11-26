using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _service;
        public TenantsController(ITenantService service) { _service = service; }

        // Reads
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] bool? onlyActive)
            => Ok(_service.GetAll(onlyActive).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] bool? onlyActive)
        {
            var r = _service.GetDetails(opt, onlyActive);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Writes
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] Tenant dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] Tenant dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var r = _service.Update(dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        // Activate / Deactivate
        [HttpPost("{id:guid}/activate")]
        [Authorize]
        public IActionResult Activate(Guid id)
        {
            var r = _service.Activate(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/deactivate")]
        [Authorize]
        public IActionResult Deactivate(Guid id)
        {
            var r = _service.Deactivate(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        /// <summary>
        /// Sistemdeki kiracıların özet görünüm listesini döner.
        /// </summary>
        /// <param name="active">true/false/null → aktif/pasif/tümü</param>
        [HttpGet("views/all")]
        [AllowAnonymous]
        public IActionResult GetAllTenantView([FromQuery] bool? active)
        {
            var result = _service.GetAllTenantView(active);
            return Ok(result.Data);
        }
    }
}
