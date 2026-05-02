namespace Orange.Training.SecondTask.Models
{
    public class UpdateProfileResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UpdatedFullName { get; set; }
        public string? UpdatedEmail { get; set; }
        public string? UpdatedProfileImageUrl { get; set; }
    }
}