using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public enum PropertyStatus
        {
            Pending = 0,
            Sale = 1,
            Rent = 2
        }
        public PropertyStatus Status { get; set; }
        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public double Area { get; set; }
        public int? CityId { get; set; }

        public int? Bedrooms { get; set; } 
        public int? Bathrooms { get; set; } 
        public string ImageUrl { get; set; }

        // --------  Admin Can Approve or Reject the property listing and transfer it to the rental section if it's for rent -------- 
        public bool IsForRent { get; set; } = false;

        // --------  Need Approval --------  
        public bool IsApproved { get; set; } = false;
        public int CategoryId { get; set; }
        public string OwnerId { get; set; }

        //--------  Navigation properties --------        

        public ApplicationUser Owner { get; set; }
        public Category Category { get; set; }
        public City City { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
