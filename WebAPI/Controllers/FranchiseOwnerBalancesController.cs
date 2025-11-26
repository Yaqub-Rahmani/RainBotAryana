using Business.Abstract;
using Core.Utilities.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FranchiseOwnerBalancesController : ControllerBase
    {
        private readonly IFranchiseOwnerBalanceService _service;
        public FranchiseOwnerBalancesController(IFranchiseOwnerBalanceService service) => _service = service;

        // GET api/FranchiseOwnerBalances
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] bool? onlyActiveOwners)
            => Ok(_service.GetAll(onlyActiveOwners).Data);

        // GET api/FranchiseOwnerBalances/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // GET api/FranchiseOwnerBalances/by-owner/{ownerId}
        [HttpGet("by-owner/{ownerId:guid}")]
        [AllowAnonymous]
        public IActionResult GetByOwnerId(Guid ownerId)
        {
            var r = _service.GetByOwnerId(ownerId);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // GET api/FranchiseOwnerBalances/list-by-owner/{ownerId}
        [HttpGet("list-by-owner/{ownerId:guid}")]
        [AllowAnonymous]
        public IActionResult ListByOwnerId(Guid ownerId)
            => Ok(_service.ListByOwnerId(ownerId).Data);

        // GET api/FranchiseOwnerBalances/details
        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt)
        {
            var r = _service.GetDetails(opt);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // GET api/FranchiseOwnerBalances/{id}/detail
        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // POST api/FranchiseOwnerBalances/ensure
        [HttpPost("ensure")]
        [Authorize]
        public IActionResult Ensure([FromBody] EnsureDto dto)
        {
            var r = _service.EnsureCreatedForOwner(dto.OwnerId, dto.UpdatedBy);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        // POST api/FranchiseOwnerBalances/in
        [HttpPost("in")]
        [Authorize]
        public IActionResult UpdateIn([FromBody] MutationDto dto)
        {
            var r = _service.UpdateIn(dto.OwnerId, dto.Amount, dto.UpdatedBy, dto.WhenUtc, dto.Note);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        // POST api/FranchiseOwnerBalances/out
        [HttpPost("out")]
        [Authorize]
        public IActionResult UpdateOut([FromBody] MutationDto dto)
        {
            var r = _service.UpdateOut(dto.OwnerId, dto.Amount, dto.UpdatedBy, dto.WhenUtc, dto.Note);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        // PUT api/FranchiseOwnerBalances/set
        [HttpPut("set")]
        [Authorize]
        public IActionResult SetBalance([FromBody] SetDto dto)
        {
            var r = _service.SetBalance(dto.OwnerId, dto.Balance, dto.UpdatedBy, dto.WhenUtc);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        public class EnsureDto { public Guid OwnerId { get; set; } public Guid UpdatedBy { get; set; } }
        public class MutationDto
        {
            public Guid OwnerId { get; set; }
            public decimal Amount { get; set; }
            public Guid UpdatedBy { get; set; }
            public DateTime? WhenUtc { get; set; }
            public string? Note { get; set; }
        }
        public class SetDto
        {
            public Guid OwnerId { get; set; }
            public decimal Balance { get; set; }
            public Guid UpdatedBy { get; set; }
            public DateTime? WhenUtc { get; set; }
        }
    }
}
