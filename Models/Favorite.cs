namespace RealEstate.Models
{
    public class Favorite
    {

        public int Id { get; set; }
        public string UserId { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }
    }
}
