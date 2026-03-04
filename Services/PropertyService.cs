using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using RealEstate.DataAccess;
using RealEstate.DTOs.Request;
using RealEstate.Models;
using System.Linq.Expressions;
using static RealEstate.Models.Property;

namespace RealEstate.Services
{
    public class PropertyService : ControllerBase, IPropertyService
    {
        #region Fields & Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PropertyService(IUnitOfWork unitOfWork,
                               IWebHostEnvironment webHostEnvironment,
                               IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        #region Helpers
        // ------ Get current user ID from Claims ------
        private string GetCurrentUserId() =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        #endregion

        #region Public Property Methods

        /// <summary>
        /// ------ Get properties with filters, sorting and pagination ------
        /// </summary>
        public async Task<IActionResult> GetFilteredPropertiesAsync(PropertyFilterDto filter)
        {
            var query = _unitOfWork.Property.Query(
                p => p.IsApproved && p.Status == filter.Status,
                includeProperties: "Category,City"
            );

            if (filter.CityId.HasValue && filter.CityId > 0)
                query = query.Where(p => p.CityId == filter.CityId);

            if (filter.CategoryIds?.Any() == true)
                query = query.Where(p => filter.CategoryIds.Contains(p.CategoryId));

            if (filter.MaxPrice.HasValue && filter.MaxPrice > 0)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            query = filter.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return Ok(new { totalCount, data });
        }

        /// <summary>
        /// ------ Add a new property linked to the logged-in user ------
        /// </summary>
        public async Task<IActionResult> AddPropertyAsync(PropertyCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User must be logged in.");

            string fileName = null;
            if (dto.ImageFile != null)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images/properties");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await dto.ImageFile.CopyToAsync(stream);
                }
            }

            var property = new Property
            {
                Title = dto.Title,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                CityId = dto.CityId,
                Bedrooms = dto.Rooms,
                Area = dto.Area,
                Description = dto.Description,
                ImageUrl = fileName,
                IsApproved = false,
                Status = dto.Status,
                CreatedAt = DateTime.Now,
                OwnerId = userId
            };

            _unitOfWork.Property.Add(property);
            await _unitOfWork.SaveAsync();
            return Ok(new { message = "Property submitted successfully." });
        }

        /// <summary>
        /// ------ Get all properties pending admin approval ------
        /// </summary>
        public async Task<IActionResult> GetPendingRequestsAsync()
        {
            var pending = await _unitOfWork.Property.Query(p => !p.IsApproved).ToListAsync();
            return Ok(pending);
        }

        /// <summary>
        /// ------ Approve or reject a property status ------
        /// </summary>
        public async Task<IActionResult> UpdateStatusAsync(int id, bool approve, PropertyStatus? newStatus = null)
        {
            var property = await _unitOfWork.Property.GetFirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound();

            if (approve)
            {
                property.IsApproved = true;
                if (newStatus.HasValue) property.Status = newStatus.Value;
            }
            else
            {
                _unitOfWork.Property.Remove(property);
            }
            await _unitOfWork.SaveAsync();
            return Ok(new { message = "Status updated successfully." });
        }

        /// <summary>
        /// ------ Get all properties owned by the current user ------
        /// </summary>
        public async Task<IActionResult> GetUserPropertiesAsync(string? userId = null)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var properties = await _unitOfWork.Property.Query(p => p.OwnerId == id).ToListAsync();
            return Ok(properties);
        }
        #endregion

        #region Public Favorite Methods

        /// <summary>
        /// ------ Add/Remove property from user favorites ------
        /// </summary>
        public async Task<IActionResult> ToggleFavoriteAsync(string? userId, int propertyId)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var fav = await _unitOfWork.Favorite.GetFirstOrDefaultAsync(f => f.UserId == id && f.PropertyId == propertyId);
            if (fav != null)
            {
                _unitOfWork.Favorite.Remove(fav);
            }
            else
            {
                _unitOfWork.Favorite.Add(new Favorite { UserId = id, PropertyId = propertyId });
            }

            await _unitOfWork.SaveAsync();
            return Ok();
        }

        /// <summary>
        /// ------ Get current user's favorite properties ------
        /// </summary>
        public async Task<IActionResult> GetUserFavoritesAsync(string? userId)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var favs = await _unitOfWork.Favorite.Query(f => f.UserId == id, includeProperties: "Property").ToListAsync();
            return Ok(favs.Select(f => f.Property));
        }
        #endregion
    }
}