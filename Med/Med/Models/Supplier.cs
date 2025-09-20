using System.ComponentModel.DataAnnotations;

namespace Med.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "Название поставщика обязательно.")]
        [StringLength(150, ErrorMessage = "Название поставщика не может превышать 150 символов.")]
        public string SupplierName { get; set; }

        [Required(ErrorMessage = "Имя поставщика обязательно.")]
        [StringLength(100, ErrorMessage = "Имя контакта не может превышать 100 символов.")]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Почта поставщика обязательна.")]
        [EmailAddress(ErrorMessage = "Введите корректный Email.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Телефон поставщика обязателен.")]
        [Phone(ErrorMessage = "Введите корректный номер телефона.")]
        [StringLength(50)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Адрес поставщика обязателен.")]
        [StringLength(200, ErrorMessage = "Адрес не может превышать 200 символов.")]
        public string AddressS { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
