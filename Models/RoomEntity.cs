using System.ComponentModel.DataAnnotations;

namespace SplitSnap.Models
{
    public class RoomEntity
    {
        [Key]
        public string RoomCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string ItemsJson { get; set; } = "[]";
        public string CreatedBy { get; set; } = string.Empty;
        public bool ReminderTriggered { get; set; } = false;
        public string ParticipantsJson { get; set; } = "[]";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
