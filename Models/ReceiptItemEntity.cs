using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class ReceiptItemEntity
    {
        [Key]
        public string ItemId { get; set; } = Guid.NewGuid().ToString();
        public string ReceiptId { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
    }
}
