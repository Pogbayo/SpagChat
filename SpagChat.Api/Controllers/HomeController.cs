using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpagChat.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ApiExplorerSettings(GroupName = "v1")]
        public ActionResult HomePage()
        {
            return Ok("Welcome to SpagChat");
        }
    }
}
