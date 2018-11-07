using MVC_Final_Project.Models;
using MVC_Final_Project.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MVC_Final_Project.Controllers
{
    public class ProjectController : Controller
    {
        public string connString = ConfigurationManager.ConnectionStrings["OJDSQLConn"].ToString();
        public ActionResult Index(int projectID, int? sprintID)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Account");
            }
            ViewBag.Username = Session["Username"].ToString();

            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT sprintID, sprintName, startDate, endDate, b.projectID, b.projectName FROM trSprint a JOIN msProject b ON a.projectID = b.projectID WHERE a.projectID = @projectID ORDER BY startDate ASC;";
                command.Parameters.AddWithValue("@projectID", projectID);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    List<Sprint> sprintList = new List<Sprint>();
                    while (reader.Read())
                    {
                        sprintList.Add(new Sprint() { sprintID = reader.GetInt32(0), sprintName = reader.GetString(1), startDate = reader.GetDateTime(2), endDate = reader.GetDateTime(3) });
                        ViewBag.projectName = reader.GetString(5);
                    }
                    ViewBag.sprintList = sprintList;
                }
                command.Parameters.Clear();
                connection.Close();
            }
            ViewBag.SprintID = sprintID;
            return View();
        }
        public async Task<PartialViewResult> SprintTable(int sprintID)
        {
            Sprint sprint = new Sprint();
            List<User> userList = new List<User>();
            List<Work> workList = new List<Work>();
            List<Models.Task> taskList = new List<Models.Task>();
            var model = new SprintWorkList();
            model.SprintItem = new Sprint();
            model.Works = new List<WorkTaskList>();

            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT sprintID, sprintName, startDate, endDate, projectID FROM trSprint WHERE sprintID = @sprintID;";
                command.Parameters.AddWithValue("@sprintID", sprintID);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        model.SprintItem.sprintID = reader.GetInt32(0);
                        model.SprintItem.sprintName = reader.GetString(1);
                        model.SprintItem.startDate = reader.GetDateTime(2);
                        model.SprintItem.endDate = reader.GetDateTime(3);
                        ViewBag.projectID = reader.GetInt32(4);
                    }
                }
                command.Parameters.Clear();
                connection.Close();

                connection.Open();
                command.CommandText = "SELECT workID, workName, workState FROM trWork WHERE sprintID = @sprintID;";
                command.Parameters.AddWithValue("@sprintID", sprintID);
                reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        workList.Add(new Work() { workID = reader.GetInt32(0), workName = reader.GetString(1), workState = reader.GetInt32(2) });
                    }
                }
                command.Parameters.Clear();
                connection.Close();

                connection.Open();
                command.CommandText = "SELECT b.taskID, b.taskName, b.taskState, b.userID, b.workID FROM trWork a JOIN trTask b ON a.workID = b.workID WHERE a.sprintID = @sprintID;";
                command.Parameters.AddWithValue("@sprintID", sprintID);
                reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        taskList.Add(new Models.Task() { taskID = reader.GetInt32(0), taskName = reader.GetString(1), taskState = reader.GetInt32(2), userID = reader.GetInt32(3), workID = reader.GetInt32(4) });
                    }
                }
                command.Parameters.Clear();
                connection.Close();

                connection.Open();
                command.CommandText = "SELECT a.userID, userName FROM msUser a JOIN trAuthUser b ON a.userID = b.userID WHERE b.projectID = @projectID AND b.userAuth = 1;";
                command.Parameters.AddWithValue("@projectID", ViewBag.projectID);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    userList.Add(new User() { userID = reader.GetInt32(0), userName = reader.GetString(1) });
                }
                command.Parameters.Clear();
                connection.Close();
            }
            foreach (var work in workList)
            {
                model.Works.Add(new WorkTaskList()
                {
                    WorkItem = work
                });
            }
            foreach (var workViewModel in model.Works)
            {
                workViewModel.Tasks = new List<Models.Task>();
                foreach (var taskListItem in taskList)
                {
                    if (workViewModel.WorkItem.workID == taskListItem.workID)
                    {
                        workViewModel.Tasks.Add(new Models.Task()
                        {
                            taskID = taskListItem.taskID,
                            taskName = taskListItem.taskName,
                            taskState = taskListItem.taskState,
                            userID = taskListItem.userID,
                            workID = taskListItem.workID
                        });
                    }
                }
            }
            ViewBag.userList = userList;
            return PartialView("_SprintTable", model);
        }
        public async Task<PartialViewResult> UpdateSprintPartial(int sprintID)
        {
            Sprint sprintData = new Sprint();
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT sprintID, sprintName, startDate, endDate, projectID FROM trSprint WHERE sprintID = @sprintID";
                command.Parameters.AddWithValue("@sprintID", sprintID);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sprintData.sprintID = reader.GetInt32(0);
                    sprintData.sprintName = reader.GetString(1);
                    sprintData.startDate = reader.GetDateTime(2);
                    sprintData.endDate = reader.GetDateTime(3);
                    sprintData.projectID = reader.GetInt32(4);
                }
                command.Parameters.Clear();
                connection.Close();
            }
            return PartialView("_UpdateSprint", sprintData);
        }
        public async Task<PartialViewResult> AddWorkItemPartial(int sprintID, int projectID)
        {
            ViewBag.sprintID = sprintID;
            ViewBag.projectID = projectID;
            return PartialView("_AddWorkItem");
        }
        public async Task<PartialViewResult> UpdateWorkItemPartial(int workID, int projectID)
        {
            Work workData = new Work();
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT workState, workName, sprintID FROM trWork WHERE workID = @workID";
                command.Parameters.AddWithValue("@workID", workID);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    workData.workState = reader.GetInt32(0);
                    workData.workName = reader.GetString(1);
                    workData.sprintID = reader.GetInt32(2);
                }
                command.Parameters.Clear();
                connection.Close();
            }
            workData.workID = workID;
            workData.projectID = projectID;
            return PartialView("_UpdateWorkItem", workData);
        }
        public async Task<PartialViewResult> UpdateTaskItemPartial(int taskID, int projectID)
        {
            Models.Task taskData = new Models.Task();
            List<User> userList = new List<User>();
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT taskState, taskName, userID, workID FROM trTask WHERE taskID = @taskID";
                command.Parameters.AddWithValue("@taskID", taskID);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    taskData.taskState = reader.GetInt32(0);
                    taskData.taskName = reader.GetString(1);
                    taskData.userID = reader.GetInt32(2);
                    taskData.workID = reader.GetInt32(2);
                }
                command.Parameters.Clear();
                connection.Close();


                connection.Open();
                try
                {
                    command.CommandText = "SELECT a.userID, userName FROM msUser a JOIN trAuthUser b ON a.userID = b.userID WHERE b.projectID = @projectID AND b.userAuth = 1;";
                    command.Parameters.AddWithValue("@projectID", projectID);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        userList.Add(new User() { userID = reader.GetInt32(0), userName = reader.GetString(1) });
                    }
                    command.Parameters.Clear();
                }
                catch (Exception e)
                {

                }
                connection.Close();
            }
            taskData.taskID = taskID;
            ViewBag.userList = userList;
            return PartialView("_UpdateTaskItem", taskData);
        }
        public async Task<PartialViewResult> AddTaskItemPartial(int workID, int projectID)
        {
            List<User> userList = new List<User>();
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT a.userID, userName FROM msUser a JOIN trAuthUser b ON a.userID = b.userID WHERE b.projectID = @projectID AND b.userAuth = 1;";
                command.Parameters.AddWithValue("@projectID", projectID);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    userList.Add(new User() { userID = reader.GetInt32(0), userName = reader.GetString(1) });
                }
                command.Parameters.Clear();
                connection.Close();
            }
            ViewBag.userList = userList;
            ViewBag.workID = workID;
            return PartialView("_AddTaskItem");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateSprintData(Sprint sprintData)
        {
            if (sprintData.sprintName == null || sprintData.sprintName == ""
                || sprintData.startDate == null || sprintData.startDate == DateTime.MinValue
                || sprintData.endDate == null || sprintData.endDate == DateTime.MinValue)
            {
                if (sprintData.sprintName == null || sprintData.sprintName == "")
                {
                    ModelState.AddModelError("sprintName", "Sprint Name cannot be empty");
                }
                else if (sprintData.startDate == null || sprintData.startDate == DateTime.MinValue)
                {
                    ModelState.AddModelError("startDate", "Start Date cannot be empty");
                }
                else if (sprintData.endDate == null || sprintData.endDate == DateTime.MinValue)
                {
                    ModelState.AddModelError("endDate", "End Date cannot be empty");
                }
                return PartialView("_UpdateSprint", sprintData);
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    connection.Open();
                    command.CommandText = "UPDATE trSprint SET sprintName = @sprintName, startDate = @startDate, endDate = @endDate WHERE sprintID = @sprintID;";
                    command.Parameters.AddWithValue("@sprintName", sprintData.sprintName);
                    command.Parameters.AddWithValue("@startDate", sprintData.startDate);
                    command.Parameters.AddWithValue("@endDate", sprintData.endDate);
                    command.Parameters.AddWithValue("@sprintID", sprintData.sprintID);
                    SqlDataReader reader = command.ExecuteReader();
                    command.Parameters.Clear();
                    connection.Close();
                    return Json(new { code = 1, projectID = sprintData.projectID, sprintID = sprintData.sprintID });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddWorkItem(Work workData)
        {
            if (workData.workName == null || workData.workName == "")
            {
                ModelState.AddModelError("workData", "Work Name cannot be empty");
                return PartialView("_AddWorkItem", workData);
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    connection.Open();
                    command.CommandText = "INSERT INTO trWork (workName, workState, sprintID) VALUES (@workName, @workState, @sprintID);";
                    command.Parameters.AddWithValue("@sprintID", workData.sprintID);
                    command.Parameters.AddWithValue("@workName", workData.workName);
                    command.Parameters.AddWithValue("@workState", workData.workState);
                    SqlDataReader reader = command.ExecuteReader();
                    command.Parameters.Clear();
                    connection.Close();
                    return Json(new { code = 1, projectID = workData.projectID, sprintID = workData.sprintID });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateWorkItem(Work workData)
        {
            if (workData.workName == null || workData.workName == "")
            {
                ModelState.AddModelError("workName", "Work Name cannot be empty");
                return PartialView("_UpdateWorkItem", workData);
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    connection.Open();
                    command.CommandText = "UPDATE trWork SET workName = @workName, workState = @workState WHERE workID = @workID;";
                    command.Parameters.AddWithValue("@workID", workData.workID);
                    command.Parameters.AddWithValue("@workName", workData.workName);
                    command.Parameters.AddWithValue("@workState", workData.workState);
                    SqlDataReader reader = command.ExecuteReader();
                    command.Parameters.Clear();
                    connection.Close();
                    return Json(new { code = 1, projectID = workData.projectID, sprintID = workData.sprintID });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddTaskItem(Models.Task taskData)
        {
            if (taskData.taskName == null || taskData.taskName == "")
            {
                ModelState.AddModelError("taskName", "Task Name cannot be empty");
                return PartialView("_AddTaskItem", taskData);
            }
            else
            {
                int[] arrayID = new int[5];
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    connection.Open();
                    command.CommandText = "INSERT INTO trTask (taskName, taskState, userID, workID) VALUES (@taskName, @taskState, @userID, @workID);";
                    command.Parameters.AddWithValue("@userID", taskData.userID);
                    command.Parameters.AddWithValue("@workID", taskData.workID);
                    command.Parameters.AddWithValue("@taskName", taskData.taskName);
                    command.Parameters.AddWithValue("@taskState", taskData.taskState);
                    SqlDataReader reader = command.ExecuteReader();
                    command.Parameters.Clear();
                    connection.Close();

                    connection.Open();
                    command.CommandText = "SELECT TOP 1 a.projectID, b.sprintID FROM trTask d JOIN trWork c ON d.workID = c.workID JOIN trSprint b ON c.sprintID = b.sprintID JOIN msProject a ON b.projectID = a.projectID ORDER BY taskID DESC;";
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        arrayID[0] = reader.GetInt32(0);
                        arrayID[1] = reader.GetInt32(1);
                    }
                    command.Parameters.Clear();
                    connection.Close();
                }
                return Json(new { code = 1, projectID = arrayID[0], sprintID = arrayID[1] });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateTaskItem(Models.Task taskData)
        {
            if (taskData.taskName == null || taskData.taskName == "")
            {
                ModelState.AddModelError("taskName", "Task Name cannot be empty");
                return PartialView("_UpdateTaskItem", taskData);
            }
            else
            {
                int[] arrayID = new int[5];
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    connection.Open();
                    command.CommandText = "UPDATE trTask SET taskName = @taskName, taskState = @taskState, userID = @userID WHERE taskID = @taskID;";
                    command.Parameters.AddWithValue("@userID", taskData.userID);
                    command.Parameters.AddWithValue("@taskID", taskData.taskID);
                    command.Parameters.AddWithValue("@taskName", taskData.taskName);
                    command.Parameters.AddWithValue("@taskState", taskData.taskState);
                    SqlDataReader reader = command.ExecuteReader();
                    command.Parameters.Clear();
                    connection.Close();

                    connection.Open();
                    command.CommandText = "SELECT a.projectID, b.sprintID FROM trTask d JOIN trWork c ON d.workID = c.workID JOIN trSprint b ON c.sprintID = b.sprintID JOIN msProject a ON b.projectID = a.projectID WHERE d.taskID = @taskID;";
                    command.Parameters.AddWithValue("@taskID", taskData.taskID);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        arrayID[0] = reader.GetInt32(0);
                        arrayID[1] = reader.GetInt32(1);
                    }
                    command.Parameters.Clear();
                    connection.Close();
                }
                return Json(new { code = 1, projectID = arrayID[0], sprintID = arrayID[1] });
            }
        }

        public async Task<ActionResult> RemoveWorkItem(int workID, int projectID)
        {
            int? sprintID = null;
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {

                connection.Open();
                command.CommandText = "SELECT TOP 1 sprintID FROM trWork WHERE workID = @workID;";
                command.Parameters.AddWithValue("@workID", workID);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sprintID = reader.GetInt32(0);
                }
                command.Parameters.Clear();
                connection.Close();

                connection.Open();
                command.CommandText = "DELETE FROM trTask WHERE workID = @workID; DELETE FROM trWork WHERE workID = @workID;";
                command.Parameters.AddWithValue("@workID", workID);
                reader = command.ExecuteReader();
                command.Parameters.Clear();
                connection.Close();
                return RedirectToAction("Index", "Project", new { projectID = projectID, sprintID = sprintID });
            }
        }

        public async Task<ActionResult> RemoveTaskItem(int taskID, int projectID)
        {
            int? sprintID = null;
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT TOP 1 b.sprintID FROM trTask a JOIN trWork b ON a.workID = b.workID WHERE taskID = @taskID;";
                command.Parameters.AddWithValue("@taskID", taskID);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    sprintID = reader.GetInt32(0);
                }
                command.Parameters.Clear();
                connection.Close();

                connection.Open();
                command.CommandText = "DELETE FROM trTask WHERE taskID = @taskID;";
                command.Parameters.AddWithValue("@taskID", taskID);
                reader = command.ExecuteReader();
                command.Parameters.Clear();
                connection.Close();
            }
            return RedirectToAction("Index", "Project", new { projectID = projectID, sprintID = sprintID });
        }
    }
}