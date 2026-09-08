using AutoMapper;
using final_project_Core.Common;
using final_project_Core.DTO;
using final_project_Core.Entities;
using final_project_Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace final_project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IMapper _mapper;

        public ReviewsController(IReviewService reviewService, IMapper mapper)
        {
            _reviewService = reviewService;
            _mapper = mapper;
        }

        // Approved reviews only — visible to every signed-in user, optionally
        // filtered to a single station.
        [HttpGet]
        public async Task<ActionResult<List<ReviewDto>>> GetApproved([FromQuery] int? stationId, CancellationToken ct)
        {
            var reviews = await _reviewService.GetApprovedAsync(stationId, ct);
            return Ok(_mapper.Map<List<ReviewDto>>(reviews));
        }

        // The caller's own reviews regardless of status, so a driver can see
        // what's still pending or got rejected.
        [HttpGet("mine")]
        public async Task<ActionResult<List<ReviewDto>>> GetMine(CancellationToken ct)
        {
            var reviews = await _reviewService.GetMineAsync(GetCallerDriverId(), ct);
            return Ok(_mapper.Map<List<ReviewDto>>(reviews));
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<ReviewDto>>> GetPending(CancellationToken ct)
        {
            var reviews = await _reviewService.GetPendingAsync(ct);
            return Ok(_mapper.Map<List<ReviewDto>>(reviews));
        }

        [HttpPost]
        public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewDto request, CancellationToken ct)
        {
            var driverId = GetCallerDriverId();
            var driverName = User.FindFirstValue(ClaimTypes.Name) ?? "Driver";

            var result = await _reviewService.CreateAsync(request.StationId, driverId, driverName, request.Rating, request.Content, ct);
            return ToActionResult(result, created: true);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReviewDto>> Approve(int id, CancellationToken ct)
        {
            var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var result = await _reviewService.ApproveAsync(id, adminName, ct);
            return ToActionResult(result);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReviewDto>> Reject(int id, CancellationToken ct)
        {
            var adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            var result = await _reviewService.RejectAsync(id, adminName, ct);
            return ToActionResult(result);
        }

        private int GetCallerDriverId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private ActionResult<ReviewDto> ToActionResult(OperationResult<Review> result, bool created = false)
        {
            switch (result.Status)
            {
                case OperationStatus.Success:
                    var dto = _mapper.Map<ReviewDto>(result.Data);
                    return created
                        ? CreatedAtAction(nameof(GetMine), new { }, dto)
                        : Ok(dto);

                case OperationStatus.NotFound:
                    return NotFound(new { message = result.Message });

                case OperationStatus.Conflict:
                    return Conflict(new { message = result.Message });

                case OperationStatus.ValidationError:
                    return BadRequest(new { message = result.Message });

                case OperationStatus.Forbidden:
                    return StatusCode(403, new { message = result.Message });

                default:
                    return StatusCode(500);
            }
        }
    }
}
