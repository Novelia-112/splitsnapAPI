using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitSnap.Data;
using SplitSnap.Models;
using SplitSnap.Services;

namespace SplitSnap.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.DisplayName) || string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password))
                return BadRequest(new { error = true, message = "Semua field wajib diisi" });

            var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (exists)
                return BadRequest(new { error = true, message = "email sudah terdaftar" });

            var user = new UserEntity
            {
                UserId = Guid.NewGuid().ToString(),
                DisplayName = request.DisplayName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user.UserId, user.Email);

            return Ok(new
            {
                message = "Registrasi berhasil",
                userId = user.UserId,
                tokenJwt = token
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { error = true, message = "Email dan password wajib diisi" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { error = true, message = "email/password salah" });

            var token = _jwtService.GenerateToken(user.UserId, user.Email);

            return Ok(new
            {
                userId = user.UserId,
                displayName = user.DisplayName,
                email = user.Email,
                photoUrl = user.PhotoUrl,
                tokenJwt = token
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new { message = "Logout berhasil" });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound(new { error = true, message = "User tidak ditemukan" });

            return Ok(new
            {
                userId = user.UserId,
                displayName = user.DisplayName,
                email = user.Email,
                photoUrl = user.PhotoUrl,
                balance = user.Balance
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
//    [Route("auth")]
//    public class AuthController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly JwtService _jwtService;

//        public AuthController(AppDbContext context, JwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
//        {
//            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email) ||
//                string.IsNullOrEmpty(request.Phone) || string.IsNullOrEmpty(request.Password))
//                return BadRequest(new { error = true, message = "Semua field wajib diisi" });

//            var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
//            if (exists)
//                return BadRequest(new { error = true, message = "email sudah terdaftar" });

//            var user = new UserEntity
//            {
//                UserId = Guid.NewGuid().ToString(),
//                FullName = request.FullName,
//                Email = request.Email,
//                Phone = request.Phone,
//                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Users.Add(user);
//            await _context.SaveChangesAsync();

//            var token = _jwtService.GenerateToken(user.UserId, user.Email);

//            return Ok(new
//            {
//                message = "Registrasi berhasil",
//                user_id = user.UserId,
//                token_jwt = token
//            });
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] LoginRequest request)
//        {
//            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
//                return BadRequest(new { error = true, message = "Email dan password wajib diisi" });

//            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

//            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
//                return Unauthorized(new { error = true, message = "email/password salah" });

//            var token = _jwtService.GenerateToken(user.UserId, user.Email);

//            return Ok(new
//            {
//                user_id = user.UserId,
//                full_name = user.FullName,
//                email = user.Email,
//                phone = user.Phone,
//                token_jwt = token
//            });
//        }

//        [HttpPost("logout")]
//        [Authorize]
//        public IActionResult Logout()
//        {
//            return Ok(new { message = "Logout berhasil" });
//        }

//        [HttpGet("profile")]
//        [Authorize]
//        public async Task<IActionResult> GetProfile()
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
//            if (user == null)
//                return NotFound(new { error = true, message = "User tidak ditemukan" });

//            return Ok(new
//            {
//                user_id = user.UserId,
//                full_name = user.FullName,
//                email = user.Email,
//                phone = user.Phone,
//                total_transaction = user.TotalTransactions,
//                total_split_bill = user.TotalSplitBills,
//                total_spending = user.TotalSpending
//            });
//        }
//    }
//}
