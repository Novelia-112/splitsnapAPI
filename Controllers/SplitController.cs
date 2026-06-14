using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSnap.Data;
using SplitSnap.Models;
using SplitSnap.Services;
using System.Text.Json;

namespace SplitSnap.Controllers
{
    [ApiController]
    [Route("split")]
    [Authorize]
    public class SplitController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public SplitController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        private static string ComputeInitials(string name)
        {
            var parts = name.Trim().Split(' ').Where(w => w.Length > 0).Take(2);
            var initials = string.Concat(parts.Select(w => w[0])).ToUpper();
            return string.IsNullOrEmpty(initials) ? "H" : initials;
        }

        private static object ToRoomJson(RoomEntity r)
        {
            var items = JsonSerializer.Deserialize<List<ItemDto>>(r.ItemsJson) ?? new();
            var participants = JsonSerializer.Deserialize<List<ParticipantDto>>(r.ParticipantsJson) ?? new();

            return new
            {
                roomCode = r.RoomCode,
                storeName = r.StoreName,
                date = r.Date,
                items,
                createdBy = r.CreatedBy,
                reminderTriggered = r.ReminderTriggered,
                participants,
                bills = new List<object>()
            };
        }

        [HttpPost("create-room")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            if (string.IsNullOrEmpty(request.RoomCode) || string.IsNullOrEmpty(request.StoreName))
                return BadRequest(new { error = true, message = "data tidak valid" });

            var roomCode = request.RoomCode.ToUpper().Trim();
            var existing = await _context.Rooms.FindAsync(roomCode);
            if (existing != null)
                return BadRequest(new { error = true, message = "room code sudah dipakai" });

            var hostUid = string.IsNullOrEmpty(request.CreatedBy) ? userId : request.CreatedBy;
            var participants = new List<ParticipantDto>
            {
                new()
                {
                    Uid = hostUid,
                    Name = request.CreatedByName,
                    Initials = ComputeInitials(request.CreatedByName),
                    IsPaid = true,
                    IsHost = true,
                    SelectedItems = new(),
                    Total = 0
                }
            };

            var room = new RoomEntity
            {
                RoomCode = roomCode,
                StoreName = request.StoreName,
                Date = request.Date,
                ItemsJson = JsonSerializer.Serialize(request.Items),
                CreatedBy = hostUid,
                ReminderTriggered = false,
                ParticipantsJson = JsonSerializer.Serialize(participants),
                CreatedAt = DateTime.UtcNow
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Room berhasil dibuat",
                room = ToRoomJson(room)
            });
        }

        [HttpPost("join-room")]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == request.RoomCode.ToUpper().Trim());

            if (room == null)
                return NotFound(new { error = true, message = "kode room tidak ditemukan" });

            var participants = JsonSerializer.Deserialize<List<ParticipantDto>>(room.ParticipantsJson) ?? new();
            var uid = string.IsNullOrEmpty(request.Uid) ? userId : request.Uid;

            if (!participants.Any(p => p.Uid == uid))
            {
                var displayName = string.IsNullOrEmpty(request.DisplayName) ? "User" : request.DisplayName;

                participants.Add(new ParticipantDto
                {
                    Uid = uid,
                    Name = displayName,
                    Initials = ComputeInitials(displayName),
                    IsPaid = false,
                    IsHost = false,
                    SelectedItems = new(),
                    Total = 0
                });

                room.ParticipantsJson = JsonSerializer.Serialize(participants);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Berhasil join room",
                room = ToRoomJson(room)
            });
        }

        [HttpGet("rooms/{room_code}")]
        public async Task<IActionResult> GetRoomDetail(string room_code)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == room_code.ToUpper().Trim());

            if (room == null)
                return NotFound(new { error = true, message = "room tidak ditemukan" });

            return Ok(ToRoomJson(room));
        }

        [HttpPut("rooms/{room_code}/select-items")]
        public async Task<IActionResult> SelectItems(string room_code, [FromBody] SelectItemsRequest request)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == room_code.ToUpper().Trim());

            if (room == null)
                return NotFound(new { error = true, message = "room tidak ditemukan" });

            var participants = JsonSerializer.Deserialize<List<ParticipantDto>>(room.ParticipantsJson) ?? new();
            var participant = participants.FirstOrDefault(p => p.Uid == request.Uid);
            if (participant == null)
                return NotFound(new { error = true, message = "peserta tidak ditemukan di room ini" });

            participant.SelectedItems = request.SelectedItems;
            participant.Total = request.SelectedItems.Sum(i => i.TotalPrice);

            room.ParticipantsJson = JsonSerializer.Serialize(participants);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Pilihan item tersimpan",
                room = ToRoomJson(room)
            });
        }

        [HttpPut("rooms/{room_code}/confirm-payment")]
        public async Task<IActionResult> ConfirmPayment(string room_code, [FromBody] ConfirmPaymentRequest request)
            => await MarkParticipantPaid(room_code, request.Uid);

        [HttpPut("rooms/{room_code}/mark-paid")]
        public async Task<IActionResult> MarkPaid(string room_code, [FromBody] MarkPaidRequest request)
            => await MarkParticipantPaid(room_code, request.Uid);

        private async Task<IActionResult> MarkParticipantPaid(string room_code, string uid)
        {
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == room_code.ToUpper().Trim());

            if (room == null)
                return NotFound(new { error = true, message = "room tidak ditemukan" });

            var participants = JsonSerializer.Deserialize<List<ParticipantDto>>(room.ParticipantsJson) ?? new();
            var participant = participants.FirstOrDefault(p => p.Uid == uid);
            if (participant == null)
                return NotFound(new { error = true, message = "peserta tidak ditemukan" });

            participant.IsPaid = true;
            room.ParticipantsJson = JsonSerializer.Serialize(participants);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Status pembayaran diperbarui",
                room = ToRoomJson(room)
            });
        }
    }
}

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using SplitSnap.Data;
//using SplitSnap.Models;
//using SplitSnap.Services;

