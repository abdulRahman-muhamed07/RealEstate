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
        //private void DeleteImageFile(string fileName)
        //{
        //    if (string.IsNullOrEmpty(fileName)) return;
        //    var path = Path.Combine(_webHostEnvironment.WebRootPath, "images/properties", fileName);
        //    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        //}

        private void DeleteImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            try
            {
                string cleanFileName = Path.GetFileName(fileName);

                var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", "properties", cleanFileName);

                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (Exception)
            {
                
            }
        }
        #endregion

        #region Property Operations

        /// <summary> ------ Get properties with filters, sorting and pagination ------ </summary>
        [AllowAnonymous]
        public async Task<IActionResult> GetFilteredPropertiesAsync(PropertyFilterDto filter)
        {
            var query = _unitOfWork.Property.Query(
                p => p.IsApproved,
                includeProperties: "Category,City,Owner"
            );

            if (filter.Status.HasValue)
            {
                query = query.Where(p => p.Status == filter.Status.Value);
            }

            if (filter.CityId.HasValue && filter.CityId > 0)
                query = query.Where(p => p.CityId == filter.CityId);

            if (filter.CategoryIds != null && filter.CategoryIds.Any())
                query = query.Where(p => filter.CategoryIds.Contains(p.CategoryId));

            if (filter.MaxPrice.HasValue && filter.MaxPrice > 0)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.MinPrice.HasValue && filter.MinPrice > 0)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            query = (filter.SortBy?.ToLower()) switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.Id)
            };

            var totalCount = await query.CountAsync();

            var page = filter.Page <= 0 ? 1 : filter.Page;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Status,
                    p.Description,
                    p.Price,
                    p.Area,
                    ImageUrl = (p.ImageUrl != null && (p.ImageUrl.StartsWith("http") || p.ImageUrl.StartsWith("data:")))
                               ? p.ImageUrl
                               : $"/images/properties/{p.ImageUrl}",
                    p.IsForRent,
                    p.IsApproved,
                    p.CreatedAt,
                    p.Bedrooms,
                    p.Bathrooms,
                    p.CategoryId,
                    p.CityId,
                    p.OwnerId,
                    Owner = p.Owner != null ? new
                    {
                        p.Owner.Id,
                        p.Owner.FirstName,
                        p.Owner.LastName,
                        p.Owner.IsCompany
                    } : null,
                    Category = p.Category != null ? new
                    {
                        p.Category.Id,
                        p.Category.Name
                    } : null,
                    City = p.City != null ? new
                    {
                        p.City.Id,
                        p.City.Name
                    } : null
                })
                .ToListAsync();

            return Ok(new { totalCount, page, pageSize, data });
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
                    ImageUrl = (p.ImageUrl != null && (p.ImageUrl.StartsWith("http") || p.ImageUrl.StartsWith("data:")))
                               ? p.ImageUrl
                               : $"/images/properties/{p.ImageUrl}",
                    p.IsForRent,
                    p.Bedrooms,
                    p.Bathrooms,
                    p.CreatedAt,
                    Owner = p.Owner != null ? new
                    {
                        p.Owner.Id,
                        p.Owner.FirstName,
                        p.Owner.LastName,
                        p.Owner.Email,
                        p.Owner.PhoneNumber
                    } : null,
                    Category = p.Category != null ? new { p.Category.Id, p.Category.Name } : null,
                    City = p.City != null ? new { p.City.Id, p.City.Name } : null
                })
                .FirstOrDefaultAsync();

            if (property == null)
                return NotFound(new { message = "Property not found or not approved." });

            return Ok(property);
        }

        [Authorize]
        public async Task<IActionResult> AddPropertyAsync([FromForm] PropertyAddDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (dto.ImageFile != null && !IsValidImage(dto.ImageFile))
                return BadRequest(new { message = "صيغة الصورة غير صالحة. يرجى استخدام JPG أو PNG أو WebP." });

            try
            {
                string fileName = "default-property.png";
                if (dto.ImageFile != null)
                {
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images/properties");

                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

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

                    Status = PropertyStatus.Pending,
                    IsApproved = false,

                    CreatedAt = DateTime.UtcNow,
                    OwnerId = userId
                };

                _unitOfWork.Property.Add(property);
                await _unitOfWork.SaveAsync();

                return Ok(new
                {
                    message = "تم إضافة العقار بنجاح، وهو الآن في انتظار مراجعة المسؤول.",
                    propertyId = property.Id
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء حفظ البيانات، يرجى المحاولة لاحقاً." });
            }
        }

        /// <summary> ------ update property ------ </summary>
        [Authorize]
        public async Task<IActionResult> UpdatePropertyAsync(int id, [FromForm] PropertyCreateDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var property = await _unitOfWork.Property.GetFirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (property == null)
                return NotFound(new { message = "عفواً، العقار غير موجود أو ليس لديك صلاحية لتعديله." });

            try
            {
                if (dto.ImageFile != null)
                {
                    if (!IsValidImage(dto.ImageFile))
                        return BadRequest(new { message = "صيغة الصورة غير مدعومة (JPG, PNG, WebP فقط)." });

                    if (!string.IsNullOrEmpty(property.ImageUrl) && property.ImageUrl != "default-property.png")
                        DeleteImageFile(property.ImageUrl);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.ImageFile.FileName);
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images/properties");

                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                    using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                    await dto.ImageFile.CopyToAsync(stream);
                    property.ImageUrl = fileName;
                }

                if (!string.IsNullOrEmpty(dto.Title)) property.Title = dto.Title;
                if (dto.Price.HasValue && dto.Price > 0) property.Price = dto.Price.Value;
                if (!string.IsNullOrEmpty(dto.Description)) property.Description = dto.Description;
                if (dto.Rooms.HasValue) property.Bedrooms = dto.Rooms.Value;
                if (dto.Area.HasValue) property.Area = dto.Area.Value;
                if (dto.CategoryId.HasValue && dto.CategoryId > 0) property.CategoryId = dto.CategoryId.Value;
                if (dto.CityId.HasValue && dto.CityId > 0) property.CityId = dto.CityId.Value;

                property.Status = PropertyStatus.Pending;
                property.IsApproved = false;

                await _unitOfWork.SaveAsync();

                return Ok(new { message = ".تم تحديث البيانات بنجاح" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = ".حدث خطأ أثناء تحديث البيانات" });
            }
        }


        [Authorize]
        /// <summary> ------ Delete property and its image (Owner or Admin) ------ </summary>
        public async Task<IActionResult> DeletePropertyAsync(int id, ClaimsPrincipal user)
        {
            try
            {
                var property = await _unitOfWork.Property.GetFirstOrDefaultAsync(p => p.Id == id);

                if (property == null)
                    return new NotFoundObjectResult(new { message = "العقار غير موجود بالفعل." });

                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = user.IsInRole("Admin");

                if (property.OwnerId != userId && !isAdmin)
                    return new ForbidResult();

                var imageToDelete = property.ImageUrl;

                _unitOfWork.Property.Remove(property);
                await _unitOfWork.SaveAsync();

                if (!string.IsNullOrEmpty(imageToDelete) && imageToDelete != "default-property.png")
                {
                    DeleteImageFile(imageToDelete);
                }

                return new OkObjectResult(new { message = "تم حذف العقار وصورته بنجاح." });
            }
            catch (Exception)
            {
                return new ObjectResult(new { message = "حدث خطأ غير متوقع أثناء الحذف." }) { StatusCode = 500 };
            }
        }

        #endregion

        #region Admin Operations

        /// <summary> ------ Get all properties waiting for admin approval ------ </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingRequestsAsync()
        {
            var pending = await _unitOfWork.Property
                .Query(p => p.Status == 0)
                .ToListAsync();

            return Ok(pending);
        }

        /// <summary> ------ Approve/Reject or Change property status (Admin) ------ </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusAsync(int id, UpdateStatusDto dto)
        {
            var property = await _unitOfWork.Property.GetFirstOrDefaultAsync(p => p.Id == id);
            if (property == null) return NotFound("العقار مش موجود.");

            if (dto.Approve)
            {
                property.IsApproved = true;

                if (dto.NewStatus.HasValue)
                {
                    property.Status = dto.NewStatus.Value;

                    property.IsForRent = (dto.NewStatus == Property.PropertyStatus.Rent);
                }
                else
                {
                    return BadRequest("لازم تحدد القسم (بيع أو إيجار) عند الموافقة.");
                }
            }
            else
            {
                DeleteImageFile(property.ImageUrl);
                _unitOfWork.Property.Remove(property);
            }

            await _unitOfWork.SaveAsync();

            // هنا حددنا النص العربي بناءً على الحالة
            var statusText = dto.NewStatus == Property.PropertyStatus.Rent ? "إيجار" : "بيع";

            return Ok(new { message = dto.Approve ? $"تمت الموافقة وتصنيف العقار كـ {statusText}" : "تم رفض العقار وحذفه بنجاح" });
        }

        #endregion

        #region User Specific

        /// <summary> ------ Get list of properties owned by a user ------ </summary>
        [Authorize]
        public async Task<IActionResult> GetUserPropertiesAsync(string? userId = null)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var properties = await _unitOfWork.Property.Query(
                p => p.OwnerId == id,
                includeProperties: "Category,City"
            ).ToListAsync();

            return Ok(properties);
        }
        /// <summary> ------ Toggle property in user favorites ------ </summary>
        [Authorize]
        public async Task<IActionResult> ToggleFavoriteAsync(string? userId, int propertyId)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return new UnauthorizedResult();

            var fav = await _unitOfWork.Favorite.GetFirstOrDefaultAsync(f => f.UserId == id && f.PropertyId == propertyId);

            string message;
            if (fav != null)
            {
                _unitOfWork.Favorite.Remove(fav);
                message = "تمت إزالة العقار من المفضلة بنجاح";
            }
            else
            {
                _unitOfWork.Favorite.Add(new Favorite { UserId = id, PropertyId = propertyId });
                message = "تم إضافة العقار إلى المفضلة بنجاح";
            }

            await _unitOfWork.SaveAsync();

            return new OkObjectResult(new { message });
        }

        /// <summary> ------ Get list of user's favorite properties ------ </summary>
        [Authorize]
        public async Task<IActionResult> GetUserFavoritesAsync(string? userId)
        {
            var id = userId ?? GetCurrentUserId();
            if (string.IsNullOrEmpty(id)) return Unauthorized();

            var favs = await _unitOfWork.Favorite.Query(f => f.UserId == id, includeProperties: "Property").ToListAsync();

            var result = favs.Where(f => f.Property != null)
                             .Select(f => f.Property)
                             .ToList();

            return Ok(result);
        }

        #endregion
    }
}