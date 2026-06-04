using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSnap.Data;
using SplitSnap.Models;
using SplitSnap.Services;

namespace SplitSnap.Controllers
{
    [ApiController]
    [Route("wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public WalletController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = true, message = "User tidak ditemukan" });

            return Ok(new
            {
                user_id = userId,
                balance = user.Balance,
                masked_account = $"•••• •••• {user.Phone[^4..]}"
            });
        }

        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            if (request.Amount <= 0)
                return BadRequest(new { error = true, message = "Jumlah tidak valid" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = true, message = "User tidak ditemukan" });

            user.Balance += request.Amount;

            var walletTransaction = new WalletTransactionEntity
            {
                WalletTransactionId = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = "topup",
                Amount = request.Amount,
                BalanceAfter = user.Balance,
                Status = "success",
                PaymentMethod = request.PaymentMethod,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(walletTransaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                wallet_transaction_id = walletTransaction.WalletTransactionId,
                amount = request.Amount,
                balance_after = user.Balance,
                status = "success",
                created_at = walletTransaction.CreatedAt
            });
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            if (request.Amount <= 0)
                return BadRequest(new { error = true, message = "Jumlah tidak valid" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = true, message = "User tidak ditemukan" });

            if (user.Balance < request.Amount)
                return BadRequest(new { error = true, message = "Saldo tidak cukup" });

            user.Balance -= request.Amount;

            var walletTransaction = new WalletTransactionEntity
            {
                WalletTransactionId = Guid.NewGuid().ToString(),
                UserId = userId,
                Type = "withdraw",
                Amount = request.Amount,
                BalanceAfter = user.Balance,
                Status = "pending",
                BankAccountNumber = request.BankAccountNumber,
                BankName = request.BankName,
                CreatedAt = DateTime.UtcNow
            };

            _context.WalletTransactions.Add(walletTransaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                wallet_transaction_id = walletTransaction.WalletTransactionId,
                amount = request.Amount,
                balance_after = user.Balance,
                status = "pending",
                created_at = walletTransaction.CreatedAt
            });
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity(
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var activities = await _context.WalletTransactions
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return Ok(new
            {
                activities = activities.Select(a => new
                {
                    wallet_transaction_id = a.WalletTransactionId,
                    type = a.Type,
                    amount = a.Amount,
                    status = a.Status,
                    created_at = a.CreatedAt
                })
            });
        }
    }
}
