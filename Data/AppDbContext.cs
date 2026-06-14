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

//using Microsoft.EntityFrameworkCore;
//using SplitSnap.Models;

//namespace SplitSnap.Data
//{
//    public class AppDbContext : DbContext
//    {
//        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

//        public DbSet<UserEntity> Users { get; set; }
//        public DbSet<ReceiptEntity> Receipts { get; set; }
//        public DbSet<ReceiptItemEntity> ReceiptItems { get; set; }
//        public DbSet<TransactionEntity> Transactions { get; set; }
//        public DbSet<RoomEntity> Rooms { get; set; }
//        public DbSet<RoomParticipantEntity> RoomParticipants { get; set; }
//        public DbSet<SelectedItemEntity> SelectedItems { get; set; }
//        public DbSet<WalletTransactionEntity> WalletTransactions { get; set; }
//        public DbSet<NotificationEntity> Notifications { get; set; }
//    }
//}
