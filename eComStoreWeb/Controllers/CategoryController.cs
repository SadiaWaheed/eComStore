using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using Microsoft.AspNetCore.Mvc;

namespace eComStoreWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _db;
        public CategoryController(ICategoryRepository db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> obj = _db.GetAll();
            return View(obj);
        }
        //Get
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var objFromDb = _db.GetFirstOrDefault(i => i.Id == id);

            if (objFromDb == null) return NotFound();

            return View(objFromDb);
        }
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.GetFirstOrDefault(i => i.Id == id);

            if (obj == null) return NotFound();

            _db.Remove(obj);
            _db.Save();
            TempData["success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }
        //Get
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var objFromDb = _db.GetFirstOrDefault(i => i.Id == id);

            if (objFromDb == null) return NotFound();

            return View(objFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order cannot be the same as the Name");
            }
            if (ModelState.IsValid)
            {
                _db.Update(obj);
                _db.Save();
                TempData["success"] = "Category updated successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        //Get
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order cannot be the same as the Name");
            }
            if (ModelState.IsValid)
            {
                _db.Add(obj);
                _db.Save();
                TempData["success"] = "Category created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
