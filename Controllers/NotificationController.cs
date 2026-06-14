using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSnap.Data;
using SplitSnap.Models;
using SplitSnap.Services;

namespace SplitSnap.Controllers
{
    [ApiController]
    [Route("notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public NotificationController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("send-reminder")]
        public async Task<IActionResult> SendReminder([FromBody] SendReminderRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            if (request.Bills.Count == 0)
                return BadRequest(new { error = true, message = "tidak ada peserta yang dipilih" });

            var sentTo = new List<string>();
            foreach (var bill in request.Bills)
            {
                _context.Notifications.Add(new NotificationEntity
                {
                    NotifId = Guid.NewGuid().ToString(),
                    UserId = bill.Uid,
                    Type = "payment_reminder",
                    Title = "Pengingat Pembayaran",
                    Body = $"Kamu masih punya tagihan untuk split bill \"{request.StoreName}\"",
                    RoomCode = request.RoomCode,
                    Amount = bill.Total,
                    Read = false,
                    CreatedAt = DateTime.UtcNow
                });
                sentTo.Add(bill.Uid);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Pengingat terkirim",
                sentTo
            });
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterTokenRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            if (string.IsNullOrEmpty(request.FcmToken))
                return BadRequest(new { error = true, message = "FCM token tidak valid" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = true, message = "User tidak ditemukan" });

            user.FcmToken = request.FcmToken;
            await _context.SaveChangesAsync();

            return Ok(new { message = "FCM token terdaftar" });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                message = "Berhasil mengambil notifikasi",
                notifications = notifications.Select(n => new
                {
                    notifId = n.NotifId,
                    type = n.Type,
                    title = n.Title,
                    body = n.Body,
                    roomCode = n.RoomCode,
                    amount = n.Amount,
                    read = n.Read,
                    createdAt = n.CreatedAt
                })
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
//    [Route("notifications")]
//    [Authorize]
//    public class NotificationController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly JwtService _jwtService;

//        public NotificationController(AppDbContext context, JwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        [HttpPost("send-reminder")]
//        public async Task<IActionResult> SendReminder([FromBody] SendReminderRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == request.RoomId);
//            if (room == null || room.CreatorId != userId)
//                return BadRequest(new { error = true, message = "bukan creator/peserta sudah lunas semua" });

//            var unpaidParticipants = await _context.RoomParticipants
//                .Where(p => p.RoomId == request.RoomId &&
//                            p.UserId != userId &&
//                            p.PaymentStatus == "unpaid")
//                .ToListAsync();

//            if (request.TargetUserIds != null && request.TargetUserIds.Count > 0)
//                unpaidParticipants = unpaidParticipants
//                    .Where(p => request.TargetUserIds.Contains(p.UserId))
//                    .ToList();

//            if (unpaidParticipants.Count == 0)
//                return BadRequest(new { error = true, message = "bukan creator/peserta sudah lunas semua" });

//            var sentTo = new List<string>();
//            foreach (var participant in unpaidParticipants)
//            {
//                _context.Notifications.Add(new NotificationEntity
//                {
//                    NotifId = Guid.NewGuid().ToString(),
//                    UserId = participant.UserId,
//                    Type = "payment_reminder",
//                    Title = "Pengingat Pembayaran",
//                    Body = $"Kamu masih punya tagihan yang belum lunas untuk split bill \"{room.RoomName}\"",
//                    RoomId = request.RoomId,
//                    IsRead = false,
//                    CreatedAt = DateTime.UtcNow
//                });
//                sentTo.Add(participant.UserId);
//            }

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "Pengingat terkirim",
//                sent_to = sentTo
//            });
//        }

//        [HttpPost("register-token")]
//        public async Task<IActionResult> RegisterToken([FromBody] RegisterFcmTokenRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            if (string.IsNullOrEmpty(request.FcmToken))
//                return BadRequest(new { error = true, message = "FCM token tidak valid" });

//            var user = await _context.Users.FindAsync(userId);
//            if (user == null)
//                return NotFound(new { error = true, message = "User tidak ditemukan" });

//            user.FcmToken = request.FcmToken;
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "FCM token terdaftar" });
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetNotifications(
//            [FromQuery] int limit = 20,
//            [FromQuery] int offset = 0)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var notifications = await _context.Notifications
//                .Where(n => n.UserId == userId)
//                .OrderByDescending(n => n.CreatedAt)
//                .Skip(offset)
//                .Take(limit)
//                .ToListAsync();

//            return Ok(new
//            {
//                message = "Berhasil mengambil notifikasi",
//                notifications = notifications.Select(n => new
//                {
//                    notif_id = n.NotifId,
//                    type = n.Type,
//                    title = n.Title,
//                    body = n.Body,
//                    room_id = n.RoomId,
//                    is_read = n.IsRead,
//                    created_at = n.CreatedAt
//                })
//            });
//        }
//    }
//}
