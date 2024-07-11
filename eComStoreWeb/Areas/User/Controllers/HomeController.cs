using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using eComStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace eComStore.Web.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
                return View(productList);
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                IEnumerable<WishList> wishlist = _unitOfWork.WishList.GetAll(x => x.ApplicationUserId == claim.Value);

                IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");

                if (wishlist != null)
                {
                    foreach (var product in productList.Where(p => wishlist.Any(x => x.ProductId == p.Id)))
                    {
                        product.InWishList = true;
                    }
                }
                return View(productList);
            }
        }
        public IActionResult Details(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCart obj = new()
            {
                Count = 1,
                ProductId = productId,
                Product = _unitOfWork.Product.GetFirstOrDefault(i => i.Id == productId, includeProperties: "Category,CoverType"),
                WishList = _unitOfWork.WishList.GetFirstOrDefault(i => i.ApplicationUserId == claim.Value && i.ProductId == productId)
            };
            return View(obj);
        }
        [Authorize]
        public IActionResult AddRemoveWishlist(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            WishList wishList = _unitOfWork.WishList.GetFirstOrDefault(w => w.ProductId == productId && w.ApplicationUserId == claim.Value);

            if (wishList == null)
            {
                wishList = new()
                {
                    ProductId = productId,
                    ApplicationUserId = claim.Value,
                };

                _unitOfWork.WishList.Add(wishList);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.WishList.Remove(wishList);
                _unitOfWork.Save();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart obj)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            obj.ApplicationUserId = claim.Value;

            ShoppingCart objFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(i => i.ApplicationUserId == claim.Value && i.ProductId == obj.ProductId);

            if (objFromDb == null)
            {
                _unitOfWork.ShoppingCart.Add(obj);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count);
            }
            else
            {
                _unitOfWork.ShoppingCart.IncrementCount(objFromDb, obj.Count);
                _unitOfWork.Save();
            }

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}