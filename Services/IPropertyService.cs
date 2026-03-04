using Microsoft.AspNetCore.Mvc;
using RealEstate.DTOs.Request;
using RealEstate.Models;
using static RealEstate.Models.Property;

namespace RealEstate.Services
{
    public interface IPropertyService
    {
        Task<IActionResult> GetFilteredPropertiesAsync(PropertyFilterDto filter);
        Task<IActionResult> AddPropertyAsync(PropertyCreateDto dto);
        Task<IActionResult> GetPendingRequestsAsync();
        Task<IActionResult> UpdateStatusAsync(int id, bool approve, PropertyStatus? newStatus = null);
        Task<IActionResult> GetUserPropertiesAsync(string userId);
        Task<IActionResult> ToggleFavoriteAsync(string userId, int propertyId);
        Task<IActionResult> GetUserFavoritesAsync(string userId);

    }
}