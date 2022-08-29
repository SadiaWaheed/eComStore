using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            return View();
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
                productVM.Product = _db.Product.GetFirstOrDefault(i => i.Id == id);
                return View(productVM);
            }
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(rootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.Product.ImageUrl != null)
                    {
                        var oldImage = Path.Combine(rootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

                }

                if (obj.Product.Id == 0)
                {
                    _db.Product.Add(obj.Product);
                }
                else
                {
                    _db.Product.Update(obj.Product);
                }

                _db.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _db.Product.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _db.Product.GetFirstOrDefault(i => i.Id == id);

            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImage = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }

            _db.Product.Remove(obj);
            _db.Save();

            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}
