using eComStore.DataAccess.Data;
using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;

namespace eComStore.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }   

        public void Update(OrderHeader obj)
        {
            _db.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var objFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);
            if (objFromDb != null)
            {
                objFromDb.OrderStatus = orderStatus;
                if (paymentStatus != null)
                {
                    objFromDb.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripPaymentID(int id, string sessionId, string PaymentIntentId)
        {
            var objFromDb = _db.OrderHeaders.FirstOrDefault(u => u.Id == id);

            objFromDb.SessionId = sessionId;
            objFromDb.PaymentIntentId = PaymentIntentId;

        }
    }
}
