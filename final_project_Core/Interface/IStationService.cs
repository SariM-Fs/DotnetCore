using final_project_Core.Common;
using final_project_Core.Entities;

namespace final_project_Core.Interface
{
    public interface IStationService
    {
        Task<IEnumerable<StationSummary>> GetAllAsync(CancellationToken ct);
        Task<OperationResult<ChargingStation>> SetActiveStatusAsync(int stationId, bool isActive, CancellationToken ct);
        Task<OperationResult<StationSummary>> CreateAsync(
            string name, string location, string connectorType, double powerKw, IEnumerable<int> amenityIds, CancellationToken ct);
        Task<OperationResult<bool>> DeleteAsync(int stationId, CancellationToken ct);
    }
}
