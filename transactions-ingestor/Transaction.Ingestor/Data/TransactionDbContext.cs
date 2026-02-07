using Microsoft.EntityFrameworkCore;
using Transaction.Ingestor.Models;

namespace Transaction.Ingestor.Data;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options) { }

    public DbSet<BankTransaction> Transactions => Set<BankTransaction>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Groceries" },
            new Category { Id = 2, Name = "Utilities" },
            new Category { Id = 3, Name = "Entertainment" },
            new Category { Id = 4, Name = "Uncategorized" }
        );
    }
}