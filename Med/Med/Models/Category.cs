using System.ComponentModel.DataAnnotations;

namespace Med.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Категория продукта обязательна.")]

        public string CategoryName { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
