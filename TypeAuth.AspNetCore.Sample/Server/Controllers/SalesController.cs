using Microsoft.AspNetCore.Authorization;
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
        private readonly TypeAuthService typeAuthService;

        public SalesController(TypeAuthService typeAuthService)
        {
            this.typeAuthService = typeAuthService;
        }

        // GET: api/<SalesController>
        [HttpGet]
        [TypeAuth(typeof(CRMActions),nameof(CRMActions.Sales),Access.Read)]
        public IActionResult Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }

        // GET api/<SalesController>/5
        [HttpGet("{id}")]
        [TypeAuth(typeof(CRMActions), nameof(CRMActions.Sales), Access.Read)]
        public IActionResult Get(int id)
        {
            return Ok("value");
        }

        // POST api/<SalesController>
        [HttpPost]
        [TypeAuth(typeof(CRMActions), nameof(CRMActions.Sales), Access.Write)]
        public IActionResult Post([FromBody] int discount)
        {
            double discountLimit = double.Parse(typeAuthService.TypeAuthContext.AccessValue(CRMActions.SalesDiscountValue));

            if (discount > discountLimit) 
                return Forbid();

            return Ok();
        }

        // PUT api/<SalesController>/5
        [HttpPut("{id}")]
        [TypeAuth(typeof(CRMActions), nameof(CRMActions.Sales), Access.Write)]
        public IActionResult Put(int id, [FromBody] int discount)
        {
            double discountLimit = double.Parse(typeAuthService.TypeAuthContext.AccessValue(CRMActions.SalesDiscountValue));

            if (discount > discountLimit)
                return Forbid();

            return Ok();
        }

        // DELETE api/<SalesController>/5
        [HttpDelete("{id}")]
        [TypeAuth(typeof(CRMActions), nameof(CRMActions.Sales), Access.Delete)]
        public IActionResult Delete(int id)
        {
            return Ok();
        }
    }
}
