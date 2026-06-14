using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class WalletTransactionEntity
    {
        [Key]
        public string WalletTransactionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public int Amount { get; set; } = 0;               // bisa negatif
        public string Status { get; set; } = string.Empty;  // "Lunas" | "Pending"
        public string Type { get; set; } = string.Empty;    // "topUp" | "withdraw" | "splitPay"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

//using System.ComponentModel.DataAnnotations;

//namespace SplitSnap.Models
//{
//    public class WalletTransactionEntity
//    {
//        [Key]
//        public string WalletTransactionId { get; set; } = Guid.NewGuid().ToString();
//        public string UserId { get; set; } = string.Empty;
//        public string Type { get; set; } = string.Empty;
//        public decimal Amount { get; set; }
//        public decimal BalanceAfter { get; set; }
//        public string Status { get; set; } = string.Empty;
//        public string PaymentMethod { get; set; } = string.Empty;
//        public string? BankAccountNumber { get; set; }
//        public string? BankName { get; set; }
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//    }
//}
