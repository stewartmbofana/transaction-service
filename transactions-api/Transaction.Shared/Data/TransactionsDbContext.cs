using Microsoft.EntityFrameworkCore;
using Transaction.Shared.Models;

namespace Transaction.Shared.Data;

public class TransactionsDbContext : DbContext
{
    public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : base(options)
    {
    }

    public DbSet<BankTransaction> Transactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasColumnType("numeric(18,2)");
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
        });

        base.OnModelCreating(modelBuilder);
    }
}