using Microsoft.AspNetCore.Mvc;
using Orange.Training.SecondTask.Models;
using Orange.Training.SecondTask.Services;

namespace Orange.Training.SecondTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UserController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPut("update-profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            if (id <= 0) return BadRequest("Invalid User ID");

            var response = await _authService.UpdateUserProfile(id, request);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
    }
}