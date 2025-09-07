using System.ComponentModel.DataAnnotations;

namespace Med.Models
{
    public class RegisterModel
    {
        [Key]
        public int UserID { get; set; }
        [Required(ErrorMessage = "Не указано имя")]
        public string? NameUser { get; set; }

        [Required(ErrorMessage = "Не указан email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Не указан логин")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Не указан пароль")]
        [DataType(DataType.Password)]
        public string? PasswordUser { get; set; }

        [Required(ErrorMessage = "Не указан адрес")]
        public string? AddressUser { get; set; }

        [Required(ErrorMessage = "Не указана роль")]
        public int RoleID { get; set; } 
    }
}
