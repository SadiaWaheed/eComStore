using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using eComStore.Model.ViewModels;
using eComStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace eComStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderViewModel orderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            orderVM = new OrderViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(i => i.OrderId == orderId, includeProperties: "Product")
            };
            return View(orderVM);
        }
        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details_Pay_Now(int orderId)
        {
            orderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == orderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            orderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(i => i.OrderId == orderVM.OrderHeader.Id, includeProperties: "Product");

            //stripe Settings
            var domain = "https://localhost:7184/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderVM.OrderHeader.Id}",
            };

            foreach (var item in orderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),//convert to $ as price is in cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripPaymentID(orderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                //check stripe status
                if (session.PaymentStatus.ToLower() == SD.PaymentStatusPaid)
                {
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            return View(orderHeaderId);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_SuperAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails()
        {
            var objFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == orderVM.OrderHeader.Id, tracked: false);

            objFromDb.Name = orderVM.OrderHeader.Name;
            objFromDb.PhoneNumber = orderVM.OrderHeader.PhoneNumber;
            objFromDb.StreetAddress = orderVM.OrderHeader.StreetAddress;
            objFromDb.City = orderVM.OrderHeader.City;
            objFromDb.State = orderVM.OrderHeader.State;
            objFromDb.PostalCode = orderVM.OrderHeader.PostalCode;

            if (orderVM.OrderHeader.Carrier != null) objFromDb.Carrier = orderVM.OrderHeader.Carrier;
            if (orderVM.OrderHeader.TrackingNumber != null) objFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;

            _unitOfWork.OrderHeader.Update(objFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Details updated successfully";
            return RedirectToAction("Details", new { orderId = objFromDb.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_SuperAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(orderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

            TempData["success"] = "Order Status updated successfully";
            return RedirectToAction("Details", new { orderId = orderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_SuperAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var objFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == orderVM.OrderHeader.Id, tracked: false);

            objFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            objFromDb.Carrier = orderVM.OrderHeader.Carrier;
            objFromDb.OrderStatus = SD.StatusShipped;
            objFromDb.ShippingDate = DateTime.Now;
            if (objFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment) objFromDb.PaymentDueDate = DateTime.Now.AddDays(30);

            _unitOfWork.OrderHeader.Update(objFromDb);
            _unitOfWork.Save();

            TempData["success"] = "Order Shipped successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_SuperAdmin)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var objFromDb = _unitOfWork.OrderHeader.GetFirstOrDefault(i => i.Id == orderVM.OrderHeader.Id, tracked: false);

            if (objFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var service = new SessionService();
                Session session = service.Get(objFromDb.SessionId);
                if (session.PaymentStatus.ToLower() == SD.PaymentStatusPaid)
                {
                    if (session.PaymentIntentId != null)
                    {
                        var options = new RefundCreateOptions
                        {
                            Reason = RefundReasons.RequestedByCustomer,
                            PaymentIntent = session.PaymentIntentId
                        };

                        var refundService = new RefundService();
                        Refund refund = refundService.Create(options);

                        _unitOfWork.OrderHeader.UpdateStatus(objFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
                    }
                }
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(objFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["success"] = "Order Cancelled successfully";
            return RedirectToAction("Details", "Order", new { orderId = orderVM.OrderHeader.Id });
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_SuperAdmin))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimIdentiry = (ClaimsIdentity)User.Identity;
                var claim = claimIdentiry.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(i => i.ApplicationUserId == claim.Value, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
