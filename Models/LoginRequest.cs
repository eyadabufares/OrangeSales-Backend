using System.ComponentModel.DataAnnotations;

namespace Orange.Training.SecondTask.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "الإيميل مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "يجب أن تكون كلمة المرور 6 خانات على الأقل")]
        public string Password { get; set; }
    }
}