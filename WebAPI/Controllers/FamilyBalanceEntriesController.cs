using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FamilyBalanceEntriesController : ControllerBase
    {
        private readonly IFamilyBalanceEntryService _service;
        public FamilyBalanceEntriesController(IFamilyBalanceEntryService service) { _service = service; }

        // Okumalar
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll().Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        //[HttpGet("details")]
        //[AllowAnonymous]
        //public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? familyMemberId)
        //{
        //    var r = _service.GetDetails(opt, familyMemberId);
        //    return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        //}


        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Özet
        [HttpGet("summary")]
        [AllowAnonymous]
        public IActionResult Summary([FromQuery] Guid? familyMemberId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
        {
            var r = _service.GetSummary(familyMemberId, fromUtc, toUtc);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // Yazmalar
        //[HttpPost]
        //[Authorize]
        //public IActionResult Add([FromBody] FamilyBalanceEntry dto)
        //{
        //    var r = _service.Add(dto);
        //    return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        //}

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] FamilyBalanceEntry dto)
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

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? familyMemberId)
        {
            var r = _service.GetDetails(opt, familyMemberId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] FamilyBalanceEntry dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

    }
}
