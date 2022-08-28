using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using Microsoft.AspNetCore.Mvc;

namespace eComStoreWeb.Controllers
{
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _db;
        public CoverTypeController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> objFromDb = _db.CoverType.GetAll();
            return View(objFromDb);
        }
        //Get
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var objFromDb = _db.CoverType.GetFirstOrDefault(i => i.Id == id);

            if (objFromDb == null) return NotFound();

            return View(objFromDb);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var objFromDb = _db.CoverType.GetFirstOrDefault(i => i.Id == id);

            if (objFromDb == null) return NotFound();

            _db.CoverType.Remove(objFromDb);
            _db.Save();
            TempData["success"] = "Cover Type deleted successfully!";
            return RedirectToAction("Index");
        }
        //Get
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0) return NotFound();

            var objFromDb = _db.CoverType.GetFirstOrDefault(i => i.Id == id);

            if (objFromDb == null) return NotFound();

            return View(objFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _db.CoverType.Update(obj);
                _db.Save();
                TempData["success"] = "Cover Type updated successfully!";
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
        public IActionResult Create(CoverType obj)
        {
            if (ModelState.IsValid)
            {
                _db.CoverType.Add(obj);
                _db.Save();
                TempData["success"] = "Cover Type created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
    }
}
