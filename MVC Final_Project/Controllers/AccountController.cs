using MVC_Final_Project.Models;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MVC_Final_Project.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public string connString = ConfigurationManager.ConnectionStrings["OJDSQLConn"].ToString();
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction("Index", "Account");
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(User userData)
        {
            using (SqlConnection connection = new SqlConnection(connString))
            using (SqlCommand command = new SqlCommand("", connection))
            {
                connection.Open();
                command.CommandText = "SELECT TOP 1 * FROM msUser WHERE userName=@userName AND userPassword=@userPassword;";
                command.Parameters.AddWithValue("@userName", userData.userName);
                command.Parameters.AddWithValue("@userPassword", userData.userPassword);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows == false)
                {
                    connection.Close();
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    while (reader.Read())
                    {
                        Session["UserID"] = reader.GetInt32(0);
                        Session["Username"] = reader.GetString(1);
                        //var Password = reader.GetString(2);
                    }
                    connection.Close();
                    return RedirectToAction("Index", "Home");
                }
            }
        }
    }
}