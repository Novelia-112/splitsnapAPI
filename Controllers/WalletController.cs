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

        private static readonly string[] MonthsId =
            { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };

        public WalletController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        private static string FormatDateId(DateTime dt) => $"{dt.Day} {MonthsId[dt.Month - 1]} {dt.Year}";
        private static string FormatTimeId(DateTime dt) => $"{dt:HH}.{dt:mm}";

        private static object ToWalletJson(WalletTransactionEntity w) => new
        {
            walletTransactionId = w.WalletTransactionId,
            title = w.Title,
            date = w.Date,
            time = w.Time,
            amount = w.Amount,
            status = w.Status,
            type = w.Type,
        };

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = true, message = "User tidak ditemukan" });

            return Ok(new { balance = user.Balance });
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

            var now = DateTime.UtcNow;
            var wallet = new WalletTransactionEntity
            {
                WalletTransactionId = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = "Top Up Wallet",
                Date = FormatDateId(now),
                Time = FormatTimeId(now),
                Amount = request.Amount,
                Status = "Lunas",
                Type = "topUp",
                CreatedAt = now
            };

            _context.WalletTransactions.Add(wallet);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Top up berhasil",
                balance = user.Balance,
                transaction = ToWalletJson(wallet)
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

            var now = DateTime.UtcNow;
            var wallet = new WalletTransactionEntity
            {
                WalletTransactionId = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = "Withdraw to Bank",
                Date = FormatDateId(now),
                Time = FormatTimeId(now),
                Amount = -request.Amount,
                Status = "Pending",
                Type = "withdraw",
                CreatedAt = now
            };

            _context.WalletTransactions.Add(wallet);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Withdraw berhasil, status Pending",
                balance = user.Balance,
                transaction = ToWalletJson(wallet)
            });
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity([FromQuery] int limit = 20, [FromQuery] int offset = 0)
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
                activities = activities.Select(ToWalletJson)
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
//    [Route("wallet")]
//    [Authorize]
//    public class WalletController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly JwtService _jwtService;

//        public WalletController(AppDbContext context, JwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        [HttpGet("balance")]
//        public async Task<IActionResult> GetBalance()
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var user = await _context.Users.FindAsync(userId);
//            if (user == null)
//                return NotFound(new { error = true, message = "User tidak ditemukan" });

//            return Ok(new
//            {
//                user_id = userId,
//                balance = user.Balance,
//                masked_account = $"•••• •••• {user.Phone[^4..]}"
//            });
//        }

//        [HttpPost("topup")]
//        public async Task<IActionResult> TopUp([FromBody] TopUpRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            if (request.Amount <= 0)
//                return BadRequest(new { error = true, message = "Jumlah tidak valid" });

//            var user = await _context.Users.FindAsync(userId);
//            if (user == null)
//                return NotFound(new { error = true, message = "User tidak ditemukan" });

//            user.Balance += request.Amount;

//            var walletTransaction = new WalletTransactionEntity
//            {
//                WalletTransactionId = Guid.NewGuid().ToString(),
//                UserId = userId,
//                Type = "topup",
//                Amount = request.Amount,
//                BalanceAfter = user.Balance,
//                Status = "success",
//                PaymentMethod = request.PaymentMethod,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.WalletTransactions.Add(walletTransaction);
//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                wallet_transaction_id = walletTransaction.WalletTransactionId,
//                amount = request.Amount,
//                balance_after = user.Balance,
//                status = "success",
//                created_at = walletTransaction.CreatedAt
//            });
//        }

//        [HttpPost("withdraw")]
//        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            if (request.Amount <= 0)
//                return BadRequest(new { error = true, message = "Jumlah tidak valid" });

//            var user = await _context.Users.FindAsync(userId);
//            if (user == null)
//                return NotFound(new { error = true, message = "User tidak ditemukan" });

//            if (user.Balance < request.Amount)
//                return BadRequest(new { error = true, message = "Saldo tidak cukup" });

//            user.Balance -= request.Amount;

//            var walletTransaction = new WalletTransactionEntity
//            {
//                WalletTransactionId = Guid.NewGuid().ToString(),
//                UserId = userId,
//                Type = "withdraw",
//                Amount = request.Amount,
//                BalanceAfter = user.Balance,
//                Status = "pending",
//                BankAccountNumber = request.BankAccountNumber,
//                BankName = request.BankName,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.WalletTransactions.Add(walletTransaction);
//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                wallet_transaction_id = walletTransaction.WalletTransactionId,
//                amount = request.Amount,
//                balance_after = user.Balance,
//                status = "pending",
//                created_at = walletTransaction.CreatedAt
//            });
//        }

//        [HttpGet("activity")]
//        public async Task<IActionResult> GetActivity(
//            [FromQuery] int limit = 20,
//            [FromQuery] int offset = 0)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var activities = await _context.WalletTransactions
//                .Where(w => w.UserId == userId)
//                .OrderByDescending(w => w.CreatedAt)
//                .Skip(offset)
//                .Take(limit)
//                .ToListAsync();

//            return Ok(new
//            {
//                activities = activities.Select(a => new
//                {
//                    wallet_transaction_id = a.WalletTransactionId,
//                    type = a.Type,
//                    amount = a.Amount,
//                    status = a.Status,
//                    created_at = a.CreatedAt
//                })
//            });
//        }
//    }
//}
