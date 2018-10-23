using System.ComponentModel.DataAnnotations;

namespace MVC_Final_Project.Models
{
    public class User
    {
        [Key]
        public int userID { get; set; }
        public string userName { get; set; }
        public string userPassword { get; set; }
        public int userRole { get; set; }
        public string userPhoto { get; set; }
        public bool userStatus { get; set; }
    }
}