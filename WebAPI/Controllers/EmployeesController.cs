using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.Employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        public EmployeesController(IEmployeeService service) { _service = service; }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] bool? active)
        {
            var r = _service.GetDetails(opt, active);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id}/profile")]
        [AllowAnonymous]
        public IActionResult GetProfile(Guid id)
        {
            var r = _service.GetProfile(id);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] Employee employee)
        {
            var r = _service.Add(employee);
            return r.Success ? Ok(employee) : BadRequest(r.Message);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] Employee employee)
        {
            employee.Id = id;
            var r = _service.Update(employee);
            return r.Success ? Ok(employee) : BadRequest(r.Message);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        [HttpPost("{id}/activate")]
        [Authorize]
        public IActionResult Activate(Guid id)
        {
            var r = _service.Activate(id);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        [HttpPost("{id}/deactivate")]
        [Authorize]
        public IActionResult Deactivate(Guid id)
        {
            var r = _service.Deactivate(id);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        [HttpGet("{id}/work-info")]
        [AllowAnonymous]
        public IActionResult GetWorkInfo(Guid id)
        {
            var r = _service.GetWorkInfo(id);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpPut("{id}/work-info")]
        [Authorize]
        public IActionResult UpsertWorkInfo(Guid id, [FromBody] EmployeeWorkInfoUpsertDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.UpsertWorkInfo(id, dto, userId);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        [HttpGet("{id}/salary-payments/details")]
        [AllowAnonymous]
        public IActionResult GetSalaryPayments(Guid id, [FromQuery] QueryOptions opt)
        {
            var r = _service.GetSalaryPayments(id, opt);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpPost("{id}/fingerprint")]
        [Authorize]
        public async Task<IActionResult> UpsertFingerprint(Guid id, [FromBody] EmployeeFingerprintUpsertDto dto)
        {
            var r = await _service.UpsertFingerprintAsync(id, dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
