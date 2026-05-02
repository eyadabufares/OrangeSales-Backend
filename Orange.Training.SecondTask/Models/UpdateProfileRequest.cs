namespace Orange.Training.SecondTask.Models
{
    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string CurrentPassword { get; set; } = string.Empty;

        public string? NewPassword { get; set; }

        public string? ProfileImageUrl { get; set; }
    }
}