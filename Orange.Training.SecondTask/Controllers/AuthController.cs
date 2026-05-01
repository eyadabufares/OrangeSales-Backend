using Microsoft.AspNetCore.Mvc;
using Orange.Training.SecondTask.Models;
using Orange.Training.SecondTask.Services;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.Register(request);

                if (result)
                {
                    return Ok(new { message = "تم إنشاء الحساب بنجاح" });
                }

                return BadRequest(new { message = "حدث خطأ أثناء إنشاء الحساب، الإيميل قد يكون مستخدماً من قبل" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "خطأ داخلي في السيرفر", details = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var isValid = await _authService.Login(request);

                if (isValid)
                {
                    return Ok(new { message = "تم تسجيل الدخول بنجاح" });
                }

                return Unauthorized(new { message = "الإيميل أو كلمة المرور غير صحيحة" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "خطأ أثناء محاولة تسجيل الدخول", details = ex.Message });
            }
        }
    }
}