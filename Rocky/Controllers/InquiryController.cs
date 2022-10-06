using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Models;
using Rocky_DataAccess.Repository;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utilitu;
using WC = Rocky_Utilitu.WC;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class InquiryController : Controller
    {
        private readonly IInquiryDetailRepository _inquiryDetRepo;
        private readonly IInquiryHeaderRepository _inquiryHeadRepo;

        [BindProperty]
        public InquiryVM InquiryVM { get; set; }
        
        public InquiryController(IInquiryDetailRepository inquiryDetRepo,
            IInquiryHeaderRepository inquiryHeadRepo)
        {
            _inquiryDetRepo = inquiryDetRepo;
            _inquiryHeadRepo = inquiryHeadRepo;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detail(int id)
        {
            InquiryVM = new InquiryVM()
            {
                InquiryHeader = _inquiryHeadRepo.FirstOfDefault(u => u.Id  == id),
                InquireDetails = _inquiryDetRepo.GetAll(u => u.InquireHeaderId == id, includeProperties:"Product")
            };
            return View(InquiryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Detail()
        {
            List<ShoppingCart> shoppingCarts = new List<ShoppingCart>();
            InquiryVM.InquireDetails = _inquiryDetRepo.GetAll(u => u.InquireHeaderId == InquiryVM.InquiryHeader.Id);

            foreach (var obj in InquiryVM.InquireDetails)
            {
                ShoppingCart shoppingCart = new ShoppingCart()
                {
                    ProductId = obj.ProductId,
                };
                shoppingCarts.Add(shoppingCart);
            }

            HttpContext.Session.Clear();
            HttpContext.Session.Set(WC.SessionCart, shoppingCarts);
            HttpContext.Session.Set(WC.SessionInquiryId, InquiryVM.InquiryHeader.Id);

            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        public IActionResult Delete()
        {
            InquiryHeader inquiryHeader = _inquiryHeadRepo.FirstOfDefault(u => u.Id == InquiryVM.InquiryHeader.Id);
            IEnumerable<InquireDetail> inquireDetails =
                _inquiryDetRepo.GetAll(u => u.InquireHeaderId == InquiryVM.InquiryHeader.Id);
            _inquiryDetRepo.RemoveRange(inquireDetails);
            
            _inquiryHeadRepo.Remove(inquiryHeader);
            _inquiryHeadRepo.Save();
            TempData[WC.Success] = "Заказ удален";


            return RedirectToAction(nameof(Index));
        }

        #region API CALLS

        [HttpGet]
        public ActionResult GetInquiryList()
        {
            return Json(new { data = _inquiryHeadRepo.GetAll() });
        }
        
        #endregion
    }
}
