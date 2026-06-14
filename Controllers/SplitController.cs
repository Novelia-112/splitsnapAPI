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
