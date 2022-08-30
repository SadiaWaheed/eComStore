using eComStore.DataAccess.Repository.IRepository;
using eComStore.Model;
using Microsoft.AspNetCore.Mvc;

namespace eComStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;
        public CompanyController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        //Get
        public IActionResult Upsert(int? id)
        {
            Company company = new();
            if (id == null || id == 0)
            {
                //Create New Product
                return View(company);
            }
            else
            {
                //Edit Existing Product
                company = _unitOfWork.Company.GetFirstOrDefault(i => i.Id == id);
                return View(company);
            }
        }
        [Microsoft.AspNetCore.Mvc.HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)
                {
                    _unitOfWork.Company.Add(obj);
                }
                else
                {
                    _unitOfWork.Company.Update(obj);
                }

                _unitOfWork.Save();
                TempData["success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }
        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _unitOfWork.Company.GetAll(); 
            return Json(new { data = companyList});

        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var obj = _unitOfWork.Company.GetFirstOrDefault(i => i.Id == id);

            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}


