using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data;

public class PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options) : AppDbContext(options)
{
}
