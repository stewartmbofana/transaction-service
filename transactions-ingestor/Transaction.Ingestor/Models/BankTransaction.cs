namespace Transaction.Ingestor.Models;

public class BankTransaction {
  public int Id { get; set; }
  public string Source { get; set; } = default!;
  public decimal Amount { get; set; }
  public DateTime Date { get; set; }
  public string Description { get; set; } = default!;
  public int CategoryId { get; set; }
  public Category Category { get; set; } = default!;
}
