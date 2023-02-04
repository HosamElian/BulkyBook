using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CategoryController(IUnitOfWork UnitOfWork)
        {
            _UnitOfWork = UnitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<Category> ObjCategoryList = _UnitOfWork.Category.GetAll();
            return View(ObjCategoryList);
        }
        //GET
        public IActionResult Create()
        {

            return View();
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("CustomError", "The Display order cannot match exactly the name");
            }
            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Add(category);
                _UnitOfWork.Save();
                TempData["success"] = "Category Created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryfromDb = _UnitOfWork.Category.GetFirstOrDefualt(u => u.Id == id);
            if (categoryfromDb == null)
            {
                return NotFound();
            }

            return View(categoryfromDb);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("CustomError", "The Display order cannot match exactly the name");
            }
            if (ModelState.IsValid)
            {
                _UnitOfWork.Category.Update(category);
                _UnitOfWork.Save();
                TempData["success"] = "Category Updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryfromDb = _UnitOfWork.Category.GetFirstOrDefualt(u => u.Id == id);
            if (categoryfromDb == null)
            {
                return NotFound();
            }

            return View(categoryfromDb);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var categoryfromDb = _UnitOfWork.Category.GetFirstOrDefualt(u => u.Id == id);
            if (categoryfromDb == null)
            {
                return NotFound();
            }

            _UnitOfWork.Category.Remove(categoryfromDb);
            _UnitOfWork.Save();
            TempData["success"] = "Category deleted successfully";

            return RedirectToAction(nameof(Index));
        }

    }
}
