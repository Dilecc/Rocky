using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace test_3.Pages.BaseModels
{
    public class NewPaymentModel : PageModel
    {
        [BindProperty, Required] 
        public string ShopId { get; set; } 

        [BindProperty, Required]
        public string SecretKey { get; set; }
    }
}
