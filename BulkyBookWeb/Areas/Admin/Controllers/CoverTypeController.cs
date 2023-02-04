using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles =SD.Role_Admin)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;

        public CoverTypeController(IUnitOfWork UnitOfWork)
        {
            _UnitOfWork = UnitOfWork;
        }
        public IActionResult Index()
        {
            IEnumerable<CoverType> ObjCoverTypeList = _UnitOfWork.CoverType.GetAll();
            return View(ObjCoverTypeList);
        }
        //GET
        public IActionResult Create()
        {

            return View();
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.CoverType.Add(coverType);
                _UnitOfWork.Save();
                TempData["success"] = "Cover Type Created successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(coverType);
        }
        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverTypefromDb = _UnitOfWork.CoverType.GetFirstOrDefualt(u => u.Id == id);
            if (coverTypefromDb == null)
            {
                return NotFound();
            }

            return View(coverTypefromDb);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType coverType)
        {
            if (ModelState.IsValid)
            {
                _UnitOfWork.CoverType.Update(coverType);
                _UnitOfWork.Save();
                TempData["success"] = "Cover Type Updated successfully";
                return RedirectToAction(nameof(Index));
            }
            return View(coverType);
        }
        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverTypefromDb = _UnitOfWork.CoverType.GetFirstOrDefualt(u => u.Id == id);
            if (coverTypefromDb == null)
            {
                return NotFound();
            }

            return View(coverTypefromDb);
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            var coverTypefromDb = _UnitOfWork.CoverType.GetFirstOrDefualt(u => u.Id == id);
            if (coverTypefromDb == null)
            {
                return NotFound();
            }

            _UnitOfWork.CoverType.Remove(coverTypefromDb);
            _UnitOfWork.Save();
            TempData["success"] = "Cover Type deleted successfully";

            return RedirectToAction(nameof(Index));
        }

    }
}
