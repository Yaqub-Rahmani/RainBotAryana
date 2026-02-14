using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.FamilyMember;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FamilyMemberController : ControllerBase
    {
        private readonly IFamilyMemberService _service;

        public FamilyMemberController(IFamilyMemberService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] bool includeInactive = false)
        {
            var r = _service.GetAll(includeInactive);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // ---------------- Detail screen ----------------
        [HttpGet("{id:guid}/profile")]
        [AllowAnonymous]
        public IActionResult GetProfile(Guid id)
        {
            var r = _service.GetProfile(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }


        [HttpPost("{id:guid}/fingerprint")]
        public async Task<IActionResult> UpsertFingerprint(Guid id, [FromBody] FamilyMemberFingerprintUpsertDto dto)
        {
            var r = await _service.UpsertFingerprintAsync(id, dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost]
        public IActionResult Add([FromBody] FamilyMemberCreateDto dto)
        {
            var r = _service.Add(dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        public IActionResult Update(Guid id, [FromBody] FamilyMemberUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("شناسه با مدل هم‌خوانی ندارد.");

            var r = _service.Update(dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpGet("views/all")]
        [AllowAnonymous]
        public IActionResult GetAllFamilyView([FromQuery] bool? active)
    => Ok(_service.GetAllFamilyView(active).Data);
    }
}
