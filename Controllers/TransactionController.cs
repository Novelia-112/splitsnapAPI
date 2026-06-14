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
    [Route("transactions")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public TransactionController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        private static object ToTransactionJson(TransactionEntity t, bool includeItems = false)
        {
            var obj = new Dictionary<string, object?>
            {
                ["transactionId"] = t.TransactionId,
                ["name"] = t.Name,
                ["date"] = t.Date,
                ["people"] = t.People,
                ["amount"] = t.Amount,
                ["status"] = t.Status,
                ["type"] = t.Type,
                ["subtitle"] = t.Subtitle,
                ["detail"] = t.Detail,
                ["roomCode"] = t.RoomCode,
            };
            if (includeItems)
            {
                obj["items"] = JsonSerializer.Deserialize<List<ItemDto>>(t.ItemsJson) ?? new();
            }
            return obj;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? type,
            [FromQuery] string? filter,
            [FromQuery] DateTime? start_date,
            [FromQuery] DateTime? end_date,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var query = _context.Transactions.Where(t => t.UserId == userId).AsQueryable();

            if (!string.IsNullOrEmpty(type) && type != "all")
                query = query.Where(t => t.Type == type);

            var now = DateTime.UtcNow;

            if (filter == "7days")
                query = query.Where(t => t.CreatedAt >= now.AddDays(-7));
            else if (filter == "this_month")
                query = query.Where(t => t.CreatedAt.Month == now.Month && t.CreatedAt.Year == now.Year);
            else if (filter == "custom" && start_date.HasValue && end_date.HasValue)
                query = query.Where(t => t.CreatedAt >= start_date.Value && t.CreatedAt <= end_date.Value);

            var totalPengeluaran = await query.SumAsync(t => t.Amount);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                message = "Berhasil mengambil riwayat transaksi",
                totalPengeluaran,
                transactions = transactions.Select(t => ToTransactionJson(t))
            });
        }

        [HttpGet("{transaction_id}")]
        public async Task<IActionResult> GetById(string transaction_id)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transaction_id && t.UserId == userId);

            if (transaction == null)
                return NotFound(new { error = true, message = "Transaksi tidak ditemukan" });

            return Ok(ToTransactionJson(transaction, includeItems: true));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            if (string.IsNullOrEmpty(request.Name))
                return BadRequest(new { error = true, message = "data tidak valid" });

            var transaction = new TransactionEntity
            {
                TransactionId = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = string.IsNullOrEmpty(request.Type) ? "pribadi" : request.Type,
                Name = request.Name,
                Date = request.Date,
                People = request.People,
                Amount = request.Amount,
                Status = request.Status,
                Subtitle = request.Subtitle,
                Detail = request.Detail,
                RoomCode = request.RoomCode,
                ItemsJson = JsonSerializer.Serialize(request.Items),
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Transaksi tersimpan",
                transactionId = transaction.TransactionId
            });
        }

        [HttpDelete("{transaction_id}")]
        public async Task<IActionResult> Delete(string transaction_id)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transaction_id && t.UserId == userId);

            if (transaction == null)
                return NotFound(new { error = true, message = "Transaksi tidak ditemukan" });

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Transaksi berhasil dihapus" });
        }

        [HttpGet("chart")]
        public async Task<IActionResult> GetChartData([FromQuery] string period = "week")
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var now = DateTime.UtcNow;
            var rangeStart = now.AddDays(-6).Date;

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.CreatedAt.Date >= rangeStart)
                .ToListAsync();

            var days = new[] { "SEN", "SEL", "RAB", "KAM", "JUM", "SAB", "MIN" };
            var labels = new List<string>();
            var amounts = new List<int>();

            for (int i = 0; i < 7; i++)
            {
                var day = rangeStart.AddDays(i);
                var dayIndex = (int)day.DayOfWeek == 0 ? 6 : (int)day.DayOfWeek - 1;
                labels.Add(days[dayIndex]);

                var dayTotal = transactions
                    .Where(t => t.CreatedAt.Date == day)
                    .Sum(t => t.Amount);
                amounts.Add(dayTotal);
            }

            return Ok(new
            {
                message = "Berhasil mengambil data chart",
                labels,
                amounts,
                totalPengeluaran = amounts.Sum()
            });
        }
    }
}
