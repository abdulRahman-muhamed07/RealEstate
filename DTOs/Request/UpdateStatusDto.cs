using static RealEstate.Models.Property;

namespace RealEstate.DTOs.Request
{
    public class UpdateStatusDto
    {
        public bool Approve { get; set; }
        public PropertyStatus? NewStatus { get; set; }
    }
}