//namespace SplitSnap.Controllers
//{
//    [ApiController]
//    [Route("split")]
//    [Authorize]
//    public class SplitController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly JwtService _jwtService;

//        public SplitController(AppDbContext context, JwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        [HttpPost("create-room")]
//        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var receiptExists = await _context.Receipts.AnyAsync(r => r.ReceiptId == request.ReceiptId);
//            if (!receiptExists)
//                return NotFound(new { error = true, message = "receipt tidak ditemukan" });

//            var roomCode = GenerateRoomCode();

//            var room = new RoomEntity
//            {
//                RoomId = Guid.NewGuid().ToString(),
//                RoomCode = roomCode,
//                RoomName = request.RoomName,
//                CreatorId = userId,
//                ReceiptId = request.ReceiptId,
//                Status = "open",
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Rooms.Add(room);

//            _context.RoomParticipants.Add(new RoomParticipantEntity
//            {
//                ParticipantId = Guid.NewGuid().ToString(),
//                RoomId = room.RoomId,
//                UserId = userId,
//                PaymentStatus = "paid",
//                JoinedAt = DateTime.UtcNow
//            });

//            var user = await _context.Users.FindAsync(userId);
//            if (user != null)
//                user.TotalSplitBills++;

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                room_id = room.RoomId,
//                room_code = roomCode,
//                qr_code_url = $"/split/rooms/{room.RoomId}/qr",
//                room_name = room.RoomName,
//                creator_id = userId,
//                status = room.Status
//            });
//        }

//        [HttpPost("join-room")]
//        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var room = await _context.Rooms
//                .FirstOrDefaultAsync(r => r.RoomCode == request.RoomCode.ToUpper());

//            if (room == null)
//                return NotFound(new { error = true, message = "kode room tidak ditemukan/room ditutup" });

//            if (room.Status == "closed")
//                return BadRequest(new { error = true, message = "kode room tidak ditemukan/room ditutup" });

//            var alreadyJoined = await _context.RoomParticipants
//                .AnyAsync(p => p.RoomId == room.RoomId && p.UserId == userId);

//            if (!alreadyJoined)
//            {
//                _context.RoomParticipants.Add(new RoomParticipantEntity
//                {
//                    ParticipantId = Guid.NewGuid().ToString(),
//                    RoomId = room.RoomId,
//                    UserId = userId,
//                    PaymentStatus = "unpaid",
//                    JoinedAt = DateTime.UtcNow
//                });

//                await _context.SaveChangesAsync();
//            }

//            var creator = await _context.Users.FindAsync(room.CreatorId);
//            var items = await _context.ReceiptItems
//                .Where(i => i.ReceiptId == room.ReceiptId)
//                .ToListAsync();

//            var participants = await _context.RoomParticipants
//                .Where(p => p.RoomId == room.RoomId)
//                .ToListAsync();

//            return Ok(new
//            {
//                room_id = room.RoomId,
//                room_name = room.RoomName,
//                creator_name = creator?.FullName ?? string.Empty,
//                items = items.Select(i => new
//                {
//                    item_id = i.ItemId,
//                    item_name = i.ItemName,
//                    quantity = i.Quantity,
//                    price = i.Price
//                }),
//                participants = participants.Select(p => new
//                {
//                    user_id = p.UserId,
//                    payment_status = p.PaymentStatus
//                })
//            });
//        }

//        [HttpGet("rooms/{room_id}")]
//        public async Task<IActionResult> GetRoomDetail(string room_id)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == room_id);
//            if (room == null)
//                return NotFound(new { error = true, message = "room tidak ditemukan" });

//            var items = await _context.ReceiptItems
//                .Where(i => i.ReceiptId == room.ReceiptId)
//                .ToListAsync();

//            var participants = await _context.RoomParticipants
//                .Where(p => p.RoomId == room_id)
//                .ToListAsync();

