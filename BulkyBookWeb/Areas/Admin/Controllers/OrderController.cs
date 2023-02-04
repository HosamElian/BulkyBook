using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area(nameof(Admin))]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM? OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        //GET
        public IActionResult Details(int OrderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefualt(u => u.Id == OrderId, indcludeProperties:"ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderId, indcludeProperties: "Product")

            };
            return View(OrderVM);
        }
        // POST
        [ActionName(nameof(Details))]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DetailsPayNow()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeader.GetFirstOrDefualt(u => u.Id == OrderVM.OrderHeader.Id, indcludeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderId == OrderVM.OrderHeader.Id, indcludeProperties: "Product");
           
            //stripe settings 
            var domain = "https://localhost:44384/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                  "card",
                },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"Admin/Order/PaymentConfirmation?OrderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"Admin/Order/Details?OrderId={OrderVM.OrderHeader.Id}",
            };

            foreach (var item in OrderVM.OrderDetail)
            {

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),//20.00 -> 2000
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);

            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int OrderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefualt(u => u.Id == OrderHeaderId);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStatus(OrderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            return View(OrderHeaderId);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefualt(u => u.Id == OrderVM.OrderHeader.Id,tracked:false);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            if(OrderVM.OrderHeader.Carrier != null)
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (OrderVM.OrderHeader.TrackingNumber!= null)
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully";
            return RedirectToAction("Details", "Order", new { OrderId = orderHeaderFromDb.Id});
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Status Updated Successfully";
            return RedirectToAction("Details", "Order", new { OrderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefualt(u => u.Id == OrderVM.OrderHeader.Id, tracked:false);
            orderHeaderFromDb.TrackingNumber = OrderVM?.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = OrderVM?.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully";
            return RedirectToAction("Details", "Order", new { OrderId = OrderVM?.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.GetFirstOrDefualt(u => u.Id == OrderVM.OrderHeader.Id, tracked: false);
            if (orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["Success"] = "Order Cancelled Successfully";
            return RedirectToAction("Details", "Order", new { OrderId = OrderVM?.OrderHeader.Id });
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(indcludeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity = User.Identity as ClaimsIdentity ;
                var claims = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _unitOfWork.OrderHeader.GetAll(u=> u.ApplicationUserId == claims.Value,indcludeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u=> u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new {data = orderHeaders});

        }
        #endregion
    }
}
