using System;
using System.ComponentModel.DataAnnotations;

namespace MVC_Final_Project.Models
{
    public class Sprint
    {
        [Key]
        public int sprintID { get; set; }
        public string sprintName { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int projectID { get; set; }
    }
}