using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class SelectedItemEntity
    {
        [Key]
        public string SelectedItemId { get; set; } = Guid.NewGuid().ToString();
        public string RoomId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ItemId { get; set; } = string.Empty;
    }
}
