using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class UserEntity
    {
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? FcmToken { get; set; }
        public int Balance { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
