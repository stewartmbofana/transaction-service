using Library.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Shared.Data;

public class TransactionsDbContext : DbContext {
  public TransactionsDbContext(DbContextOptions<TransactionsDbContext> options) : base(options) {
  }
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<Customer> Customers => Set<Customer>();
  public DbSet<BankTransaction> Transactions { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    modelBuilder.Entity<Category>().HasData(new Category { Id = 1, Name = "Groceries" },
                                            new Category { Id = 2, Name = "Utilities" },
                                            new Category { Id = 3, Name = "Entertainment" },
                                            new Category { Id = 4, Name = "Uncategorized" });
    modelBuilder.Entity<Customer>(b => {
      b.HasKey(c => c.Id);
      b.HasIndex(c => c.Email).IsUnique();
      b.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
      b.Property(c => c.LastName).IsRequired().HasMaxLength(100);
      b.Property(c => c.Email).IsRequired().HasMaxLength(200);
      b.Property(c => c.CreatedAt).HasDefaultValueSql("now()");
      b.HasMany(c => c.Transactions)
          .WithOne(t => t.Customer)
          .HasForeignKey(t => t.CustomerId)
          .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<BankTransaction>(entity => {
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
