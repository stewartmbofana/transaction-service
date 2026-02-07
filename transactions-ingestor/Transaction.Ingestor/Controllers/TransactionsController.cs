using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transaction.Ingestor.Data;
using Transaction.Ingestor.Models;

namespace Transaction.Ingestor.Controllers;

[ApiController] 
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionDbContext _context;

    public TransactionsController(TransactionDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Ingest(BankTransaction transaction)
    {
        // Simple categorization logic (replace with your own)
        transaction.CategoryId = transaction.Description.Contains("grocery", StringComparison.OrdinalIgnoreCase) ? 1 :
                                 transaction.Description.Contains("utility", StringComparison.OrdinalIgnoreCase) ? 2 : 3;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return Ok(transaction);
    }
}