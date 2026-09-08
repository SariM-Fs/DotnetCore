using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using final_project_Core.Repositories;
using final_project_Service.Services;
using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Enum;

namespace final_project_Test.Tests
{
    public class ChargingSpotServiceTests
    {
        private readonly Mock<ISpotRepository> _spotRepoMock = new();
        private readonly Mock<IChargingSessionRepository> _sessionRepoMock = new();
        private readonly Mock<ILogger<ChargingSpotService>> _loggerMock = new();
        private readonly ChargingSpotService _sut;

        public ChargingSpotServiceTests()
        {
            _sut = new ChargingSpotService(_spotRepoMock.Object, _sessionRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task StartSessionAsync_WhenSpotIsAvailable_CreatesSessionAndMarksSpotOccupied()
        {
            var spot = new ChargingSpot { Id = 1, StationId = 1, SpotNumber = 5, Status = SpotStatus.Available };
            _spotRepoMock.Setup(r => r.GetByIdWithStationAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(spot);
            _spotRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _sut.StartSessionAsync(spotId: 1, driverId: 42, CancellationToken.None);

            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(SpotStatus.Occupied, spot.Status);
            _sessionRepoMock.Verify(r => r.AddAsync(It.Is<ChargingSession>(
                s => s.SpotId == 1 && s.DriverId == 42), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task StartSessionAsync_WhenSpotAlreadyOccupied_ReturnsConflictWithoutTouchingDb()
        {
            var spot = new ChargingSpot { Id = 1, StationId = 1, SpotNumber = 5, Status = SpotStatus.Occupied };
            _spotRepoMock.Setup(r => r.GetByIdWithStationAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(spot);

            var result = await _sut.StartSessionAsync(spotId: 1, driverId: 42, CancellationToken.None);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            _spotRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task StartSessionAsync_WhenSpotDoesNotExist_ReturnsNotFound()
        {
            _spotRepoMock.Setup(r => r.GetByIdWithStationAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((ChargingSpot?)null);

            var result = await _sut.StartSessionAsync(spotId: 99, driverId: 1, CancellationToken.None);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task StartSessionAsync_WhenConcurrentUpdateLosesTheRace_ReturnsConflictInsteadOfThrowing()
        {
            // Arrange
            var spot = new ChargingSpot { Id = 1, StationId = 1, SpotNumber = 5, Status = SpotStatus.Available };
            _spotRepoMock.Setup(r => r.GetByIdWithStationAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(spot);

            _spotRepoMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateConcurrencyException(
                    "Simulates driver A having already taken this spot between our read and our write."));

            // Act
            var result = await _sut.StartSessionAsync(spotId: 1, driverId: 99, CancellationToken.None);

            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains("taken", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task EndSessionAsync_WhenSessionIsActive_SetsEndTimeAndFreesSpot()
        {
            // Arrange
            var spot = new ChargingSpot { Id = 1, Status = SpotStatus.Occupied };
            var session = new ChargingSession
            {
                Id = 1,
                SpotId = 1,
                DriverId = 42,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                Spot = spot
            };

            _sessionRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);
            _sessionRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            // Act
            var result = await _sut.EndSessionAsync(sessionId: 1, driverId: 42, energyDeliveredKwh: 12.5, CancellationToken.None);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.NotNull(session.EndTime);
            Assert.Equal(12.5, session.EnergyDeliveredKwh);
            Assert.Equal(SpotStatus.Available, spot.Status);
        }

        [Fact]
        public async Task EndSessionAsync_WhenSessionAlreadyEnded_ReturnsValidationError()
        {
            var session = new ChargingSession
            {
                Id = 1,
                SpotId = 1,
                DriverId = 42,
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow.AddMinutes(-30) // already ended
            };
            _sessionRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

            var result = await _sut.EndSessionAsync(sessionId: 1, driverId: 42, energyDeliveredKwh: 5, CancellationToken.None);

            Assert.Equal(OperationStatus.ValidationError, result.Status);
        }

        [Fact]
        public async Task EndSessionAsync_WhenCallerIsNotTheDriverWhoStartedTheSession_ReturnsForbiddenWithoutTouchingDb()
        {
            var spot = new ChargingSpot { Id = 1, Status = SpotStatus.Occupied };
            var session = new ChargingSession
            {
                Id = 1,
                SpotId = 1,
                DriverId = 42,
                StartTime = DateTime.UtcNow.AddMinutes(-30),
                Spot = spot
            };
            _sessionRepoMock.Setup(r => r.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(session);

            var result = await _sut.EndSessionAsync(sessionId: 1, driverId: 99, energyDeliveredKwh: 5, CancellationToken.None);

            Assert.Equal(OperationStatus.Forbidden, result.Status);
            Assert.Null(session.EndTime);
            Assert.Equal(SpotStatus.Occupied, spot.Status);
            _sessionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task EndExpiredSessionsAsync_WhenSessionsExceedMaxDuration_EndsThemAndFreesTheirSpots()
        {
            var spot1 = new ChargingSpot { Id = 1, Status = SpotStatus.Occupied };
            var spot2 = new ChargingSpot { Id = 2, Status = SpotStatus.Occupied };
            var expiredSessions = new List<ChargingSession>
            {
                new() { Id = 1, SpotId = 1, DriverId = 1, StartTime = DateTime.UtcNow.AddHours(-5), Spot = spot1 },
                new() { Id = 2, SpotId = 2, DriverId = 2, StartTime = DateTime.UtcNow.AddHours(-6), Spot = spot2 },
            };
            _sessionRepoMock
                .Setup(r => r.GetActiveSessionsStartedBeforeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredSessions);
            _sessionRepoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(2);

            var endedCount = await _sut.EndExpiredSessionsAsync(TimeSpan.FromHours(4), CancellationToken.None);

            Assert.Equal(2, endedCount);
            Assert.All(expiredSessions, s => Assert.NotNull(s.EndTime));
            Assert.Equal(SpotStatus.Available, spot1.Status);
            Assert.Equal(SpotStatus.Available, spot2.Status);
            _sessionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EndExpiredSessionsAsync_WhenNoSessionsExpired_ReturnsZeroWithoutTouchingDb()
        {
            _sessionRepoMock
                .Setup(r => r.GetActiveSessionsStartedBeforeAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ChargingSession>());

            var endedCount = await _sut.EndExpiredSessionsAsync(TimeSpan.FromHours(4), CancellationToken.None);

            Assert.Equal(0, endedCount);
            _sessionRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
