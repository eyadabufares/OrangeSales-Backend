using System.ComponentModel.DataAnnotations.Schema;

namespace Orange.Training.SecondTask.Models
{
    [Table("users")]
    public class User
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("fullname")]
        public string FullName { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("passwordhash")]
        public string PasswordHash { get; set; }

        [Column("profileimageurl")]
        public string? ProfileImageUrl { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}