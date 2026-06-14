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
        public int Amount { get; set; } = 0;            
        public string Status { get; set; } = string.Empty;  
        public string Type { get; set; } = string.Empty;    

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
