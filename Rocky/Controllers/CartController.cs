using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Utilitu;
using Umbraco.Core.Deploy;

namespace Rocky.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _proRepos;
        private readonly IApplicationUserRepository _applicatUserRepos;
        private readonly IInquiryDetailRepository _inqDetailRepos;
        private readonly IInquiryHeaderRepository _inqHederlRepos;
        private readonly IOrderHeaderRepository _orderHederlRepos;
        private readonly IOrderDetailRepository _orserDetailRepos;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        [BindProperty]
        public ProductUserVM ProductUserVm { get; set; }

        public CartController(IProductRepository proRepos,
            IApplicationUserRepository applicatUserRepos,
            IOrderDetailRepository orderDetailRepos,
            IOrderHeaderRepository orderHederlRepos,
            IInquiryDetailRepository inqDetailRepos,
            IInquiryHeaderRepository inqHederlRepos,
            IWebHostEnvironment webHostEnvironment, 
            IEmailSender emailSender)
        {
            _proRepos = proRepos;
            _inqDetailRepos = inqDetailRepos;
            _inqHederlRepos = inqHederlRepos;
            _orderHederlRepos = orderHederlRepos;
            _orserDetailRepos = orderDetailRepos;
            _applicatUserRepos = applicatUserRepos;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodIntCart = shoppingCartList.Select(u =>u.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _proRepos.GetAll().Where(u => prodIntCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var obj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == obj.ProductId);
                prodTemp.TempSqFt = obj.SqFt;
                prodList.Add(prodTemp);
            }
           
            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Index))]
        public IActionResult IndexPost(List<Product> prodlist)
        {
            List<ShoppingCart> shoppingCartsList = new List<ShoppingCart>();
            foreach (var obj in prodlist)
            {
                shoppingCartsList.Add(new ShoppingCart { ProductId = obj.Id, SqFt = obj.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartsList);
            return RedirectToAction(nameof(Summary));
        }

        [Authorize]
        public IActionResult Summary()
        {
            ApplicationUser applicationUser;
            if (User.IsInRole(WC.AdminRole))
            {
                if (HttpContext.Session.Get<int>(WC.SessionInquiryId) != 0)
                {
                    InquiryHeader inquiryHeader =
                        _inqHederlRepos.FirstOfDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        FullName = inquiryHeader.FullName,
                        Email = inquiryHeader.Email,
                        PhoneNumber = inquiryHeader.PhoneNumber
                    };
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                // var UserId = User.FindFirstValue(ClaimTypes.Name);

                applicationUser = _applicatUserRepos.FirstOfDefault(u => u.Id == claim.Value);
            }

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodIntCart = shoppingCartList.Select(u => u.ProductId).ToList();
            IEnumerable<Product> prodList = _proRepos.GetAll(u => prodIntCart.Contains(u.Id));

            ProductUserVm = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
            };

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = _proRepos.FirstOfDefault(u => u.Id  == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                ProductUserVm.ProductList.Add(prodTemp);
            }

            return View(ProductUserVm);
        }

        [ActionName(nameof(Summary))]
        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SummaryPost(ProductUserVM productUserVm)
        {
            // Находит пользователя? его id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(WC.AdminRole)) //код для создания заказа
            {
                var orderTotal = 0.0;
                foreach (var prod in ProductUserVm.ProductList)
                {
                    orderTotal += prod.Price * prod.TempSqFt;
                }

                OrderHeader orderHeader = new OrderHeader
                {
                    CreatedByUserId = claims.Value,
                    FinalOrderTotal = orderTotal,
                    OrderDate = DateTime.Now,
                    PhoneNumber = productUserVm.ApplicationUser.PhoneNumber,
                    StreetAddress = productUserVm.ApplicationUser.StreetAddress,
                    Email = productUserVm.ApplicationUser.Email,
                    City = productUserVm.ApplicationUser.City,
                    State = productUserVm.ApplicationUser.State,
                    PostalCode = productUserVm.ApplicationUser.PostalCode,
                    FullName = productUserVm.ApplicationUser.FullName,
                    OrderStatus = WC.StatusPending
                };
                _orderHederlRepos.Add(orderHeader);
                _orderHederlRepos.Save();
                
                foreach (var prod in ProductUserVm.ProductList)
                {
                    OrderDetail orderDetail = new OrderDetail()
                    { 
                        OrderHeaderId = orderHeader.Id,
                        PricePerSqFt = prod.Price,
                        Sqft = prod.TempSqFt,
                        ProductId = prod.Id
                    };
                    _orserDetailRepos.Add(orderDetail);
                }
                _orderHederlRepos.Save();
                return RedirectToAction(nameof(InquaryConfirmation), new {id = orderHeader.Id});

            }
            else // код для создания запроса
            {
                var pathToTemplate = _webHostEnvironment.WebRootPath
                     + Path.DirectorySeparatorChar.ToString()
                     + "templates" + Path.DirectorySeparatorChar.ToString() +
                     "Inquiry.html";
                var subject = "new inquiry";
                var htmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(pathToTemplate))
                {
                    htmlBody = sr.ReadToEnd();
                }

                StringBuilder productList = new StringBuilder();
                foreach (var prod in productUserVm.ProductList)
                {
                    productList.Append($" - Name: {prod.Name} <span style='font-size=14px;'> (ID: {prod.Id}) </span> <br>");
                }

                // Name: { 0 }
                // Email: { 1 } 
                // Phone : { 2 }
                // Products: { 3 }

                string messageBodu = string.Format(htmlBody,
                    ProductUserVm.ApplicationUser.FullName,
                    ProductUserVm.ApplicationUser.Email,
                    ProductUserVm.ApplicationUser.PhoneNumber,
                    productList.ToString()
                );

                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBodu);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claims.Value,
                    FullName = ProductUserVm.ApplicationUser.FullName,
                    PhoneNumber = ProductUserVm.ApplicationUser.PhoneNumber,
                    Email = ProductUserVm.ApplicationUser.Email,
                    InquiryDate = DateTime.Now
                };

                _inqHederlRepos.Add(inquiryHeader);

                _inqHederlRepos.Save();

                foreach (var prod in ProductUserVm.ProductList)
                {
                    InquireDetail inquireDetail = new InquireDetail()
                    {
                        InquireHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id
                    };
                    _inqDetailRepos.Add(inquireDetail);
                }
                _inqDetailRepos.Save();
                TempData[WC.Success] = "Товар оформлен";
            }



            return RedirectToAction(nameof(InquaryConfirmation));
        }

        public IActionResult InquaryConfirmation( )
        {
            HttpContext.Session.Clear();

            return View();
        }


        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var shoppingCatItem = shoppingCartList.SingleOrDefault(u => u.ProductId == id);
            if (shoppingCatItem != null)
            {
                shoppingCartList.Remove(shoppingCatItem);
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            TempData[WC.Success] = "Товар удален из корзины";


            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCart(List<Product> prodlist)
        {
            List<ShoppingCart> shoppingCartsList = new List<ShoppingCart>();
            foreach (var obj in prodlist)
            {
                shoppingCartsList.Add(new ShoppingCart {ProductId = obj.Id,SqFt = obj.TempSqFt});
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartsList);
            return RedirectToAction(nameof(Index));
        }

    }
}
