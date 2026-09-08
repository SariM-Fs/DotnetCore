using final_project_Core.Entities;

namespace final_project_Core.Repositories
{
    public interface IStationRepository : IRepository<ChargingStation>
    {
        // Loads stations together with their Spots (one-to-many) and Amenities
        // (many-to-many) in one round trip via Include/ThenInclude.
        Task<List<ChargingStation>> GetAllWithDetailsAsync(CancellationToken ct);
    }
}
