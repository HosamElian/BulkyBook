using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Stripe;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CompanyController(IUnitOfWork UnitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _UnitOfWork = UnitOfWork;
            _hostEnvironment = hostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        //GET
        public IActionResult Upsert(int? id)
        {
            Company company = new();
            if (id == null || id == 0)
            {
                // Create company
                return View(company);
            }
            else
            {
                // Update company
                company = _UnitOfWork.Company.GetFirstOrDefualt(u=> u.Id == id);
                return View(company);
            }
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                
                if(company.Id == 0) 
                { 
                    _UnitOfWork.Company.Add(company);
                    TempData["success"] = "Company Created successfully";
                }
                else
                {
                    _UnitOfWork.Company.Update(company);
                    TempData["success"] = "Company Updated successfully";
                }
                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var companyList = _UnitOfWork.Company.GetAll();
            return Json(new {data = companyList});
        }
        //Post
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            var companyfromDb = _UnitOfWork.Company.GetFirstOrDefualt(u => u.Id == id);
            if (companyfromDb == null)
            {
                return Json(new {success = false, message = "Error while deleting"});
            }

            _UnitOfWork.Company.Remove(companyfromDb);
            _UnitOfWork.Save();
            return Json(new {success = true, message = "Company deleted successfully" });
        }
        #endregion

    }

}

