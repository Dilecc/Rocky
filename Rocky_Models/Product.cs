using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rocky.Models
{
    public class Product
    {
        public Product()
        {
            TempSqFt = 1;
        }

        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не может быть пустым")]
        public string Name { get; set; }
        public string Description { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Значение {0} должно быть от {1} до {2}.")]
        public int Price { get; set; }
        public string Imge { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию")]
        [Display(Name ="Category Type")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Выберите категорию")]
        [Display(Name = "Category Type")]
        public int ApplicationTypeId { get; set; }
        [ForeignKey("ApplicationTypeId")]
        public virtual ApplicationType ApplicationType { get; set; }

        [NotMapped]
        [Range(1, 100000)]
        public int TempSqFt { get; set; }
    }
}
