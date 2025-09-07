using System.ComponentModel.DataAnnotations;

namespace Med.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }
        [Required(ErrorMessage = "Имя пользователя обязательно.")]

        public string NameUser { get; set; }
        [Required(ErrorMessage = "Email обязателен.")]

        public string Email { get; set; }
        [Required(ErrorMessage = "Логин обязателен.")]

        public string Username { get; set; }
        [Required(ErrorMessage = "Пароль обязателен.")]

        public string PasswordUser { get; set; }
        [Required(ErrorMessage = "Адрес обязателен.")]

        public string AddressUser { get; set; }
        [Required(ErrorMessage = "Роль обязательна.")]

        public int RoleID { get; set; }
        public virtual List<Order> Orders { get; set; } = new List<Order>();

    }
}
