using eComStore.DataAccess.Data;
using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;

namespace eComStore.DataAccess.Repository
{
    public class ApplicationUserRepository:Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
    }
}
