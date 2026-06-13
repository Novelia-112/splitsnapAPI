using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSnap.Data;
using SplitSnap.Services;

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

            var totalSpending = await query.SumAsync(t => t.Total);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                message = "Berhasil mengambil riwayat transaksi",
                total_spending = totalSpending,
                transactions = transactions.Select(t => new
                {
                    transaction_id = t.TransactionId,
                    type = t.Type,
                    store_name = t.StoreName,
                    total = t.Total,
                    date = t.Date,
                    location_name = t.LocationName,
                    payment_status = t.PaymentStatus
                })
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

            var items = new List<object>();
            if (!string.IsNullOrEmpty(transaction.ReceiptId))
            {
                var receiptItems = await _context.ReceiptItems
                    .Where(i => i.ReceiptId == transaction.ReceiptId)
                    .ToListAsync();

                items = receiptItems.Select(i => (object)new
                {
                    item_name = i.ItemName,
                    quantity = i.Quantity,
                    price = i.Price
                }).ToList();
            }

            return Ok(new
            {
                transaction_id = transaction.TransactionId,
                type = transaction.Type,
                store_name = transaction.StoreName,
                date = transaction.Date,
                items,
                total = transaction.Total,
                latitude = transaction.Latitude,
                longitude = transaction.Longitude,
                location_name = transaction.LocationName,
                room_id = transaction.RoomId
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
        public async Task<IActionResult> GetChartData(
            [FromQuery] string period = "week",
            [FromQuery] DateTime? start_date = null,
            [FromQuery] DateTime? end_date = null)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var now = DateTime.UtcNow;
            DateTime from;
            DateTime to = now;

            if (period == "week")
            {
                from = now.AddDays(-6);
            }
            else if (period == "month")
            {
                from = new DateTime(now.Year, now.Month, 1);
            }
            else
            {
                from = start_date ?? now.AddDays(-6);
                to = end_date ?? now;
            }

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.CreatedAt >= from && t.CreatedAt <= to)
                .ToListAsync();

            var walletActivities = await _context.WalletTransactions
                .Where(w => w.UserId == userId && w.CreatedAt >= from && w.CreatedAt <= to)
                .ToListAsync();

            var labels = new List<string>();
            var spendingData = new List<decimal>();
            var balanceData = new List<decimal>();

            if (period == "week")
            {
                var days = new[] { "SEN", "SEL", "RAB", "KAM", "JUM", "SAB", "MIN" };
                for (int i = 6; i >= 0; i--)
                {
                    var day = now.AddDays(-i);
                    var dayName = days[(int)day.DayOfWeek == 0 ? 6 : (int)day.DayOfWeek - 1];
                    labels.Add(dayName);

                    var daySpending = transactions
                        .Where(t => t.CreatedAt.Date == day.Date)
                        .Sum(t => t.Total);

                    var dayBalance = walletActivities
                        .Where(w => w.CreatedAt.Date == day.Date)
                        .Sum(w => w.Type == "topup" ? w.Amount : -w.Amount);

                    spendingData.Add(daySpending);
                    balanceData.Add(dayBalance);
                }
            }
            else
            {
                var current = from;
                while (current <= to)
                {
                    labels.Add(current.ToString("dd/MM"));

                    var daySpending = transactions
                        .Where(t => t.CreatedAt.Date == current.Date)
                        .Sum(t => t.Total);

                    var dayBalance = walletActivities
                        .Where(w => w.CreatedAt.Date == current.Date)
                        .Sum(w => w.Type == "topup" ? w.Amount : -w.Amount);

                    spendingData.Add(daySpending);
                    balanceData.Add(dayBalance);
                    current = current.AddDays(1);
                }
            }

            var totalSpending = transactions.Sum(t => t.Total);

            return Ok(new
            {
                message = "Berhasil mengambil data chart",
                labels,
                spending_data = spendingData,
                balance_data = balanceData,
                total_spending = totalSpending
            });
        }
    }
}
