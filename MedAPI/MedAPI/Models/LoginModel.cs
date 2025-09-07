using System.ComponentModel.DataAnnotations;

namespace MedAPI.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Не указан логин")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]

        public string PasswordUser { get; set; }

    }
}
