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
    public class StationsController : ControllerBase
    {
        private readonly IStationService _stationService;
        private readonly IMapper _mapper;

        public StationsController(IStationService stationService, IMapper mapper)
        {
            _stationService = stationService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var stations = await _stationService.GetAllAsync(ct);
            return Ok(_mapper.Map<List<StationDto>>(stations));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StationDto>> Create([FromBody] CreateStationDto request, CancellationToken ct)
        {
            var result = await _stationService.CreateAsync(
                request.Name, request.Location, request.ConnectorType, request.PowerKw, request.AmenityIds, ct);

            var dto = new StationDto
            {
                Id = result.Data!.Id,
                Name = result.Data.Name,
                Location = result.Data.Location,
                ConnectorType = result.Data.ConnectorType,
                PowerKw = result.Data.PowerKw,
                IsActive = result.Data.IsActive,
                TotalSpots = 0,
                AvailableSpots = 0,
                Amenities = result.Data.Amenities.Select(a => a.Name).ToList()
            };
            return CreatedAtAction(nameof(GetAll), new { }, dto);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] SetStationStatusDto dto, CancellationToken ct)
        {
            var result = await _stationService.SetActiveStatusAsync(id, dto.IsActive, ct);

            if (result.Status == OperationStatus.NotFound)
            {
                return NotFound(new { message = result.Message });
            }

            return Ok(new { message = "Station status updated.", isActive = result.Data!.IsActive });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _stationService.DeleteAsync(id, ct);

            return result.Status switch
            {
                OperationStatus.Success => NoContent(),
                OperationStatus.NotFound => NotFound(new { message = result.Message }),
                _ => StatusCode(500)
            };
        }
    }
}
