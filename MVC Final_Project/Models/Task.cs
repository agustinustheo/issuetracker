using System.ComponentModel.DataAnnotations;

namespace MVC_Final_Project.Models
{
    public class Task
    {
        [Key]
        public int taskID { get; set; }
        public string taskName { get; set; }
        public int taskState { get; set; }
        public int userID { get; set; }
        public int workID { get; set; }
    }
}