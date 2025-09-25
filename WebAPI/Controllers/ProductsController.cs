using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Performance;
using Core.Aspects.Autofac.Validation;
using Core.Utilities.Paging;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        public ProductsController(IProductService service) { _service = service; }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return result.Success ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpGet("paged")]
        [AllowAnonymous]
        public IActionResult GetPaged([FromQuery] PagedRequest req)
        {
            var result = _service.GetPaged(req);
            return result.Success ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            var result = _service.GetById(id);
            return result.Success ? Ok(result.Data) : NotFound(result.Message);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] Product dto)
        {
            var result = _service.Add(dto);
            return result.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(result.Message);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Product dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var result = _service.Update(dto);
            return result.Success ? Ok() : BadRequest(result.Message);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var result = _service.Delete(id);
            return result.Success ? Ok() : NotFound(result.Message);
        }
    }

}
