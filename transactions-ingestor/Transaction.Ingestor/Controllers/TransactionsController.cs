using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transaction.Ingestor.Data;
using Transaction.Ingestor.Models;
using Transaction.Ingestor.Services;

namespace Transaction.Ingestor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly TransactionDbContext _context;
    private readonly ITransactionCategorizer _categorizer;

    public TransactionsController(TransactionDbContext context, ITransactionCategorizer categorizer)
    {
        _context = context;
        _categorizer = categorizer;
    }

    [HttpPost]
    public async Task<IActionResult> Ingest(BankTransaction transaction)
    {
        if (transaction == null)
            return BadRequest();

        // Determine category name using intelligent, configuration-driven categorizer
        var categoryName = _categorizer.GetCategoryName(transaction.Description ?? string.Empty);

        // Try find existing category, otherwise create it
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == categoryName);

        if (category == null)
        {
            category = new Category { Name = categoryName };
            _context.Categories.Add(category);
            await _context.SaveChangesAsync(); // ensure Category.Id is set
        }

        transaction.CategoryId = category.Id;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return Ok(transaction);
    }
}