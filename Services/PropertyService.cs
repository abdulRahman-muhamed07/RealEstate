using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.DataAccess;
using RealEstate.DTOs.Request;
using RealEstate.Models;
using System.Security.Claims;
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
        // ------ Get the current logged-in user ID ------
        private string GetCurrentUserId() =>
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // ------ Image extension validation ------
        private bool IsValidImage(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        // ------ Physical file deletion from server ------
        private void DeleteImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "images/properties", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }
        #endregion

        #region Property Operations

        /// <summary> ------ Get properties with filters, sorting and pagination ------ </summary>
        [AllowAnonymous]
        public async Task<IActionResult> GetFilteredPropertiesAsync(PropertyFilterDto filter)
        {
            var query = _unitOfWork.Property.Query(
                p => p.IsApproved && p.Status == filter.Status,
                includeProperties: "Category,City,Owner"
            );

            // --- الـ Filtering ---
            if (filter.CityId.HasValue && filter.CityId > 0)
                query = query.Where(p => p.CityId == filter.CityId);

            if (filter.CategoryIds?.Any() == true)
                query = query.Where(p => filter.CategoryIds.Contains(p.CategoryId));

            if (filter.MaxPrice.HasValue && filter.MaxPrice > 0)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            // --- الـ Sorting ---
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
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Status,
                    p.Description,
                    p.Price,
                    p.Area,
                    ImageUrl = p.ImageUrl,
                    p.IsForRent,
                    p.IsApproved,
                    p.CreatedAt,
                    p.Bedrooms,
                    p.Bathrooms,
                    p.CategoryId,
                    p.CityId,
                    p.OwnerId,
                    Owner = new
                    {
                        p.Owner.Id,
                        p.Owner.FirstName,
                        p.Owner.LastName,
                        p.Owner.IsCompany,
                    },
                    Category = new
                    {
                        p.Category.Id,
                        p.Category.Name
                    },
                    City = new
                    {
                        p.City.Id,
                        p.City.Name
                    }
                })
                .ToListAsync();

            return Ok(new { totalCount, data });
        }

        /// <summary> ------ Get detailed info for a single approved property ------ </summary>
        [AllowAnonymous]
        public async Task<IActionResult> GetPropertyByIdAsync(int id)
        {
                        var property = await _unitOfWork.Property.Query(
                    p => p.Id == id && p.IsApproved,
                    includeProperties: "Category,City,Owner"
                )
                .Select(p => new {
                    p.Id,
                    p.Title,
                    p.Status,
                    p.Description,
                    p.Price,
                    p.Area,
                    p.ImageUrl,
                    p.IsForRent,
                    p.Bedrooms,
                    p.Bathrooms,
                    p.CreatedAt,
                    Owner = new
                    {
                        p.Owner.Id,
                        p.Owner.FirstName,
                        p.Owner.LastName,
                        p.Owner.Email,
                        p.Owner.PhoneNumber
                    },
                    Category = new { p.Category.Id, p.Category.Name },
                    City = new { p.City.Id, p.City.Name }
                })
                .FirstOrDefaultAsync();

            if (property == null)
                return NotFound("Property not found or not approved.");

            return Ok(property);
        }

        /// <summary> ------ Add a new property and upload image ------ </summary>
        [Authorize]
        public async Task<IActionResult> AddPropertyAsync(PropertyCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (dto.ImageFile != null && !IsValidImage(dto.ImageFile))
                return BadRequest("Invalid image format. Use JPG, PNG, or WebP.");

            string fileName = null;
            if (dto.ImageFile != null)
            {
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images/properties");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);
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
            return Ok(new { message = "Property submitted successfully and waiting for approval." });
        }

        /// <summary> ------ Update property details and reset approval (Owner Only) ------ </summary>
        [Authorize]
        public async Task<IActionResult> UpdatePropertyAsync(int id, PropertyCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var property = await _unitOfWork.Property.GetFirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (property == null) return NotFound("Property not found or access denied.");

            if (dto.ImageFile != null)
            {
                if (!IsValidImage(dto.ImageFile)) return BadRequest("Invalid image format.");
                DeleteImageFile(property.ImageUrl);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images/properties");
                using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                await dto.ImageFile.CopyToAsync(stream);
                property.ImageUrl = fileName;
            }

            property.Title = dto.Title;
            property.Price = dto.Price;
            property.Description = dto.Description;
            property.Bedrooms = dto.Rooms;
            property.Area = dto.Area;
            property.CategoryId = dto.CategoryId;
            property.CityId = dto.CityId;
            property.Status = dto.Status;
            property.IsApproved = false; // Must be re-approved after edit

            await _unitOfWork.SaveAsync();
            return Ok(new { message = "Property updated and pending re-approval." });
        }

        /// <summary> ------ Delete property and its image (Owner or Admin) ------ </summary>
        [Authorize]
        public async Task<IActionResult> DeletePropertyAsync(int id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") ?? false;

            var property = await _unitOfWork.Property.GetFirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound();

            if (property.OwnerId != userId && !isAdmin) return Forbid();

            DeleteImageFile(property.ImageUrl);
            _unitOfWork.Property.Remove(property);
            await _unitOfWork.SaveAsync();
            return Ok(new { message = "Property deleted successfully." });
        }

        #endregion

        #region Admin Operations

        /// <summary> ------ Get all properties waiting for admin approval ------ </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRequestsAsync()
        {
            var pending = await _unitOfWork.Property.Query(p => !p.IsApproved).ToListAsync();
            return Ok(pending);
        }

        /// <summary> ------ Approve/Reject or Change property status (Admin) ------ </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusAsync(int id, bool approve, PropertyStatus? newStatus = null)
        {
            var property = await _unitOfWork.Property.GetFirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound("Request not found.");

            if (approve)
            {
                property.IsApproved = true;
                if (newStatus.HasValue) property.Status = newStatus.Value;
            }
            else
            {
                DeleteImageFile(property.ImageUrl);
                _unitOfWork.Property.Remove(property);
            }

            await _unitOfWork.SaveAsync();
            return Ok(new { message = approve ? "Property approved." : "Property rejected and deleted." });
        }

        #endregion

        #region User Specific

        /// <summary> ------ Get list of properties owned by a user ------ </summary>
        [Authorize]
        public async Task<IActionResult> GetUserPropertiesAsync(string? userId = null)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var properties = await _unitOfWork.Property.Query(p => p.OwnerId == id).ToListAsync();
            return Ok(properties);
        }

        /// <summary> ------ Toggle property in user favorites ------ </summary>
        [Authorize]
        public async Task<IActionResult> ToggleFavoriteAsync(string? userId, int propertyId)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var fav = await _unitOfWork.Favorite.GetFirstOrDefaultAsync(f => f.UserId == id && f.PropertyId == propertyId);
            if (fav != null)
                _unitOfWork.Favorite.Remove(fav);
            else
                _unitOfWork.Favorite.Add(new Favorite { UserId = id, PropertyId = propertyId });

            await _unitOfWork.SaveAsync();
            return Ok();
        }

        /// <summary> ------ Get list of user's favorite properties ------ </summary>
        [Authorize]
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