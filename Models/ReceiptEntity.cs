using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class ReceiptEntity
    {
        [Key]
        public string ReceiptId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
