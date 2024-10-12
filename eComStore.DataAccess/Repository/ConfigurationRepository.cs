using eComStore.DataAccess.Data;
using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using Microsoft.Extensions.Configuration;

namespace eComStore.DataAccess.Repository
{
    public class ConfigurationRepository : Repository<Configurations>, IConfigurationRepository
    {
        private ApplicationDbContext _db;
        public ConfigurationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Configurations obj)
        {
            _db.Update(obj);
        }
    }
}
