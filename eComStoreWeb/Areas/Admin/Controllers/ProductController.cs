using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model.ViewModels;
using eComStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace eComStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
                }),
                GenreList = _db.Genre.GetAll().Select(i => new SelectListItem
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
        public IActionResult Upsert(ProductViewModel obj, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                string rootPath = _hostEnvironment.WebRootPath;
                if (files != null || files?.Count != 0)
                {
                    if (obj.Product.ImageUrl != null)
                    {
                        var imageList = obj.Product.ImageUrl.Split(',').SkipLast(1);
                        if (files.Count() + imageList.Count() >= 10)
                        {

                            TempData["error"] = "No more than 10 images per product be added. Please remove extra images.";
                            return View(obj);
                        }
                        foreach (var url in imageList)
                        {
                            var oldImage = Path.Combine(rootPath, obj.Product.ImageUrl.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImage))
                            {
                                System.IO.File.Delete(oldImage);
                            }
                        }

                    }

                    var uploads = Path.Combine(rootPath, @"images\products");
                    foreach (var file in files)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        var extension = Path.GetExtension(file.FileName);
                        using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                        {
                            file.CopyTo(fileStreams);
                        }
                        obj.Product.ImageUrl += @"\images\products\" + fileName + extension + ",";
                    }
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

        public IActionResult RemoveImage(int id, string imageUrl)
        {
            var obj = _db.Product.GetFirstOrDefault(x => x.Id == id);
            var images = obj.ImageUrl.Split(',').SkipLast(1).Where(img => img != imageUrl).ToList();

            string rootPath = _hostEnvironment.WebRootPath;
            var oldImage = Path.Combine(rootPath, imageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }

            obj.ImageUrl = "";
            foreach (var img in images)
            {
                obj.ImageUrl += img + ",";
            }

            _db.Product.Update(obj);
            _db.Save();
            return RedirectToAction("Upsert", new { id = id });
        }
        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _db.Product.GetAll(includeProperties: "Category,CoverType,Genre");
            //includeProperties: "Category,CovertType"
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
