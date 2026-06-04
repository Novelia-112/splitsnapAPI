using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class TransactionEntity
    {
        [Key]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Date { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string? RoomId { get; set; }
        public string? ReceiptId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
