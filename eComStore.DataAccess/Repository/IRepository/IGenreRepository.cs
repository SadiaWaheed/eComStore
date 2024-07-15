using eComStore.Model;

namespace eComStore.DataAccess.Repository.IRepository
{
    public interface IGenreRepository :IRepository<Genre>
    {
        void Update(Genre obj);
    }
}
