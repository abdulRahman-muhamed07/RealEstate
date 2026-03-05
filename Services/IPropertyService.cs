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
        Task<IActionResult> AddPropertyAsync(PropertyCreateDto dto);
        Task<IActionResult> UpdatePropertyAsync(int id, PropertyCreateDto dto);
        Task<IActionResult> DeletePropertyAsync(int id);

        // Admin Operations
        Task<IActionResult> GetPendingRequestsAsync();
        Task<IActionResult> UpdateStatusAsync(int id, bool approve, Property.PropertyStatus? newStatus = null);

        // User Specific
        Task<IActionResult> GetUserPropertiesAsync(string? userId = null);
        Task<IActionResult> ToggleFavoriteAsync(string? userId, int propertyId);
        Task<IActionResult> GetUserFavoritesAsync(string? userId);
    }
}