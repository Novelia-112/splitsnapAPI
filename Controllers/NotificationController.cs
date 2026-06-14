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
