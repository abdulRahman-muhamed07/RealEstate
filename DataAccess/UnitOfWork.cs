using RealEstate.Models;
using RealEstate.Repositories;

namespace RealEstate.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Property = new Repository<Property>(_db);
            Category = new Repository<Category>(_db);
            Favorite = new Repository<Favorite>(_db);
        }

        public IRepository<Property> Property { get; private set; }
        public IRepository<Category> Category { get; private set; }
        public IRepository<Favorite> Favorite { get; private set; }

        public async Task SaveAsync() => await _db.SaveChangesAsync();
        public void Dispose() => _db.Dispose();
    }
}