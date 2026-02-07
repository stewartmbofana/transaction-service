namespace Transaction.Api.Dto;

public record TransactionDto(
    Guid Id,
    string AccountNumber,
    decimal Amount,
    string Currency,
    string Type,
    string? Description,
    DateTimeOffset CreatedAt
);