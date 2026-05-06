using Microsoft.EntityFrameworkCore;
using RobotManagementSystem.Shared.Models.Users;

namespace RobotManagementSystem.Data;

public class RobotApiDbContext : DbContext
{
    public DbSet<UserAccount> Users { get; set; }

    public RobotApiDbContext(DbContextOptions<RobotApiDbContext> options) : base(options)
    {
        
    }
}