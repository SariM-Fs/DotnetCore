using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Core.Interface;
using final_project_Core.Repositories;

namespace final_project_Service.Services
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;
        private readonly IAmenityRepository _amenityRepository;

        public StationService(IStationRepository stationRepository, IAmenityRepository amenityRepository)
        {
            _stationRepository = stationRepository;
            _amenityRepository = amenityRepository;
        }

        public async Task<IEnumerable<StationSummary>> GetAllAsync(CancellationToken ct)
        {
            var stations = await _stationRepository.GetAllWithDetailsAsync(ct);

            return stations.Select(st => new StationSummary
            {
                Id = st.Id,
                Name = st.Name,
                Location = st.Location,
                ConnectorType = st.ConnectorType,
                PowerKw = st.PowerKw,
                IsActive = st.IsActive,
                TotalSpots = st.Spots.Count,
                AvailableSpots = st.Spots.Count(s => s.Status == SpotStatus.Available),
                Amenities = st.Amenities.Select(a => a.Name).ToList()
            });
        }

        public async Task<OperationResult<ChargingStation>> SetActiveStatusAsync(int stationId, bool isActive, CancellationToken ct)
        {
            var station = await _stationRepository.GetByIdAsync(stationId, ct);
            if (station is null)
            {
                return OperationResult<ChargingStation>.NotFound($"Station {stationId} not found.");
            }

            station.IsActive = isActive;
            _stationRepository.Update(station);
            await _stationRepository.SaveChangesAsync(ct);

            return OperationResult<ChargingStation>.Success(station);
        }

        public async Task<OperationResult<ChargingStation>> CreateAsync(
            string name, string location, string connectorType, double powerKw, IEnumerable<int> amenityIds, CancellationToken ct)
        {
            var amenities = await _amenityRepository.GetByIdsAsync(amenityIds, ct);

            var station = new ChargingStation
            {
                Name = name,
                Location = location,
                ConnectorType = connectorType,
                PowerKw = powerKw,
                IsActive = true
            };
            foreach (var amenity in amenities)
            {
                station.Amenities.Add(amenity);
            }

            await _stationRepository.AddAsync(station, ct);
            await _stationRepository.SaveChangesAsync(ct);

            return OperationResult<ChargingStation>.Success(station);
        }

        public async Task<OperationResult<bool>> DeleteAsync(int stationId, CancellationToken ct)
        {
            var station = await _stationRepository.GetByIdAsync(stationId, ct);
            if (station is null)
            {
                return OperationResult<bool>.NotFound($"Station {stationId} not found.");
            }

            _stationRepository.Remove(station);
            await _stationRepository.SaveChangesAsync(ct);

            return OperationResult<bool>.Success(true);
        }
    }
}
