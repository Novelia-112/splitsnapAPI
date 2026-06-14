using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class TransactionEntity
    {
        [Key]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = "pribadi";       // "pribadi" | "splitBill"
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string People { get; set; } = string.Empty;
        public int Amount { get; set; } = 0;
        public string Status { get; set; } = string.Empty;  // "Pribadi" | "Lunas" | "Belum"
        public string Subtitle { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string? RoomCode { get; set; }
        public string ItemsJson { get; set; } = "[]";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

//using System.ComponentModel.DataAnnotations;

//namespace SplitSnap.Models
//{
//    public class TransactionEntity
//    {
//        [Key]
//        public string TransactionId { get; set; } = Guid.NewGuid().ToString();
//        public string UserId { get; set; } = string.Empty;
//        public string Type { get; set; } = string.Empty;
//        public string StoreName { get; set; } = string.Empty;
//        public decimal Total { get; set; }
//        public string Date { get; set; } = string.Empty;
//        public double? Latitude { get; set; }
//        public double? Longitude { get; set; }
//        public string LocationName { get; set; } = string.Empty;
//        public string PaymentStatus { get; set; } = string.Empty;
//        public string? RoomId { get; set; }
//        public string? ReceiptId { get; set; }
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    }
//}
