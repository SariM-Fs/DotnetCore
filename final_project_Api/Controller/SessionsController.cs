using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using final_project_Core.Entities;
using final_project_Core.Interface;
using final_project_Core.DTO;
using final_project_Core.Common;
using System.Security.Claims;

namespace final_project_API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SessionsController : ControllerBase
    {
        private readonly IChargingSpotService _chargingSpotService;
        private readonly IMapper _mapper;

        public SessionsController(IChargingSpotService chargingSpotService, IMapper mapper)
        {
            _chargingSpotService = chargingSpotService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChargingSessionDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _chargingSpotService.GetByIdAsync(id, ct);
            return ToActionResult(result);
        }

        // Creates a session for the SPOT in the body; the driver is always the
        // authenticated caller (from the JWT), never a client-supplied id.
        [HttpPost]
        public async Task<ActionResult<ChargingSessionDto>> Start([FromBody] StartSessionRequestDto request, CancellationToken ct)
        {
            var driverId = GetCallerDriverId();
            var result = await _chargingSpotService.StartSessionAsync(request.SpotId, driverId, ct);

            if (result.Status != OperationStatus.Success)
            {
                return ToActionResult(result);
            }

            var dto = _mapper.Map<ChargingSessionDto>(result.Data);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // Ends the session identified by the route id (partial update, not a verb in the URL).
        [HttpPatch("{id}")]
        public async Task<ActionResult<ChargingSessionDto>> End(int id, [FromBody] EndSessionRequestDto request, CancellationToken ct)
        {
            var driverId = GetCallerDriverId();
            var result = await _chargingSpotService.EndSessionAsync(id, driverId, request.EnergyDeliveredKwh, ct);
            return ToActionResult(result);
        }

        private int GetCallerDriverId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private ActionResult<ChargingSessionDto> ToActionResult(OperationResult<ChargingSession> result)
        {
            switch (result.Status)
            {
                case OperationStatus.Success:
                    return Ok(_mapper.Map<ChargingSessionDto>(result.Data));

                case OperationStatus.NotFound:
                    return NotFound(new { message = result.Message });

                case OperationStatus.Conflict:
                    return Conflict(new { message = result.Message }); // HTTP 409

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
