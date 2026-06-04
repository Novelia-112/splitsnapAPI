using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class UserEntity
    {
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FcmToken { get; set; }
        public decimal Balance { get; set; } = 0;
        public int TotalTransactions { get; set; } = 0;
        public int TotalSplitBills { get; set; } = 0;
        public decimal TotalSpending { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
