using eComStore.Model;

namespace eComStore.DataAccess.Repository.IRepository
{
    public interface ICompanyRepository: IRepository<Company>
    {
        void Update(Company obj);
    }
}
