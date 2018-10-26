using System.ComponentModel.DataAnnotations;

namespace MVC_Final_Project.Models
{
    public class Work
    {
        [Key]
        public int workID { get; set; }
        public string workName { get; set; }
        public int workState { get; set; }
        public int sprintID { get; set; }
        public int projectID { get; set; }
    }
}