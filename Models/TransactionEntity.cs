using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class TransactionEntity
    {
        [Key]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = "pribadi";  
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string People { get; set; } = string.Empty;
        public int Amount { get; set; } = 0;
        public string Status { get; set; } = string.Empty;  
        public string Subtitle { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string? RoomCode { get; set; }
        public string ItemsJson { get; set; } = "[]";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
