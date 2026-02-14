using Business.Abstract;
using Core.Utilities.Query;
using Entities.Dtos.employee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeTransactionController : ControllerBase
    {
        private readonly IEmployeeTransactionService _service;
        public EmployeeTransactionController(IEmployeeTransactionService service) { _service = service; }

        [HttpGet("by-employee/{employeeId:guid}")]
        [Authorize]
        public IActionResult GetByEmployee(Guid employeeId, [FromQuery] QueryOptions opt, [FromQuery] int? year, [FromQuery] int? month)
        {
            var r = _service.GetByEmployee(employeeId, opt, year, month);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] EmployeeTransactionCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
