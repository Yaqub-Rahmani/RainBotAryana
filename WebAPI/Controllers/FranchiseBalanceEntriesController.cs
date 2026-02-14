using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.FranchiseOwner.entry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FranchiseBalanceEntriesController : ControllerBase
    {
        private readonly IFranchiseBalanceEntryService _service;
        public FranchiseBalanceEntriesController(IFranchiseBalanceEntryService service) { _service = service; }


        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? ownerId)
        {
            var r = _service.GetDetails(opt, ownerId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // Yazmalar
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] FranchiseBalanceEntryCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            if (!r.Success) return BadRequest(r.Message);

            return Ok(dto);

        }


    }
}
