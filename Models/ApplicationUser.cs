using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RealEstate.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string? FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; } = string.Empty;
        public string? PhoneNumbers { get; set; } = string.Empty;
        public bool IsCompany { get; set; } = false;

        public ICollection<Property> MyProperties { get; set; }
    }
}
