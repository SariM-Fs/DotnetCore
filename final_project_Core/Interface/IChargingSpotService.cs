using final_project_Core.Common;
using final_project_Core.Entities;

namespace final_project_Core.Interface
{
    public interface IChargingSpotService
    {
        Task<OperationResult<ChargingSession>> GetByIdAsync(int sessionId, CancellationToken ct);

        Task<OperationResult<ChargingSession>> StartSessionAsync(int spotId, int driverId, CancellationToken ct);

        Task<OperationResult<ChargingSession>> EndSessionAsync(int sessionId, int driverId, double energyDeliveredKwh, CancellationToken ct);

        // Ends any session still active after maxDuration, freeing its spot. System-initiated
        // (no driver ownership check), used by the session timeout sweep. Returns how many were ended.
        Task<int> EndExpiredSessionsAsync(TimeSpan maxDuration, CancellationToken ct);
    }
}
