using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using AmazonWeb.Core.DTO.AccountDTO;
using Microsoft.AspNetCore.Mvc;

namespace AmazonWeb.API.Controllers.v1
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    public class AccountController : CustomControllerBase
    {
        [HttpPost] 
        [Route("[Action]")]
        public IActionResult Register(RegisterDTO registerDTO )
        {
            return Ok("Registration successful");
        }
    }
}
