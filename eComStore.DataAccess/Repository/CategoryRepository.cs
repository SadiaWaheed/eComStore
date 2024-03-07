using eComStore.DataAccess.Data;
using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;

namespace eComStore.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }   

        public void Update(Category obj)
        {
            _db.Update(obj);
        }
    }
}
