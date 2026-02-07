using Library.Shared.Data;
using Library.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Customers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
	private readonly TransactionsDbContext _db;

	public CustomersController(TransactionsDbContext db)
	{
		_db = db;
	}

	// GET: api/customers
	[HttpGet]
	public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
	{
		// include transactions
		var customers = await _db.Customers.AsNoTracking()
							.Include(c => c.Transactions)
							.ToListAsync(cancellationToken);
		return Ok(customers);
	}

	// GET: api/customers/{id}
	[HttpGet("{id:guid}")]
	public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
	{
		var customer = await _db.Customers.AsNoTracking()
						   .Include(c => c.Transactions)
						   .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
		if (customer == null)
			return NotFound();
		return Ok(customer);
	}

	// POST: api/customers
	[HttpPost]
	public async Task<IActionResult> Create([FromBody] Customer input,
											CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		input.Id = Guid.NewGuid();
		input.CreatedAt = DateTimeOffset.UtcNow;

		// ensure transactions (if any) have ids and customer id
		if (input.Transactions != null)
		{
			foreach (var t in input.Transactions)
			{
				if (t.Id == Guid.Empty)
					t.Id = Guid.NewGuid();
				t.CustomerId = input.Id;
			}
		}

		_db.Customers.Add(input);
		try
		{
			await _db.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException ex)
		{
			// Unique constraint on Email, etc.
			return Conflict(new { message = ex.Message });
		}

		return CreatedAtAction(nameof(GetById), new { id = input.Id }, input);
	}

	// PUT: api/customers/{id}
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, [FromBody] Customer input,
											CancellationToken cancellationToken)
	{
		if (id != input.Id)
			return BadRequest("Id mismatch");
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var existing = await _db.Customers.Include(c => c.Transactions)
						   .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
		if (existing == null)
			return NotFound();

		// update scalar fields
		existing.FirstName = input.FirstName;
		existing.LastName = input.LastName;
		existing.Email = input.Email;

		// synchronize transactions: simple replace strategy
		if (input.Transactions != null)
		{
			// remove those not present
			var toRemove =
				existing.Transactions?.Where(et => !input.Transactions.Any(it => it.Id == et.Id))
					.ToList();
			if (toRemove != null)
			{
				foreach (var r in toRemove) _db.Transactions.Remove(r);
			}

			// upsert incoming
			foreach (var incoming in input.Transactions)
			{
				var match = existing.Transactions?.FirstOrDefault(t => t.Id == incoming.Id);
				if (match == null)
				{
					if (incoming.Id == Guid.Empty)
						incoming.Id = Guid.NewGuid();
					incoming.CustomerId = existing.Id;
					_db.Transactions.Add(incoming);
				}
				else
				{
					match.Amount = incoming.Amount;
					match.Currency = incoming.Currency;
					match.CreatedAt = incoming.CreatedAt;
					match.Description = incoming.Description;
				}
			}
		}

		try
		{
			await _db.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!await _db.Customers.AnyAsync(c => c.Id == id, cancellationToken))
				return NotFound();
			throw;
		}
		catch (DbUpdateException ex)
		{
			return Conflict(new { message = ex.Message });
		}

		return NoContent();
	}

	// DELETE: api/customers/{id}
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var customer = await _db.Customers.FindAsync(new object[] { id }, cancellationToken);
		if (customer == null)
			return NotFound();

		_db.Customers.Remove(customer);
		await _db.SaveChangesAsync(cancellationToken);

		return NoContent();
	}
}
