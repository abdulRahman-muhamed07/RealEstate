using System.ComponentModel.DataAnnotations;
using static RealEstate.Models.Property; 
namespace RealEstate.DTOs.Request
{
    public class PropertyCreateDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int CityId { get; set; }
        public int Rooms { get; set; }
        public int Area { get; set; }
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }

        [Required]
        public PropertyStatus Status { get; set; }
    }
}