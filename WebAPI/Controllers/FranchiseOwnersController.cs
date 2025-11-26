using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FranchiseOwnersController : ControllerBase
    {
        private readonly IFranchiseOwnerService _service;
        public FranchiseOwnersController(IFranchiseOwnerService service) { _service = service; }

        // Okumalar
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] bool? active)
            => Ok(_service.GetAll(active).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] bool? active)
        {
            var r = _service.GetDetails(opt, active);
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
        public IActionResult Add([FromBody] FranchiseOwner dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] FranchiseOwner dto)
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

        [HttpGet("views/all")]
        [AllowAnonymous]
        public IActionResult GetAllOwnerView([FromQuery] bool? active)
            => Ok(_service.GetAllOwnerView(active).Data);

        [HttpGet("views")]
        [AllowAnonymous]
        public IActionResult GetOwnerViews([FromQuery] QueryOptions opt, [FromQuery] bool? active)
        {
            var r = _service.GetOwnerViews(opt, active);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // GET: /api/FranchiseOwners/{ownerId}/profile
        [HttpGet("{ownerId:guid}/profile")]
        [AllowAnonymous] // okuma uçları gibi anonim bırakmak istersen
        public IActionResult GetProfile([FromRoute] Guid ownerId)
        {
            var result = _service.GetProfile(ownerId);

            if (!result.Success)
            {
                // Service "yok" için ErrorDataResult döndürüyorsa 404 daha doğru
                // (Manager.GetProfile içinde "یافت نشد." ile dönüyorsun)
                return NotFound(result.Message ?? "Not found.");
            }

            if (result.Data == null)
                return NotFound("Not found.");

            return Ok(result.Data);
        }


    }
}
