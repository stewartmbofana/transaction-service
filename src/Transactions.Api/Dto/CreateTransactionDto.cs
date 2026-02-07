using System.ComponentModel.DataAnnotations;

namespace Transactions.Api.Dto;

public class CreateTransactionDto
{
    [Required]
    public string AccountNumber { get; set; } = default!;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "ZAR";

    [Required]
    public string Type { get; set; } = "Credit";

    public string? Description { get; set; }
}