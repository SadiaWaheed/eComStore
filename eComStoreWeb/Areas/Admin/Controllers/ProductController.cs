using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using eComStore.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace eComStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _db;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProductController(IUnitOfWork db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _hostEnvironment = hostEnvironment;
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
            ProductViewModel productVM = new()
            {
                Product = new(),
                CategoryList = _db.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                CoverTypeList = _db.CoverType.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            if (id == null || id == 0)
            {
                //Create New Product
                return View(productVM);
            }
            else
            {
                //Edit Existing Product

            }
            return View(productVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _hostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(rootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    using(var fileStreams = new FileStream(Path.Combine(uploads,fileName+extension),FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                _db.Product.Add(obj.Product);
                _db.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
