namespace SplitSnap.Models
{
    // ── Auth ──────────────────────────────
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

    // ── Item struk / split (selalu: name, quantity, unitPrice, totalPrice) ──
    public class ItemDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public int UnitPrice { get; set; } = 0;
        public int TotalPrice { get; set; } = 0;
    }

    // ── Transaction (sama seperti TransactionItem Flutter) ──
    public class CreateTransactionRequest
    {
        public string Type { get; set; } = "pribadi";   // "pribadi" | "splitBill"
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

    // ── Wallet (sama seperti WalletTransaction Flutter) ──
    public class TopUpRequest
    {
        public int Amount { get; set; }
    }

    public class WithdrawRequest
    {
        public int Amount { get; set; }
    }

    // ── Split / Room (sama seperti dokumen room Firestore) ──
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

    // ── Notification ──────────────────────────────────
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

//namespace SplitSnap.Models
//{
//    public class RegisterRequest
//    {
//        public string FullName { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public string Phone { get; set; } = string.Empty;
//        public string Password { get; set; } = string.Empty;
//    }

//    public class LoginRequest
//    {
//        public string Email { get; set; } = string.Empty;
//        public string Password { get; set; } = string.Empty;
//    }

//    public class SavePersonalReceiptRequest
//    {
//        public string ReceiptId { get; set; } = string.Empty;
//        public string StoreName { get; set; } = string.Empty;
//        public string Date { get; set; } = string.Empty;
//        public List<ReceiptItemDto> Items { get; set; } = new();
//        public decimal Total { get; set; }
//        public double? Latitude { get; set; }
//        public double? Longitude { get; set; }
//        public string LocationName { get; set; } = string.Empty;
//    }

//    public class ReceiptItemDto
//    {
//        public string ItemName { get; set; } = string.Empty;
//        public int Quantity { get; set; } = 1;
//        public decimal Price { get; set; }
//    }

//    public class CreateRoomRequest
//    {
//        public string ReceiptId { get; set; } = string.Empty;
//        public string RoomName { get; set; } = string.Empty;
//        public List<ReceiptItemDto> Items { get; set; } = new();
//        public decimal Total { get; set; }
//    }

//    public class JoinRoomRequest
//    {
//        public string RoomCode { get; set; } = string.Empty;
//    }

//    public class SelectItemsRequest
//    {
//        public List<string> SelectedItems { get; set; } = new();
//        public decimal MyTotalBill { get; set; }
//    }

//    public class ConfirmPaymentRequest
//    {
//        public string PaymentMethod { get; set; } = string.Empty;
//        public string? ProofImageUrl { get; set; }
//    }

//    public class MarkPaidRequest
//    {
//        public string ParticipantUserId { get; set; } = string.Empty;
//    }

//    public class SendReminderRequest
//    {
//        public string RoomId { get; set; } = string.Empty;
//        public List<string>? TargetUserIds { get; set; }
//    }

//    public class RegisterFcmTokenRequest
//    {
//        public string FcmToken { get; set; } = string.Empty;
//    }

//    public class TopUpRequest
//    {
//        public decimal Amount { get; set; }
//        public string PaymentMethod { get; set; } = string.Empty;
//    }

//    public class WithdrawRequest
//    {
//        public decimal Amount { get; set; }
//        public string BankAccountNumber { get; set; } = string.Empty;
//        public string BankName { get; set; } = string.Empty;
//    }
//}
