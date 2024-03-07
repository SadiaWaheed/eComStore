using eComStore.DataAccess.Data;
using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;

namespace eComStore.DataAccess.Repository
{
    public class CoverTypeRepository : Repository<CoverType>, ICoverTypeRepository
    {
        private ApplicationDbContext _db;
        public CoverTypeRepository(ApplicationDbContext db):base(db)    
        {
            _db = db;
        }
    
        public void Update(CoverType obj)
        {
            _db.Update(obj);
        }

    }
}
