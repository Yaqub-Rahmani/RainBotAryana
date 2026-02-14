using Business.Abstract;
using Core.Utilities.Query;
using Entities.Dtos.AdminUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _service;
        public AdminUsersController(IAdminUserService service)
        {
            _service = service;
        }

        // GET: api/AdminUsers/details?page=1&pageSize=60&search=...&sort=Name:asc
        [HttpGet("details")]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] bool? active)
        {
            var r = _service.GetDetails(opt, active);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // GET: api/AdminUsers?active=true
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool? active)
        {
            var r = _service.GetAll(active);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // GET: api/AdminUsers/5
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // POST: api/AdminUsers
        [HttpPost]
        public IActionResult Add([FromBody] AdminUserCreateDto dto)
        {
            var r = _service.Add(dto);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        // PUT: api/AdminUsers/5
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] AdminUserUpdateDto dto)
        {
            var r = _service.Update(id, dto);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        // DELETE: api/AdminUsers/5   (soft disable -> Status=false)
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok() : BadRequest(r.Message);
        }
    }
}
