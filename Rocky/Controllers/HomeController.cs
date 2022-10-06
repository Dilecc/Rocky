using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky_Utilitu;
using Rocky_DataAccess.Repository.IRepository;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _proRepos;
        private readonly ICategoryRepository _catRepos;

        public HomeController(ILogger<HomeController> logger, 
            IProductRepository proRepos, 
            ICategoryRepository catRepos)
        {
            _logger = logger;
            _proRepos = proRepos;
            _catRepos = catRepos;
        }

        public IActionResult Index()
        {
            HomeVM homeVm = new HomeVM()
            {
                Products = _proRepos.GetAll(includeProperties: "ApplicationType,Category"),
                Category = _catRepos.GetAll()
            };
            return View(homeVm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult details(int Id)
        {
            List<ShoppingCart> shopinCartsList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shopinCartsList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            DetailsVM detailsVm = new DetailsVM()
            {
                Product = _proRepos.FirstOfDefault(u => u.Id == Id ,includeProperties: "Category,ApplicationType"),
                ExistsINCart = false
            };
            foreach (var item in shopinCartsList)
            {
                if (Id == item.ProductId)
                {
                    detailsVm.ExistsINCart = true;
                }
            }
            return View(detailsVm);
        }

        [HttpPost,ActionName("details")]
        public IActionResult detailsHttpPost(int Id, DetailsVM detailsVm)
        {
            List<ShoppingCart> shopinCartsList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shopinCartsList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            shopinCartsList.Add(new ShoppingCart() {ProductId = Id, SqFt = detailsVm.Product.TempSqFt});
            HttpContext.Session.Set(WC.SessionCart, shopinCartsList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveFromCart(int Id)
        {
            List<ShoppingCart> shopinCartsList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Any())
            {
                shopinCartsList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            var shopinCartItem = shopinCartsList.SingleOrDefault(u => u.ProductId == Id);


            if (shopinCartItem != null)
            {
                shopinCartsList.Remove(shopinCartItem);
            }

            HttpContext.Session.Set(WC.SessionCart, shopinCartsList);
            TempData[WC.Success] = "Товар удален из корзины";
            return RedirectToAction(nameof(Index));
        }
    }
}
