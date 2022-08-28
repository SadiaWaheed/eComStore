using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace eComStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _db;
        public ProductController(IUnitOfWork db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Product> objFromDb = _db.Product.GetAll();
            return View(objFromDb);
        }
        //Get
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var objFromDb = _db.Product.GetFirstOrDefault(i => i.Id == id);

            if (objFromDb == null) return NotFound();

            return View(objFromDb);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.Product.GetFirstOrDefault(i => i.Id == id);

            if (obj == null) return NotFound();

            _db.Product.Remove(obj);
            _db.Save();
            TempData["success"] = "Product deleted successfully!";
            return RedirectToAction("Index");
        }
        //Get
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _db.Category.GetAll().Select(
                i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
            IEnumerable<SelectListItem> CoverTypeList = _db.CoverType.GetAll().Select(
               i => new SelectListItem
               {
                   Text = i.Name,
                   Value = i.Id.ToString()
               });
            if (id == null || id == 0)
            {
                //Create New Product
                ViewBag.CategoryList = CategoryList;
                ViewData["CoverTypeList"] =CoverTypeList;
                return View();
            }
            else
            {
                //Edit Existing Product

            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Product obj)
        {
            if (ModelState.IsValid)
            {
                _db.Product.Update(obj);
                _db.Save();
                TempData["success"] = "Product updated successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
