using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Rocky.Models.ViewModels;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utilitu;

namespace Rocky.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderHeaderRepository _orderHederlRepos;
        private readonly IOrderDetailRepository _orserDetailRepos;
        private readonly IConfiguration _config;

        [BindProperty]
        public OrderVM OrderVm { get; set; }

        public OrderController(
            IOrderDetailRepository orderDetailRepos,
            IOrderHeaderRepository orderHederlRepos,
            IConfiguration config)
        { 
            _orderHederlRepos = orderHederlRepos;
            _orserDetailRepos = orderDetailRepos; 
            _config = config;
        }
        public IActionResult Index()
        {
            OrderListVM orderListVM = new OrderListVM()
            {
                OrderHList = _orderHederlRepos.GetAll(),
                StatusList = WC.ListStatus.ToList().Select(i => new SelectListItem(
                    text: i,
                    value: i))

            };
            return View(orderListVM);
        }

        public IActionResult Detail(int id)
        {
            OrderVm = new OrderVM()
            {
                OrderHeader = _orderHederlRepos.FirstOfDefault(u => u.Id == id),
                OrderDetails = _orserDetailRepos.GetAll(u => u.OrderHeaderId == id,
                    includeProperties:"Product")

            };
            return View(OrderVm);
        }
    }
}
