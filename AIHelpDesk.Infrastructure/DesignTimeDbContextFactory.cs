using AIHelpDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // 🔹 stringa di servizio solo per le migration
        const string conn =
            "Server=DESKTOP-VQ8Q438\\SQLEXPRESS;Database=AIHelpDesk;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(conn)
            .Options;

        return new ApplicationDbContext(options);
    }
}
