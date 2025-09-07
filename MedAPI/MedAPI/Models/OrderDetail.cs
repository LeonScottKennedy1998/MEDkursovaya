using System.ComponentModel.DataAnnotations;

namespace MedAPI.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        [Required(ErrorMessage = "ID заказа обязательно.")]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "ID продукта обязательно.")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Количество продукта в заказе обязательно.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Конечная стоимость обязательна.")]
        public decimal TotalPrice { get; set; }

        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
