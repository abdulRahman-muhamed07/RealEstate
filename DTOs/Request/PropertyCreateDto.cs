using System.ComponentModel.DataAnnotations;
using static RealEstate.Models.Property; 
namespace RealEstate.DTOs.Request
{
    public class PropertyCreateDto
    {
        public string? Title { get; set; }
        public decimal? Price { get; set; }
        public int? CategoryId { get; set; }
        public int? CityId { get; set; }
        public int? Rooms { get; set; }
        public int? Area { get; set; }
        public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }

     
    }
}