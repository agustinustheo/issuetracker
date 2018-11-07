using MVC_Final_Project.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MVC_Final_Project.Controllers
{
    public class HomeController : Controller
    {
        public string connString = ConfigurationManager.ConnectionStrings["OJDSQLConn"].ToString();
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Account");
            }
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT a.projectID, a.projectName, a.projectDesc FROM msProject a JOIN trAuthUser b ON a.projectID = b.projectID WHERE b.userID = @userID AND b.userAuth = 1 ORDER BY a.projectName ASC;";
                command.Parameters.AddWithValue("@userID", Convert.ToInt32(Session["UserID"]));
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    List<Project> projectList = new List<Project>();
                    while (reader.Read())
                    {
                        projectList.Add(new Project() { projectID = reader.GetInt32(0), projectName = reader.GetString(1), projectDesc = reader.GetString(2) });
                    }
                    ViewBag.projectList = projectList;
                }
                command.Parameters.Clear();
                connection.Close();
            }
            ViewBag.Username = Session["Username"].ToString();
            return View();
        }

        public async Task<PartialViewResult> ProjectPartial()
        {
            return PartialView("_NewProject");
        }
        public async Task<PartialViewResult> AuthUserPartial(int projectID)
        {
            List<User> userList = new List<User>();
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT a.userID, a.userName, b.userAuth, a.userPhoto FROM msUser a JOIN trAuthUser b ON a.userID = b.userID WHERE b.projectID = @projectID;";
                command.Parameters.AddWithValue("@projectID", projectID);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        int userAuthStatus = reader.GetInt32(2);
                        userList.Add(new User()
                        {
                            userID = reader.GetInt32(0),
                            userName = reader.GetString(1),
                            userPhoto = reader.GetString(3),
                            userStatus = userAuthStatus == 1 ? true : false
                        });
                    }
                }
                connection.Close();
            }
            ViewBag.projectID = projectID;
            ViewBag.userList = userList;
            return PartialView("_AuthorizedUsers");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProject(Project projectData)
        {
            if (projectData.projectName == null || projectData.projectName == "" || projectData.projectDesc == null || projectData.projectDesc == "")
            {
                if (projectData.projectName == null || projectData.projectName == "")
                {
                    ModelState.AddModelError("projectName", "Project Name cannot be empty");
                }
                else if (projectData.projectDesc == null || projectData.projectDesc == "")
                {
                    ModelState.AddModelError("projectDesc", "Project Description cannot be empty");
                }
                return PartialView("_NewProject", projectData);
            }
            else
            {
                using (SqlConnection connection = new SqlConnection(connString))
                using (SqlCommand command = new SqlCommand("", connection))
                {
                    connection.Open();
                    command.CommandText = "INSERT INTO msProject (projectName, projectDesc) VALUES (@projectName, @projectDesc); INSERT INTO trAuthUser (projectID, userID, userAuth) SELECT (SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC) AS [projectID], userID, '0' AS [userAuth] FROM msUser; UPDATE trAuthUser SET userAuth = 1 WHERE userID = @userID AND projectID = (SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC); INSERT INTO trSprint VALUES ('Sprint 1', GETDATE(), DATEADD(week, 2, GETDATE()), (SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC)); INSERT INTO trSprint VALUES ('Sprint 2', DATEADD(week, 2, GETDATE()), DATEADD(week, 4, GETDATE()), (SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC)); INSERT INTO trSprint VALUES ('Sprint 3', DATEADD(week, 4, GETDATE()), DATEADD(week, 6, GETDATE()), (SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC)); INSERT INTO trSprint VALUES ('Sprint 4', DATEADD(week, 6, GETDATE()), DATEADD(week, 8, GETDATE()), (SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC)); INSERT INTO trSprint VALUES ('Sprint 5', DATEADD(week, 8, GETDATE()), DATEADD(week, 10, GETDATE()), (SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC));";
                    command.Parameters.AddWithValue("@userID", Convert.ToInt32(Session["UserID"]));
                    command.Parameters.AddWithValue("@projectName", projectData.projectName);
                    command.Parameters.AddWithValue("@projectDesc", projectData.projectDesc);
                    SqlDataReader reader = command.ExecuteReader();
                    command.Parameters.Clear();
                    connection.Close();
                    return Json(new { code = 1 });
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateProjectAuth(Project projectData, int[] authorizedUsers)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "UPDATE trAuthUser SET userAuth = 0 WHERE projectID = @projectID;";
                command.Parameters.AddWithValue("@projectID", projectData.projectID);
                SqlDataReader reader = command.ExecuteReader();
                command.Parameters.Clear();
                connection.Close();
                foreach (var userID in authorizedUsers)
                {
                    connection.Open();
                    command.CommandText = "UPDATE trAuthUser SET userAuth = 1 WHERE userID = @userID AND projectID = @projectID;";
                    command.Parameters.AddWithValue("@userID", userID);
                    command.Parameters.AddWithValue("@projectID", projectData.projectID);
                    reader = command.ExecuteReader();
                    command.Parameters.Clear();
                    connection.Close();
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}