using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Transaction.Shared.Models;

public class BankTransaction
{
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();

	[Required]
	[MaxLength(50)]
	public string AccountNumber { get; set; } = default!;

	[Column(TypeName = "numeric(18,2)")]
	public decimal Amount { get; set; }

	[Required]
	[MaxLength(3)]
	public string Currency { get; set; } = "ZAR";

	[Required]
	[MaxLength(20)]
	public string Type { get; set; } = "Credit";

	[MaxLength(500)]
	public string? Description { get; set; }

	[Required]
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

	[Required]
	public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;


	[Required]
	public Guid CustomerId { get; set; }

	[ForeignKey(nameof(CustomerId))]
	public Customer? Customer { get; set; }
}
