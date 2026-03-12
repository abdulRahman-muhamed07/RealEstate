using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.DTOs.Request;
using RealEstate.Services;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _service;
        public PropertyController(IPropertyService service) => _service = service;

        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] PropertyFilterDto filter) => await _service.GetFilteredPropertiesAsync(filter);

        [HttpGet("GetById/{id}")]
        [AllowAnonymous] 
        public async Task<IActionResult> GetById(int id) => await _service.GetPropertyByIdAsync(id);

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromForm] PropertyCreateDto dto) => await _service.AddPropertyAsync(dto);

        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromForm] PropertyCreateDto dto) => await _service.UpdatePropertyAsync(id, dto);


        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id) => await _service.DeletePropertyAsync(id, User);


        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending() => await _service.GetPendingRequestsAsync();

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, [FromBody] UpdateStatusDto dto) => await _service.UpdateStatusAsync(id, dto);

        [HttpGet("mylist")]
        [Authorize]
        public async Task<IActionResult> GetMyProperties([FromQuery] string? userId = null)
        {
            return await _service.GetUserPropertiesAsync(userId);
        }

        [HttpPost("favorites/{propertyId}")]
        [Authorize]
        public async Task<IActionResult> ToggleFavorite(int propertyId, [FromQuery] string? userId = null)
        {
            return await _service.ToggleFavoriteAsync(userId, propertyId);
        }

        [HttpGet("myfavorites")]
        [Authorize]
        public async Task<IActionResult> GetFavorites([FromQuery] string? userId = null)
        {
            return await _service.GetUserFavoritesAsync(userId);
        }


    }
}