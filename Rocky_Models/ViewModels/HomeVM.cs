using System.Collections;
using System.Collections.Generic;

namespace Rocky.Models
{
    public class HomeVM
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Category { get; set; }
    }
}
