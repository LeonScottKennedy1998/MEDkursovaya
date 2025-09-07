using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedAPI.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        [Required(ErrorMessage = "Имя продукта обязательно.")]

        public string NameProduct { get; set; }
        [Required(ErrorMessage = "Описание продукта обязательно.")]

        public string DescriptionProduct { get; set; }
        [Required(ErrorMessage = "Цена продукта обязательна.")]

        public decimal Price { get; set; }
        [Required(ErrorMessage = "Категория продукта обязательна.")]

        public int CategoryID { get; set; }
        [Required(ErrorMessage = "Количество продукта обязательно.")]

        public int Stock { get; set; }
        [Required(ErrorMessage = "Изображение продукта обязательно.")]


        public string ImageUrl { get; set; }

        public Category? Category { get; set; }

        public ICollection<Review>? Reviews { get; set; }

    }
}

