using Business.Abstract;
using Entities.Concrete;
using Entities.Dtos.TaxPaymentEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxPaymentEntriesController : ControllerBase
    {
        private readonly ITaxPaymentEntryService _service;

        public TaxPaymentEntriesController(ITaxPaymentEntryService service)
        {
            _service = service;
        }

        // Liste (filtreli)
        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] DateTime? fromOccurredAtUtc,
            [FromQuery] DateTime? toOccurredAtUtc,
            [FromQuery] DateTime? fromCreatedAtUtc,
            [FromQuery] DateTime? toCreatedAtUtc,
            [FromQuery] BalanceDirection? direction,
            [FromQuery] bool? approvedOnly,
            [FromQuery] decimal? minAmount,
            [FromQuery] decimal? maxAmount,
            [FromQuery] Guid? ownerDistributionId,
            [FromQuery] Guid? collectionId,
            [FromQuery] Guid? contractId,
            [FromQuery] int? createdByUserId,
            [FromQuery] int? approvedByUserId)
        {
            var filter = new TaxPaymentEntryFilterDto
            {
                FromOccurredAtUtc = fromOccurredAtUtc,
                ToOccurredAtUtc = toOccurredAtUtc,
                FromCreatedAtUtc = fromCreatedAtUtc,
                ToCreatedAtUtc = toCreatedAtUtc,
                Direction = direction,
                ApprovedOnly = approvedOnly,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                OwnerDistributionId = ownerDistributionId,
                CollectionId = collectionId,
                ContractId = contractId,
                CreatedByUserId = createdByUserId,
                ApprovedByUserId = approvedByUserId
            };

            var result = _service.GetAll(filter);
            return Ok(result.Data);
        }

        // Özet
        [HttpGet("summary")]
        public IActionResult GetSummary(
            [FromQuery] DateTime? fromOccurredAtUtc,
            [FromQuery] DateTime? toOccurredAtUtc,
            [FromQuery] BalanceDirection? direction,
            [FromQuery] bool? approvedOnly)
        {
            var filter = new TaxPaymentEntryFilterDto
            {
                FromOccurredAtUtc = fromOccurredAtUtc,
                ToOccurredAtUtc = toOccurredAtUtc,
                Direction = direction,
                ApprovedOnly = approvedOnly
            };

            var result = _service.GetSummary(filter);
            return Ok(result.Data);
        }

        // Tek kayıt
        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id)
        {
            var result = _service.GetById(id);
            return result.Success ? Ok(result.Data) : NotFound(result.Message);
        }

        [HttpGet("{id:guid}/detail")]
        public IActionResult Detail(Guid id)
        {
            var result = _service.GetDetail(id);
            return result.Success ? Ok(result.Data) : NotFound(result.Message);
        }

        // Create
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] TaxPaymentEntryCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var result = _service.Add(dto, userId);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }

        // Update
        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] TaxPaymentEntryUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("شناسه نامعتبر است.");

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var result = _service.Update(dto, userId);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }

        // Delete
        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var result = _service.Delete(id);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }

        // Approve
        [HttpPost("{id:guid}/approve")]
        [Authorize]
        public IActionResult Approve(Guid id)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var result = _service.Approve(id, userId);
            return result.Success ? Ok(result.Message) : BadRequest(result.Message);
        }
    }
}
