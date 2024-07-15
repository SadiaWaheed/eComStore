using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using eComStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eComStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class GenreController : Controller
    {
        private readonly IUnitOfWork _db;
        public GenreController(IUnitOfWork db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Genre> obj = _db.Genre.GetAll();
            return View(obj);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var objFromDb = _db.Genre.GetFirstOrDefault(x => x.Id == id);

            if(objFromDb == null) return NotFound();

            return View(objFromDb);
        }
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var obj = _db.Genre.GetFirstOrDefault(i=>i.Id == id);

            if(id == null) return NotFound();

            _db.Genre.Remove(obj);
            _db.Save();
            TempData["success"] = "Genre deleted successfully!";
            return RedirectToAction("Index");
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var objFromDb = _db.Genre.GetFirstOrDefault(i=> i.Id == id);

            if(objFromDb == null) return NotFound();

            return View(objFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Genre obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order cannot be the same as the Name");
            }
            if (ModelState.IsValid)
            {
                _db.Genre.Update(obj);
                _db.Save();
                TempData["success"] = "Genre updated successfully!";
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
        public IActionResult Create(Genre obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order cannot be the same as the Name");
            }
            if (ModelState.IsValid)
            {
                _db.Genre.Add(obj);
                _db.Save();
                TempData["success"] = "Genre created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
