using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AmazonWeb.API.Controllers
{
    public class Shopping : CustomControllerBase
    {
        [Route("[Action]")]
        [HttpGet]
        public IActionResult Get()
        {
            return Content("Hello this is amazon", "text/html");
        }
    }
}
