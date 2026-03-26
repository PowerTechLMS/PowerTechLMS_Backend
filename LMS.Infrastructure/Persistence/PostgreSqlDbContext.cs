using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Persistence;

public class PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options) : AppDbContext(options)
{
}
