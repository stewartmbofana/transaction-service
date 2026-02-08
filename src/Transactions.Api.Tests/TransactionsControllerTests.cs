using Library.Shared.Data;
using Library.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transactions.Api.Controllers;
using Transactions.Api.Dto;
using Xunit;

namespace Transactions.Api.Tests;

public class TransactionsControllerTests : IDisposable
{
	private readonly TransactionsDbContext _context;
	private readonly TransactionsController _controller;

	public TransactionsControllerTests()
	{
		var options = new DbContextOptionsBuilder<TransactionsDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new TransactionsDbContext(options);
		_context.Database.EnsureCreated();
		_controller = new TransactionsController(_context);
	}

	[Fact]
	public async Task GetAll_ReturnsAllTransactions()
	{
		// Arrange
		var customerId = Guid.NewGuid();
		_context.Transactions.AddRange(
			new BankTransaction { Id = Guid.NewGuid(), AccountNumber = "ACC001", Amount = 100, Currency = "ZAR", Type = "Credit", CustomerId = customerId, CategoryId = 1 },
			new BankTransaction { Id = Guid.NewGuid(), AccountNumber = "ACC002", Amount = 200, Currency = "ZAR", Type = "Debit", CustomerId = customerId, CategoryId = 1 }
		);
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.GetAll(CancellationToken.None);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var transactions = Assert.IsAssignableFrom<IEnumerable<TransactionDto>>(okResult.Value);
		Assert.Equal(2, transactions.Count());
	}

	[Fact]
	public async Task GetById_ExistingId_ReturnsTransaction()
	{
		// Arrange
		var id = Guid.NewGuid();
		var customerId = Guid.NewGuid();
		_context.Transactions.Add(new BankTransaction { Id = id, AccountNumber = "ACC001", Amount = 100, Currency = "ZAR", Type = "Credit", CustomerId = customerId, CategoryId = 1 });
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.GetById(id, CancellationToken.None);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result.Result);
		var transaction = Assert.IsType<TransactionDto>(okResult.Value);
		Assert.Equal(id, transaction.Id);
		Assert.Equal("ACC001", transaction.AccountNumber);
	}

	[Fact]
	public async Task GetById_NonExistingId_ReturnsNotFound()
	{
		// Act
		var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.IsType<NotFoundResult>(result.Result);
	}

	[Fact]
	public async Task Update_ExistingTransaction_ReturnsNoContent()
	{
		// Arrange
		var id = Guid.NewGuid();
		var customerId = Guid.NewGuid();
		_context.Transactions.Add(new BankTransaction { Id = id, AccountNumber = "ACC001", Amount = 100, Currency = "ZAR", Type = "Credit", CustomerId = customerId, CategoryId = 1 });
		await _context.SaveChangesAsync();

		var updateDto = new UpdateTransactionDto { AccountNumber = "ACC002", Amount = 150, Currency = "USD", Type = "Debit" };

		// Act
		var result = await _controller.Update(id, updateDto, CancellationToken.None);

		// Assert
		Assert.IsType<NoContentResult>(result);
		var updated = await _context.Transactions.FindAsync(id);
		Assert.Equal("ACC002", updated!.AccountNumber);
		Assert.Equal(150, updated.Amount);
	}

	[Fact]
	public async Task Update_NonExistingTransaction_ReturnsNotFound()
	{
		// Arrange
		var updateDto = new UpdateTransactionDto { AccountNumber = "ACC001", Amount = 100, Currency = "ZAR", Type = "Credit" };

		// Act
		var result = await _controller.Update(Guid.NewGuid(), updateDto, CancellationToken.None);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task Delete_ExistingTransaction_ReturnsNoContent()
	{
		// Arrange
		var id = Guid.NewGuid();
		var customerId = Guid.NewGuid();
		_context.Transactions.Add(new BankTransaction { Id = id, AccountNumber = "ACC001", Amount = 100, Currency = "ZAR", Type = "Credit", CustomerId = customerId, CategoryId = 1 });
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.Delete(id, CancellationToken.None);

		// Assert
		Assert.IsType<NoContentResult>(result);
		Assert.Null(await _context.Transactions.FindAsync(id));
	}

	[Fact]
	public async Task Delete_NonExistingTransaction_ReturnsNotFound()
	{
		// Act
		var result = await _controller.Delete(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	public void Dispose()
	{
		_context.Database.EnsureDeleted();
		_context.Dispose();
	}
}
