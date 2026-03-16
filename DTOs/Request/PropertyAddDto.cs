namespace RealEstate.DTOs.Request
{
    public class PropertyAddDto
    {
        public string Title { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }

        public int Rooms { get; set; }

        public double Area { get; set; }

        public int CategoryId { get; set; }

        public int CityId { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}
