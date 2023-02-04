using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize(Roles =SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork UnitOfWork, IWebHostEnvironment hostEnvironment)
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
            ProductVM productVM = new()
            {
                Product = new(),
                CategoryList = _UnitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                CoverTypeList = _UnitOfWork.CoverType.GetAll().Select(u=> new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };

            if (id == null || id == 0)
            {
                // Create Product
                return View(productVM);
            }
            else
            {
                // Update Product
                productVM.Product = _UnitOfWork.Product.GetFirstOrDefualt(u=> u.Id == id);
                return View(productVM);
            }
        }
        //Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);
                    if(productVM.Product.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName+extension), FileMode.CreateNew))
                    {
                        file.CopyTo(fileStreams);
                    }
                    productVM.Product.ImageUrl = @"\images\products\" + fileName+extension;
                }
                if(productVM.Product.Id == 0) 
                { 
                    _UnitOfWork.Product.Add(productVM.Product);
                    TempData["success"] = "Product Created successfully";
                }
                else
                {
                    _UnitOfWork.Product.Update(productVM.Product);
                    TempData["success"] = "Product Updated successfully";
                }
                _UnitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(productVM);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _UnitOfWork.Product.GetAll(indcludeProperties:"Category,CoverType");
            return Json(new {data = productList});
        }
        //Post
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            var productfromDb = _UnitOfWork.Product.GetFirstOrDefualt(u => u.Id == id);
            if (productfromDb == null)
            {
                return Json(new {success = false, message = "Error while deleting"});
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, productfromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _UnitOfWork.Product.Remove(productfromDb);
            _UnitOfWork.Save();
            return Json(new {success = true, message = "Product deleted successfully" });
        }
        #endregion

    }

}

