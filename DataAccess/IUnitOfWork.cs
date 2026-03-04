using RealEstate.Models;
using RealEstate.Repositories;

namespace RealEstate.DataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Property> Property { get; }
        IRepository<Category> Category { get; }
        IRepository<Favorite> Favorite { get; }
        Task SaveAsync();
    }
}