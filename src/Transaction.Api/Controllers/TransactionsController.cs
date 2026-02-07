using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


//using Microsoft.EntityFrameworkCore;
using Transaction.Api.Dto;
using Transaction.Shared.Data;
using Transaction.Shared.Models;

namespace Transaction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
	private readonly TransactionsDbContext _db;

	public TransactionsController(TransactionsDbContext db)
	{
		_db = db;
	}

	// GET: api/transactions
	[HttpGet]
	public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAll(
		CancellationToken cancellationToken)
	{
		var transactions =
			await _db.Transactions.OrderByDescending(t => t.CreatedAt).ToListAsync(cancellationToken);

		var result =
			transactions.Select(t => new TransactionDto(t.Id, t.AccountNumber, t.Amount, t.Currency,
														t.Type, t.Description, t.CreatedAt));

		return Ok(result);
	}

	// GET: api/transactions/{id}
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<TransactionDto>> GetById(Guid id,
															CancellationToken cancellationToken)
	{
		var t = await _db.Transactions.FindAsync([id], cancellationToken);
		if (t == null)
			return NotFound();

		return Ok(new TransactionDto(t.Id, t.AccountNumber, t.Amount, t.Currency, t.Type,
									 t.Description, t.CreatedAt));
	}

	// POST: api/transactions
	[HttpPost]
	public async Task<ActionResult<TransactionDto>> Create(CreateTransactionDto dto,
														   CancellationToken cancellationToken)
	{
		var entity =
			new BankTransaction
			{
				AccountNumber = dto.AccountNumber,
				Amount = dto.Amount,
				Currency = dto.Currency,
				Type = dto.Type,
				Description = dto.Description,
				CreatedAt = DateTimeOffset.UtcNow
			};

		_db.Transactions.Add(entity);
		await _db.SaveChangesAsync(cancellationToken);

        using (var producer = new ProducerBuilder<string, string>(new ProducerConfig()
        {
            BootstrapServers = "localhost:9092",
            Acks = Acks.All
        }).Build())
		{
			await producer.ProduceAsync("TransactionsTopic", new Message<string, string>()
            {
                Key = entity.Id.ToString(),
                Value = JsonSerializer.Serialize(entity)
            }, cancellationToken);
		}

		var result =
			new TransactionDto(entity.Id, entity.AccountNumber, entity.Amount, entity.Currency,
							   entity.Type, entity.Description, entity.CreatedAt);

		return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
	}

	// PUT: api/transactions/{id}
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, UpdateTransactionDto dto,
											CancellationToken cancellationToken)
	{
		var entity = await _db.Transactions.FindAsync([id], cancellationToken);
		if (entity == null)
			return NotFound();

		entity.AccountNumber = dto.AccountNumber;
		entity.Amount = dto.Amount;
		entity.Currency = dto.Currency;
		entity.Type = dto.Type;
		entity.Description = dto.Description;

		_db.Transactions.Update(entity);
		await _db.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

	// DELETE: api/transactions/{id}
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var entity = await _db.Transactions.FindAsync([id], cancellationToken);
		if (entity == null)
			return NotFound();

		_db.Transactions.Remove(entity);
		await _db.SaveChangesAsync(cancellationToken);

		return NoContent();
	}
}
