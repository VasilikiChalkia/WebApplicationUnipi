using DevExpress.Data.ODataLinq.Helpers;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Numerics;
using System.Web.Mvc;
using System.Web.Security;

namespace WebApplicationUnipi.Controllers
{
    public class LoginController : Controller
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        MyApplicationUnipiEntities ctx = new MyApplicationUnipiEntities();
        public static Logger logger = LogManager.GetLogger("logger");


        // GET: Login
        public ActionResult Index()
        {
            // If called from Forgot pass / Registration display message
            if (TempData["Message"] != null)
                ViewBag.success = TempData["Message"];

            return View();
        }

      
        public static List<int> GenerateAttributeList(int usrId)
        {
            //using (var db = new MyApplicationUnipiEntities())
            //    return db.Attributes_Values.Where(x => x.User_Id == usrId && x.Value == true.ToString()).Select(x => x.Attribute_Id).ToList();
            using (var db = new MyApplicationUnipiEntities())

                return db.Attributes_Values.Where(x => x.User_Id == usrId && x.Status == 1).Include(v => v.Attributes.Name).OrderBy(b => b.Attributes.Name).Select(x => x.Attribute_Id).ToList();
        }

        [HttpPost]
        public ActionResult LoginUser(User u)
        {
            List<int> PriveList = new List<int>();

            if (ModelState.IsValid)
            {
                try
                {
                    var user = new HR_Employee
                    {
                        Personal_Email = u.Username,
                        Password = u.Password,
                    };

                    HR_Employee dbUser = ctx.HR_Employee.FirstOrDefault(x => x.Personal_Email == user.Personal_Email && x.Password == user.Password && x.Status != 3);
                    if (dbUser != null)
                    {
                        Session["timerOn"] = System.Web.HttpContext.Current.Session.Timeout;
                        Session["Username"] = dbUser.Personal_Email; // Use the database value
                        Session["EmployeeName"] = dbUser.Employee_Fname; // Correctly set the Employee_Fname
                    
                        Session["UserId"] = dbUser.Id;



                        PriveList = GenerateAttributeList(dbUser.Id);
                        Session["UserRights"] = PriveList;

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                    
                        ViewData["Message"] = "Invalid Credentials, try Again.";
            
                        return View("Index");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.Message);
                    ViewData["Message"] = ex.Message;
                }
            }
            return View("Index");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["Username"] = null;
            Session.Abandon();
            return RedirectToAction("Index");
        }

    }

}