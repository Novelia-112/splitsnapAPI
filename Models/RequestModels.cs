namespace SplitSnap.Models
{
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SavePersonalReceiptRequest
    {
        public string ReceiptId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public List<ReceiptItemDto> Items { get; set; } = new();
        public decimal Total { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string LocationName { get; set; } = string.Empty;
    }

    public class ReceiptItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public decimal Price { get; set; }
    }

    public class CreateRoomRequest
    {
        public string ReceiptId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public List<ReceiptItemDto> Items { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class JoinRoomRequest
    {
        public string RoomCode { get; set; } = string.Empty;
    }

    public class SelectItemsRequest
    {
        public List<string> SelectedItems { get; set; } = new();
        public decimal MyTotalBill { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public string? ProofImageUrl { get; set; }
    }

    public class MarkPaidRequest
    {
        public string ParticipantUserId { get; set; } = string.Empty;
    }

    public class SendReminderRequest
    {
        public string RoomId { get; set; } = string.Empty;
        public List<string>? TargetUserIds { get; set; }
    }

    public class RegisterFcmTokenRequest
    {
        public string FcmToken { get; set; } = string.Empty;
    }

    public class TopUpRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class WithdrawRequest
    {
        public decimal Amount { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
    }
}
