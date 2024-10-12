using eComStore.Model;

namespace eComStore.DataAccess.Repository.IRepository
{
    public interface IConfigurationRepository:IRepository<Configurations>
    {
        void Update(Configurations obj);
    }
}
