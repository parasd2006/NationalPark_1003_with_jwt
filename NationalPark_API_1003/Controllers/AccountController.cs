using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NationalPark_API_1003.Models;
using NationalPark_API_1003.Repository.IRepository;

namespace NationalPark_API_1003.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        public AccountController(IUserRepository userRepository)
        {
            _userRepository= userRepository;
        }
        [HttpPost("register")]
        public IActionResult Register([FromBody]User user)
        {
            if(ModelState.IsValid)
            {
                var isUniqueUser = _userRepository.IsUniqueUser(user.UserName);
                if (!isUniqueUser) return BadRequest("User in use");
                var userInfo = _userRepository.Register(user.UserName, user.Password);
                if (userInfo == null) return NotFound();
                user = userInfo;


            }
            return Ok(user);
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody]User user)
        {
            var userInDb = _userRepository.Authenticate(user.UserName, user.Password);
            if (userInDb == null) return BadRequest("wrong user/ password...");
            return Ok(userInDb);
        }
    }
}
