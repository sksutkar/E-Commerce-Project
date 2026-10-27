using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult PublicApi()
        {
            return Ok(new
            {
                message = "This API is public."
            });
        }

        [Authorize]
        [HttpGet("secure")]
        public IActionResult SecureApi()
        {
            return Ok(new
            {
                message = "JWT Authentication Successful!",
                user = User.Identity?.Name
            });
        }
    }
}