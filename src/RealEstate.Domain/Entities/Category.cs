namespace RealEstate.Domain.Entities;

public sealed class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
