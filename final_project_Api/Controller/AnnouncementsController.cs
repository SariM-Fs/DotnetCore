using AutoMapper;
using final_project_Core.Common;
using final_project_Core.DTO;
using final_project_Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace final_project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnnouncementsController : ControllerBase
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IMapper _mapper;

        public AnnouncementsController(IAnnouncementService announcementService, IMapper mapper)
        {
            _announcementService = announcementService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AnnouncementDto>>> GetAll(CancellationToken ct)
        {
            var announcements = await _announcementService.GetAllAsync(ct);
            return Ok(_mapper.Map<List<AnnouncementDto>>(announcements));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AnnouncementDto>> Create([FromBody] CreateAnnouncementDto request, CancellationToken ct)
        {
            var driverId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var driverName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";

            var result = await _announcementService.CreateAsync(request.Title, request.Content, driverId, driverName, ct);
            var dto = _mapper.Map<AnnouncementDto>(result.Data);
            return CreatedAtAction(nameof(GetAll), new { }, dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _announcementService.DeleteAsync(id, ct);

            return result.Status switch
            {
                OperationStatus.Success => NoContent(),
                OperationStatus.NotFound => NotFound(new { message = result.Message }),
                _ => StatusCode(500)
            };
        }
    }
}
