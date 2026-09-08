using final_project_Core.Common;
using final_project_Core.Entities;
using final_project_Core.Enum;
using final_project_Core.Interface;
using final_project_Core.Repositories;

namespace final_project_Service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IStationRepository _stationRepository;

        public ReviewService(IReviewRepository reviewRepository, IStationRepository stationRepository)
        {
            _reviewRepository = reviewRepository;
            _stationRepository = stationRepository;
        }

        public async Task<IEnumerable<Review>> GetApprovedAsync(int? stationId, CancellationToken ct) =>
            await _reviewRepository.GetApprovedAsync(stationId, ct);

        public async Task<IEnumerable<Review>> GetPendingAsync(CancellationToken ct) =>
            await _reviewRepository.GetPendingAsync(ct);

        public async Task<IEnumerable<Review>> GetMineAsync(int driverId, CancellationToken ct) =>
            await _reviewRepository.GetByDriverAsync(driverId, ct);

        public async Task<OperationResult<Review>> CreateAsync(int stationId, int driverId, string driverName, int rating, string content, CancellationToken ct)
        {
            var station = await _stationRepository.GetByIdAsync(stationId, ct);
            if (station is null)
            {
                return OperationResult<Review>.NotFound($"Station {stationId} not found.");
            }

            if (rating < 1 || rating > 5)
            {
                return OperationResult<Review>.ValidationError("Rating must be between 1 and 5.");
            }

            var review = new Review
            {
                StationId = stationId,
                DriverId = driverId,
                DriverName = driverName,
                Rating = rating,
                Content = content,
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review, ct);
            await _reviewRepository.SaveChangesAsync(ct);

            review.Station = station;
            return OperationResult<Review>.Success(review);
        }

        public Task<OperationResult<Review>> ApproveAsync(int id, string adminName, CancellationToken ct) =>
            DecideAsync(id, ReviewStatus.Approved, adminName, ct);

        public Task<OperationResult<Review>> RejectAsync(int id, string adminName, CancellationToken ct) =>
            DecideAsync(id, ReviewStatus.Rejected, adminName, ct);

        private async Task<OperationResult<Review>> DecideAsync(int id, ReviewStatus status, string adminName, CancellationToken ct)
        {
            var review = await _reviewRepository.GetByIdWithDetailsAsync(id, ct);
            if (review is null)
            {
                return OperationResult<Review>.NotFound($"Review {id} not found.");
            }

            if (review.Status != ReviewStatus.Pending)
            {
                return OperationResult<Review>.Conflict($"Review {id} was already {review.Status}.");
            }

            review.Status = status;
            review.DecidedAt = DateTime.UtcNow;
            review.DecidedByName = adminName;

            _reviewRepository.Update(review);
            await _reviewRepository.SaveChangesAsync(ct);

            return OperationResult<Review>.Success(review);
        }
    }
}
