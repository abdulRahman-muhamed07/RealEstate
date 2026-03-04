using static RealEstate.Models.Property;

namespace RealEstate.DTOs.Request
{
    public class PropertyFilterDto
    {

        public PropertyStatus Status { get; set; }
        public List<int>? CategoryIds { get; set; }
        public decimal? MaxPrice { get; set; }

        // -------- Dropdown list for the city filters  -------- 
        public int? CityId { get; set; }

        // -------- newest, price_asc, price_desc --------
        public string? SortBy { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 6;
    }
}
