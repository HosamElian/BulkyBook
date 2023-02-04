using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionCart) != null)
                {

                }else
                {
                    HttpContext.Session.SetInt32(SD.SessionCart, 
                        _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claims.Value).ToList().Count);

                }
                return View(HttpContext.Session.GetInt32(SD.SessionCart));
            }
            HttpContext.Session.Clear();
            return View(0);
        }
    }
}
