using Microsoft.AspNetCore.Mvc;
using RealEstate.DTOs.Request;
using RealEstate.Models;

namespace RealEstate.Services
{
    public interface IPropertyService
    {
        // Property Operations
        Task<IActionResult> GetFilteredPropertiesAsync(PropertyFilterDto filter);
        Task<IActionResult> GetPropertyByIdAsync(int id);
        Task<IActionResult> AddPropertyAsync(PropertyAddDto dto);
        Task<IActionResult> UpdatePropertyAsync(int id, PropertyCreateDto dto);
        Task<IActionResult> DeletePropertyAsync(int id, System.Security.Claims.ClaimsPrincipal user);

        // Admin Operations
        Task<IActionResult> GetPendingRequestsAsync();
        Task<IActionResult> UpdateStatusAsync(int id, UpdateStatusDto dto); 
        // User Specific
        Task<IActionResult> GetUserPropertiesAsync(string? userId = null);
        Task<IActionResult> ToggleFavoriteAsync(int propertyId, string? userId = null);
        Task<IActionResult> GetUserFavoritesAsync(string? userId);
    }
}