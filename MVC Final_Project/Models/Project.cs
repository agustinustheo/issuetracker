using System.ComponentModel.DataAnnotations;

namespace MVC_Final_Project.Models
{
    public class Project
    {
        [Key]
        public int projectID { get; set; }
        public string projectName { get; set; }
        public string projectDesc { get; set; }
    }
}