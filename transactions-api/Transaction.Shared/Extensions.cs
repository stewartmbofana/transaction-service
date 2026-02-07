using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Transaction.Shared.Data;

namespace Transaction.Shared;

public static class TransactionsExtensions
{
	public static IServiceCollection AddSharedServices(this IServiceCollection services,
										 IConfiguration configuration)
	{
		services.AddDbContext<TransactionsDbContext>(
			options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
		// Add any shared services here if needed in the future

		return services;
	}
}
