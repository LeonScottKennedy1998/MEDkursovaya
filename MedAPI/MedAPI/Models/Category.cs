using System.ComponentModel.DataAnnotations;

namespace MedAPI.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Категория продукта обязательна.")]

        public string CategoryName { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
