namespace SplitSnap.Models
{
    public class RegisterRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public int UnitPrice { get; set; } = 0;
        public int TotalPrice { get; set; } = 0;
    }

    public class CreateTransactionRequest
    {
        public string Type { get; set; } = "pribadi";   
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string People { get; set; } = string.Empty;
        public int Amount { get; set; } = 0;
        public string Status { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string? RoomCode { get; set; }
        public List<ItemDto> Items { get; set; } = new();
    }

    public class TopUpRequest
    {
        public int Amount { get; set; }
    }

    public class WithdrawRequest
    {
        public int Amount { get; set; }
    }

    public class ParticipantDto
    {
        public string Uid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public bool IsPaid { get; set; } = false;
        public bool IsHost { get; set; } = false;
        public List<ItemDto> SelectedItems { get; set; } = new();
        public int Total { get; set; } = 0;
    }

    public class CreateRoomRequest
    {
        public string RoomCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public List<ItemDto> Items { get; set; } = new();
        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = "Host";
    }

    public class JoinRoomRequest
    {
        public string RoomCode { get; set; } = string.Empty;
        public string Uid { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class SelectItemsRequest
    {
        public string Uid { get; set; } = string.Empty;
        public List<ItemDto> SelectedItems { get; set; } = new();
    }

    public class MarkPaidRequest
    {
        public string Uid { get; set; } = string.Empty;
    }

    public class ConfirmPaymentRequest
    {
        public string Uid { get; set; } = string.Empty;
    }

    public class BillDto
    {
        public string Uid { get; set; } = string.Empty;
        public int Total { get; set; } = 0;
    }

    public class SendReminderRequest
    {
        public string RoomCode { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public List<BillDto> Bills { get; set; } = new();
    }

    public class RegisterTokenRequest
    {
        public string Uid { get; set; } = string.Empty;
        public string FcmToken { get; set; } = string.Empty;
    }
}
