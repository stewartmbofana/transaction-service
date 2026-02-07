using System.ComponentModel.DataAnnotations;
using Transaction.Shared.Models;

namespace Transaction.Shared.Models; 
public class Customer {
[Key]
public Guid Id { get; set; }

[Required]
[MaxLength(100)]
public string FirstName { get; set; } = null!;

[Required]
[MaxLength(100)]
public string LastName { get; set; } = null!;

[Required]
[MaxLength(200)]
public string Email { get; set; } = null!;

public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

// Navigation property - one customer has many transactions
public List<BankTransaction>? Transactions { get; set; }
}
