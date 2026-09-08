using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Interface;
using final_project_Core.Repositories;

namespace final_project_Service.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IAnnouncementRepository _announcementRepository;

        public AnnouncementService(IAnnouncementRepository announcementRepository)
        {
            _announcementRepository = announcementRepository;
        }

        public async Task<IEnumerable<Announcement>> GetAllAsync(CancellationToken ct) => await _announcementRepository.GetAllOrderedAsync(ct);

        public async Task<OperationResult<Announcement>> CreateAsync(string title, string content, int createdByDriverId, string createdByName, CancellationToken ct)
        {
            var announcement = new Announcement
            {
                Title = title,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                CreatedByDriverId = createdByDriverId,
                CreatedByName = createdByName
            };
            await _announcementRepository.AddAsync(announcement, ct);
            await _announcementRepository.SaveChangesAsync(ct);

            return OperationResult<Announcement>.Success(announcement);
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id, CancellationToken ct)
        {
            var announcement = await _announcementRepository.GetByIdAsync(id, ct);
            if (announcement is null)
            {
                return OperationResult<bool>.NotFound($"Announcement {id} not found.");
            }

            _announcementRepository.Remove(announcement);
            await _announcementRepository.SaveChangesAsync(ct);

            return OperationResult<bool>.Success(true);
        }
    }
}
