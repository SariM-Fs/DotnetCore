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
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenityService _amenityService;
        private readonly IMapper _mapper;

        public AmenitiesController(IAmenityService amenityService, IMapper mapper)
        {
            _amenityService = amenityService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AmenityDto>>> GetAll(CancellationToken ct)
        {
            var amenities = await _amenityService.GetAllAsync(ct);
            return Ok(_mapper.Map<List<AmenityDto>>(amenities));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AmenityDto>> Create([FromBody] CreateAmenityDto request, CancellationToken ct)
        {
            var result = await _amenityService.CreateAsync(request.Name, ct);
            var dto = _mapper.Map<AmenityDto>(result.Data);
            return CreatedAtAction(nameof(GetAll), new { }, dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _amenityService.DeleteAsync(id, ct);

            return result.Status switch
            {
                OperationStatus.Success => NoContent(),
                OperationStatus.NotFound => NotFound(new { message = result.Message }),
                _ => StatusCode(500)
            };
        }
    }
}
