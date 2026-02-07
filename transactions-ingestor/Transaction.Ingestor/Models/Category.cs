using System.Collections.Generic;

namespace Transaction.Ingestor.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ICollection<BankTransaction> Transactions { get; set; } = [];
}