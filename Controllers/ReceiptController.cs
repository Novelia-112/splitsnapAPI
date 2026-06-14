using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitSnap.Data;
using SplitSnap.Models;
using SplitSnap.Services;
using System.Text.Json;

namespace SplitSnap.Controllers
{
    [ApiController]
    [Route("receipts")]
    [Authorize]
    public class ReceiptController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public ReceiptController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // Mock OCR — output shape sama dengan ReceiptItem Flutter (name, quantity, unitPrice, totalPrice)
        [HttpPost("scan")]
        [Consumes("multipart/form-data")]
        public IActionResult Scan(IFormFile image)
        {
            var userId = _jwtService.GetUserIdFromToken(HttpContext);
            if (userId == null)
                return Unauthorized(new { error = true, message = "unauthorized" });

            if (image == null || image.Length == 0)
                return BadRequest(new { error = true, message = "OCR gagal/gambar tidak valid" });

            var items = new List<ItemDto>
            {
                new() { Name = "Item 1", Quantity = 1, UnitPrice = 9000, TotalPrice = 9000 },
                new() { Name = "Item 2", Quantity = 1, UnitPrice = 20000, TotalPrice = 20000 },
                new() { Name = "Item 3", Quantity = 1, UnitPrice = 12000, TotalPrice = 12000 },
            };

            return Ok(new
            {
                storeName = "Toko",
                date = DateTime.UtcNow.ToString("d MMM yyyy"),
                items,
                total = items.Sum(i => i.TotalPrice)
            });
        }

        // Sama seperti POST /transactions, tapi dipaksa type="pribadi" / status="Pribadi"
        [HttpPost("save-personal")]
        public async Task<IActionResult> SavePersonal([FromBody] CreateTransactionRequest request)
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
                Type = "pribadi",
                Name = request.Name,
                Date = request.Date,
                People = string.IsNullOrEmpty(request.People) ? "1 Orang" : request.People,
                Amount = request.Amount,
                Status = "Pribadi",
                Subtitle = request.Subtitle,
                Detail = string.IsNullOrEmpty(request.Detail) ? request.Name : request.Detail,
                ItemsJson = JsonSerializer.Serialize(request.Items),
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Tersimpan sebagai pengeluaran pribadi",
                transactionId = transaction.TransactionId
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
//    [Route("receipts")]
//    [Authorize]
//    public class ReceiptController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly JwtService _jwtService;

//        public ReceiptController(AppDbContext context, JwtService jwtService)
//        {
//            _context = context;
//            _jwtService = jwtService;
//        }

//        [HttpPost("scan")]
//        [Consumes("multipart/form-data")]
//        public async Task<IActionResult> Scan(
//            IFormFile image,
//            [FromForm] double? latitude,
//            [FromForm] double? longitude)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            if (image == null || image.Length == 0)
//                return BadRequest(new { error = true, message = "OCR gagal/gambar tidak valid" });

//            var receiptId = Guid.NewGuid().ToString();
//            var storeName = "Struk Belanja";
//            var date = DateTime.UtcNow.ToString("d MMMM yyyy");

//            var items = new List<object>
//            {
//                new { item_name = "Item 1", quantity = 1, price = 9000 },
//                new { item_name = "Item 2", quantity = 1, price = 20000 },
//                new { item_name = "Item 3", quantity = 1, price = 12000 }
//            };

//            var total = 41000m;

//            var receipt = new ReceiptEntity
//            {
//                ReceiptId = receiptId,
//                UserId = userId,
//                StoreName = storeName,
//                Date = date,
//                Total = total,
//                Latitude = latitude,
//                Longitude = longitude,
//                LocationName = string.Empty,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Receipts.Add(receipt);
//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                receipt_id = receiptId,
//                store_name = storeName,
//                date,
//                items,
//                total,
//                location = new { latitude, longitude }
//            });
//        }

//        [HttpPost("save-personal")]
//        public async Task<IActionResult> SavePersonal([FromBody] SavePersonalReceiptRequest request)
//        {
//            var userId = _jwtService.GetUserIdFromToken(HttpContext);
//            if (userId == null)
//                return Unauthorized(new { error = true, message = "unauthorized" });

//            if (string.IsNullOrEmpty(request.StoreName) || request.Items.Count == 0)
//                return BadRequest(new { error = true, message = "data tidak valid" });

//            var existingReceipt = await _context.Receipts.FirstOrDefaultAsync(r => r.ReceiptId == request.ReceiptId);

//            if (existingReceipt == null)
//            {
//                var receipt = new ReceiptEntity
//                {
//                    ReceiptId = request.ReceiptId,
//                    UserId = userId,
//                    StoreName = request.StoreName,
//                    Date = request.Date,
//                    Total = request.Total,
//                    Latitude = request.Latitude,
//                    Longitude = request.Longitude,
//                    LocationName = request.LocationName,
//                    CreatedAt = DateTime.UtcNow
//                };
//                _context.Receipts.Add(receipt);
//            }

//            foreach (var item in request.Items)
//            {
//                _context.ReceiptItems.Add(new ReceiptItemEntity
//                {
//                    ItemId = Guid.NewGuid().ToString(),
//                    ReceiptId = request.ReceiptId,
//                    ItemName = item.ItemName,
//                    Quantity = item.Quantity,
//                    Price = item.Price
//                });
//            }

//            var transaction = new TransactionEntity
//            {
//                TransactionId = Guid.NewGuid().ToString(),
//                UserId = userId,
//                Type = "personal",
//                StoreName = request.StoreName,
//                Total = request.Total,
//                Date = request.Date,
//                Latitude = request.Latitude,
//                Longitude = request.Longitude,
//                LocationName = request.LocationName,
//                PaymentStatus = "paid",
//                ReceiptId = request.ReceiptId,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Transactions.Add(transaction);

//            var user = await _context.Users.FindAsync(userId);
//            if (user != null)
//            {
//                user.TotalTransactions++;
//                user.TotalSpending += request.Total;
//            }

//            await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                transaction_id = transaction.TransactionId,
//                message = "Tersimpan sebagai pengeluaran pribadi"
//            });
//        }
//    }
//}
