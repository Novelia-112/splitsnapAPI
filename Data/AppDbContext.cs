using Microsoft.EntityFrameworkCore;
using SplitSnap.Models;

namespace SplitSnap.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<TransactionEntity> Transactions { get; set; }
        public DbSet<WalletTransactionEntity> WalletTransactions { get; set; }
        public DbSet<RoomEntity> Rooms { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }
    }
}
