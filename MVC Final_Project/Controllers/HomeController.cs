using MVC_Final_Project.Models;
using System;
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
            ViewBag.Username = Session["Username"].ToString();
            return View();
        }

        public ActionResult About()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Account");
            }
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Account");
            }
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<PartialViewResult> ProjectPartial()
        {
            return PartialView("_NewProject");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateProject(Project projectData)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "INSERT INTO msProject (projectName, projectDesc) VALUES (@projectName, @projectDesc); INSERT INTO trAuthUser (projectID, userID) VALUES ((SELECT TOP 1 projectID FROM msProject ORDER BY projectID DESC), @userID);";
                command.Parameters.AddWithValue("@userID", Convert.ToInt32(Session["UserID"]));
                command.Parameters.AddWithValue("@projectName", projectData.projectName);
                command.Parameters.AddWithValue("@projectDesc", projectData.projectDesc);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == false)
                {
                    connection.Close();
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    connection.Close();
                    return RedirectToAction("Index", "Home");
                }
            }
        }
    }
}