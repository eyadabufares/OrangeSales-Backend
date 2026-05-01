using Microsoft.AspNetCore.Mvc;
using Orange.Training.SecondTask.Models;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = _authService.Register(request);

            if (result)
            {
                return Ok(new { message = "تم إنشاء الحساب بنجاح" });
            }

            return BadRequest(new { message = "حدث خطأ أثناء إنشاء الحساب، الإيميل مستخدم من قبل" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isValid = _authService.Login(request);

            if (isValid)
            {
                return Ok(new { message = "تم تسجيل الدخول بنجاح" });
            }

            return Unauthorized(new { message = "الإيميل أو كلمة المرور خطأ" });
        }
    }
}