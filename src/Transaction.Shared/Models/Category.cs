namespace Transaction.Shared.Models;

public class Category {
  public int Id { get; set; }
  public string Name { get; set; } = default!;
  public ICollection<BankTransaction> Transactions { get; set; } = [];
}
