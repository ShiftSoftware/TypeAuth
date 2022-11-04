using Microsoft.AspNetCore.Mvc;
using ShiftSoftware.TypeAuth.Core;
using TypeAuth.AspNetCore.Sample.Shared.ActionTrees;
using TypeAuth.AspNetCore.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TypeAuth.AspNetCore.Sample.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        

        // GET: api/<SalesController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        // GET api/<SalesController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok("value");
        }

        // POST api/<SalesController>
        [HttpPost]
        public IActionResult Post([FromBody] int discount)
        {
            return Ok();
        }

        // PUT api/<SalesController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] int discount)
        {
            return Ok();
        }

        // DELETE api/<SalesController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return Ok();
        }
    }
}
