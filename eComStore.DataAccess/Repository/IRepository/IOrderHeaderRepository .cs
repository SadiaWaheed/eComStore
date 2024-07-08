using eComStore.Model;

namespace eComStore.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripPaymentID(int id, string sessionId, string PaymentIntentId);
    }
}
