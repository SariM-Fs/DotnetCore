using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Interface;
using final_project_Core.Repositories;

namespace final_project_Service.Services
{
    public class AmenityService : IAmenityService
    {
        private readonly IAmenityRepository _amenityRepository;

        public AmenityService(IAmenityRepository amenityRepository)
        {
            _amenityRepository = amenityRepository;
        }

        public async Task<IEnumerable<Amenity>> GetAllAsync(CancellationToken ct) => await _amenityRepository.GetAllAsync(ct);

        public async Task<OperationResult<Amenity>> CreateAsync(string name, CancellationToken ct)
        {
            var amenity = new Amenity { Name = name };
            await _amenityRepository.AddAsync(amenity, ct);
            await _amenityRepository.SaveChangesAsync(ct);

            return OperationResult<Amenity>.Success(amenity);
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken ct)
        {
            var amenity = await _amenityRepository.GetByIdAsync(id, ct);
            if (amenity is null)
            {
                return OperationResult<bool>.NotFound($"Amenity {id} not found.");
            }

            _amenityRepository.Remove(amenity);
            await _amenityRepository.SaveChangesAsync(ct);

            return OperationResult<bool>.Success(true);
        }
    }
}
