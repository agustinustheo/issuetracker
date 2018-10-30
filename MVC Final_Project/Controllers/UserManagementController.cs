using MVC_Final_Project.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MVC_Final_Project.Controllers
{
    public class UserManagementController : Controller
    {
        public string connString = ConfigurationManager.ConnectionStrings["OJDSQLConn"].ToString();
        public ActionResult Index()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Index", "Account");
            }
            if (Convert.ToInt32(Session["UserRole"]) != 1)
            {
                return RedirectToAction("Index", "Home");
            }
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT userID, userName, userPhoto, userRole, userStatus FROM msUser ORDER BY userName ASC;";
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    List<User> userList = new List<User>();
                    while (reader.Read())
                    {
                        int userBool = reader.GetInt32(4);
                        if (userBool == 1)
                        {
                            userList.Add(new User() { userID = reader.GetInt32(0), userName = reader.GetString(1), userPhoto = reader.GetString(2), userRole = reader.GetInt32(3), userStatus = true });
                        }
                        else
                        {
                            userList.Add(new User() { userID = reader.GetInt32(0), userName = reader.GetString(1), userPhoto = reader.GetString(2), userRole = reader.GetInt32(3), userStatus = false });
                        }

                    }
                    ViewBag.userList = userList;
                }
                connection.Close();
            }
            ViewBag.Username = Session["Username"].ToString();

            return View();
        }
        public async Task<PartialViewResult> UserPartial()
        {
            return PartialView("_AddUser");
        }
        public async Task<PartialViewResult> EditUserPartial(int userID)
        {
            User user = new User();
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT userID, userName, userPassword, userRole, userPhoto, userStatus FROM msUser WHERE userID = @userID;";
                command.Parameters.AddWithValue("@userID", userID);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == true)
                {
                    while (reader.Read())
                    {
                        user.userID = reader.GetInt32(0);
                        user.userName = reader.GetString(1);
                        user.userPassword = reader.GetString(2);
                        user.userRole = reader.GetInt32(3);
                        user.userPhoto = reader.GetString(4);
                        int userCurrentStatus = reader.GetInt32(5);
                        if (userCurrentStatus == 1)
                        {
                            user.userStatus = true;
                        }
                        else
                        {
                            user.userStatus = false;
                        }
                    }
                }
                command.Parameters.Clear();
                connection.Close();
            }
            return PartialView("_EditUser", user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(User userData)
        {
            userData.userPhoto = "";
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "INSERT INTO msUser (userName, userPassword, userRole, userPhoto, userStatus) VALUES (@userName, @userPassword, @userRole, @userPhoto, @userStatus); INSERT INTO trAuthUser(userID, projectID, userAuth) SELECT (SELECT TOP 1 userID FROM msUser ORDER BY userID DESC) AS [userID], projectID, '0' AS [userAuth] FROM msProject;";
                command.Parameters.AddWithValue("@userName", userData.userName);
                command.Parameters.AddWithValue("@userPassword", userData.userPassword);
                command.Parameters.AddWithValue("@userRole", userData.userRole);
                command.Parameters.AddWithValue("@userPhoto", userData.userPhoto);
                if (userData.userStatus == true)
                {
                    command.Parameters.AddWithValue("@userStatus", 1);
                }
                else
                {
                    command.Parameters.AddWithValue("@userStatus", 0);
                }
                SqlDataReader reader = command.ExecuteReader();
                command.Parameters.Clear();
                connection.Close();
                return RedirectToAction("Index", "UserManagement");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateUser(User userData)
        {
            userData.userPhoto = "";
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "UPDATE msUser SET userName = @userName, userPassword = @userPassword, userRole = @userRole, userPhoto = @userPhoto, userStatus = @userStatus WHERE userID = @userID;";
                command.Parameters.AddWithValue("@userID", userData.userID);
                command.Parameters.AddWithValue("@userName", userData.userName);
                command.Parameters.AddWithValue("@userPassword", userData.userPassword);
                command.Parameters.AddWithValue("@userRole", userData.userRole);
                command.Parameters.AddWithValue("@userPhoto", userData.userPhoto);
                if (userData.userStatus == true)
                {
                    command.Parameters.AddWithValue("@userStatus", 1);
                }
                else
                {
                    command.Parameters.AddWithValue("@userStatus", 0);
                }
                SqlDataReader reader = command.ExecuteReader();
                command.Parameters.Clear();
                connection.Close();

                connection.Open();
                command.CommandText = "SELECT userName, userRole FROM msUser WHERE userID=@userID;";
                command.Parameters.AddWithValue("@userID", Convert.ToInt32(Session["UserID"]));
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Session["Username"] = reader.GetString(0);
                    Session["UserRole"] = reader.GetInt32(1);
                }
                command.Parameters.Clear();
                connection.Close();
            }
            if (Convert.ToInt32(Session["UserRole"]) == 1)
            {
                return RedirectToAction("Index", "UserManagement");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}