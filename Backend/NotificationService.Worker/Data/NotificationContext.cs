using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace NotificationService.Worker.Data;

public class NotificationContext(DbContextOptions<NotificationContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
    }
}