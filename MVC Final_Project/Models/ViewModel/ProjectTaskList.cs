using System.Collections.Generic;

namespace MVC_Final_Project.Models.ViewModel
{
    public class WorkTaskList
    {
        public Work WorkItem { get; set; }
        public List<Task> Tasks { get; set; }
    }
    public class SprintWorkList
    {
        public List<WorkTaskList> Works { get; set; }
        public Sprint SprintItem { get; set; }
    }
}