using AutoMapper;
using final_project_Core.Common;
using final_project_Core.DTO;
using final_project_Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace final_project_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SpotsController : ControllerBase
    {
        private readonly ISpotService _spotService;
        private readonly IMapper _mapper;

        public SpotsController(ISpotService spotService, IMapper mapper)
        {
            _spotService = spotService;
            _mapper = mapper;
        }

        // GET api/spots?stationId=1&page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ChargingSpotDto>>> GetAll(
            [FromQuery] int? stationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var paged = await _spotService.GetPagedAsync(stationId, page, pageSize, ct);
            var dtos = _mapper.Map<List<ChargingSpotDto>>(paged.Items);

            var sessionMap = await _spotService.GetActiveSessionIdsAsync(dtos.Select(d => d.Id), ct);
            foreach (var dto in dtos)
            {
                dto.ActiveSessionId = sessionMap.TryGetValue(dto.Id, out var sessionId) ? sessionId : null;
            }

            return Ok(new PagedResultDto<ChargingSpotDto>
            {
                Items = dtos,
                TotalCount = paged.TotalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChargingSpotDto>> GetById(int id, CancellationToken ct)
        {
            var spot = await _spotService.GetByIdAsync(id, ct);
            if (spot is null)
            {
                return NotFound(new { message = $"Spot {id} not found." });
            }

            var dto = _mapper.Map<ChargingSpotDto>(spot);
            var sessionMap = await _spotService.GetActiveSessionIdsAsync(new[] { id }, ct);
            dto.ActiveSessionId = sessionMap.TryGetValue(id, out var sessionId) ? sessionId : null;
            return Ok(dto);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] SetSpotStatusDto dto, CancellationToken ct)
        {
            var result = await _spotService.SetStatusAsync(id, dto.Status, ct);

            return result.Status switch
            {
                OperationStatus.Success => Ok(new { message = "Spot status updated.", status = result.Data!.Status.ToString() }),
                OperationStatus.NotFound => NotFound(new { message = result.Message }),
                _ => StatusCode(500)
            };
        }
    }
}
