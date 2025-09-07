using System.ComponentModel.DataAnnotations;

namespace MedAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleID { get; set; }
        [Required(ErrorMessage = "Роль обязательна.")]

        public string RoleName { get; set; }
        public ICollection<User>? Users { get; set; }

    }
}
