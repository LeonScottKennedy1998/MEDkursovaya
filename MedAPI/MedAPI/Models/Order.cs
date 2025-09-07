using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedAPI.Models
{
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required(ErrorMessage = "ID пользователя обязательно.")]

        public int UserID { get; set; }
        [Required(ErrorMessage = "Дата заказа обязательна.")]

        public DateTime OrderDate { get; set; }= DateTime.UtcNow;
        public User? User { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
