using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AmazonWeb.API.Controllers.v2
{
    [ApiVersion("2.0")]
    public class Shopping : CustomControllerBase
    {
        [Route("[Action]")]
        [HttpGet]
        public IActionResult Get()
        {
            return Content("Hello this is amazon from v2", "text/html");
        }
    }
}
