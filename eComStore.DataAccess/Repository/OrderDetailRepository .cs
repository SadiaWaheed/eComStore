using eComStore.DataAccess.Data;
using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;

namespace eComStore.DataAccess.Repository
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private ApplicationDbContext _db;
        public OrderDetailRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }   

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }

    }
}
