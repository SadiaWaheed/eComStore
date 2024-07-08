using eComStore.Model;

namespace eComStore.DataAccess.Repository.IRepository
{
    public interface ICoverTypeRepository:IRepository<CoverType>
    {
        void Update(CoverType obj);
    }
}
