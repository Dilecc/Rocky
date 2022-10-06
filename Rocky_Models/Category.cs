using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Rocky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле не может быть пустым")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле не может быть пустым")]
        [Range(1, int.MaxValue, ErrorMessage = "Значение {0} должно быть от {1} до {2}.")]
        [DisplayName("Display Order")]
        public int DispalyOrder { get; set; }
    }
}
