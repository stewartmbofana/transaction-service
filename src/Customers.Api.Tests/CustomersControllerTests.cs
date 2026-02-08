using Customers.Api.Controllers;
using Library.Shared.Data;
using Library.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Customers.Api.Tests;

public class CustomersControllerTests : IDisposable
{
	private readonly TransactionsDbContext _context;
	private readonly CustomersController _controller;

	public CustomersControllerTests()
	{
		var options = new DbContextOptionsBuilder<TransactionsDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new TransactionsDbContext(options);
		_context.Database.EnsureCreated();
		_controller = new CustomersController(_context);
	}

	[Fact]
	public async Task GetAll_ReturnsAllCustomers()
	{
		// Arrange
		_context.Customers.AddRange(
			new Customer { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@test.com" },
			new Customer { Id = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Email = "jane@test.com" }
		);
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.GetAll(CancellationToken.None);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var customers = Assert.IsAssignableFrom<IEnumerable<Customer>>(okResult.Value);
		Assert.Equal(2, customers.Count());
	}

	[Fact]
	public async Task GetById_ExistingId_ReturnsCustomer()
	{
		// Arrange
		var id = Guid.NewGuid();
		_context.Customers.Add(new Customer { Id = id, FirstName = "John", LastName = "Doe", Email = "john@test.com" });
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.GetById(id, CancellationToken.None);

		// Assert
		var okResult = Assert.IsType<OkObjectResult>(result);
		var customer = Assert.IsType<Customer>(okResult.Value);
		Assert.Equal(id, customer.Id);
		Assert.Equal("John", customer.FirstName);
	}

	[Fact]
	public async Task GetById_NonExistingId_ReturnsNotFound()
	{
		// Act
		var result = await _controller.GetById(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task Create_ValidCustomer_ReturnsCreatedAtAction()
	{
		// Arrange
		var customer = new Customer { FirstName = "John", LastName = "Doe", Email = "john@test.com" };

		// Act
		var result = await _controller.Create(customer, CancellationToken.None);

		// Assert
		var createdResult = Assert.IsType<CreatedAtActionResult>(result);
		var returnedCustomer = Assert.IsType<Customer>(createdResult.Value);
		Assert.NotEqual(Guid.Empty, returnedCustomer.Id);
		Assert.Equal("John", returnedCustomer.FirstName);
	}

	[Fact]
	public async Task Create_DuplicateEmail_ReturnsConflict()
	{
		// Arrange
		_context.Customers.Add(new Customer { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@test.com" });
		await _context.SaveChangesAsync();

		var duplicate = new Customer { FirstName = "Jane", LastName = "Smith", Email = "john@test.com" };

		// Act
		var result = await _controller.Create(duplicate, CancellationToken.None);

		// Assert
		Assert.IsType<ConflictObjectResult>(result);
	}

	[Fact]
	public async Task Update_ExistingCustomer_ReturnsNoContent()
	{
		// Arrange
		var id = Guid.NewGuid();
		_context.Customers.Add(new Customer { Id = id, FirstName = "John", LastName = "Doe", Email = "john@test.com" });
		await _context.SaveChangesAsync();

		var updated = new Customer { Id = id, FirstName = "Johnny", LastName = "Doe", Email = "johnny@test.com" };

		// Act
		var result = await _controller.Update(id, updated, CancellationToken.None);

		// Assert
		Assert.IsType<NoContentResult>(result);
		var customer = await _context.Customers.FindAsync(id);
		Assert.Equal("Johnny", customer!.FirstName);
		Assert.Equal("johnny@test.com", customer.Email);
	}

	[Fact]
	public async Task Update_NonExistingCustomer_ReturnsNotFound()
	{
		// Arrange
		var id = Guid.NewGuid();
		var customer = new Customer { Id = id, FirstName = "John", LastName = "Doe", Email = "john@test.com" };

		// Act
		var result = await _controller.Update(id, customer, CancellationToken.None);

		// Assert
		Assert.IsType<NotFoundResult>(result);
	}

	[Fact]
	public async Task Update_IdMismatch_ReturnsBadRequest()
	{
		// Arrange
		var customer = new Customer { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Email = "john@test.com" };

		// Act
		var result = await _controller.Update(Guid.NewGuid(), customer, CancellationToken.None);

		// Assert
		Assert.IsType<BadRequestObjectResult>(result);
	}

	[Fact]
	public async Task Delete_ExistingCustomer_ReturnsNoContent()
	{
		// Arrange
		var id = Guid.NewGuid();
		_context.Customers.Add(new Customer { Id = id, FirstName = "John", LastName = "Doe", Email = "john@test.com" });
		await _context.SaveChangesAsync();

		// Act
		var result = await _controller.Delete(id, CancellationToken.None);

		// Assert
		Assert.IsType<NoContentResult>(result);
		Assert.Null(await _context.Customers.FindAsync(id));
	}

	[Fact]
	public async Task Delete_NonExistingCustomer_ReturnsNotFound()
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
