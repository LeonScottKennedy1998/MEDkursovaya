using Microsoft.EntityFrameworkCore;

namespace Med.Models
{
    public class AdminTables
    {
        public List<User> Users { get; set; }
        public List<Product> Products { get; set; }
        public List<Order> Orders { get; set; }
        public List<Category> Categories { get; set; }
        public List<Role> Roles { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }



    }
}
