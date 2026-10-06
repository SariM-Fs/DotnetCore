
using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace final_project_Test.Tests
{

    public class ConcurrencyIntegrationTests
    {
        private const string ConnectionString =
            "Host=localhost;Port=5432;Database=EVChargingDb;Username=postgres;Password=postgres";

        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task TwoRealDbContexts_UpdatingSameSpot_SecondSaveThrowsConcurrencyException()
        {
            // Arrange: create a fresh spot to compete over, in its own context.
            int spotId;
            using (var setupContext = CreateContext())
            {
                var station = new ChargingStation
                {
                    Name = "ConcurrencyTest-" + Guid.NewGuid(),
                    Location = "Test",
                    ConnectorType = "Type2",
                    PowerKw = 50
                };
                var spot = new ChargingSpot { SpotNumber = 999, Status = SpotStatus.Available };
                station.Spots.Add(spot);

                setupContext.ChargingStations.Add(station);
                await setupContext.SaveChangesAsync();
                spotId = spot.Id;
            }

            // Act: two SEPARATE DbContexts (simulating two separate web requests)
            // both load the exact same row.
            using var context1 = CreateContext();
            using var context2 = CreateContext();

            var spot1 = await context1.ChargingSpots.FirstAsync(s => s.Id == spotId);
            var spot2 = await context2.ChargingSpots.FirstAsync(s => s.Id == spotId);

            // Both "think" the spot is available and both try to occupy it.
            spot1.Status = SpotStatus.Occupied;
            spot2.Status = SpotStatus.Occupied;

            // First save wins - succeeds, and PostgreSQL bumps the row's xmin.
            await context1.SaveChangesAsync();

            // Second save is still holding the OLD xmin (Version) it originally read.
            // Its UPDATE ... WHERE Id=@id AND xmin=@old matches zero rows.
            Func<Task> secondSave = () => context2.SaveChangesAsync();

            // Assert: EF Core detects zero rows affected and throws exactly this exception.
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(secondSave);

            // Cleanup: remove the test data we created.
            using var cleanupContext = CreateContext();
            var stationToRemove = await cleanupContext.ChargingStations
                .Include(s => s.Spots)
                .FirstAsync(s => s.Spots.Any(sp => sp.Id == spotId));
            cleanupContext.ChargingStations.Remove(stationToRemove);
            await cleanupContext.SaveChangesAsync();
        }
    }
}