//            var participantDetails = new List<object>();
//            foreach (var p in participants)
//            {
//                var user = await _context.Users.FindAsync(p.UserId);
//                var selectedItems = await _context.SelectedItems
//                    .Where(s => s.RoomId == room_id && s.UserId == p.UserId)
//                    .Select(s => s.ItemId)
//                    .ToListAsync();

//                participantDetails.Add(new
//                {
//                    user_id = p.UserId,
//                    full_name = user?.FullName ?? string.Empty,
//                    selected_items = selectedItems,
//                    total_bill = p.TotalBill,
//                    payment_status = p.PaymentStatus
//                });
//            }

//            return Ok(new
//            {
//                room_id = room.RoomId,
//                room_name = room.RoomName,
//                room_code = room.RoomCode,
//                creator_id = room.CreatorId,
//                status = room.Status,
//                items = items.Select(i => new
//                {
//                    item_id = i.ItemId,
//                    item_name = i.ItemName,
//                    quantity = i.Quantity,
//                    price = i.Price
//                }),
//                participants = participantDetails
//            });
//        }

//        [HttpPut("rooms/{room_id}/select-items")]
//        public async Task<IActionResult> SelectItems(string room_id, [FromBody] SelectItemsRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == room_id);
//            if (room == null)
//                return NotFound(new { error = true, message = "item tidak valid" });

//            var existingSelections = _context.SelectedItems
//                .Where(s => s.RoomId == room_id && s.UserId == userId);
//            _context.SelectedItems.RemoveRange(existingSelections);

//            foreach (var itemId in request.SelectedItems)
//            {
//                _context.SelectedItems.Add(new SelectedItemEntity
//                {
//                    SelectedItemId = Guid.NewGuid().ToString(),
//                    RoomId = room_id,
//                    UserId = userId,
//                    ItemId = itemId
//                });
//            }

//            var participant = await _context.RoomParticipants
//                .FirstOrDefaultAsync(p => p.RoomId == room_id && p.UserId == userId);

//            if (participant != null)
//                participant.TotalBill = request.MyTotalBill;

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                user_id = userId,
//                selected_items = request.SelectedItems,
//                my_total_bill = request.MyTotalBill
//            });
//        }

//        [HttpPut("rooms/{room_id}/confirm-payment")]
//        public async Task<IActionResult> ConfirmPayment(string room_id, [FromBody] ConfirmPaymentRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var participant = await _context.RoomParticipants
//                .FirstOrDefaultAsync(p => p.RoomId == room_id && p.UserId == userId);

//            if (participant == null)
//                return NotFound(new { error = true, message = "sudah dikonfirmasi/tidak valid" });

//            if (participant.PaymentStatus == "paid")
//                return BadRequest(new { error = true, message = "sudah dikonfirmasi/tidak valid" });

//            participant.PaymentStatus = "paid";
//            participant.PaymentMethod = request.PaymentMethod;
//            participant.ProofImageUrl = request.ProofImageUrl;

//            var room = await _context.Rooms.FindAsync(room_id);
//            if (room != null)
//            {
//                var creatorUser = await _context.Users.FindAsync(room.CreatorId);
//                if (creatorUser != null)
//                {
//                    _context.Notifications.Add(new NotificationEntity
//                    {
//                        NotifId = Guid.NewGuid().ToString(),
//                        UserId = room.CreatorId,
//                        Type = "payment_confirmed",
//                        Title = "Peserta sudah bayar!",
//                        Body = $"Peserta telah mengkonfirmasi pembayaran untuk split bill \"{room.RoomName}\"",
//                        RoomId = room_id,
//                        IsRead = false,
//                        CreatedAt = DateTime.UtcNow
//                    });
//                }
//            }

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "Pembayaran dikonfirmasi",
//                payment_status = "paid"
//            });
//        }

//        [HttpPut("rooms/{room_id}/mark-paid")]
//        public async Task<IActionResult> MarkPaid(string room_id, [FromBody] MarkPaidRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == room_id);
//            if (room == null || room.CreatorId != userId)
//                return Forbid();

//            var participant = await _context.RoomParticipants
//                .FirstOrDefaultAsync(p => p.RoomId == room_id && p.UserId == request.ParticipantUserId);

//            if (participant == null)
//                return NotFound(new { error = true, message = "peserta tidak ditemukan" });

//            participant.PaymentStatus = "paid";

//            _context.Notifications.Add(new NotificationEntity
//            {
//                NotifId = Guid.NewGuid().ToString(),
//                UserId = request.ParticipantUserId,
//                Type = "marked_paid",
//                Title = "Tagihan kamu sudah lunas!",
//                Body = $"Creator telah menandai kamu lunas untuk split bill \"{room.RoomName}\"",
//                RoomId = room_id,
//                IsRead = false,
//                CreatedAt = DateTime.UtcNow
//            });

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "Peserta ditandai lunas",
//                participant_user_id = request.ParticipantUserId,
//                payment_status = "paid"
//            });
//        }

//        private string GenerateRoomCode()
//        {
//            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
//            var random = new Random();
//            return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
//        }
//    }
//}
