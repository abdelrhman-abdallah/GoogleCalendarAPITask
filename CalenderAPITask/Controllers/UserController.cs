using CalenderAPITask.Models;
using CalenderAPITask.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CalenderAPITask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService) 
        { 
            _userService = userService;
        }

        [HttpPost]
        [Route("/signup")]
        public async Task<IActionResult> SignUp([FromBody] UserData newUserCredentials) 
        {
            try
            {
                var token = await _userService.SignUp(newUserCredentials.Gmail, newUserCredentials.Password);
                return CreatedAtAction(nameof(SignUp),new {jwt = token });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpPost]
        [Route("/signin")]
        public async Task<IActionResult> SignIn([FromBody] UserData exisitingUserCredentials) 
        {
            try
            {
                var token = await _userService.SignIn(exisitingUserCredentials.Gmail, exisitingUserCredentials.Password);
                return Ok(new { jwt = token });
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
