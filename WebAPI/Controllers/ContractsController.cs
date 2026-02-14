using Business.Abstract;
using Business.ValidationRules.FluentValidation.Contract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _service;
        public ContractsController(IContractService service) => _service = service;

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll([FromQuery] bool includeArchived = false)
            => Ok(_service.GetAll(includeArchived).Data);

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

        [HttpGet("list-items")]
        [AllowAnonymous]
        public IActionResult GetListItems([FromQuery] QueryOptions opt)
        {
            var result = _service.GetListItems(opt);

            return result.Success
                ? Ok(result.Data)
                : BadRequest(result.Message);
        }



        [HttpPost, Authorize]
        public IActionResult Add([FromBody] ContractCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] ContractUpdateDto dto)
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

        [HttpPost("{id:guid}/archive"), Authorize]
        public IActionResult Archive(Guid id, [FromBody] string? reason)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");
            var r = _service.Archive(id, userId, reason);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/unarchive"), Authorize]
        public IActionResult Unarchive(Guid id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");
            var r = _service.Unarchive(id, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
